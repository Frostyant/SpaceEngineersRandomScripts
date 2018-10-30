//Experimental Coordinator : Moving Emiting Side (ECMES) 
 
// Block names 
const string TXT_NAME="intel";
 
//Block Variables 
IMyTextPanel[] txt; 
IMyLaserAntenna[] lsr;

//Other Variables
string Ref;
 
string GetRaw(IMyTextPanel txt){ 
 
	string output; 
 
	output=txt.GetPublicText(); 
 
	return output; 
} 
 
string[] GetRef(IMyTextPanel txt,String sep){ 
 
	string raw;
 
	raw=GetRaw(txt); 
 
 
	string [] output; 
 
output =raw.Split(sep[0]); 

return output;
 
} 
 
void SendMessage(IMyLaserAntenna lsr,string message,string pos){ 

	lsr.SetCustomName(message); 
 
	lsr.SetTargetCoords(pos); 
 
	lsr.Connect(); 
 
} 
 
 
 
 
 
 
 
 
 
void Main(){
 
txt[0] =  
		GridTerminalSystem.GetBlockWithName(TXT_NAME) 
			as IMyTextPanel; 
 
	Ref = GetRef(txt[0],';'); 
 
lsr[0] =  
		GridTerminalSystem.GetBlockWithName(Ref[0]) 
			as IMyLaserAntenna; 
 
	SendMessage(lsr,Ref[2],Ref[1]); 
 
}