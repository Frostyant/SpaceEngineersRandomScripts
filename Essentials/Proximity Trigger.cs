//Proximity Trigger

// Block names (editable)//
const string TXT0_NAME = "trigger";
//Format : GPS Location ; Trigger Timer ; Repeater Timer
const string CNTRL_NAME = "cntrl";
const float SENSITIVITY = 5;




//Block Variables//
IMyTextPanel txt0;
IMyRemoteControle cntrl;
IMyTimerBlock timer0;
IMyTimerBlock timer1;




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
	//Initialisation//

	IMyRemoteControl cntrl = GridTerminalSystem.GetBlockWithName(CNTRL_NAME) as IMyRemoteControl; 
 
	IMyTextPanel txt = GridTerminalSystem.GetBlockWithName(TXT_NAME) as IMyTextPanel;



	//Extracting Trigger//

	string[] txt0_ref;

	txt0_ref = GetRef(txt0,';');



	//Getting Current Position//

	Vector3 me_pos = cntrl.GetPosition();



	//Getting Target Position//

	txt0_ref_target = txt0_ref.Split(':')

	

	//Pending, need to change indices
	Vector3 target_pos = new Vector3(Convert.ToSingle(txt0_ref[2]), 
							 Convert.ToSingle([3]), 
							 Convert.ToSingle(ref1[4])); 



	//Getting Distance//

	Vector3 relative_pos = target_pos - me_pos;

	distance = relative_pos.Magnitude;


	//Checking Distance

	if(distance < SENSITIVITY){

		Execute(txt0_ref[1],"Start")
	}else{
		Execute(txt0_ref[2],"Start")
	}


}