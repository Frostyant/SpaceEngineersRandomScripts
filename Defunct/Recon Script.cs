//Recon/Patrol Detection Script


//initializing names
const String TXT_NAME = "emergencyinfo";
const String COMM_NAME = "Comm";
const String TURRET_NAME = "Turret";

//initializing block variables
IMyLargeTurretBase turret = void;
IMyLaserAntenna comm = void;


void Reset(ref IMyLargeTurretBase turret, ref IMyLaserAntenna comm, Strin COMM_NAME){
	turret.SetElevation = 0;
	turret.SetAzimuth = 0;
	comm.SetCustomName(COMM_NAME);
}

void Alarm(IMyTextPanel txt, IMyLaserAntenna comm, IMyLargeTurretBase turret){
	//setting up message : current coords
	posvector=comm.getPosition;
	X=float.ToString(posvector.X);
	Y=float.ToString(posvector.Y);
	Z=float.ToString(posvector.Z);
	pos=X+","+Y+","+Z;
	comm.SetCustomName(pos);

	//get coords of base that we have to warn
	objective=txt.GetPublicText();
	comm.SetTargetCoords(objective);
	comm.connect();

}

Main(){

	//Getting the blocks

	txt = GridTerminalSystem.GetBlockWithName(TXT_NAME) as IMyTextPanel;
	comm = GridTerminalSystem.GetBlockWithName(COMM_NAME) as IMyLaserAntenna;
	turret = GridTerminalSystem.GetBlockWithName(TURRET_NAME) as IMyLargeTurretBase;

	if (turret.getElevation != 0){
	Alarm(txt,comm,turret);
	Reset(turret,comm,COMM_NAME);

	}else if (turret.getAzimuth != 0){
		Alarm(txt,comm,turret);
		Reset(turret,comm,COMM_NAME);
		}






}