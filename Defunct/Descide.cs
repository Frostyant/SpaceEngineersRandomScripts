//Descide//

// Block names (editable)  //

const string TXT0_NAME = "info";
//Info Should be of the form : Ship ID $ Name of Laser Antenna 1 % Name of Laser Antenna 2 % ... $ other Stuff
const string TXT1_NAME = "orders";
//This has to of the form : GPS coordinats ; message
const string TXT2_NAME = "message";

//Block Variables  //
IMyTextPanel txt0;  
IMyTextPanel txt1;
IMyTextPanel txt2;


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