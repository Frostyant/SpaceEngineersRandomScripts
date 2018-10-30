//Gets "CMD #" TXT panel, where # is a number, and extracts coordinates (first ones) to go to a point. 

//"M" = Move here, other info 0 = position to move to
//Message is Assumed to be of the form : Sender ID; Receiver ID; Order Type; other info 0; other info 1 etc... 
 
//Adjust Names Here //
const string CNTRL_NAME = "cntrl"; 
const string TXT_NAME = "orders"; 
 
//Initialising Variables//
IMyRemoteControl cntrl;
IMyTextPanel txt;
 
 
 
 
 
 
 
 
 
 
 
 
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
 
 
 
 
public void GotTo(IMyRemoteControl cntrl, Vector3 Pos, bool Relative){ 
	 
	Vector3 obj; 
 
	if (Relative == true){ 
			obj = Pos + cntrl.GetPosition(); 
	}else{ 
		obj=Pos; 
	} 
 
	cntrl.AddWaypoint(obj,"obj"); 
	cntrl.SetAutoPilotEnabled(true); 
} 
 
//MAIN 
 
public void Main(string Argument){ 
 
 
	IMyRemoteControl cntrl = GridTerminalSystem.GetBlockWithName(CNTRL_NAME) as IMyRemoteControl; 
 
	string[] ref0;
	string[] ref1;
 
	//"M" = Move here, other info 0 = position to move to
	//Message is Assumed to be of the form : Sender ID; Receiver ID; Order Type; other info 0; other info 1 etc... 
	ref0 = Argument.Split(';'); 
 
	//Now This Part May be edited but here the position is of format : GPS:antenna_name:X:Y:Z:
	ref1 =ref0[3].Split(':');
 
	Vector3 obj = new Vector3(Convert.ToSingle(ref1[2]), 
							 Convert.ToSingle(ref1[3]), 
							 Convert.ToSingle(ref1[4])); 
 
	GotTo(cntrl,obj,false); 
}