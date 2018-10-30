//Get Offer Refinery//





//Block names (editable) //
//const string TXT0_NAME = "info";
const string TXT1_NAME = "orders";
//text panel 1 is orders in this format : Assembler ID; Refinery ID ;GO; Ressource ID 0, Ressource ID 1,...; Amount of ID 0, Amount of ID 1,...
const string TXT2_NAME = "ID_to_ressource";
//text panel 2 is RessourcesToID in this format : Name of ID 0; Name of ID 1;etc
//const string TXT3_NAME = "current_work"; OUTDATED, Removed because unnecessary
//text panel 3 in this format : Refinery ID ; Assembler ID;RO; Ressource ID 0, Ressource ID 1,...; Remainder of ID 0, Remainder of ID 1,...
const string TXT4_NAME = "work_queue";
//text panel 4 is a collection of infos in format from TXT3 separated by a "#"
/*const*/ string[] CARGO_INPUT_NAMES = {"input"};
/*const*/ string[] CARGO_OUTPUT_NAMES = {"output"};
//format is : Ressource name of ID0; Ressource name of ID1;etc...


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


public void Execute(string TIMER_NAME){

	IMyTimerBlock timer = GridTerminalSystem.GetBlockWithName(TIMER_NAME) as IMyTimerBlock;

	if(timer != null){

		//False find what is the variable type of the actions.

		ITerminalAction start;

		start=timer.GetActionWithName("Start");

		start.Apply(timer);
	}
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

public string[] ExctractFirstSimpleMemory(string MEM_TYPE){
	//Exctracts first element in memory of format info 00,info 01;etc

	string[] synapses;
	//array of all the memories

	string[] output;

	synapses = GetMemory(MEM_TYPE);

	output = synapses[0].Split(',');

	return output;
}

public string[] ExctractRelevantMemory(string MEM_TYPE, string RELEVANT_ID){
	//Extracts memory referring to given ID

	string[] synapses;
	//array of all the memories

	string[] output = {""};

	synapses = GetMemory(MEM_TYPE);

	for(int it = 0; it < synapses.Length; it++){
		//checking if relevant
		if(synapses[it].Split(',')[0] == RELEVANT_ID){

			output = synapses[it].Split(',');

		}
	}

	return output;
}

public int CheckForItemInCargo(IMyInventory inventory, string ITEM_NAME){
	var items = inventory.GetItems();
	int AmountinCargo = 0;

	for(int it = items.Count - 1; it >= 0; it--){
		if(items[it].Content.SubtypeName == ITEM_NAME){
			AmountinCargo += items.Amount;
		}
	}

	return AmountinCargo;
}




public IMyInventory AccessInventory(string CARGO_NAME,int inventory_type){
	//inventory type should be 0 for ALL blocks EXCEPT assemblers & Refineries
	//they have 2 inventories, input (0) & output(1)
	//returns the inventory variable
	IMyTerminalBlock cargo_container = GridTerminalSystem.GetBlockWithName(CARGO_NAME) as IMyTerminalBlock;

	var cargo = cargo_container as IMyInventoryOwner;

	IMyInventory inventory = cargo.GetInventory(inventory_type) as IMyInventory;

	return inventory;

}




public string GetMoveInventory(string amount_string, string ITEM_NAME, string[] CARGO_INPUT_NAMES,string[] CARGO_OUTPUT_NAMES){
	//This method transfers "amount" items of type ITEM_NAME from CARGO_INPUT_NAMES to CARGO_OUTPUT_NAMES
	//note this works with Cargo Containers only

	int amount;
	int amount_step;
	IMyInventory inventory0;
	//will change depending on wether or not SE accepts floats or ints for (ints I believe)

	amount = ConvertToInt32(amount_string);

	for(int it0 = 0; it0 < CARGO_INPUT_NAMES.Length; it0++){
		if(amount <= 0){
			break;
		}

		//Accessing input inventory
		inventory0 = AccessInventory(CARGO_INPUT_NAMES[it0],0);


		//Checking for Item in the cargo
		amount_step = CheckForItemInCargo(inventory0,ITEM_NAME);

		amount -= amount_step;

		//Checking where we have space for the output iif we can move stuff from the first one
		if(amount_step > 0){
			for(int it1 = 0; it1 < CARGO_OUTPUT_NAMES.Length ; it1++){
				IMyTerminalBlock cargo1_container = GridTerminalSystem.GetBlockWithName(CARGO_OUTPUT_NAMES[it1]) as IMyTerminalBlock;

				//Accesing output inventory
				var inventory1 = AccessInventory(CARGO_OUTPUT_NAMES[it1],0);

				//Checking if we have enough space in the target cargo
				if ((double)inventory1.MaxVolume - (double)inventory1.CurrentVolume > amount_step){
					//Getting Access to Items//

					var items = inventory0.GetItems();

					//Transfer Items//

					for(int it2 = items.Count - 1; it2 >= 0; it2--){
						if (containerItems[it2].Content.SubtypeName == ITEM_NAME) 
							{ 
							continue; 
							} 
						inventory1.TransferItemFrom(inventory0,it,null,true,amount_step);
					}

				}
			}
		}


	}

	return amount.ToString();
}



public string[] GetOffer(IMyTextPanel txt1,string[] CARGO_INPUT_NAMES,string[] CARGO_OUTPUT_NAMES){

	IMyTextPanel txt2 = GridTerminalSystem.GetBlockWithName(TXT2_NAME) as IMyTextPanel;

	string[] txt1_ref;

	string[] ressources_ID;

	string[] ressources_amount;

	string[] Remainder;

	Remainder = new string [Math.Min(ressources_ID.Length,ressources_amount.Length)];



	//text panel 1 is orders in this format : Assembler ID; Refinery ID ;GO; Ressource ID 0, Ressource ID 1,...; Amount of ID 0, Amount of ID 1,...

	txt1_ref = GetRef(txt1,';');

	//text panel 2 is RessourcesToID in this format : Name of ID 0; Name of ID 1;etc

	txt2_ref = GetRef(txt2,';');

	//Extracting ressource information
	ressources_ID = txt_ref.Split(',');

	ressources_amount = txt_ref.Split(',');

	for(int it = 0; it < ressources_ID.Length && it < ressources_amount.Length; it++){
		Remainder[it] = GetMoveInventory(
			ressources_amount[it],
			txt2_ref[ConvertToInt32(ressources_ID)],
			CARGO_INPUT_NAMES,
			CARGO_OUTPUT_NAMES
			);
	}

	return Remainder;
}

public void AddToWorkQueue(IMyTextPanel txt1, IMyTextPanel txt2){
	//txt1 is orders
	//txt2 is the queue
	//saves orders and adds it to queue

	txt2.WritePublicText(GetRaw(txt2) + "#" + GetRaw(txt1));

}









public void Main(string FromQueue){
	//Initialising//

	IMyTextPanel txt1 = GridTerminalSystem.GetBlockWithName(TXT1_NAME) as IMyTextPanel;
	IMyTextPanel txt4 = GridTerminalSystem.GetBlockWithName(TXT4_NAME) as IMyTextPanel;

	//Checking Queue, txt3 = current work//
	string txt4_ref = GetRef(txt4,'#');

	if(FromQueue == "false"){
		//Saving in queue//
		AddToWorkQueue(IMyTextPanel txt1, IMyTextPanel txt4)
	}

	if(txt4_ref == null && FromQueue == "false"){

		//Too tired for comments
		//Get Remainder & performing offer//

		string[] Remainder;

		Remainder = GetOffer(txt1,CARGO_INPUT_NAMES,CARGO_OUTPUT_NAMES);

		//CHANGE//
		//Already did this in method but will optimise later
		string[] txt1_ref;

		txt1_ref = GetRef(txt1);

		//Getting Memory//
		string[] memory;

		memory = ExctractRelevantMemory("MAN",txt1_ref[0]);

		//Setting up return offer message//
		//return offer is in this format : Refinery ID ; Assembler ID;RO; Ressource ID 0, Ressource ID 1,...; Remainder of ID 0, Remainder of ID 1,...
		string orders;

		orders = memory[1] +"&"+ txt1_ref[1] + ";" + txt1_ref[0] + ";" + "RO" + ";" + txt1_ref[3] + ";" + string.Join(Remainder,",");


		//Saving new Orders//

		txt1.WritePublicText(orders);


		//Returning Offer//
		//will have to fix this to trigger now instead of just executing it after 1 sec
		Execute("SM");
	}else{

		
		
	}

}