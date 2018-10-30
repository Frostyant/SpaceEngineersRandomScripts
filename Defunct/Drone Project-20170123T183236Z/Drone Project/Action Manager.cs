//Function of Code//
/*
The point of this code is to translate messages received from the Comms Manager
Then Apply the various actions while keeping security

CURRENT FUNCTIONS :
  move to location (done)
  move in formation (WIP)
  move to relative location (WIP)
  send orders (done)
  send order to move to location of ship (done)
  send order to move into formation (WIP)
  send order to selected IDs to move to set location (WIP)
  send order to selected IDs to move to relative location (WIP)

//Standard format using ',' as separator : "orderID,password,orderinfo0,orderinfo1,..."
//example move order : "M,1234,GPS:0:0:0"
*/
float output_inventoryFreeVol = (float)output_temp.MaxVolume - (float)output_temp.CurrentVolume;
int TransferQty = (int)Math.Floor(output_inventoryFreeVol * (1 / density));
bool isTransferrable = TransferFrom.TransferItemTo(output_temp, j, 1, true, TransferQty);
amount -= TransferQty;

/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/
//Setup//
//IMPORTANT//
//ENSURE BLOCK NAMES BELOW MATCH !
/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/

//Block Names//
//Text block with info in it (such as ID and passwords to acces this machine)
public const string INFO_NAME = "info";//needs to be on the LEFT of cntrl
//Remote Controle
public const string CNTRL_NAME = "cntrl";
//Comms Manager Programmable block
public const string COMMS_CONTROLLER_NAME = "comms manager";
//Text Block with Formations definitions
public const string TACTICAL_INFO_NAME = "tactical"; //needs to be FRONT of cntrl
//Beacon
//public const string BACON_NAME = "bacon";


//END OF SETUP//

//orders list//
//Go to GPS
//orderinfo0 = GPS
public const string ORD_MOVE = "M"; //works
//Go to GPS
//orderinfo0 = GPS
//orderinfo1 = formationID
//orderinfo2 = forward vector
//orderinfo3 = side vector (left)
public const string ORD_MF = "MF";
//Send order
//orderinfo0 = targetID
//orderinfo1 = orderID
//orderinfo(n) = orderinfo(n-2) of that type ()
//last order info : comms manager password
public const string ORD_SEND = "S"; //works
//Move Target to Waypoint (WIP)
//orderinfo0 = targetID
//orderinfo1 = GPS location
//orderinfo2 = comms manager password
public const string ORD_MTW = "MTW";
//Move Target here
//orderinfo0 = targetID
//orderinfo1 = comms manager password
public const string ORD_MTH = "MTH"; //works
//Move Target here in formation
//orderinfo0 = target ID
//orderinfo1 = formationID
//orderinfo2 = comms manager password
public const string ORD_MTHF = "MTHF";
//Move Targets to relative positions (WIP)
public const string ORD_MTR = "MTR";
//move to relative position (WIP)
public const string ORD_MR = "MR";


//PASSORD//
//this password is more secure than the Comms one BUT it is "hardwired"
//ie you have to change the programming block to edit this
//also dont use the standard one
//really DONT
public const string PASSWORD = "1234";



//Indicators//
//MUST MATCH Comms Manager indicators !
//Indicates this is a send message order
//applied before the "main" argument
public const string SEND_MESSAGE_IND = "Send";

//Indicates the separation between parts of the argument
//for instance if this indicators is ';', then the send message "message" command is : "Send;Receive;message"
//More about Receive later
//it NEEDS to be a single charachter though
public const char ARG_SEP_IND = ' ';

//Indicates that this is just to check for received messages
//For Various Design reasons you NEED to apply the Receive indicator AFTER the Send one
//THis Insures that the PB on the receiving end doesn't confuse this with some other order
public const string REC_MESSAGE_IND = "Receive";

