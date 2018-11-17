
const string FIGHTER_MODE = "fightermode"; //text panel storing
const string NORMAL_MODE = "normalmode"; //text panel storing
const string ORDER = "orders";//Text panel for monitoring current status [order type];info1;info2etc
const string TargetingGun = "targetinggun";
const string CNTRL = "Remote Control";
const string Front = "Front";

public bool CheckForTargets(string gatlingname){
	return True;
}

public Vector3 GetTargetVector(string gatlingname){
	return null
}

public Vector3 GetFrontVec(string center_,string front_){

	Vector3 center = GetBlockPosition(center_);

	Vector3 front = GetBlockPosition(front_);

	output = front-center;

	return output;
}

public Vector3 GetBlockPosition(string BLOCK_NAME){
	//Returns Block Position//

	Vector3 output;

	IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName(BLOCK_NAME) as IMyTerminalBlock;

	output = block.GetPosition();

	return output;
}

public bool ShouldIEngage(){

	IMyTextPanel txt = GridTerminalSystem.GetBlockWithName(ORDER) as IMyTextPanel;

	string[] orders =  GetRef(txt);

	output = True;
	if (order == "Move"){
		output = False;
	}else{
		if(orders == "Hold"){
			output = False;
		}
	}
	return output
}

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

//For Remote Controls, makes you go to Pos
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

public void Main(string Argument){

	if(ShouldIEngage()){
		if(CheckForTargets(TargetingGun)){

			ExecuteSimple(FIGHTER_MODE); //Executes high speed fighter mode

			Vector3 Target = GetTargetVector(TargetingGun);//Gets target location

			GoTo(cntrl,Target,False);//Go to Target

			Vector3 ourvec = GetFrontVec(CNTRL,Front);//Get front facing vector

			//Check ScanDistance
			
			//if yes fire


		}else{
			ExecuteSimple(NORMAL_MODE); //revert to normal
		}
	}else{
		ExecuteSimple(NORMAL_MODE); //revert to normal
	}

}
