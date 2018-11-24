
const string TXT_NAME = "memory";//Text panel for general data
const string TargetingGun = "targetinggun";
const string CNTRL_NAME = "Remote Control";
const string FRONT_NAME = "Front";
const string FIRING_MODE = "firingmode";//Timer Block for Firing Mode (turn shoot on)
const string PEACE_MODE = "peacemode";//Timer Block for PEace Mode (turn shoot off)
const string AMOVE = "AMove"; // Attack Move Order AMove|location
const string MOVE = "Move"; //Move Orders
const string DEFEND = "Defend";//Defend order
const string MAIN = "Main"; //Main fighter loop
const string CURRENT_ORDER_DATATYPE = "COrder";//This is the Current order Datatype
const string FALLBACK_DATATYPE = "FBack";//Fback datatype : Fback|GPS
const float EffectiveRange = (float) 300.0;
const string HP_DISP_NAME = "HPDisplay";
const string SYST_HP_DATATYPE = "SystemHp";//datatype for total system Hp
const string THRUST_HP_DATATYPE = "ThrusterHp";//datatype for thruster system Hp
const string WEAPONS_HP_DATAPYE = "WeaponHp";//datatype for Weapons systems hp
const float CritSystHp = (float) 0.7;
const float CritThustHp = (float) 0.75;
const float CritWeapHp = (float) 0.5;
const char InternalSep = '|';


#region DETECTION
	public bool CheckForTargets(IMyLargeTurretBase director){

		bool TargetFound = director.HasTarget;//Taken from Rdav Interceptor

		return TargetFound;
	}

	//Checks if we should engage, returns 0, 1 or a location we have to go to
	public string ShouldIEngage(IMyRemoteControl cntrl, char InternalSep = '|'){

		List<string> orders =  GetData(TXT_NAME,CURRENT_ORDER_DATATYPE);

		string datachunk = orders[0];

		string[] data = datachunk.Split(InternalSep);

		string order = data[0];//If we have multiple ones (we should not) then take first

		string output = "1";
		//Move => ignore enemy
		if (order == MOVE){
			output = "0";

		}else if (order == DEFEND){

			Vector3 RelativeVec = cntrl.Position - GPStoVector(data[1]);

			if(RelativeVec.Length() > Convert.ToSingle(data[2])){

				output = data[1];//Returns center of defend ball
			}

		}

		return output;
	}
#endregion

#region TARGETING

	public Vector3 GetTargetVector(IMyLargeTurretBase director){

		var target = director.GetTargetedEntity();

		//Again this is inspired by Rdav
		Vector3 targetpos = target.Position;

		Vector3 targetvel = target.Velocity;

		return targetpos + targetvel;
	}
#endregion

#region BASIC FUNCTIONS
	public string GetRaw(IMyTextPanel txt){

		//Gets RAW output from text panel (the text)

		string output;

		output=txt.GetPublicText();

		return output;
	}

	public string[] GetRef(IMyTextPanel txt,char sep){
		//Divides RAW into an array for later use

		string raw;

		raw=GetRaw(txt);

		string[] output =raw.Split(sep);

		return output;

	}

	/// <summary>
	/// does what is says on the tin
	/// </summary>
	/// <param name="gps"></param>
	/// in GPS format (same as if you would copy past it from a GPS coords in-game)
	/// <returns></returns>
	public Vector3 GPStoVector(string gps)
	{

			string[] coords;

			//Getting Target Position//

			coords = gps.Split(':');

			//Pending, need to change indices

			Vector3 gpsvector = new Vector3(Convert.ToSingle(coords[2]),
															 Convert.ToSingle(coords[3]),
															 Convert.ToSingle(coords[4]));

			return gpsvector;
	}

	/// <summary>
	/// Does what it says on the tin
	/// </summary>
	/// <param name="vec"></param>
	/// <returns></returns>
	public string VectorToGPS(Vector3 vec)
	{
			string output;

			output = "GPS:location:"
					+ Convert.ToString(vec.X) + ":"
					+ Convert.ToString(vec.Y) + ":"
					+ Convert.ToString(vec.Z) + ":";

			return output;
	}

	public void ExecuteSimple(string TIMER_NAME){
		//Starts a Timer Block

		IMyTimerBlock timer = GridTerminalSystem.GetBlockWithName(TIMER_NAME) as IMyTimerBlock;


		if(timer != null){


			ITerminalAction start;

			start=timer.GetActionWithName("Start");

			start.Apply(timer);
		}
	}

	//For Remote Controls, makes you go to Pos
	public void GoTo(IMyRemoteControl cntrl, Vector3 Pos, bool Relative = false){
		//orders ship to move to Vector location
		Vector3 obj;

		if (Relative == true){
				obj = Pos + cntrl.GetPosition();
		}else{
			obj=Pos;
		}

		cntrl.ClearWaypoints();
		cntrl.AddWaypoint(obj,"obj");
		cntrl.SetAutoPilotEnabled(true);
	}

	public Vector3 GetBlockPosition(string BLOCK_NAME){
		//Returns Block Position//

		Vector3 output;

		IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName(BLOCK_NAME) as IMyTerminalBlock;

		output = block.GetPosition();

		return output;
	}
