//Transfer Message//

//For this Script to work we need : 
/*
a txt panel called info, orders & <RECEIVER_TYPE>_memory
	//memory of format : Ship ID, last known GPS, Status; etc
the Send M timer block initiates the Send Message MOD script
a remote control

*/

//WAY WAY to messy will have to clean up.
//note to self : use more methods, hard to read.

// Block names (editable)//
const string TXT0_NAME = "info" ;
//Info Should be of the form : Ship ID $ Name of Laser Antenna 1 % Name of Laser Antenna 2 % ... $ other Stuff
const string TXT1_NAME = "orders";
//orders is Assumed to be of the form : Sender ID; Receiver ID; Order Type; other info 0; other info 1 etc...
const string CNTRL_NAME = "cntrl";
const float lsr_radius = 20000;
//Taking small ship radius because of generality
//Note Control Radius is therefore also 20000
const float min_rad = 5000;
//minimum radius for construction outside of "main base"
//does not apply to all contruction
const string TIMER_NAME = "send_transfer";
const string TIMER0_NAME = "send_back";
const string TIMER1_NAME = "offer_demand";

//Just initialising a bunch of variables//
bool found;
string[] synapses;
int it;
int closest;


//Block Variables //
IMyRemoteControl cntrl;
IMyTextPanel txt0; 
IMyTextPanel txt1;
IMyTextPanel txt2;


//Functions//

public float Square(float f){

	float output;

	output = f * f;

	return output;
}

public float GetMagnitude(Vector3 vec){

	float SquaredMag;

	float output;

	SquaredMag = Square(vec.X) + Square(vec.Y) + Square(vec.Z); 

	output = (float) Math.Sqrt(SquaredMag);

	return output;
}
  
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


