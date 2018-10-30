//Read Message

//Message Type Overview//
/*
Format : "order" = meaning
"M" = Move here, other info 0 = GPS position to move to
"ML" = Move here AND Land (on planets), other info 0 = position to move to
"B" = Build ship (for shipyards of all types),() other info 0 = number of ships, other info 1 = type)
"S" = S, Saves info for future referencs (usefull for networks), other info 0 = Type of info (& location to store), other info 1 = info
"MI" = Mine here, other info 0 = position around which to mine(, other info 1 = radius)
"T" = Transfer Items from point A to B, other info 0 = GPS of Point A, other info 1 = GPS of point B(, other info 2 = Items to Transfer)
"P" = Packet this message, other info 0 = Antenna to Send Packet to, other info 1 = Message to Packet (Use $ and % instead of ; and ,)
 A Packet is a Ship that goes from one "Network" of laser antennas to another physically carrying the message across distances.
 This Avoids having to create Antennas EVERYWHERE and is cost effective BUT slow
"D" = Deploy Satellite/Base At given location, other info 0 = Location Where to Deploy, other info 1 = Type
 "TM" = Transfer Message, Transfers Message to another antenna, other info 0 = message
 This is used if the Receiver ID is not that of this Ship
*/

//IMPORTANT NOTICE//
/*
Technically This Script does NOT do ANYTHING.
It Executes A Timer Block with the name "Order" (Depending which one is selected obviously)
The Rest is up to another script to execute
So technically the above are mostly Suggestions 
*/

// Block names (editable)//
const string TXT0_NAME = "info" ;
//Info Should be of the form : Ship ID $ Name of Laser Antenna 1 % Name of Laser Antenna 2 % ... $ other Stuff
const string TXT1_NAME = "message" ;
//Message is Assumed to be of the form : Sender ID; Receiver ID; Order Type; other info 0; other info 1 etc... 
const string TXT2_NAME = "orders";

//Block Variables //
IMyTextPanel txt0; 
IMyTextPanel txt1;
IMyTextPanel txt2;

//Functions//
  
public string GetRaw(IMyTextPanel txt){   
   
	string output;   
   
	output=txt.GetPublicText();   
   
	return output;   
}   
   
   
   
   
   
public string[] GetRef(IMyTextPanel txt,char sep){   
   
	string raw;  
   
	raw=GetRaw(txt);   
   
	string[] output =raw.Split(sep);   
   
	return output;   
   
}

public void Execute(string TIMER_NAME){

	//IMyBeacon bacon = GridTerminalSystem.GetBlockWithName("Bacon") as IMyBeacon;

	//bacon.SetCustomName("IN EXECUTE");

	IMyTimerBlock timer = GridTerminalSystem.GetBlockWithName(TIMER_NAME) as IMyTimerBlock;

	if(timer != null){

		//False find what is the variable type of the actions.

		ITerminalAction start;

		start=timer.GetActionWithName("Start");

		start.Apply(timer);
	}
}



public void Main(){

	//Initialising Variables//

	//IMyBeacon bacon = GridTerminalSystem.GetBlockWithName("Bacon") as IMyBeacon;

	//bacon.SetCustomName("WORKS");
  
	IMyTextPanel txt0 = GridTerminalSystem.GetBlockWithName(TXT0_NAME) as IMyTextPanel;   

	IMyTextPanel txt1 = GridTerminalSystem.GetBlockWithName(TXT1_NAME) as IMyTextPanel;

	IMyTextPanel txt2 = GridTerminalSystem.GetBlockWithName(TXT2_NAME) as IMyTextPanel;

		//bacon.SetCustomName("0");




	//Extracting Info//

	string[] txt0_ref;

	txt0_ref = GetRef(txt0,'$');

	//bacon.SetCustomName("1");




	//Extracting Message//

	string[] txt1_ref;

	txt1_ref = GetRef(txt1,';');

				//bacon.SetCustomName("1.5");

	string message;

	message = String.Join(";",txt1_ref);
			//bacon.SetCustomName("2");



	//Verifiying Recipient//
	if(txt0_ref[0] == txt1_ref[1]){

		//If We Are The Intended Recipient//

			//bacon.SetCustomName("Bacon");

		Execute(txt1_ref[2]);


	}else{

		//If We Are Not The Intended Recipient//

					//bacon.SetCustomName("Bacon");

		Execute("TM");
	}



	//Generating Orders//

	txt2.WritePublicText(message);



	//Resetting message//

	txt1.WritePublicText("");
}