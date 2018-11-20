
const string FIGHTER_MODE = "fightermode"; //Timer Block for Fighting Mode
const string NORMAL_MODE = "normalmode"; //Timer Block for Normal Mode
const string ORDER = "orders";//Text panel for monitoring current status [order type];info1;info2etc
const string TargetingGun = "targetinggun";
const string CNTRL_NAME = "Remote Control";
const string FRONT_NAME = "Front";
const string FIRING_MODE = "firingmode";//Timer Block for Firing Mode (turn shoot on)
const string PEACE_MODE = "peacemode";//Timer Block for PEace Mode (turn shoot off)

public bool CheckForTargets(IMyLargeTurretBase director){

	bool TargetFound = director.HasTarget;//Taken from Rdav Interceptor

	return True;
}

public Vector3 GetTargetVector(IMyLargeTurretBase director){

	var target = director.GetTargetedEntity();

	//Again this is inspired by Rdav
	Vector3 targetpos = target.Position;

	Vector3 targetvel = target.Velocity;

	return targetpos+targetvel;
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

public bool  HitOrMiss(IMyRemoteControl cntrl, IMyLargeTurretBase director,Vector3 FrontVec,Vector3 TargetPosition){

	var TargetInfo = director.GetTargetedEntity();

	float EnemySize = (TargetInfo.BoundingBox.Max - TargetInfo.BoundingBox.Min).Length();//Get Enemy SIZE

	//basically frontvec * (our pos - enemy pos) = cos(miss angle)
	//cos(miss angle) * distance between us <= enemysize => we hit it !
	if(Vector3.Dot(FrontVec,cntrl.Position - TargetPosition)*Vector3.Magnitude(cntrl.Position - TargetPosition) <= EnemySize){
		return True;
	}else{
		return False;
	}
}

public void Main(string Argument){

	if(ShouldIEngage()){

		IMyLargeTurretBase director = GridTerminalSystem.GetBlockWithName(TargetingGun) as IMyLargeTurretBase;

		IMyRemoteControl cntrl = GridTerminalSystem.GetBlockWithName(CNTRL_NAME) as IMyRemoteControl;


		if(CheckForTargets(director)){

			ExecuteSimple(FIGHTER_MODE); //Executes high speed fighter mode

			Vector3 Target = GetTargetVector(director);//Gets target location + lead

			GoTo(cntrl,Target,False);//Go to Target

			Vector3 FrontVec = GetFrontVec(CNTRL_NAME,FRONT_NAME);//Get front facing vector

			if(HitOrMiss(cntrl, director,FrontVec,Target)){
				ExecuteSimple(FIRING_MODE);
			}else{
				ExecuteSimple(PEACE_MODE);
			}


		}else{
			ExecuteSimple(NORMAL_MODE); //revert to normal
		}
	}else{
		ExecuteSimple(NORMAL_MODE); //revert to normal
	}

}
