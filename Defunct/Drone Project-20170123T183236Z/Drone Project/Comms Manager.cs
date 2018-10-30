//Function of Code//
/*
This piece of code performs 2 tasks, sending and receiving/managing antenna messages.
Indicators serve to guide the program but can be edited for custumisation purposes (just make sure every
grid in your system is on the same verison of the Comms manager ie: has the same Indicators)

When Sending a message the argument should be of the form : Send ReceiverID;message(;password)
When Receiving the argument should be of the form :  Receive SenderID;ReceiverID;message(;password)

(this program automatically makes the send command into an appropriate Receivable message)
The above are using the Default Separators of course, change them if you customized those
*/

/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/
//Setup//
//IMPORTANT//
//ENSURE BLOCK NAMES MATCH !
/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/

//Block Names//
//Antenna used for these purposes (note it must be a laser antenna)
public const string ANTENNA_NAME = "comms";
//Text block with info in it (such as ID and passwords to acces this machine)
public const string INFO_NAME = "info";
//Name of the Programmable Block Actually DOING stuff (or at least ORDERING stuff to be done)
//not included btw (its a dlc, EA business model)
public const string ACTION_CONTROLLER_NAME = "control";
//Beacon
//public const string BACON_NAME = "bacon";




//Indicators//
//MUST MATCH Action Manager indicators !

//Indicates this is a send message order
//applied before the "main" argument
public const string SEND_MESSAGE_IND = "Send";

//Indicates the separation between parts of the argument
//for instance if this indicators is ';', then the send message "message" command is : "Send;Receive;message"
//More about Receive later
//it NEEDS to be a single charachter though
public const char ARG_SEP_IND = ' '; //its a space

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

//Indicates ID Separator
//used to separate IDs, usefull for Grouping grids together and giving collective orders
public const char ID_SEP_IND = '_';

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

  for(int it = 0; it <= ID0_full.Length-1; it++){
    if(ID0_full[it] != ID1_full[it]){
      output = false;
      break;
    }
  }

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



/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/
//dont edit unless you know what you are doing !//
/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/

void Main(string argument)
{
//Initialisation
string[] orders = argument.Split(ARG_SEP_IND);
IMyTextPanel info = GridTerminalSystem.GetBlockWithName(INFO_NAME) as IMyTextPanel;


//Getting Info
string[] intel = GetRef(info,MES_SEP_IND);

    if(orders[0]==SEND_MESSAGE_IND){

      //Getting Antenna
      var antenna = GridTerminalSystem.GetBlockWithName(ANTENNA_NAME) as IMyRadioAntenna;

      string[] fullorders = orders[1].Split(MES_SEP_IND);

      string message = REC_MESSAGE_IND +ARG_SEP_IND
       + intel[0] + MES_SEP_IND + fullorders[1]
       + MES_SEP_IND + fullorders[2] + MES_SEP_IND + fullorders[3];



      //Sending Messages in correct format (orders)
      antenna.TransmitMessage(message);

    }

    if(orders[0]==REC_MESSAGE_IND){
      //getting full message
      string[] fullmessage = orders[1].Split(MES_SEP_IND);

      //checks if message is intended for us

      bool Proceed;
      Proceed = CompareIDs(fullmessage[1],intel[0],ID_SEP_IND);


      if(Proceed == true){

        //Checks if there is a password and if it is correct and puts it in Proceed
        if(intel.Length >= 2){
          if(fullmessage.Length >= 4){

            //if Given Password equals ID password, Proceed, otherwise nope
            if(intel[1]==fullmessage[3]){

              Proceed = true;
            }else{

              Proceed = false;
            }

          }else{
            //no password despite requiring one, so nope
            //bacon.SetCustomName(orders[1]);
            Proceed = false;
          }
        }else{
          //No password in  ID so always Proceed
          Proceed = true;
        }

        if(Proceed == true){
          //Now that we dealt with passwords & IDs Actually do Stuff
          //Get the action controller programmable block
          IMyProgrammableBlock controller = GridTerminalSystem.GetBlockWithName(
          ACTION_CONTROLLER_NAME) as IMyProgrammableBlock;

          //run controller with message as argument
          controller.TryRun(fullmessage[0] + MES_SEP_IND + fullmessage[2]);
        }
      }
    }
}
