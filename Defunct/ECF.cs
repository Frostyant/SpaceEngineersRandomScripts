//Experimental Coordinator : Flagship

// Block names
const string TXT_NAME="info"
const string TXT1_NAME="temp"
const string TXT2_NAME="intel"
const string CNTRL_NAME="cntrl"

//Block Variables
IMyTextPanel[] txt;
IMyRemoteControl[] cntrl;

void init(out IMyTextPanel[] txt_array , string[] txt_name,
		 out IMyLaserAntenna[] lsr_array , string[] lsr_name,
		 out IMyRemoteControl[] cntrl_array , string[] cntrl_name,
		 out IMyLargeGatlingTurret[] turret_array, string[] turret_name,
		 out IMyTerminalBlock[] term_array,string[] term_namename)
{



		if(txt_array != null){
			n = txt_array.Length;
			for (i=0,i <= n-1, i++){

				txt_array[i] =
					GridTerminalSystem.GetBlockWithName(txt_name[i])
						 as IMyTextPanel;
			}
		}



				if(lsr_array != null){
			n = lsr_array.Length;
			for (i=0,i <= n-1, i++){

				lsr_array[i] =
					GridTerminalSystem.GetBlockWithName(lsr_name[i])
						 as IMyLaserAntenna;
			}
		}


						if(cntrl_array != null){
			n = cntrl_array.Length;
			for (i=0,i <= n-1, i++){

				cntrl_array[i] =
					GridTerminalSystem.GetBlockWithName(cntrl_name[i])
						 as IMyRemoteControl;
			}
		}

						if(turret_array != null){
			n = turret_array.Length;
			for (i=0,i <= n-1, i++){

				turret_array[i] =
					GridTerminalSystem.GetBlockWithName(turret_name[i])
						 as IMyLargeGatlingTurret;
			}
		}

						if(term_array != null){
			n = term_array.Length;
			for (i=0,i <= n-1, i++){

				term_array[i] =
					GridTerminalSystem.GetBlockWithName(term_name[i])
						 as IMyTerminalBlock;
			}
		}

						if(tim_array != null){
			n = tim_array.Length;
			for (i=0,i <= n-1, i++){

				tim_array[i] =
					GridTerminalSystem.GetBlockWithName(tim_name[i])
						 as IMyTimerBlock;
			}
		}

						if(thrust_array != null){
			n = thrust_array.Length;
			for (i=0,i <= n-1, i++){

				thrust_array[i] =
					GridTerminalSystem.GetBlockWithName(thrust_name[i])
						 as IMyThrust;
			}
		}

		 out IMyTimerBlock[] tim_array,string[] tim_name
		 out IMyThrust[] thrust_array,string[] thrust_
}

void save(string[] raw,string sep,IMyTextPanel txt){

	string Ref;
	int n;

	Ref = '';

	n = info.Length;



	for (i = 0 , i <= n-1, i++ ){

		Ref = Ref + raw[i];

}


string GetRaw(IMyTextPanel txt){

	string output;

	output=txt.GetPublicText();

	return output;
}



string[] GetRef(IMyTextPanel txt,String sep){

	string raw

	raw=GetRaw(txt);


	string [] output;

	string[] output =raw.Split(sep);

}

void save(string[] raw,string sep,IMyTextPanel txt){

	string Ref;
	int n;

	Ref = '';

	n = info.Length;



	for (i = 0 , i <= n-1, i++ ){

		Ref = Ref + raw[i];

}

/*
int[] AllToint(string[] input){

	int[] output;

	for (iterator = 0,
		 iterator <= input.Length-1,
		  iterator++){

		output=output + ToInt32(input[iterator])
	}
}
*/

float[] AllTofloat(string[] input){

	int[] output;

	for (iterator = 0,
		 iterator <= input.Length-1,
		  iterator++){

		output=output + [Convert.ToSingle(input[iterator])]
	}
}

Vector3 StringToVector(string strvector, string sep){

	string[] raw =strvector.Split(sep);

	Ref=AllTofloat(raw);

	output = new Vector3 (Ref[0],Ref[1],Ref[2])
}

Vector3 GetVelocity(IMyRemoteControl cntrl, string old_position){

	Vector3 pos0;
	Vector3 pos1;
	Vector3 output;

//Exctracting Values
	pos0=StringToVector(old_position);

	pos1=IMyRemoteControl.GetPosition();

//Calculating Speed Vector
	output=pos1-pos0;
}

void Main(string argument){

	Main(string argument){
	init(txt,[TXT_NAME,TXT1_NAME,TXT2_NAME]
		,null,null
		,cntrl,[CNTRL_NAME]
		,null,null
		,null,null
		,null,null
		,null,null);

	pos0=GetRef(txt[1],';');

	name=

	if (pos0 != 'null'){

		Velocity=GetVelocity(cntrl[0],pos0[0]);

	}

	save()
}