//Indicates the message Separators AND in the info text panel
//Used to separate info in a message
//Usually all messages should be of the form : "SenderID;ReceiverID;orders (;password)" when SENT
//for info it will usually be : "ID (;password)"
//you might not have the password bit hence the ()
//note that Receiver ID can just indicate a GROUP of IDs,
//but that depends on how you have your ID system Setup
public const char MES_SEP_IND = ';';

//Indicates orders Separator
//this is the separator used within the message part above
//Standard format using ',' as separator : "orderID,password,orderinfo0,orderinfo1,..."
//example move order : "M,1234,GPS:0:0:0"
//within full message : "SenderID;ReceiverID;M,1234,GPS:0:0:0;comms password"
public const char ORD_SEP_IND = ',';

//Indicates ID Separator
//used to separate IDs, usefull for Grouping grids together and giving collective orders
public const char ID_SEP_IND = '_';

//Indicates tactical separator
//the formations panel has 3 Separators
//the MES separator to distinguish formations
//the ORD separator to distinguish positions in a formation
//the tactical separator to indicate which number is x y and z
//using the standard on that makes :
//position in formation;formation ID;x0:y0:z0,x1,y1,z1;formation 2 ID;x0 etc...
public const char TACT_SEP_IND = ':';



/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/
//dont edit unless you know what you are doing !//
/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/

//Methods//

//Extracts "raw" info from text panel
public string GetRaw(IMyTextPanel txt){
  string output;
  output =txt.GetPublicText();
  return output;
}

public string[] GetRef(IMyTextPanel txt,char sep){
	//Divides RAW into an array for later use
	string raw;
	raw=GetRaw(txt);
	string[] output =raw.Split(sep);
	return output;
}

public bool CompareIDs(string ID0,string ID1,char sep){
  //Checks if ID0 contains or is equal to ID1
  //if yes, return true, else returns false

  bool output;

  output = true;

  string[] ID0_full = ID0.Split(sep);
  string[] ID1_full = ID1.Split(sep);

  for(int it = 0; it <= ID0_full.Length; it++){
    if(ID0_full[it] != ID1_full[it]){
      output = false;
      break;
    }
  }

  return output;
}

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

public Vector3 GPStoVector(string gps){

	string[] coords;

	//Getting Target Position//

	coords = gps.Split(':');

	//Pending, need to change indices

	Vector3 gpsvector = new Vector3(Convert.ToSingle(coords[2]),
							 Convert.ToSingle(coords[3]),
							 Convert.ToSingle(coords[4]));

	return gpsvector;
}

public void SendOrder(string[] fullorders/*, IMyBeacon bacon*/){

  IMyProgrammableBlock controller = GridTerminalSystem.GetBlockWithName(
  COMMS_CONTROLLER_NAME) as IMyProgrammableBlock;

  //full order[3] contains the orderID we are sending, then password etc.
  string message = fullorders[4] + ORD_SEP_IND + PASSWORD;

  //this is for special info
  for(int it = 5 ; it <= fullorders.Length-2;it++){
      message = message + ORD_SEP_IND + fullorders[it];
  }


  //prepping message in appropriate format
  //fullorder[2] contains the targetID


  string order = SEND_MESSAGE_IND + ARG_SEP_IND + "C" + MES_SEP_IND +
    fullorders[3] + MES_SEP_IND +
     message + MES_SEP_IND +
      fullorders[fullorders.Length-1];

  //bacon.SetCustomName(order);


  controller.TryRun(order);
}

public string VectorToGPS(Vector3 vec){
	string output;

	output = "GPS:location:"
		+ Convert.ToString(vec.X) +	":"
		+ Convert.ToString(vec.Y) + ":"
		+ Convert.ToString(vec.Z) + ":";

	return output;
}