#endregion

#region Data Handling

	/// <summary>
	/// Extracts txt info, splits it and gets relevant DataChunks
	/// </summary>
	/// <param name="txt"></param>
	/// txt panel
	/// <param name="sep"></param>
	/// separator used in split, which distinguishes between the different data chunks
	/// <param name="type"></param>
	/// type of data chunk you want
	/// <param name="InternalSep"></param>
	/// separator used in split, which distinguishes between the info within a data chunk
	/// <returns></returns>
	public List<string> GetData(string txtname, string type, char sep = '\n', char InternalSep = '|')
	{

			IMyTextPanel txt = GridTerminalSystem.GetBlockWithName(txtname) as IMyTextPanel;

			string[] FullData = GetRef(txt, sep);
			List<string> output = new List<string>();

			foreach (string DataChunk in FullData)
			{
					string[] Data = DataChunk.Split(InternalSep);
					if (Data[0] == type)
					{
							//Adds full Data chunk
							output.Add(DataChunk);
					}

			}
			return output;
	}


	//Removes ALL instances of a datatype
	public void RemoveDataType(string txtname, string type, char sep = '\n', char InternalSep = '|')
	{

			IMyTextPanel txt = GridTerminalSystem.GetBlockWithName(txtname) as IMyTextPanel;

			string[] FullData = GetRef(txt, sep);

			if(FullData.Length != 0){

				//convert to list
				List<string> output = new List<string>(FullData);

				output.RemoveAll(x => x.Split(InternalSep).First().Equals(type));

				LcdClear(txtname);

				foreach(string data in output){
					LcdPrintln(data,txtname);
				}

			}


	}

	public void AddData(string  txtname ,string type, string data, char sep = '\n', char InternalSep = '|'){

		string Datachunk = type + InternalSep + data + sep;

		LcdPrint(Datachunk,txtname);

	}

		public void LcdPrintln(string msg, string lcdName = "VarPanel")
		{
		    LcdPrint(msg + '\n', lcdName);
		}

		public void LcdPrint(string msg, string lcdName = "VarPanel")
		{
		    IMyTextPanel lcd =
		      GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
		    lcd.WritePublicText(lcd.GetPublicText() + msg);
		}

		/// <summary>
		/// Clears selected LCD's public text and returns it.
		/// </summary>
		/// <param name="lcdName">Block Name of LCD (not the Title!)</param>
		/// <returns></returns>
		public string LcdClear(string lcdName)
		{
				IMyTextPanel lcd =
					GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
				string text = lcd.GetPublicText();
				lcd.WritePublicText("");
				return text;

		}
#endregion

