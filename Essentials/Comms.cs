//Gets "CMD #" TXT panel, where # is a number, and extracts coordinates (first ones) to go to a point.

//Adjust Names Here //
const string CNTRL_NAME = "cntrl";
const string TXT_NAME = "memory";
const string PING_STATUS = "fleet_status";
const string COMMS_NAME = "comms";

//Initialising Variables












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
#endregion





//MAIN

public void Main(string ArgumentFull){
  string[] arguments = ArgumentFull.Split('|');
  //loops over all inputs (allows for sending multiple messages)
  foreach(string argument in arguments){
    string[] input = argument.Split(';');

    //Is this a message ?
    if(input[0] == "MSG"){
      //Is this for me ?
      if(IDCheck(input[2])){
        //Now the various types of orders
        //PingRequest, send my info back
        if(input[3] == "PNGR"){
          bool Success = SendMessage(input[1],"PNG");//todo : ADD dmg report(ie integrity)
        }else if (input[3] == "PNG") {
          LcdPrintln(input[3],PING_STATUS);
        }
      }

    }else if (input[0] == "SND") {
      //Is this an order to send a message ?
      bool Success = SendMessage(input[1],input[2]);
      if(input[2] == "PNGR"){
        LcdClear(PING_STATUS);
      }
    }
  }

}
