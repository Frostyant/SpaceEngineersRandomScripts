//IMPORTANT SE specific functions :

IMyRemotControl remote:

remote.GetFreeDestination (Direction,checkradius,shipradius)
// This is the additional radius off the ships radius for collision checks
const float checkRadius = 1f;
// This is the "ships radius" for the calculation
const float shipRadius = 1f;
//Direction in which to check
const Vector3 Direction

remote.AddWaypoint(Vector3)

Vector3 remote.GetNaturalGravity()



void GotTo(IMyRemoteControl cntrl, Vector3 Pos, bool Relative){
	
	Vector3 obj;

	if (Relative == true){
			obj = Pos + cntrl.GetPosition();
	}else{
		obj=Pos
	}

	cntrl.AddWaypoint(obj,"obj");
	cntrl.SetAutoPilotEnabled(true);
}

Vector3[] GetOrientation(IMyRemoteControl cntrl, IMyTerminalBlock Front, IMyTerminalBlock Top){

	Vector3 FrontVector;

	Vector3 NormalVector;

	Vector3[] output;



	FrontVector  = Front.GetPosition() - cntrl.GetPosition();

	NormalVector = Top.GetPosition() - cntrl.GetPosition();



	FrontVector.Normalize();

	NormalVector.Normalize();



	output=[FrontVector,NormalVector];
}



void init(out IMyTextPanel[] txt_array , string[] txt_name,
		 out IMyLaserAntenna[] lsr_array , string[] lsr_name,
		 out IMyRemoteControl[] cntrl_array , string[] cntrl_name,
		 out IMyLargeGatlingTurret[] turret_array, string[] turret_name,
		 out IMyTerminalBlock[] term_array,string[] term_name
		 out IMyTimerBlock[] tim_array,string[] tim_name
		 out IMyThrust[] thrust_array,string[] thrust_name){



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

}