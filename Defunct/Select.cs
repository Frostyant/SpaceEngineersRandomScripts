//This script selects a friendly grid using raycast from selector and stores its ID and location in a text panel called squad

//Adjust Names Here //
const string CMRA_NAME = "selector";//camera
const string SQUAD_NAME = "squad"; //text panel storing
double SCAN_DISTANCE = 1000; //max range of raycase, I dunno who is going to select stuff beyond 1 km... Ignaty ?
float PITCH = 0; //Pitch of scan. Yeah humm lets not do selection at 90 degrees of your FoV
float YAW = 0; //Yaw of scan. Yeah humm lets not do selection at 90 degrees of your FoV

//Initialising Variables//
IMyCameraBlock CMRA;//camera
IMyTextPanel squad;//txt panel


//Functions//

/*
Scans in a direction returns a MyDetectedEntityInfo
For further reference see : https://forum.keenswh.com/threads/new-camera-raycast-and-sensor-api-update-01-162-dev.7389290/
*/
public MyDetectedEntityInfo Scan(IMyCameraBlock camera,double ScanDistance,float pitch,float yaw){

public MyDetectedEntityInfo info;//init info

if (camera.CanScan(ScanDistance))//checks if raycast has charged up enough

info = camera.Raycast(ScanDistance,pitch,yaw);//does raycast

return info // May be empty
}

/*
humm do I need to do this ?
truns a vector 3 into space engineers gps format
*/
public string VectorToGPS(Vector3 vec){
string output;//init output

output = "GPS:location:"
	+ Convert.ToString(vec.X) +	":"
	+ Convert.ToString(vec.Y) + ":"
	+ Convert.ToString(vec.Z) + ":";//converts

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
#endregion

//MAIN

public void Main(string Argument){

  if(Argument == "Select"){
  	IMyCameraBlock camera = GridTerminalSystem.GetBlockWithName(CMRA_NAME) as IMyCameraBlock;//init camera

    public MyDetectedEntityInfo info;//init info

    info = Scan(camera, SCAN_DISTANCE,PITCH,YAW)//scans

    if(info != null){

      string fof = info.Relationship;//Gets friend,foe or neutral relashionship. You can't select enemies or rocks

      //TOFIX Make so that it only works on allies
      //if(fof == "Ally"){

        Vector3 LocationVector = info.Position;//Location of detected entity

        string location = VectorToGPS(LocationVector);//Converts location into gps

        string ID = info.EntityId;//Gets unique Entity ID. IMPORTANT : IDS MAY CHANGE IF MERGEBLOCKS INVOLVED !!!!

        string msg = ID + ',' + location //Combines it all into 1 string separating info by,

        LcdPrintln(msg,SQUAD_NAME) //saves it all
      //}
    }


}

}
