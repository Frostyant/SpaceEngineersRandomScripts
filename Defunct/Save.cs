//Save Info

// Block names (editable)//
const string TXT0_NAME = "message" ;
//"S" = S, Saves info for future referencs (usefull for networks), other info 0 = Type of info (& location to store), other info 1 = info, other info 2 = *nothin*/0(comms type info or not)




//Block Variables //
IMyTextPanel txt0;



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



//MAIN//

public void Main(){

	//Initialising Variables//
  
	IMyTextPanel txt0 = GridTerminalSystem.GetBlockWithName(TXT0_NAME) as IMyTextPanel;



	//Extracting message//

	string[] txt0_ref;

	txt0_ref = GetRef(txt0,';');


	//Initialising Memory//

	IMyTextPanel txt1 = GridTerminalSystem.GetBlockWithName(txt0_ref[3]) as IMyTextPanel;

	if(txt1 != null){


		//Extracting Current Memory//

		string[] txt1_ref;

		txt1_ref = GetRef(txt1,';');



		//Checking if Ship ID Specific info//

		bool IDSpecific;

		if (txt0_ref[5] != null){
			IDSpecific = true;
		}else{
			IDSpecific = false;
		}



		//Checking for Duplicates//

		bool IsDuplicate;

		IsDuplicate = false;

		foreach(string Synapse in txt1_ref){

			if(Synapse == txt0_ref[4]){
				
				IsDuplicate = true;
			}

		}




	if(IsDuplicate = false){
		if(IDSpecific = false){
			

				//Generating New Memory//

				string Memory;

				Memory = txt1_ref + ";" + txt0_ref[4];



				//Saving New Memory//

				txt1.WritePublicText(Memory);




			


		}else{
			//Extract ID//

			string[] txt0_ref_4_array;

			txt0_ref_4_array = txt0_ref[4].Split(',');



			//Check for Other Elements with Same ID//

			for(int i =1, i < txt1_ref.GetLength(),i++){

				Synapse = txt1_ref[i]

				string[] Synapse_memory;

				Synapse_memory = Synapse.Split(',');

				if(Synapse_memory[0] == txt0_ref_4_array[0]){

					//If we found one, replace it with the new info//

					Synapse = txt0_ref[4];

					IsDuplicate = true;

					break;
				}
			}

			if(IsDuplicate = false){
				
				//Generating New Memory//

				string Memory;

				Memory = txt1_ref + ";" + txt0_ref[4];



				//Saving New Memory//

				txt1.WritePublicText(Memory);

			}
		}
	}


	}


}