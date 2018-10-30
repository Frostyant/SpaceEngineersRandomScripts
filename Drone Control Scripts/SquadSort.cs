const string SQUAD_NAME = "squad"; //text panel storing

IMyTextPanel squad;//txt panel


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

public Vector3 GPStoVector(string gps){

string[] coords;

//Getting Target Position//

coords = gps.Split(':');

//Pending, need to change indices

Vector3 gpsvector = new Vector3(Convert.ToSingle(coords[1]),
						 Convert.ToSingle(coords[2]),
						 Convert.ToSingle(coords[3]));

return gpsvector;
}

public string VectorToGPS(Vector3 vec){
string output;

output = "GPS:location:"
	+ Convert.ToString(vec.X) +	":"
	+ Convert.ToString(vec.Y) + ":"
	+ Convert.ToString(vec.Z) + ":";

return output;
}

#region LCD-Print-Basic

//ADDS msg on VarPanel while skipping line
public void LcdPrintln(string msg)
{
    LcdPrint(msg + '\n'); //ADDS msg on VarPanel while skipping line
}

//ADDS msg on VarPanel while NOT skipping line  (so next msg is attached it directly)
public void LcdPrint(string msg)
{
    LcdPrint(msg, "VarPanel");//ADDS msg on VarPanel while NOT skipping line  (so next msg is attached it directly)
}

//ADDS msg on lcdname while skipping line
public void LcdPrintln(string msg, string lcdName = "VarPanel")
{
    LcdPrint(msg + '\n', lcdName);//ADDS msg on lcdname while skipping line
}

//ADDS msg on lcdname while NOT skipping line (so next msg is attached it directly)
public void LcdPrint(string msg, string lcdName = "VarPanel")
{
    IMyTextPanel lcd =
      GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel; //init lcd
    lcd.WritePublicText(lcd.GetPublicText() + msg); //ADDS msg on lcdname while NOT skipping line (so next msg is attached it directly)
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

//main

public void Main(string Argument){
  if(Argument == "Sort"){
    IMyTextPanel txt = GridTerminalSystem.GetBlockWithName(SQUAD_NAME) as IMyTextPanel;//init txt

    string[] SquadList;//init Squad list

    SquadList = GetRef(txt,'\n');//Get lists

    Vector3 mean = new Vector3(0,0,0);//init mean vector

    string gps;//Init gps string

    for(int it = 0, it < len(SquadList), it++){

      gps = SquadList.Split(',')[1];

      mean = mean + GPStoVector(SquadList[it]);//Adding all vectors together
    }
    mean = mean/len(SquadList);//Dividing by number of elements to get mean

    LcdClear(txt);//Clear lcd panel

    string temp;//initialising temporary GPS vector

    string msg;//initialisng message

    for(int it = 0, it < len(SquadList), it++){

      temp = VectorToGPS(GPStoVector(SquadList[it]) - mean);//Translating General vectors to local vectors with respect to mean

      msg = SquadList.Split(',')[0] + ',' + temp

      LcdPrintln(msg,txt)
    }

  }


}
