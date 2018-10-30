//Script for determinin planetary center in Space engineers 

//For this script to work you need to have have some altitude and to have  intertial dampeners on


//change the following to the names in this particular ship
//Name of where the base info for planets will go
const String TXT_NAME = "planetinfo";
//Name of temporary memory
const String TXTTEMP_NAME = "temp";
//Name of remote Control
const String CONTROL_NAME = "cntrl";
//Name of Timer
const String TIMER_NAME = "timer";

//Name of 2 thrusters which are set to maximum thrust override facing up and down respectively
const String THRUSTVERTICAL_NAME = "down";
const String THRUSTHORIZONTAL_NAME = "move";
const string FORMAT = "{0};{1},{2},{3};{4},{5},{6};{7},{8},{9};{10},{11},{12}";
//dont change beyond this point


//I mean seriously dont

//intializing blocks
IMyTextPanel txt = null;
IMyTextPanel temp = null;
IMyRemoteControl center = null;
IMyThrust down = null;
IMyThrust move = null;
IMyTimerBlock Timer= null;

//keeping this in a functions for other programs finds intersection of 2 lines
Vector3D Intersection(Vector3D a,Vector3D b,Vector3D v1,Vector3D v2){
	Vector3D vcross1= Vector3D.CrossProduct(v1,v2);
	Vector3D vcross2= Vector3D.CrossProduct(b,v2)- Vector3D.CrossProduct(a,v2);
	double k = vcross2.x/vcross1.x;
	if (a+k*v1==b+k*v2){
	output=a+k*v1;
	}else{
	output=null;
	}
	return output;
}

//again keeping this for later finds the normal vector to 2 other vectors


Vector3 CrossProduct(Vector3 a, Vector3 b){
	Vector3 sol = (a.Y*b.Z-b.Y*a.Z,a.X*b.Z-b.X*a.Z,a.X*b.Y-b.X*a.Y);
	return sol
}

Vector3 GetNormal (Vector3 a, Vector3 b) {
	Vector3 vcross=CrossProduct(v1,v2);
	Vector3 output = vcross.Normalize();
	return output;
}


void Main()
{
// getting the blocks
	txt = GridTerminalSystem.GetBlockWithName(TXT_NAME) as IMyTextPanel;
	temp = GridTerminalSystem.GetBlockWithName(TXT_NAME) as IMyTextPanel;
	cntrl = GridTerminalSystem.GetBlockWithName(CONTROL_NAME) as IMyRemoteControl;
	timer = GridTerminalSystem.GetBlockWithName(TIMER_NAME) as IMyTimerBlock;
	down = GridTerminalSystem.GetBlockWithName(THRUSTVERTICAL_NAME) as IMyThrust;
	move = GridTerminalSystem.GetBlockWithName(THRUSTHORIZONTAL_NAME) as IMyThrust;

//immobilizing ourselves
	down.GetActionWithName("OnOff_Off").Apply(down);
	move.GetActionWithName("OnOff_Off").Apply(move);

//getting memory
	raw=temp.GetPublicText();
    string[] res = raw.Split(';');
    state= Int32.Parse(res [0]);
    //using the first of string in temp to determine the current stage of the measurments
    if (state >= 1){
    	pos1=Vector3D.Parse(res [1]);
    	if (state >= 2){
	    	pos2=Vector3D.Parse(res [2]);
	    	if (state>=3){
	    		pos3=Vector3D.Parse(res[3]);
	    		if (state>=4){
	    			pos4=Vector3D.Parse(res[4]);
	    			//time for some glorious cross product (to find the center of planet)
	    			Center=Intersection(pos1,pos3,pos1-pos2,pos3-pos4);
	    			n=GetNormal(pos1,pos3);
	    			//record final values
	    			temp.WritePublicText(String.Format('{0},{1},{2};{4},{5},{6};{7},{8},{9}',center.X,center.Y,center.Z,pos1.X,pos1.Y,pos1.Z,n.X,n.Y,n.Z));
	    		
	    			// ok all this are to record info and startup the next cycle
	    			tempstate=5
	    		}else{pos4=cntrl.GetPosition();
	    			tempstate=4;
	    			cntrl.GetActionWithName("DampenersOverride").Apply(cntrl);
	    			}
    		}else{pos3=cntrl.GetPosition();
    			tempstate=3;
    			cntrl.GetActionWithName("DampenersOverride").Apply(cntrl);
    			move.GetActionWithName("OnOff_Off").Apply(move);
    			}
    	}else{pos2=cntrl.GetPosition();
    		tempstate=2;
    		cntrl.GetActionWithName("DampenersOverride").Apply(cntrl);
    		move.GetActionWithName("OnOff_On").Apply(move);
    		}

    }else{pos1=cntrl.GetPosition();
    	tempstate=1;
    	cntrl.GetActionWithName("DampenersOverride").Apply(cntrl);

    	}

    	// record current temporary data
    temp.WritePublicText(String.Format(FORMAT,tempstate,pos1.X,pos1.Y,pos1.Z,pos2.X,pos2.Y,pos2.Z,pos3.X,pos3.Y,pos3.Z,pos4.X,pos4.Y,pos4.Z));

    //prep next measurment
    if (tempstate==5){
    stop=timer.GetActionWithName("OnOff_Off");
    stop.apply(timer);
    }

}