public Vector3 GPStoVector(string gps){

	string[] coords;

	//Getting Target Position//

	coords = gps.Split(':');

	//Pending, need to change indices

	Vector3 gpsvector = new Vector3(Convert.ToSingle(coords[1]), 
							 Convert.ToSingle(coords[2]), 
							 Convert.ToSingle(coords[3]));

	return gpsvector;
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




public void Main(){

	//Initialising Variables//
  
	IMyTextPanel txt0 = GridTerminalSystem.GetBlockWithName(TXT0_NAME) as IMyTextPanel;   

	IMyTextPanel txt1 = GridTerminalSystem.GetBlockWithName(TXT1_NAME) as IMyTextPanel;




	//Extracting Info//

	string[] txt0_ref;

	string U_ID;

	string[] U_ID_info;

	txt0_ref = GetRef(txt0,'$');

	U_ID = txt0_ref[0];

	U_ID_info = U_ID.Split('-');





	//Extracting orders//

	string[] txt1_ref;

	txt1_ref = GetRef(txt1,';');




	//Getting Receiver-ID Type//

	string R_ID;

	string[] R_ID_info;

	R_ID = txt1_ref[1];

	R_ID_info = R_ID.Split('-');





	//Getting Memory for R_ID type//

	string MEMORY_NAME;

	MEMORY_NAME = R_ID_info[0] + "_memory";

	IMyTextPanel txt2 = GridTerminalSystem.GetBlockWithName(MEMORY_NAME) as IMyTextPanel;




	//Extracting Memory//

	string[] txt2_ref;

	txt2_ref = GetRef(txt2,';');


	//memory of format : Ship ID, last known GPS, Status; etc




	//Check specificity of order//

	bool specific;

	specific = true;

	if(R_ID_info.Length == 1){

		specific = false;

	}





	//If specific transfer message//
	if (specific == true){

		//Check in Memory for given ID//

		 found = false;


		for(int i = 0 ; i < txt2_ref.Length; i++){

			synapses = txt2_ref[i].Split(',');

			if(synapses[0] == R_ID){

				found = true;

				break;

			}

		}


	//If non-specific transfer message//

	}else{

		//Check in memory for anything//

		 found = false;

		it = 0;

		synapses = txt2_ref[it].Split(',');

		if(synapses != null){
			found = true;
		}

	}







		
	repeat:

		//Check if Found//

		if(found == true){


			//Getting Target Vector//

			Vector3 target_vector = GPStoVector(synapses[1]);




			//Getting our position//

			IMyRemoteControl cntrl = GridTerminalSystem.GetBlockWithName(CNTRL_NAME) as IMyRemoteControl; 

			Vector3 pos_vector = cntrl.GetPosition();




			//Getting Distance//

			Vector3 pos_relat_vector;

			pos_relat_vector = target_vector - pos_vector;

			float distance;

			distance = GetMagnitude(pos_relat_vector);




			//Testing Distance//

			if(distance <= lsr_radius){

				//Generating Orders


				txt2.WritePublicText(synapses[1] + "&" + String.Join(";",txt1_ref));

				//Exectuting Send Message

				Execute(TIMER_NAME);
			}else{

				if(specific == true){

					//Getting Comms Memory//

					IMyTextPanel txt3 = GridTerminalSystem.GetBlockWithName("COM_memory") as IMyTextPanel;



					//Extracting Comms Memory//

					string[] txt3_ref;

					txt3_ref = GetRef(txt3,';');



					//Checking their distance//

					Vector3 previous_min_vector_0 = target_vector - pos_vector;


					string[] synapses_0;

					for(int i = 0; i < txt3_ref.Length; i++){

						//Getting info for each memory//



						synapses_0 = txt3_ref[i].Split(',');


						//Getting Position//

						Vector3 synapses_position_0;

						synapses_position_0 = GPStoVector(synapses[1]);


						//Getting distances//

						Vector3 synapses_position_relat_0;

						Vector3 synapses_position_to_target_0;

						//Vector from comms to current pos

						synapses_position_relat_0 = synapses_position_0 - pos_vector;

						//Vector from comms to target pos

						synapses_position_to_target_0 = synapses_position_0 - target_vector;



						//Checking distances//
						//Code here is quite simplified, only gets the closest available comms unit



						if(GetMagnitude(synapses_position_relat_0) <= lsr_radius && GetMagnitude(synapses_position_to_target_0) <= GetMagnitude(previous_min_vector_0)){

							previous_min_vector_0 = synapses_position_relat_0;

							closest = i;


						}

					}


					//Found closest comms unit//

					if(closest != null){

						//Generating Orders//

						synapses_0 = txt3_ref[closest].Split(',');

						txt2.WritePublicText(synapses_0[1] + '&' + String.Join(";",txt1_ref));

					}

				//If not specific//

				}else{

					it += 1;

					synapses = txt2_ref[it].Split(',');

					if(synapses != null){
						found = true;
					}

					goto repeat;

				}

		}

		//Above can probably be put into a module and optimised but will do that later



		}else{

			//This implies 1 of 2 things : 

			/*
			if specific
			We are dealing with something on the other side of the galaxy, of which we are not aware, in which case the sender should be able to inform us
			*/

			/*
			if not specific
			We are dealing with factories/drones which we have not yet built, so we need more of them clearly
			specifically, we are dealing with factories/drones of which we have NONE
			this should be dealt with by another script
			temporary solution : ask OTHER swarms to build required factories/drones for us ie : transfer a message to someone else
			however for the moment we just have to note this down
			*/

			//Asking for more info//

			if(specific = true){

				//Generating Orders

				txt2.WritePublicText(txt0_ref[0] + ";" + txt1_ref[0] + ";" + "GI" + ";" + "GPS" + ";" + txt1_ref[1]);

				//Executing Send Message

				Execute(TIMER0_NAME);

			//Need more of something//

			}else{
				//Welcome to hell//

				//Generating Demand

				txt2.WritePublicText(txt0_ref[0] + ";" + " " + ";" + "D" + txt1_ref[1]);

				//Executing Offer/Demand

				Execute(TIMER1_NAME);


			}
		}


	







}