public Vector3 GetTactical(string tacticsID, string[] tactical_intel){
  Vector3 output;
  int relevant = 0;
  //checks for relevant formation
  for(int it = 1; it <= (tactical_intel.Length-2);it += 2){
    if(tacticsID == tactical_intel[it]){
      relevant = it;
      break;
    }
  }

  if(relevant != 0){
    //gets relevant formation from ID
    string[] relevant_tactic = tactical_intel[relevant+1].Split(ORD_SEP_IND);

    //applies its position in formation
    string[] our_tactic =  relevant_tactic[ Convert.ToInt32(tactical_intel[0])].Split(TACT_SEP_IND);

    //converting to GPS string to use the GPS to VEC Methods
    string gps = "GPS"+ ":" +"rnd" +":"
    +our_tactic[0] + ":" + our_tactic[1] + ":" +our_tactic[2] + ":";

    //converting to Vector3
    output = GPStoVector(gps);



  }else{
    //fill in tactical properly//
    output = new Vector3(0,0,0);
  }

  return output;
}

public Vector3 GetUnitVector(Vector3 v0, Vector3 v1){
  //gets unit vector in direction of v0-v1
  Vector3 output;

  output = v0 - v1;

  output.Normalize();

  return output;
}

public Vector3 GetUnitNormalVector(Vector3 v0,Vector3 v1){
  //gets unit vector in the normal direction to v0 v1

  Vector3 output;

  output = CrossProduct(v0,v1);

  output.Normalize();

  return output;
}

//picked up from the Internet, because its not built in
//by James Ramsden
Vector3 CrossProduct(Vector3 v1, Vector3 v2)
{
  double x, y, z;
  x = v1.Y * v2.Z - v2.Y * v1.Z;
  y = (v1.X * v2.Z - v2.X * v1.Z) * -1;
  z = v1.X * v2.Y - v2.X * v1.Y;

  var rtnvector = new Vector3(x, y, z);
  return rtnvector;
}



/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/
//dont edit unless you know what you are doing !//
/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/

