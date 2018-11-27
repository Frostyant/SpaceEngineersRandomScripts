
const string FIGHTER_MODE = "fightermode"; //Timer Block for Fighting Mode
const string NORMAL_MODE = "normalmode"; //Timer Block for Normal Mode
const string TXT_NAME = "memory";//Text panel for monitoring current status [order type];info1;info2etc
const string TargetingGun = "targetinggun";
const string CNTRL_NAME = "Remote Control";
const string FRONT_NAME = "Front";
const string FIRING_MODE = "firingmode";//Timer Block for Firing Mode (turn shoot on)
const string PEACE_MODE = "peacemode";//Timer Block for PEace Mode (turn shoot off)
const string AMOVE = "AMove"; // Attack Move Order
const string MOVE = "Move"; //Move Order
const string DEFEND = "Defend";//Defend order
const string MAIN = "Main"; //Main fighter loop
const string CURRENT_ORDER_DATATYPE = "COrder";//This is the Current order Datatype


#region DETECTION
	public bool CheckForTargets(IMyLargeTurretBase director){

		bool TargetFound = director.HasTarget;//Taken from Rdav Interceptor

		return True;
	}

	//Checks if we should engage, returns 0, 1 or a location we have to go to
	public string ShouldIEngage(IMyRemoteControl cntrl, char InternalSep = ':'){

		List<strings> orders =  DetData(TXT_NAME,CURRENT_ORDER_DATATYPE);

		string datachunk = orders[0];

		string[] data = datachunk.Split(InternalSep)

		string order = data[0];//If we have multiple ones (we should not) then take first

		output = '1';
		//Move => ignore enemy
		if (order == MOVE){
			output = '0';

		}else if (order == DEFEND){

			Vector3 RelativeVec = order.position() - GPStoVector(data[1]);

			if(Vector3.Magnitude(RelativeVec) >> Convert.ToSingle(data[2])){

				output = data[1];//Returns center of defend ball
			}

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

		return targetpos+targetvel;
	}
	public bool  HitOrMiss(IMyRemoteControl cntrl, IMyLargeTurretBase director,Vector3 FrontVec,Vector3 TargetPosition){

		var TargetInfo = director.GetTargetedEntity();

		float EnemySize = (TargetInfo.BoundingBox.Max - TargetInfo.BoundingBox.Min).Length();//Get Enemy SIZE

		//basically frontvec * (our pos - enemy pos) = cos(miss angle)
		//cos(miss angle) * distance between us <= enemysize => we hit it !
		if(Vector3.Dot(FrontVec,cntrl.Position - TargetPosition)*Vector3.Magnitude(cntrl.Position - TargetPosition) <= EnemySize){
			return True;
		}else{
			return False;
		}
	}

	public Vector3 GetFrontVec(string center_,string front_){

		Vector3 center = GetBlockPosition(center_);

		Vector3 front = GetBlockPosition(front_);

		output = front-center;

		return output;
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

			Vector3 gpsvector = new Vector3(Convert.ToSingle(coords[1]),
															 Convert.ToSingle(coords[2]),
															 Convert.ToSingle(coords[3]));

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
	public void GoTo(IMyRemoteControl cntrl, Vector3 Pos, bool Relative){
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
	public List<string> GetData(string txtname, string type, char sep = '\n', char InternalSep = ':')
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
	public void RemoveDataType(IMyTextPanel txt, string type, char sep = '\n', char InternalSep = ':')
	{

	    string[] FullData = GetRef(txt, sep);

	    //convert to list
	    List<string> output = new List<string>(FullData);

	    output.RemoveAll(x => x.Split(InternalSep).First().Equals(type));

	    LcdClear(DATABASE_NAME);

	    LcdPrint(DATABASE_NAME, output);

	}


	//Removes ALL instances of a datatype
	public void RemoveDataType(string txtname, string type, char sep = '\n', char InternalSep = ':')
	{

			IMyTextPanel txt = GridTerminalSystem.GetBlockWithName(txtname) as IMyTextPanel;

			string[] FullData = GetRef(txt, sep);

			//convert to list
			List<string> output = new List<string>(FullData);

			output.RemoveAll(x => x.Split(InternalSep).First().Equals(type));

			LcdClear(DATABASE_NAME);

			LcdPrint(DATABASE_NAME, output);

	}

	public void AddData(string  txtname ,string type, string data, char sep = '\n', char InternalSep = ':'){

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
#endregion

public void Main(string ArgumentFull){

	string[] Arguments = ArgumentFull.Split(';');


	if(Arguments[0] == 'Main'){
		//Check if we are allowed to engage
		string Orders = ShouldIEngage();
		if(Orders == '1'){
			//Amove or Defend in bounds
			IMyLargeTurretBase director = GridTerminalSystem.GetBlockWithName(TargetingGun) as IMyLargeTurretBase;

			if(CheckForTargets(director)){

				ExecuteSimple(FIGHTER_MODE); //Executes high speed fighter mode

				Vector3 Target = GetTargetVector(director);//Gets target location + lead

				GoTo(cntrl,Target,False);//Go to Target

				Vector3 FrontVec = GetFrontVec(CNTRL_NAME,FRONT_NAME);//Get front facing vector

				if(HitOrMiss(cntrl, director,FrontVec,Target)){
					ExecuteSimple(FIRING_MODE);
				}else{
					ExecuteSimple(PEACE_MODE); //stop shooting
				}


			}else{
				ExecuteSimple(NORMAL_MODE); //revert to normal
				ExecuteSimple(PEACE_MODE); //stop shooting
			}
		}else if (orders == '0'){
			//This was a move
			ExecuteSimple(NORMAL_MODE); //revert to normal
			ExecuteSimple(PEACE_MODE); //stop shooting
		}else{
			//Else it was a defend and we were out of bounds
			GoTo(GPStoVector(orders));
		}

	}else if (Argument == "AddOrder") {
		ExecuteSimple(NORMAL_MODE);//revert to normal
		ExecuteSimple(PEACE_MODE); //stop shooting

		RemoveDataType(TXT_NAME,CURRENT_ORDER_DATATYPE);//Remove Past orders

		AddData(TXT_NAME,CURRENT_ORDER_DATATYPE,Arguments[1]);//Adds New Parameters
	}

}
