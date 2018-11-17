//Gets "CMD #" TXT panel, where # is a number, and extracts coordinates (first ones) to go to a point.

//Adjust Names Here //
const string CNTRL_NAME = "cntrl";
const string TXT_NAME = "memory";
const string PING_STATUS = "fleet_status";

//Initialising Variables












//Functions//


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

public bool SendMessage(string target,string rawmessage,char sep = '\n', char InternalSep = ':'){

  string MyId = Me.CubeGrid.EntityId;

  string message ="MSG"+';'+ MyId + ';' + target + ';' + rawmessage;//note raw message should use same separator (ie no internals, so ;)

  return TransmitMessage( message, MyTransmitTarget target = MyTransmitTarget.Default );
}

public bool IDCheck(string given){
  string MyID = Me.CubeGrid.EntityId;

  if(MyID == given){
    return true;
  }else{
    List<string> groups = GetData(TXT_NAME,"group",sep,InternalSep);
    foreach(string group in groups){
      string[] data = DataChunk.Split(InternalSep);
      if(data[1] == given){
        return true;
      }
    }
  }else{
    return false;
  }
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
          LcdPrintln(input[3],PING_STATUS,)
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