#region HPSystem
  public void InitHpSystem(){

    //initialising

    List<IMyTerminalBlock> AllTerminalBlocks = new List<IMyTerminalBlock>();

    List<IMyTerminalBlock> AllThrustBlocks = new List<IMyTerminalBlock>();


    //Terminal Health

    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(AllTerminalBlocks);

    RemoveDataType(TXT_NAME,SYST_HP_DATATYPE);//just in case

    AddData(TXT_NAME,SYST_HP_DATATYPE,AllTerminalBlocks.Count.ToString());


    //Thrust Health

    GridTerminalSystem.GetBlocksOfType<IMyThrust>(AllThrustBlocks);

    RemoveDataType(TXT_NAME,THRUST_HP_DATATYPE);//just in case

    AddData(TXT_NAME,THRUST_HP_DATATYPE,AllThrustBlocks.Count.ToString());


    //Weapons Health

    int WeaponsCount = 0;

    foreach(IMyTerminalBlock block in AllTerminalBlocks){
      if(is_weapon(block)){
        WeaponsCount += 1;
      }
    }

    RemoveDataType(TXT_NAME,WEAPONS_HP_DATAPYE);//just in case

    AddData(TXT_NAME,WEAPONS_HP_DATAPYE,WeaponsCount.ToString());
  }

  //This is from Phil
  private bool is_weapon(IMyTerminalBlock block) {
    if (block is IMySmallMissileLauncher || block is IMySmallMissileLauncherReload ||
    block is IMyLargeMissileTurret || block is IMyLargeGatlingTurret || block is IMyLargeInteriorTurret ||
    block is IMySmallGatlingGun) {return true;}
    else { return false;}
  }



  public List<float> CheckHp(char InternalSep = '|'){
    //initialising

    List<IMyTerminalBlock> AllTerminalBlocks = new List<IMyTerminalBlock>();

    List<IMyTerminalBlock> AllThrustBlocks = new List<IMyTerminalBlock>();


    //Terminal Health

    int Hp = 0;

    int WeaponsHp = 0;

    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(AllTerminalBlocks);

    foreach(IMyTerminalBlock block in AllTerminalBlocks){
      if(block.IsFunctional){

        Hp = Hp + 1;

        if(is_weapon(block)){
          WeaponsHp = WeaponsHp + 1;
        }

      }

    }



    //Thrust Health

    int ThrustHp = 0;

    GridTerminalSystem.GetBlocksOfType<IMyThrust>(AllThrustBlocks);

    foreach(IMyTerminalBlock block in AllThrustBlocks){

      if(block.IsFunctional){

        ThrustHp = ThrustHp +  1;

      }

    }



    //Getting max hps

    List<string> FullHpString = GetData(TXT_NAME,SYST_HP_DATATYPE);

    List<string> FullThrustString = GetData(TXT_NAME,THRUST_HP_DATATYPE);

    List<string> FullWeaponsString = GetData(TXT_NAME,WEAPONS_HP_DATAPYE);

    int FullHp = Convert.ToInt32(FullHpString[0].Split(InternalSep)[1]);

    int FullThrustHp = Convert.ToInt32(FullThrustString[0].Split(InternalSep)[1]);

    int FullWeaponsHp = Convert.ToInt32(FullWeaponsString[0].Split(InternalSep)[1]);


    //Getting percentages

    float HpPerc = (float) Hp / (float) FullHp;

    float ThrustPerc = (float) ThrustHp / (float) FullThrustHp;

    float WeapPerc = (float) WeaponsHp / (float) FullWeaponsHp;

    List<float> AllHps = new List<float>();


    AllHps.Add(HpPerc);

    AllHps.Add(ThrustPerc);

    AllHps.Add(WeapPerc);

    return AllHps;
  }

	public bool CheckHPThreshold(){
		List<float> Hps = CheckHp();

		if(Hps[0] < CritSystHp || Hps[1] < CritThustHp || Hps[2] < CritWeapHp){
			return false;
		}else{
			return true;
		}
	}
#endregion

#region comms
  public void ReportEnemy(IMyLargeTurretBase director){
    GetTargetVector(director);

    //Run Comms with full argument
  }
#endregion

public void Main(string ArgumentFull){

	string[] Arguments = ArgumentFull.Split(';');

	//First init AI
	if(Arguments[0] == "init"){
		InitHpSystem();
	}else{
		IMyRemoteControl cntrl = GridTerminalSystem.GetBlockWithName(CNTRL_NAME) as IMyRemoteControl;

		//Check if we have to retreat due to lack of HP
		if(CheckHPThreshold()){
			if(Arguments[0] == "Main"){
				//Check if we are allowed to engage from orders

				string orders = ShouldIEngage(cntrl);
				if(orders == "1"){
					//Amove or Defend in bounds
					IMyLargeTurretBase director = GridTerminalSystem.GetBlockWithName(TargetingGun) as IMyLargeTurretBase;


					if(CheckForTargets(director)){

		        //If Targets Found Maintain Positiong & Report
		        //Go to my current position (TO DO : add a bit of randomization for harder targeting by the enemy)
						Vector3 Enemy = GetTargetVector(director);

						Vector3 Us = cntrl.GetPosition();

						Vector3 EnemyRel = Enemy - Us;//Get Enemy Relative To Us

						Vector3 EnemyDir = EnemyRel/EnemyRel.Length();//Normalizing

						Vector3 BestRange = Enemy - EffectiveRange * EnemyDir;//Moving to Effective Range

		        GoTo(cntrl,BestRange);

		        //Report enemy
		        ReportEnemy(director);
					}

				}else if (orders == "0"){
					//This was a move
				}else{
					//Else it was a defend and we were out of bounds
					GoTo(cntrl,GPStoVector(orders));
				}

			}else if (Arguments[0] == "AddOrder") {
				RemoveDataType(TXT_NAME,CURRENT_ORDER_DATATYPE);//Remove Past orders

				AddData(TXT_NAME,CURRENT_ORDER_DATATYPE,Arguments[1]);//Adds New Parameters


				string[] NewOrder = Arguments[1].Split(InternalSep);

				if(NewOrder[0] == AMOVE || NewOrder[0] == MOVE){

					Vector3 vec  = GPStoVector(NewOrder[1]);

					GoTo(cntrl,vec);

				}
			}
		}else{
			List<string> Datachunk =  GetData(TXT_NAME,FALLBACK_DATATYPE);

			string[] data = Datachunk[0].Split(InternalSep);

			//We cannot engage, so we must flee
			GoTo(cntrl,GPStoVector(data[1]));
		}
	}

}
