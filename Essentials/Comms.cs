//Gets "CMD #" TXT panel, where # is a number, and extracts coordinates (first ones) to go to a point.

//Adjust Names Here //
const string CNTRL_NAME = "cntrl";
const string TXT_NAME = "memory";
const string PING_STATUS = "fleet_status";
const string COMMS_NAME = "comms";
const string AI_PB_NAME = "MainAI";
const string CNTRL_NAME = "Remote Control";
const string FRONT_NAME = "Front";

//Initialising Variables
#region PossibleArgument
	const string MESSAGE_ARG = "MSG";//Used to receive messages
	const string SEND_ARG = "SND";//used to send general messages (usually pre-defined by another PB)
	const string AMOVE_ARG = "AMOVE";//used to order a squad to Attack move, format : AMOVE ; SQUADNAME ; POSITION ; [bool] Relative
	//in the above relative means if this is is relative to this ship or in global coords
	//if it IS relative then it is assumed that the vessel is directly point at the target position and POSITION is just a distance
#endregion
#region Separators
	const string ARG_SEP = ';';
	const string MULTI_MSG_SEP = '+';
	const string DATA_SEP_0 = '\n';
	const string DATA_SEP_1 = '|';
	const string GPS_SEP = ':';
#endregion



//Functions//

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

	public void RunPB(string PB_name,argument){
		((IMyProgrammableBlock)GridTerminalSystem.GetBlockWithName(PB_name)).TryRun(argument) //Taken from discord thanks to Malware and Inflex
	}

	public Vector3 GetFrontVec(string center_ = CNTRL_NAME,string front_ = FRONT_NAME){

		Vector3 center = GetBlockPosition(center_);

		Vector3 front = GetBlockPosition(front_);

		output = front-center;

		output = output/output.Length();//Normalizing

		return output;
	}
#endregion


#region Data Handling
  //Checks Given ID
  public bool IDCheck(string given){
    string MyID = Me.CubeGrid.EntityId.ToString();

    if(MyID == given){
      return true;
    }else{/*
      List<string> groups = GetData(TXT_NAME,"group",sep,InternalSep);
      foreach(string group in groups){
        string[] data = DataChunk.Split(InternalSep);
        if(data[1] == given){
          return true;
        }
      }

    }else{*/
      return false;
    }
  }


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

#region comms
  public bool SendMessage(string target,string rawmessage){

    string MyId = Me.CubeGrid.EntityId.ToString();

    string message ="MSG"+';'+ MyId + ';' + target + ';' + rawmessage;//note raw message should use same separator (ie no internals, so ;)

    IMyRadioAntenna radio = GridTerminalSystem.GetBlockWithName(COMMS_NAME) as IMyRadioAntenna;

    return radio.TransmitMessage( message, MyTransmitTarget.Default );
  }

	public void ReceiveMessage(string[] input,char SEP = ARG_SEP){
		//Is this for me ?
		if(IDCheck(input[2])){

			//Now the various types of orders
			//PingRequest, send my info back
			if(input[3] == "PNGR"){
				bool Success = SendMessage(input[1],"PNG");//todo : ADD dmg report(ie integrity)
			}else if (input[3] == "PNG") {
				LcdPrintln(input[3],PING_STATUS);
			}else if(input[3] == "AddOrder"){
				RunPB(AI_PB_NAME,input[3] + SEP + input[4]); // Addorder MSG type : MSG ; SENDERID ; RECEIVER ID ; AddOrder ; AddOrder info
			}

		}


	}

	public void SendMultipleMessages(string[] targets,string[] rawmessages,char SEP = ARG_SEP, char MULTI_SEP = MULTI_MSG_SEP){
		string MyId = Me.CubeGrid.EntityId.ToString();

		string messages;

		//setting up multi message
		for(int it = 0;it < targets.Length; it++){

			string message = message + MULTI_SEP + MESSAGE_ARG + ARG_SEP + MyId + ARG_SEP + target + ARG_SEP + rawmessage;//note raw message should use same separator (ie no internals, so ;)

		}

		//sending message
    IMyRadioAntenna radio = GridTerminalSystem.GetBlockWithName(COMMS_NAME) as IMyRadioAntenna;

    return radio.TransmitMessage( message, MyTransmitTarget.Default );
	}
#endregion

#region Direct C&C
	public void AMove(string SquadName,string GPS, string Relative , char INT_SEP = DATA_SEP_1){
		//Getting squad from memory
		List<string> Squads = GetData(TXT_NAME,SquadName);

		//it is assumed there is only 1 version of each squad
		string SelectedSquad = Squads[0];

		//Extracting Squad info
		string[] SquadFullInfo = SelectedSquad.Split(DATA_SEP_1);

		//Removes first element because thats just the squad name
		SquadFullInfo = SquadFullInfo.Skip(1).ToArray();

		//unit IDs are on even positions (at 0, 2, 4, etc.)
		string[] units = SquadFullInfo.Where((x, i) => i % 2 == 0).ToArray();

		//unit relative formation positions are on odd Positions (at 1, 3, 5, etc)
		string[] positions = SquadFullInfo.Where((x, i) => i % 2 != 0).ToArray();

		bool relative =  Convert.ToBoolean(Relative);

		//if relative holds then we need to add the command ships position to
		if(relative){
			float distance = (float) Convert.ToDouble(GPS);

			Vector3 frontvec = GetFrontVec();

			Vector3 obj = GetPosition(CNTRL_NAME) + distance * frontvec;//Getting objective position in absolute coordinates
		}else{
			Vector3 obj = GPStoVector(GPS);//Converting GPS to vector3
		}

		Vector3 PosVec;

		for(int it = 0;it < positions.Length; it++){

			PosVec = GPStoVector(positions[it]);//Getting position in Vector3

			PosVec = PosVec + obj; // Adding objective to get this units personnale objective point

			positions[it] = VectorToGPS(PosVec);//converting back
		}

		SendMultipleMessages(units,positions);//Send all the messages
	}
#endregion



//MAIN

public void Main(string ArgumentFull){
  string[] arguments = ArgumentFull.Split(MULTI_MSG_SEP);
  //loops over all inputs (allows for sending multiple messages)
  foreach(string argument in arguments){
    string[] input = argument.Split(ARG_SEP);

    //Is this a message ?
    if(input[0] == MESSAGE_ARG){
			ReceiveMessage(input)
      }

    }else if (input[0] == SEND_ARG) {
      //Is this an order to send a message ?
      bool Success = SendMessage(input[1],input[2]);
      if(input[2] == "PNGR"){
        LcdClear(PING_STATUS);
      }
    }else if(input[0] == AMOVE_ARG){
			AMove(input[1],input[2],input[3])
		}
  }

}