void Main(string argument)
{
  //IMyBeacon bacon = GridTerminalSystem.GetBlockWithName(BACON_NAME) as IMyBeacon;
  //getting full message
  string[] fullmessage = argument.Split(MES_SEP_IND);

  //bacon.SetCustomName("0");

  //bacon.SetCustomName(fullmessage[0]);
  //getting full orders
  string[] fullorders = fullmessage[1].Split(ORD_SEP_IND);
  //bacon.SetCustomName(fullmessage[1]);


  //checking hardwired password
  if(PASSWORD == fullorders[1]){

    //Now time to chekc for every single possible order//

    //if order is to move
    if(fullorders[0] == ORD_MOVE){

      //bacon.SetCustomName("2");

      //Getting Remote control
      IMyRemoteControl cntrl = GridTerminalSystem.GetBlockWithName(CNTRL_NAME) as IMyRemoteControl;

      //Clearing PAst waypoints

      //get objective vector
      Vector3 ObjectiveVector = GPStoVector(fullorders[2]);
      //move to location
      GoTo(cntrl,ObjectiveVector,false);
    }else{
      //if order is to send order
      if(fullorders[0] == ORD_SEND){
        SendOrder(fullorders/*,bacon*/);
      }else{
        if(fullorders[0] == ORD_MTH){
          //bacon.SetCustomName("MTH");
          //Getting Remote control
          IMyRemoteControl cntrl = GridTerminalSystem.GetBlockWithName(CNTRL_NAME) as IMyRemoteControl;
          //Getting local position
          Vector3 pos = cntrl.GetPosition();

          //converting to GPS for formatting purposes
          string GPS = VectorToGPS(pos);

          //preping send order
          string sendorders =
          "who cares about this ?" + ORD_SEP_IND + "no one, these are skipped"
          + ORD_SEP_IND + fullorders[2] + ORD_SEP_IND + ORD_MOVE
          + ORD_SEP_IND + GPS + ORD_SEP_IND + fullorders[3];


          //formatting send order for the send function
          string[] sendordersfull = sendorders.Split(ORD_SEP_IND);
          //bacon.SetCustomName(sendorders);

          //sending ! (to Comms Manager)
          SendOrder(sendordersfull/*,bacon*/);
        }else{
          if(fullorders[0] == ORD_MTW){
            //preping send order
            string sendorders =
            "who cares about this ?" + ORD_SEP_IND + "no one, these are skipped"
            + ORD_SEP_IND + fullorders[2] + ORD_SEP_IND + ORD_MOVE
            + ORD_SEP_IND + fullorders[3] + ORD_SEP_IND + fullorders[4];

            //formatting send order for the send function
            string[] sendordersfull = sendorders.Split(ORD_SEP_IND);

            //sending ! (to Comms Manager)

            SendOrder(sendordersfull/*,bacon*/);

          }else{
            if(fullorders[0] == ORD_MF){
              //bacon.SetCustomName(fullorders[3]);
              //Getting Remote control
              IMyRemoteControl cntrl = GridTerminalSystem.GetBlockWithName(CNTRL_NAME) as IMyRemoteControl;
              //Getting formations
              IMyTextPanel tact = GridTerminalSystem.GetBlockWithName(TACTICAL_INFO_NAME) as IMyTextPanel;



              //Getting Front Direction Vector

              //Getting Left Direction Vector

              //Getting UP direction Vector

              string[] tact_intel = GetRef(tact,MES_SEP_IND);
              //bacon.SetCustomName(tact_intel[1]);

              //getting displacement due to formation
              Vector3 displacement = GetTactical(fullorders[3],tact_intel);

              //Getting objective Vector
              Vector3 ObjectiveVector = GPStoVector(fullorders[2]);

              //Getting front vector
              Vector3 front = GPStoVector(fullorders[4]);

              //Getting left vector
              Vector3 left = GPStoVector(fullorders[5]);

              //Getting Normal Vector (ie UP vector)
              Vector3 up = GetUnitNormalVector(front,left);

              //Moving
              GoTo(cntrl,
              ObjectiveVector
              + displacement*front + displacement*left + displacement *up
              , false);
              //bacon.SetCustomName("t");
            }else{
              if(fullorders[0] == ORD_MTHF){
                //bacon.SetCustomName("MTHF");
                //Getting Remote control
                IMyRemoteControl cntrl = GridTerminalSystem.GetBlockWithName(CNTRL_NAME) as IMyRemoteControl;
                //Getting formations
                IMyTextPanel tact = GridTerminalSystem.GetBlockWithName(TACTICAL_INFO_NAME) as IMyTextPanel;
                //Getting info
                IMyTextPanel info = GridTerminalSystem.GetBlockWithName(INFO_NAME) as IMyTextPanel;

                //Getting local position
                Vector3 pos = cntrl.GetPosition();

                //Getting Front unit vector
                Vector3 front = GetUnitVector(pos,tact.GetPosition());

                //Getting Left unit vector
                Vector3 left = GetUnitVector(pos,info.GetPosition());

                //converting to GPS for formatting purposes
                string GPS = VectorToGPS(pos);

                //preping send order
                string sendorders =
                "who cares about this ?" + ORD_SEP_IND + "no one, these are skipped"
                + ORD_SEP_IND + fullorders[2] + ORD_SEP_IND + ORD_MF
                + ORD_SEP_IND + GPS + ORD_SEP_IND
                + fullorders[3] + ORD_SEP_IND
                + VectorToGPS(front) + ORD_SEP_IND
                + VectorToGPS(left) + ORD_SEP_IND
                + fullorders[4];


                //formatting send order for the send function
                string[] sendordersfull = sendorders.Split(ORD_SEP_IND);
                //bacon.SetCustomName(sendorders);

                //sending ! (to Comms Manager)
                SendOrder(sendordersfull/*,bacon*/);

              }
            }

          }

        }
      }
    }


  }else{
    //TO DO issue warning
  }






}
