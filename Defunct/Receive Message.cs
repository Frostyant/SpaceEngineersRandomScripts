//Receive Message 
 
// Block names (editable) //
const string TXT0_NAME = "info" ;
//Info Should be of the form : Ship ID $ Name of Laser Antenna 1 % Name of Laser Antenna 2 % ... $ other Stuff
const string TXT1_NAME = "message" ;
//Message Should be empty (or at least unimportant)
//const string TXT2_NAME = "message_queue";
 
//Block Variables //
IMyTextPanel txt0; 
IMyTextPanel txt1; 
IMyLaserAntenna lsr; 
 
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
 

 
string[] GetAntennaName(IMyTextPanel txt0){ 
 
	string[] ref0; 
 
	string[] ref00; 
 
	ref0 = GetRef(txt0,'$'); 
 
	ref00 = ref0[1].Split('%'); 
 
	return ref00; 
 
} 
 
string GetIntel(IMyLaserAntenna lsr){ 
	string raw_output;
	string[] part_output;
 
	raw_output = lsr.DetailedInfo;

	part_output = raw_output.Split(' ');
 
	return part_output[7]; 
} 
 
 
 
public void Main(){ 
 
 	//Initialising Variables//
	IMyTextPanel txt0 = GridTerminalSystem.GetBlockWithName(TXT0_NAME) as IMyTextPanel; 
 
	IMyTextPanel txt1 = GridTerminalSystem.GetBlockWithName(TXT1_NAME) as IMyTextPanel;

	//IMyTextPanel txt2 = GridTerminalSystem.GetBlockWithName(TXT2_NAME) as IMyTextPanel;


 
 
	//Getting Antenna Name//

	string[] ref00;
 
	ref00 = GetAntennaName(txt0); 
 
 
	//Since The Basis of Laser Comms is that the Lasers change name we have to do this. 
 
	IMyLaserAntenna lsr = GridTerminalSystem.GetBlockWithName(ref00[0]) as IMyLaserAntenna; 


 
 
	//Extracting Message//

	string raw;
 
	raw = GetIntel(lsr); 


/*
	//Extracting queue//

	string[] queue;

	queue = GetRef(txt2,";");
*/

 
 	//Checking queue//

 	//if(queue != null){


	//Saving Message//
 
	txt1.WritePublicText(raw);

	//}
 
 
}