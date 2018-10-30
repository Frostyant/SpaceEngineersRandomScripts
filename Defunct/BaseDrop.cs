//Drop A Base (Space)//

//"D" = Deploy Satellite/Base At given location, other info 0 = Location Where to Deploy, other info 1 = Type
//Message is Assumed to be of the form : Sender ID; Receiver ID; Order Type; other info 0; other info 1 etc... 

//Adjust Names Here//
const string CNTRL_NAME = "cntrl";
const string TXT0_NAME = "orders";
const string TXT1_NAME = "trigger";
//Format : GPS Location ; Trigger Timer ; Repeater Timer
const string TRIGGER_NAME = "drop";
const string REPEAT_NAME = "repeater";

//Initialising Variables// 
IMyRemoteControl cntrl;
IMyTextPanel txt;



//Functions//



public string Vector3ToString(Vector3 vector){

	string[] output_array;

	string output;

	output_array[0] = Convert.ToString(vector.X);

	output_array[1] = Convert.ToString(vector.Y);

	output_array[2] = Convert.ToString(vector.Z);

	output = output_array[0] + " " + output_array[1] + " " + output_array[2];
}



public void Execute(string TIMER_NAME, string ACTION_NAME){
	IMyTimerBlock timer;

	IMyTimerBlock timer = GridTerminalSystem.GetBlockWithName(TIMER_NAME) as IMyTimerBlock;

	if(timer != null){

		ITerminalAction start;

		start=timer.GetActionWithName(ACTION_NAME);

		start.apply(timer);
	}
}





//Main//

public void Main(){

	//Initialising Variables//
  
	IMyTextPanel txt0 = GridTerminalSystem.GetBlockWithName(TXT0_NAME) as IMyTextPanel;

	IMyTextPanel txt1 = GridTerminalSystem.GetBlockWithName(TXT1_NAME) as IMyTextPanel;

	IMyRemoteControl cntrl = GridTerminalSystem.GetBlockWithName(CNTRL_NAME) as IMyRemoteControl;




	//Extracting orders//

	string[] txt0_ref;

	txt0_ref = GetRef(txt0,;);




	//Seting Up Move Order//

	string move;

	move = txt0_ref[0] + ";" + txt0_ref[1] + ";" + "M" +";" + txt0_ref[3];




	//Saving Move Order//

	txt0.WritePublicText(move);




	//Setting Up Trigger//

	trigger = txt0_ref[3] + ";" + TRIGGER_NAME + ";" + REPEAT_NAME;




	//Saving Trigger//

	txt1.WritePublicText(trigger);




	//Starting Trigger Timer Block//

	Execute(REPEAT_NAME,"Start");



	//Starting Move order//

	Execute("M","Start");

}

