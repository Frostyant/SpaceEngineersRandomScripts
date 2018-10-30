//New Orders//

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





//Main//

public void Main(){

	//Initialising Variables//
  
	IMyTextPanel txt0 = GridTerminalSystem.GetBlockWithName(TXT0_NAME) as IMyTextPanel;

	IMyTextPanel txt1 = GridTerminalSystem.GetBlockWithName(TXT1_NAME) as IMyTextPanel;

	IMyTextPanel txt2 = GridTerminalSystem.GetBlockWithName(TXT2_NAME) as IMyTextPanel;



	//Extracting Info//

	string[] txt0_ref;

	txt0_ref = GetRef(txt0,'$');

	string[] info = txt0_ref.Split('-');

	string Ship_ID;

	Ship_ID = info[0];

	.

	//Extracting Message//

	string[] txt2_ref;

	txt2_ref = GetRef(txt2,';');


	//in Command ?//
	//Checking if we are a command type, command types ALWAYS have C as a second letter & L as fourth

	bool incommand = false

	if(Ship_ID[4] != null){
		if(Ship_ID[1] == 'C' && Ship_ID[4] == 'L'){

			//Checking If ordered//
			if(txt2_ref[2] == "M"){
				break
			}

			if(txt2_ref[2] == "TM"){
				break
			}

			if(txt2_ref[2] = "S")



		}
	}





}