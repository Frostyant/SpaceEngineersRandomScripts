//Gets "CMD #" TXT panel, where # is a number, and extracts coordinates (first ones) to go to a point. 
 
//Adjust Names Here //
const string CNTRL_NAME = "cntrl"; 
const string TXT_NAME = "CMD 0"; 
 
//Initialising Variables 
 
 
 
 
 
 
 
 
 
 
 
 
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
 
public void Main(){ 
 
 
	IMyRemoteControl cntrl = GridTerminalSystem.GetBlockWithName(CNTRL_NAME) as IMyRemoteControl; 
 
	IMyTextPanel txt = GridTerminalSystem.GetBlockWithName(TXT_NAME) as IMyTextPanel; 
 
	string[] ref0;
	string[] ref1;
 
	//Not We Are Assuming the Command is of the form (position);(other Orders)  in this order 
	ref0 = GetRef(txt,';'); 
 
	//Now This Part May be edited but here the position is of format : x,y,z 
	ref1 =ref0[0].Split(',');
 
	Vector3 obj = new Vector3(Convert.ToSingle(ref1[0]), 
							 Convert.ToSingle(ref1[1]), 
							 Convert.ToSingle(ref1[2])); 
 
 
	GotTo(cntrl,obj,false); 
}