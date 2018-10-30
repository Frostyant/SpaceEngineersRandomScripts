//Ping Status//
//This Script regularly pings a station to update status & get orders

//Block names (editable) //
const string TXT0_NAME = "info";
//Info Should be of the form : Ship ID $ Name of Laser Antenna 1 % Name of Laser Antenna 2 % ... $ Commander ID $ Commander GPS $ other info 0 $ other info 1 $ status
//There are 2 empty spots between GPS & status, this may be confusing, but is because I did some scripts that use those before this one
//you can change it if you want, but be carefull you dont use one of my Assembler/Refinery Coordination scripts
const string TXT1_NAME = "orders";
//ORDERS is Assumed to be of the form : Sender ID; Receiver ID; Order Type; other info 0; other info 1 etc... 
// "PS" = Ping Status, other info 0 = antenna position, other info 1 = status


//Functions//
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

public void ExecuteSimple(string TIMER_NAME){
	//Starts a Timer Block

	IMyTimerBlock timer = GridTerminalSystem.GetBlockWithName(TIMER_NAME) as IMyTimerBlock;


	if(timer != null){


		ITerminalAction start;

		start=timer.GetActionWithName("Start");

		start.Apply(timer);
	}
}

public void Execute(string TIMER_NAME, string ACTION_NAME){
	//Executes Timer Action 
	IMyTimerBlock timer;

	IMyTimerBlock timer = GridTerminalSystem.GetBlockWithName(TIMER_NAME) as IMyTimerBlock;

	if(timer != null){

		ITerminalAction start;

		start=timer.GetActionWithName(ACTION_NAME);

		start.apply(timer);
	}
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

public Vector3 GetBlockPosition(BLOCK_NAME){
	//Returns Block Position//

	Vector3 output;

	IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName(BLOCK_NAME) as IMyTerminalBlock;

	output = block.GetPosition();

	return output;
}







public void Main(){
	//Initialising//
	IMyTextPanel txt0 = GridTerminalSystem.GetBlockWithName(TXT0_NAME) as IMyTextPanel;
	IMyTextPanel txt1 = GridTerminalSystem.GetBlockWithName(TXT1_NAME) as IMyTextPanel;

	//Extracting info//
	string[] txt0_ref;
	txt0_ref = GetRef(txt0,'$');

	//Getting laser position//
	string[] lsr;
	Vector3 vector_lsr;
	string gps_lsr;

	lsr = txt0_ref[1].Split('%');
	vector_lsr = GetBlockPosition(lsr[0]);
	gps_lsr = VectorToGPS(vector_lsr);

	//Setting Up Orders//
	string orders;
	orders = txt0_ref[3] + "&" + txt0_ref[2] + ";" + "PS" + ";" + gps_lsr + ";" + txt0_ref[6];


	//Saving orders & executing
	txt1.WritePublicText(orders);
	ExecuteSimple("SM")
}