// Space Engineers Laser-Emission Script  


  
// Block names (editable)  //

const string TXT0_NAME = "info";
//Info Should be of the form : Ship ID $ Name of Laser Antenna 1 % Name of Laser Antenna 2 % ... $ other Stuff
const string TXT1_NAME = "orders";
//This has to of the form ("&"" is a VERY charachter in the string): GPS coordinates & message
//VERY IMPORTANT//
/*
GPS COORDINATES MUST BE of the form that you copy paste them out of a laser antenna.
Any other format will NOT work
it shoulde something like (not exact though) : GPS: AntennaName :x:y:z:
*/



  
//Block Variables  //
IMyTextPanel txt0;  
IMyTextPanel txt1;  
IMyLaserAntenna lsr;  




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
  
  
void SendMessage(/*IMyBeacon bacon,*/ IMyLaserAntenna lsr,string message,string pos){ 
	lsr.SetCustomName(message);  
  
	lsr.SetTargetCoords(pos);  
  
	lsr.Connect();
	
	//bacon.SetCustomName("Sending Passed");  
  
}  
  
/*  
string ExtractVector(string coords){  
		ref = coords.Split(",");  
  
	Vector3 target = new Vector3(Convert.ToSingle(ref[0]),   
						 Convert.ToSingle(ref[1]),   
						 Convert.ToSingle(ref[2]));  
	  
	return target;  
}  
*/  
  
  
  
  
  
public void Main() {  




	//Initialising Variables//
  
	IMyTextPanel txt0 = GridTerminalSystem.GetBlockWithName(TXT0_NAME) as IMyTextPanel;   
  
	IMyTextPanel txt1 = GridTerminalSystem.GetBlockWithName(TXT1_NAME) as IMyTextPanel;

	//IMyBeacon bacon = GridTerminalSystem.GetBlockWithName("Bacon") as IMyBeacon;

	//bacon.SetCustomName("Setup Passed");
  
	//Info Format is : Ship ID$Name of Antenna 1% Name of Antenna 2%...$ other stuff  
  
	// For Orders Format is : coordinates to connect to; message  
  
  





 	
	//Getting Laser Antenna// 
  
	string[] txt0ref;  
  
	string[] txt0refelement;  
  
	txt0ref = GetRef(txt0,'$');  
  
	txt0refelement = txt0ref[1].Split('%');  
  
	//Since The Basis of Laser Comms is that the Lasers change name we have to do this.  
  
	IMyLaserAntenna lsr = GridTerminalSystem.GetBlockWithName(txt0refelement[0]) as IMyLaserAntenna;

	//bacon.SetCustomName("Lsr Passed");
 	


 	 
  
  
	//Generating message now//

	string[] txt1ref;  
  
	txt1ref = GetRef(txt1,'&');  
  
	//message is of format : Ship ID ; message

  
	//DANGER : DOUBLe-CHECK LINE BELOW IN C# 
 
	string message; 
  
	message = txt0ref[0] + ";" + txt1ref[1];  
  
  	//bacon.SetCustomName("Message Passed");




  
  
	//Getting Target Coordinates//
 
	string target; 
  
	target = txt1ref[0]; 
 
	string raw; 
 
	raw = target + ":;:"+ message;

	//bacon.SetCustomName("Target Passed");
 



  
  
	//Sending Message//

  	if(lsr != null){
  	//bacon.SetCustomName("lsr Check Passed");
	SendMessage(/*bacon,*/lsr,message,target);


	//Saving NEW Antenna Name//

	txt0refelement[0] = message;


	string txt0refelementtotal = String.Join("%",txt0refelement);

	txt0ref[1]=txt0refelementtotal;

	string txt0reftotal = String.Join("$",txt0ref);

	txt0.WritePublicText(txt0reftotal);

	}
}