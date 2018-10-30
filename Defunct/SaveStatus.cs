//SaveStatus//
//Saves a status "order" which was received

//Block names (editable) //
//const string TXT0_NAME = "info";
const string TXT1_NAME = "orders";
//ORDERS is Assumed to be of the form : Sender ID; Receiver ID; Order Type; other info 0; other info 1 etc... 
// "PS" = Ping Status, other info 0 = antenna position, other info 1 = status

//Functions//
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

public string[] GetMemory(string MEM_TYPE){

	//Initialising//

	IMyTextPanel txt0 = GridTerminalSystem.GetBlockWithName(MEM_TYPE + "_memory") as IMyTextPanel;



	//Extracting memory//

	string[] txt0_ref;

	//CHECK THIS LINE !//
	txt0_ref = GetRef(txt0,';');


	//Returning Value//

	return txt0_ref;
}

public int FindRelevantMemory(string MEM_TYPE, string RELEVANT_ID){
	//Extracts memory referring to given ID

	string[] synapses;
	//array of all the memories

	int output;

	output = -1; 

	synapses = GetMemory(MEM_TYPE);

	for(int it = 0; it <= synapses.Length; it++){
		//Breaking if in last loop
		if(it == synapses.Length){
			output = -1;
		}
		//checking if relevant
		if(synapses[it].Split(',')[0] == RELEVANT_ID){

			output = it;
			break;
		}
	}

	return output;
}

public string[] ChangeArray(string[] array,string value,int it0){
	string[] output;
	output = new string[array.Length];

	for(int it =0 ; it < array.Length; it++){
		if(it != it0){
			output[it] = array[it];
			
			
		}else{
			output[it] = value;
			
			
		}
	}

	return output;
}

public void AddToMemory(string MEM_TYPE,string Addition){
	//Exctrating ID//
	string[] Additions;
	Additions = Addition.Split(',');

	//Finding Relevant memory//
	int relevant;
	relevant = FindRelevantMemory(MEM_TYPE,Additions[0]);

	if(relevant == -1){
		SimpleAddToMemory(MEM_TYPE,Addition);
	}else{
		//Initialising//
		IMyTextPanel txt = GridTerminalSystem.GetBlockWithName(MEM_TYPE + "_memory") as IMyTextPanel;

		//Getting current memory//
		string[] memory = GetRef(txt,';');

		//editing memory//
		string[] newmemory = ChangeArray(memory,Addition,relevant);

		//Saving new memory//
		txt.WritePublicText(string.Join(";",newmemory));
	}


	

}

public void SimpleAddToMemory(string MEM_TYPE, string Addition){
	//Initialising//
	IMyTextPanel txt = GridTerminalSystem.GetBlockWithName(MEM_TYPE + "_memory") as IMyTextPanel;

	//Getting current memory//
	string memory = GetRaw(txt);

	//Setting up new memory//
	memory = memory + ";" + Addition;

	//Saving New Memory//
	txt.WritePublicText(memory);
}


public void Main(){
	//Initialisation//
	//IMyTextPanel txt0 = GridTerminalSystem.GetBlockWithName(TXT0_NAME) as IMyTextPanel;
	IMyTextPanel txt1 = GridTerminalSystem.GetBlockWithName(TXT1_NAME) as IMyTextPanel;

	//Extracting info//
	//string[] txt0_ref;
	//txt0_ref = GetRef(txt0,'$');

	//Extracting orders//
	string[] txt1_ref;
	txt1_ref = GetRef(txt1,';');

	//Getting ID-type of sender//
	string[] ID;
	ID = txt1_ref[0].Split('_');

	//Adding memory//
	AddToMemory(ID[0],txt1_ref[0] + "," + txt1_ref[3] + "," + txt1_ref[4]);
}