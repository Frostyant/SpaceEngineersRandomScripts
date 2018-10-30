/*====================================================================================     
                                                                               HI THERE!    
    
Welcome! and thank you for downloading my Fleet Command Script!     
   
                                                                 [ASSAULTCRAFT MODULE]   
     
About The Script:     
----------------------------    
    The concept for this project was to give players the ability to control a fleet of ships, effortlessly and reliably     
    in vanilla.     
     
    However this is not merely a drone following script, this script will give a sequence of AI to ships that will     
    enable a player to fully and completely control the actions and commands of their fleet, control formation,     
    tactics, squadron manoeuvres, and automated docking, all with an easy to use LCD menu.     
     
    Self-AI will control fighter-class ships to roll and dogfight enemy ships in a 3D area, frigates will stay at a     
    medium range, using their superior agility to avoid heavy weapons fire from enemy cruisers, while larger     
    cruiser class ships will stay at distance, using their longer range weapons for maximum effect against     
    larger enemy targets, turning to bring their best weapons to bear.    
    
    You are currently looking at the 'AssaultCraft' module used on small Assaultcraft.    
    It requires:    
    ~P-block (self) named 'AssaultCraft'  
    ~Correctly oriented gyros (up pointing forward)   
    ~A Remote control unit facing forward   
    ~A connector with its behind facing the bottom of the Rc unit   
    ~Any sensors set to detect enemy targets    
   
    Make sure your ship can stop in UNDER 1500m!!   
         
    The script as is is in a very WIP state, this is an alpha release simply allowing people to get to grips with the    
    concept, a lot of the things here are highly experimental. If you have any crippling bug reports and issues     
    please report on the workshop page so I can get them fixed soon as possible, along with any requests!      
    
Have fun!     
Also I highly advise using this in a damage-disabled world to get to grips with it, if you set things up badly     
collisions can be somewhat.. destructive,     
      
======================================================================================== */    
         
//ASSAULTCRAFT AI    
/*                  
Latest Changenotes               
-------------------------------------------------------------------------------------     
~Added displaynametext for colecting connectors, so shouldn't collect ejectors 
~Adjusted docking precision (0.4m now) and set lower precise thrust  
~Adjusted collect functions to collect large thrusters too  
~Fixed error with Leader tag    
~Added some more error handling (index out of bounds)    
~Refined Stopping for SQ-leader (eg 20m sphere)    
~Refined Leader toggle and refined large scale formation flying, (decreased turning speed)    
~Refined docking procedure (0.7m dock)    
~Prototyped Roll damping on docking    
~Prototyped Precision Pilot    
~Upped ship-Max velocity to 70 m/s         
~Automatically turned off thrusters on docking         
~Changed collision avoidance for dogfight scenarios     
*/         
    
    
/*                
Large Functions and their performance             
-----------------------------------------------------             
~FighterControl - slightly improved, [still a lot of unecessary docking procedures]             
~logblocks - for permanent aquisition of thrust,gyros,cons,RC  [lots of procedures, needs a high degree of cutting]            
~Travelingautopilot - for going to places   [fairly good, GFD still needs some work though]            
~DockPilot - to provide specialist docking procedures [unknown]        
*/              
    
//Pre Setup Programme     
//-------------------------------         
public Program()       
{    
Connectorfound = false; 
Main("");     
}    
     
            
//Constants /And Ship Variables            
//==========================================================================            
              
//Permanently Logged Items              
//---------------------------------------------------              
//PrimaryComponents               
List<IMyTerminalBlock> GYROS = new List<IMyTerminalBlock>();        //ShipGyros               
IMyShipConnector CONNECTOR;                                                                //ShipConnector               
IMyRemoteControl RC_UNIT;                                                                       //ShipRCUnit               
bool runlogblocks = false;                                                                          //firsttimesetup initialiser         
bool Connectorfound = false;              //boolean statement to decide whether or not to attempt to find connector          
               
//ThrustBlocks               
List<IMyTerminalBlock> THRUST = new List<IMyTerminalBlock>();               
List<IMyTerminalBlock> FORWARD_THRUST = new List<IMyTerminalBlock>();               
List<IMyTerminalBlock> UP_THRUST = new List<IMyTerminalBlock>();               
List<IMyTerminalBlock> DOWN_THRUST = new List<IMyTerminalBlock>();                          
              
//Orientation Variables               
Vector3D SHIPFORWARD;                
Vector3D SHIPBACK;                
Vector3D SHIPUP;                
Vector3D SHIPDOWN;                
Vector3D SHIPLEFT;                
Vector3D SHIPRIGHT;               
Vector3D SHIPLAUNCH;               
              
              
//Dynamic Variables               
//----------------------------------------              
bool SETBACK = false;                   //For Fight Logic                 
bool AVOID_POS_GEN = false;     //For Fight Logic                 
Vector3D AVOID_POS;                  //For Fight Logic                 
bool COORDSHASRUN;          //For Docking Procedure                   
Vector3D P_COORDS;            //previous position of target              
bool SQLEAD;                        //whether or not the ship is squadron leader           
double LAUNCH_DISTANCE = 0;           
bool DOCKINOPERATION = true;      
              
//Travelingautopilot specific               
Vector3D PREVPOSITION;          //previous position of ship                
Vector3D PREVCOORDS;          //previous position of target                   
              
//User Inputs:              
//--------------------------------                 
double BRAKING_DISTANCE = 1400;             //Distance required for a ship to reach full stop from 100m/s                 
float SHIPRADIUS = 5;                                    //Largest diameter of ship                 
double MAX_VELOCITY = 70;                           //Formation cruising velocity, (less than 90 m/s)                
              
              
              
/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================                               
  High Level Function: FighterControl                              
  ------------------------------------------------                              
  function will: Give Fighter Drones Rudimentary AI and combat capabilities                             
  Inputs: Named self, inputs command data                          
  Requires: Quite a few things        */                  
//=======================================================================================                  
void Main(string argument)                      
{                           
//Initial Constant and Ship Setup                           
//==================================                      
                      
           //First Time Setup, Run Logblocks + FIND Connector              
           //---------------------------------------------------------------------         
     
          if(Connectorfound == false || CONNECTOR.IsWorking == false)     
          {      
          //Finding Onboard Docking Connector                        
          List<IMyTerminalBlock> CONNECTORS = new List<IMyTerminalBlock>();                            
          GridTerminalSystem.GetBlocksOfType<IMyShipConnector> (CONNECTORS);       
          for(int j = 0; j < CONNECTORS.Count;j++)        
          {if(CONNECTORS[j].CubeGrid == Me.CubeGrid && CONNECTORS[j].DisplayNameText.Contains("Ejector") == false )                             
          {CONNECTOR = CONNECTORS[j] as IMyShipConnector;}     
          Connectorfound = true;       
          }                                                
          }     
       
          if(CONNECTOR == null) 
          {return; Echo( "no connector");} 
 
           if(runlogblocks == false && CONNECTOR.IsConnected == false)              
           {Logblocks(); runlogblocks = true;}              
              
            //Setting Up Constants                        
            //-----------------------------------------------------                      
            String COMMAND = "";                        
            Vector3D COORDINATES = new Vector3D(0,0,0);                        
            Vector3D HEADING = new Vector3D(0,0,0);                    
          
    
          //Mask to Prevent Programme From Spinning Out Gyros    
          //--------------------------------------------------------------------------     
           if(GYROS.Count > 0 && argument == "")                
            {for(int j = 0; j < GYROS.Count;j++)                                                                   
                {                                       
                var GYROA = GYROS[j] as IMyGyro;        
                GYROA.SetValue<float>("Yaw", (0));                                  
                GYROA.SetValue<float>("Roll", (0));                                             
                GYROA.SetValue<float>("Pitch", (0));                                             
                GYROA.SetValue("Override", true);                          
                }              
            }        
    
         //Mask to Prevent Programme From Thrusting Weirdly     
         //------------------------------------------------------------------------            
          for(int j = 0; j < THRUST.Count;j++)                                                                    
            {                  
            THRUST[j].SetValue<float>("Override", 0.0f);           
            THRUST[j].ApplyAction("OnOff_On");                  
            }           
                     
            //Extracting Command Data From Name                      
            //-----------------------------------------------------                      
                String[] STORED_DATA = (argument).Split('#');                       
                if(STORED_DATA.Length >= 4)                       
                    {                       
                    COMMAND = STORED_DATA[0];                       
                    Vector3D.TryParse(STORED_DATA[1], out COORDINATES);                       
                    Vector3D.TryParse(STORED_DATA[2], out  HEADING);       
                    double.TryParse(STORED_DATA[3], out  LAUNCH_DISTANCE);             
    
                    //Squadron Leader Test                  
                    SQLEAD = false;                  
                    if(STORED_DATA.Length >= 5)    
                         {if(STORED_DATA[4] == "LEADER")                  
                            {SQLEAD = true;}     
                    }                
                    }                      
      
              
                 
            //Test If In a Docking Scenario And Silence If So                   
            //----------------------------------------------------------------                   
            if(CONNECTOR.IsConnected && COMMAND == "Dock")       //Alot of unecessary work, possibly needs slimming down a bit                   
            {                           
            for(int j = 0; j < GYROS.Count;j++)                   
                {IMyGyro GYRO = GYROS[j] as IMyGyro; GYRO.SetValue("Override", false);}                                                
            for(int j = 0; j < THRUST.Count;j++)                    
                {THRUST[j].SetValue<float>("Override", 0.0f); THRUST[j].ApplyAction("OnOff_Off");}                   
            return;                   
            }                   
                  
            //Calculating Target Velocity And Distance To Target                      
            //---------------------------------------------------------------------                      
                double T_VELOCITY = ((COORDINATES-P_COORDS).Length())/(double)(ElapsedTime.TotalSeconds);                      
                double D_T_TARGET = Math.Abs(((COORDINATES - Me.GetPosition()).Length()));                             
                               
               
//Using Command Decide On What Action To Take                         
//=========================================                      
               
//------------+----------------------+------------------+---------------+------------------+---------------+------------------+---------------+                            
            //'FOLLOW' Command, Run autopilot with coordinates, heading and RC unit, HEADING                      
            //--------------------------------------------------------------------------------------------------------------                      
                if(COMMAND == "GoTo" )                      
                    {                  
                        if(DOCKINOPERATION == true)                   //check to make sure doesn't override main ship                   
                        {         
                        CONNECTOR.ApplyAction("Unlock");                
                        Dockpilot(COORDINATES, HEADING, HEADING,"Launch");                          
                        return;                   
                        }                  
                  
                        SHIPRADIUS = 5;                                //Standard Assaultcraft Collision Avoidance    
    
                    RC_UNIT.SetAutoPilotEnabled( 	false );                  
                    CONNECTOR.ApplyAction("Unlock");                  
                    COORDSHASRUN = false;                  
                      
                    Travellingautopilot(COORDINATES, RC_UNIT, HEADING);                  
                    P_COORDS = COORDINATES;                      
                    return;                      
                    }                      
//------------+----------------------+------------------+---------------+------------------+---------------+------------------+---------------+                         
            //'FIGHT' Command, Run autopilot with attack criteria and fire weapons                       
            //--------------------------------------------------------------------------------------------------------------                      
           if(COMMAND == "Attack" )                       
                    {                  
                        if(DOCKINOPERATION == true)                   //check to make sure doesn't override main ship                    
                        {          
                        CONNECTOR.ApplyAction("Unlock");                 
                        Dockpilot(COORDINATES, HEADING, HEADING,"Launch");                           
                        return;                    
                        }             
              
                        SHIPRADIUS = 20;                                     //Dogfight Collision Avoidance    
    
                        RC_UNIT.SetAutoPilotEnabled( 	false );                   
                        CONNECTOR.ApplyAction("Unlock");                  
                        COORDSHASRUN = false;                  
             
                    //Setting Up Boolean Setback AI                      
                    //-------------------------------------------                      
                    if(D_T_TARGET < 200)                      
                    {                     
                    SETBACK = true;                     
                    if(AVOID_POS_GEN == false)                     
                        {                     
                        var PROG_POS = Me.GetPosition();                       
                        var FORWARDPOS3 = Me.Position + Base6Directions.     GetIntVector(Me.Orientation.TransformDirection(Base6Directions.Direction.Forward));       var FORWARD3 = Me.CubeGrid.GridIntegerToWorld(FORWARDPOS3);                                          
                        var FORWARDVECTOR3A = Vector3D.Normalize(FORWARD3 - PROG_POS);                       
                        AVOID_POS = (PROG_POS + FORWARDVECTOR3A*10000);                    
                        AVOID_POS_GEN = true;                     
                        }                      
                    }                      
                      
                    if(D_T_TARGET > (300-(4*T_VELOCITY)))         //Cancel setback when...              
                    {                     
                    SETBACK = false;                     
                    AVOID_POS_GEN = false;                     
                    }                      
             
                    //Fighter Fly-to CNS                      
                    //-----------------------------                      
                        if(SETBACK == false)                      
                            {                      
                            //Point-To Function For Generating Targetposition                      
                            //-----------------------------------------------------------------                      
                            double BULLET_TIME_TO_TARGET = (D_T_TARGET/300);                      
                            Vector3D TARGET_DIRECTION_VECTOR = (COORDINATES-P_COORDS)/(double)(ElapsedTime.TotalSeconds);                      
                            Vector3D AIM_POINT = TARGET_DIRECTION_VECTOR*BULLET_TIME_TO_TARGET+COORDINATES;                      
                            Travellingautopilot(AIM_POINT, RC_UNIT, HEADING);                  
                            }                      
                        else                      
                            {                      
                            Travellingautopilot(AVOID_POS, RC_UNIT, HEADING);                      
                            }                      
                                      
               //Gun Firing In Required Scenarios                    
               //---------------------------------------------                    
               List<IMyTerminalBlock> LAUNCHERS =  new List<IMyTerminalBlock>();                    
               if(Math.Abs(PREV_MISSILEELEVATION) < 0.02 && Math.Abs(PREV_MISSILEAZIMUTH) <0.02 && D_T_TARGET < 800 && SETBACK == false)                      
                    GridTerminalSystem.GetBlocksOfType<IMyUserControllableGun> (LAUNCHERS);                        
                        for( int i=0; i<LAUNCHERS.Count; ++i )                      
                        {                      
                        LAUNCHERS[ i ].ApplyAction( "ShootOnce" );                      
                        }                   
                    P_COORDS = COORDINATES;                        
                    return;                      
                    }                      
              
//------------+----------------------+------------------+---------------+------------------+---------------+------------------+---------------+                          
            //'Dock' Command, Run autopilot with coordinates, heading and RC unit, HEADING                       
            //--------------------------------------------------------------------------------------------------------------                      
            if(COMMAND == "Dock" )                        
                    {              
    
                    SHIPRADIUS = 10;                                     //Docking Collision Avoidance    
        
                    //If distance from first point is large use standard autopilot            
                    //--------------------------------------------------------------------------------            
                    Vector3D READYPOS;            
                    Vector3D.TryParse(STORED_DATA[3], out READYPOS);            
                    if(((READYPOS-(CONNECTOR.GetPosition())).Length()) > 40 && DOCKINOPERATION == false)            
                        {           
                        Travellingautopilot(READYPOS, RC_UNIT, HEADING);            
                        }            
                    else                  
                        {              
                        DOCKINOPERATION = true;             
                        Dockpilot(COORDINATES, HEADING, READYPOS,"Dock");           
                        }           
                    CONNECTOR.ApplyAction("Lock");                     
                      
                    P_COORDS = COORDINATES;                  
                    return;                        
                    }                               
}                      
//==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#==                  
                   
              
/*====================================Dockpilot=======================================================                                
  High Level Function: Dockpilot                               
  -------------------------------------------------------                               
  function will: Manage docking and undocking of ships                                
  Inputs: RC_UNIT, two points to form a launching/landing vector, heading, launching/landing, landing type (timer)               
  Requires: Gyroturn2,                                
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/                      
string CURRENT_STAGE;                     
Vector3D ALIGNMENTVECTOR;               //A vector used by thruster collect function                          
double PREV_MISSILEROLL;                    //For Gyro roll damping    
void Dockpilot(Vector3D CONPOS, Vector3D HEADING, Vector3D READYPOS, string OPERATION  )                                  
{            
         
        //Final Desetter         
        //--------------------------         
        if(LAUNCH_DISTANCE > 60)         
        {DOCKINOPERATION = false;}         
         
       //Velocity Setup                                
       //================================================================                               
            //Linear Velocity                                
            //-----------------------------------------------------------------                                
            double VELOCITY =((Me.GetPosition()-PREVPOSITION).Length())/(double)(ElapsedTime.TotalSeconds);                                 
            PREVPOSITION = Me.GetPosition();                               
                           
            //Target Velocity For Matching Velocities                                
            //------------------------------------------------------                              
            double TARGETVELOCITY =((CONPOS-PREVCOORDS).Length())/(double)(ElapsedTime.TotalSeconds);                           
            PREVCOORDS = CONPOS;                           
                           
            //Generating An Infinity Targetpostion For Heading Matching                           
            //--------------------------------------------------------------------------------                                   
            HEADING = (HEADING*9999999999 + CONPOS);                           
                           
           //Calculating Stopping Distance                            
           //--------------------------------------------------------------------------------                            
               double DECEL = 5000/BRAKING_DISTANCE;                           
               double STOPPING_DIST = (((VELOCITY*VELOCITY)-(TARGETVELOCITY*TARGETVELOCITY)))/(2*DECEL);             
          
        
//Docking Procedure                                  
//================================================================            
 if(OPERATION == "Dock")         
     {         
      //Docking           
      //at dock start it will be within 40m of first target position,            
     //aim at connector with gyros, align with vector with thrusters and if angle is small, gently thrust towards conn,            
            
     //Turning Gyros To Match heading           
     //-------------------------------------------------           
      IMyGyro GYRO = GYROS[0] as IMyGyro;                             
      GyroTurn(HEADING, GYRO, 20 );                     
      for(int j = 0; j < GYROS.Count;j++)                                                                 
            {                                     
            var GYROA = GYROS[j] as IMyGyro;                                  
            GYROA.SetValue<float>("Roll", (AZOVER));                                           
            GYROA.SetValue<float>("Pitch", (ELOVER));                                           
            GYROA.SetValue("Override", true);                        
            }                     
      
     //Turning Gyros To Match Roll (with damping)    
     //-------------------------------------------------------------                             
      float ROLLANGLE = (float)(20*(Vector3D.Dot(Vector3D.Normalize(CONPOS-READYPOS), Vector3D.Normalize(ExtractDirection("RIGHT", RC_UNIT)))));           
      float YAW_VEL = (float)(((ROLLANGLE - PREV_MISSILEROLL)/(double)(ElapsedTime.TotalSeconds)));      
      PREV_MISSILEROLL = ROLLANGLE;         
      ROLLANGLE = ROLLANGLE + YAW_VEL;    
      for(int j = 0; j < GYROS.Count;j++)                                                                  
            {                                      
            var GYROA = GYROS[j] as IMyGyro;                                   
            GYROA.SetValue<float>("Yaw", (ROLLANGLE));                                                                  
            GYROA.SetValue("Override", true);                         
            }           
           
     //Retrieving A Vector That Will Point to on targetvector           
     //----------------------------------------------------------------------------           
     Vector3D LANDINGVECTOR = Vector3D.Normalize(READYPOS-CONPOS);           
     var GOTOPOINT = CONPOS+LANDINGVECTOR*((CONNECTOR.GetPosition()-CONPOS).Length());           
     ALIGNMENTVECTOR = Vector3D.Normalize(CONNECTOR.GetPosition()-GOTOPOINT);           
          
     //And Thrusters To Fire            
     //----------------------------------           
     if((GOTOPOINT-CONNECTOR.GetPosition()).Length() > (STOPPING_DIST + 0.1))          
     {          
     List<IMyTerminalBlock> PRECISE_THRUST = new List<IMyTerminalBlock>();               
     GridTerminalSystem.GetBlocksOfType<IMyThrust> (PRECISE_THRUST, DockDirection);              
     for(int j = 0; j < PRECISE_THRUST.Count;j++)                                                                   
            {          
            PRECISE_THRUST[j].SetValue<float>("Override", 30.0f);                  
            }           
     }           
         
     if((GOTOPOINT-CONNECTOR.GetPosition()).Length() < (0.4) && (CONPOS - CONNECTOR.GetPosition()).Length() > (STOPPING_DIST + 0.1) && ROLLANGLE < 2)          
     {          
     for(int j = 0; j < DOWN_THRUST.Count;j++)                                                                    
            {           
            DOWN_THRUST[j].SetValue<float>("Override", 100.0f);                   
            }           
     }          
}          
         
          
//Launching Procedure                                   
//================================================================            
//Undocking           
//undock connector and Thrust in opposite direction to connector activates and            
//when 150m away from connector it will disengage autopilot bool,            
if(OPERATION == "Launch")         
    {         
    CONNECTOR.ApplyAction("Unlock");         
         for(int j = 0; j < UP_THRUST.Count;j++)                                                                     
            {     
            UP_THRUST[j].ApplyAction("OnOff_On");            
            UP_THRUST[j].SetValue<float>("Override", 100.0f);                    
            }           
    }        
}            
                 
    //A Draft Of a Function That Will Collect thrusters that will align a ship (if dist bigger than 1m)           
    bool DockDirection(IMyTerminalBlock block)                                
        {                               
            Vector3D ORIGINPOS3 = block.GetPosition();                                   
            var FORWARDPOS3 = block.Position + Base6Directions.     GetIntVector(block.Orientation.TransformDirection(Base6Directions.Direction.Forward));       var FORWARD3 = block.CubeGrid.GridIntegerToWorld(FORWARDPOS3);                                       
            var FORWARDVECTOR3 = Vector3D.Normalize(FORWARD3 - ORIGINPOS3);                                     
            double Comparator;                            
            Comparator = Vector3D.Dot(ALIGNMENTVECTOR, FORWARDVECTOR3);                            
            bool same = ( Comparator > 0.5 && block.CubeGrid == Me.CubeGrid );                               
            return same;                               
        }             
//====================================Dockpilot=======================================================                                 
     
            
           
           
           
           
           
           
           
           
           
           
           
            
            
            
            
            
            
            
            
           
           
           
           
           
           
              
              
              
              
                
              
              
              
              
              
              
              
              
              
              
              
              
/*Secondary Functions, Contents:              
  -------------------------------------------              
          ~Travelling Autopilot     //provides a basic autopilot for space travel            
          ~Logblocks                    //permanently logs all necessary components (needs to be done off-ship)            
          ~ExtractDirection          //Extracts vector direction of a block            
          ~IsOrient                       //collect function for blocks of a similar orientation            
          ~Gyroturn2                    //Turns a gyro to required location            
*/              
              
              
              
              
               
               
               
/*====================================TravellingAutopilot=======================================================                               
  High Level Function: Travellingautopilot                              
  -------------------------------------------------------                              
  function will: Turn thrusters on and rotate the ship to face the target with collision avoidance and damping                              
  Inputs: COORDINATES, RC_UNIT, FOLLOW_HEADING, Thruster + gyro lists                    
  Requires: Gyroturn2, ISORIENT                              
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/                     
void Travellingautopilot(Vector3D COORDINATES, IMyRemoteControl RC_UNIT, Vector3D FOLLOW_HEADING )                                 
{                                    
    //COLLECT STAGE                              
    //================================================================                              
            //Getting free vector to avoid collision avoidance                              
            //-----------------------------------------------------------------                              
            Vector3D TARGETPOSITION;                              
            TARGETPOSITION =  RC_UNIT.GetFreeDestination( 	COORDINATES, (190+SHIPRADIUS), 	SHIPRADIUS );                              
                                                             
            //Retrieve First Gyro                                
            //----------------------------------                         
             IMyGyro GYRO = GYROS[0] as IMyGyro;                            
             Vector3D ORIGINPOS2 = GYRO.GetPosition();                                     
                                        
            //Distance To Target Position                              
            //-------------------------------------------                               
            double DISTANCE_TO_TARGET = (ORIGINPOS2 - COORDINATES).Length();                              
       //================================================================                              
                              
                               
       //SETUP OF VELOCITIES                                
       //================================================================                              
            //Linear Velocity                               
            //-----------------------------------------------------------------                               
            double VELOCITY =((Me.GetPosition()-PREVPOSITION).Length())/(double)(ElapsedTime.TotalSeconds);                                
            PREVPOSITION = Me.GetPosition();                              
                          
            //Target Velocity For Matching Velocities                               
            //------------------------------------------------------                             
            double TARGETVELOCITY =((COORDINATES-PREVCOORDS).Length())/(double)(ElapsedTime.TotalSeconds);                          
            PREVCOORDS = COORDINATES;                          
                          
            //Generating An Infinity Targetpostion For Heading Matching                          
            //--------------------------------------------------------------------------------                           
            Vector3D HEADING = FOLLOW_HEADING;                          
            HEADING = (HEADING*9999999999 + COORDINATES);                          
                          
           //Calculating Stopping Distance                           
           //--------------------------------------------------------------------------------                           
               double DECEL = 5000/BRAKING_DISTANCE;                          
               double STOPPING_DIST = (((VELOCITY*VELOCITY)-(TARGETVELOCITY*TARGETVELOCITY)))/(2*DECEL);                          
                  
           //Deciding On HangBack Value                            
           //------------------------------------------                        
            double HANGBACK = 0;                        
            if(TARGETVELOCITY>10);                        
            {HANGBACK = 10;}                              
                   
            //Creating Checks To Identify If It Needs To Exceed Cruise Velocity                   
            //----------------------------------------------------------------------------------------                   
            double CRUISE_VELOCITY = MAX_VELOCITY;                   
            if(SQLEAD == false)                    
            {CRUISE_VELOCITY = 200;}                   
                  
        //APPLY STAGE                               
       //================================================================                              
                                                                       
                                               
            //Applying Base Overrides                               
            //-----------------------------------------------------------------------------------                               
            GYRO.SetValue<float>("Roll", (0));                                         
            GYRO.SetValue<float>("Pitch", (0));       
                         
            if(DISTANCE_TO_TARGET>40)                          
             {                     
                GyroTurn(TARGETPOSITION, GYRO, 30 );                   
                for(int j = 0; j < GYROS.Count;j++)                                                               
                    {                                   
                    var GYROA = GYROS[j] as IMyGyro;       
                    GYROA.SetValue<float>("Yaw", (0));                                 
                    GYROA.SetValue<float>("Roll", (AZOVER));                                         
                    GYROA.SetValue<float>("Pitch", (ELOVER));                                         
                    GYROA.SetValue("Override", true);         
    
                    //Applying Thrust In Correct Direction                                
                    //------------------------------------------------                                
                    for(int i = 0; i < FORWARD_THRUST.Count;i++)                                                              
                    {                                      
                         var THRUSTER_A = FORWARD_THRUST[i] as IMyThrust;                                  
                         THRUSTER_A.ApplyAction("OnOff_On");                    
                                          
                         if(( DISTANCE_TO_TARGET > (STOPPING_DIST+5)) && DISTANCE_TO_TARGET > (HANGBACK+5)                    
                          && VELOCITY < CRUISE_VELOCITY)                               
                             {THRUSTER_A.SetValue<float>("Override", 10000.0f);}                               
                         else                           
                             {THRUSTER_A.SetValue<float>("Override", 0.0f);}                             
                        }                
                    }                      
              }                              
            else if(DISTANCE_TO_TARGET > 3)                         
              {        
                GyroTurn(HEADING, GYRO, 20 );                    
                for(int j = 0; j < GYROS.Count;j++)                                                                
                    {                                    
                    var GYROA = GYROS[j] as IMyGyro;           
                    GYROA.SetValue<float>("Yaw", (0));                           
                    GYROA.SetValue<float>("Roll", (AZOVER));                                          
                    GYROA.SetValue<float>("Pitch", (ELOVER));                                          
                    GYROA.SetValue("Override", true);                       
                    }                      
    
                  //And Thrusters To Fire             
                  //----------------------------------            
                  double STDIST = 4;    
                  if(SQLEAD) {STDIST = 20;}    
                  if(DISTANCE_TO_TARGET > (STOPPING_DIST + STDIST))           
                   {     
                   ALIGNMENTVECTOR = Vector3D.Normalize(GYRO.GetPosition()-TARGETPOSITION);          
                   List<IMyTerminalBlock> PRECISE_THRUST = new List<IMyTerminalBlock>();                
                   GridTerminalSystem.GetBlocksOfType<IMyThrust> (PRECISE_THRUST, DockDirection);               
                   for(int j = 0; j < PRECISE_THRUST.Count;j++)                                                                    
                         {           
                           PRECISE_THRUST[j].SetValue<float>("Override", 100.0f);                   
                           }            
                         }         
                   }                             
}                                       
//==================================TravellingAutopilot==================================================                   
                
              
/*=====================================Logblocks======================================================                                     
  Secondary Function: Logblocks                                
  -----------------------------------------------------                                    
  Function will: Permanently log all required ship-components                    
  Inputs: None Self allocating              
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/                    
void Logblocks()              
{              
//Retrieving And Setting Primary Components              
//-------------------------------------------------------------              
     //Finding Onboard RC Unit                        
     List<IMyTerminalBlock> RC_UNITS = new List<IMyTerminalBlock>();                          
     GridTerminalSystem.GetBlocksOfType<IMyRemoteControl> (RC_UNITS);     
     for(int j = 0; j < RC_UNITS.Count;j++)      
        {if(RC_UNITS[j].CubeGrid == Me.CubeGrid)                           
        {RC_UNIT = RC_UNITS[j] as IMyRemoteControl;}     
        }                  
                  
     //Collecting Gyros On Ship                                            
     GridTerminalSystem.GetBlocksOfType<IMyGyro> (GYROS);                   
              
     //Retrieving And Setting Orientation Of Ship              
     //-------------------------------------------------------------              
     SHIPFORWARD = ExtractDirection("FORWARD", RC_UNIT);              
     SHIPBACK = -1*ExtractDirection("FORWARD", RC_UNIT);              
     SHIPUP = ExtractDirection("UP", RC_UNIT);              
     SHIPDOWN = -1*ExtractDirection("UP", RC_UNIT);              
     SHIPLEFT = ExtractDirection("LEFT", RC_UNIT);              
     SHIPRIGHT = ExtractDirection("RIGHT", RC_UNIT);              
     SHIPLAUNCH = ExtractDirection("BACK", CONNECTOR);              
     //Retrieving And Setting Thrusters               
     //----------------------------------------------       
            GridTerminalSystem.GetBlocksOfType<IMyThrust> (THRUST);                  
            GridTerminalSystem.GetBlocksOfType<IMyThrust> (FORWARD_THRUST, ISORIENT_FORWARD);                   
            GridTerminalSystem.GetBlocksOfType<IMyThrust> (UP_THRUST, ISORIENT_UP);              
            GridTerminalSystem.GetBlocksOfType<IMyThrust> (DOWN_THRUST, ISORIENT_DOWN);                                 
}              
//=====================================Logblocks=============================================              
              
              
/*================================ExtractDirection======================================================                                  
  Secondary Function: ExtractDirection                             
  -----------------------------------------------------                                 
  Function will: Given a direction and a block, return forward, up or left vectors                  
  Inputs: DIRECTION, BLOCK                 
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/                 
Vector3D ExtractDirection(String DIRECTION, IMyTerminalBlock BLOCK)                      
{                 
Vector3D OUTPUTDIRECTION = new Vector3D();                 
                 
if(DIRECTION == "UP")                 
    {                 
    Vector3D ORIGINPOS4 = BLOCK.GetPosition();var FORWARDPOS4 = BLOCK.Position + Base6Directions.    GetIntVector(BLOCK.Orientation.TransformDirection(Base6Directions.Direction.Up));       var FORWARD4 = BLOCK.CubeGrid.GridIntegerToWorld(FORWARDPOS4);                                               
    OUTPUTDIRECTION = Vector3D.Normalize(FORWARD4 - ORIGINPOS4);                  
    }                 
                 
if(DIRECTION == "LEFT")                 
    {                 
    Vector3D ORIGINPOS4 = BLOCK.GetPosition();var FORWARDPOS4 = BLOCK.Position + Base6Directions.    GetIntVector(BLOCK.Orientation.TransformDirection(Base6Directions.Direction.Left));       var FORWARD4 = BLOCK.CubeGrid.GridIntegerToWorld(FORWARDPOS4);                                                
    OUTPUTDIRECTION = Vector3D.Normalize(FORWARD4 - ORIGINPOS4);                  
    }                 
                 
if(DIRECTION == "FORWARD")                 
    {                 
    Vector3D ORIGINPOS4 = BLOCK.GetPosition();var FORWARDPOS4 = BLOCK.Position + Base6Directions.    GetIntVector(BLOCK.Orientation.TransformDirection(Base6Directions.Direction.Forward));       var FORWARD4 = BLOCK.CubeGrid.GridIntegerToWorld(FORWARDPOS4);                                                
    OUTPUTDIRECTION = Vector3D.Normalize(FORWARD4 - ORIGINPOS4);                  
    }                 
                
if(DIRECTION == "RIGHT")                  
    {                  
    Vector3D ORIGINPOS4 = BLOCK.GetPosition();var FORWARDPOS4 = BLOCK.Position + Base6Directions.    GetIntVector(BLOCK.Orientation.TransformDirection(Base6Directions.Direction.Left));       var FORWARD4 = BLOCK.CubeGrid.GridIntegerToWorld(FORWARDPOS4);                                                 
    OUTPUTDIRECTION = -1*Vector3D.Normalize(FORWARD4 - ORIGINPOS4);                   
    }                 
                 
return OUTPUTDIRECTION;                 
}                 
//=================================ExtractDirection======================================================                 
                
              
//=======================================================================================                            
//ACCOMPANYING FUNCTION  'ISORIENT(bool collection)                           
//Provides a bool function to collect blocks of a similar orientation (requires forward, back etc to be declared prior)                              
//---------------------------------------------------------------------------------------                           
   bool ISORIENT_FORWARD(IMyTerminalBlock block)                                     
        {                                    
            Vector3D ORIGINPOS3 = block.GetPosition();                                        
            var FORWARDPOS3 = block.Position + Base6Directions.     GetIntVector(block.Orientation.TransformDirection(Base6Directions.Direction.Forward));       var FORWARD3 = block.CubeGrid.GridIntegerToWorld(FORWARDPOS3);                                            
            var FORWARDVECTOR3 = -1*Vector3D.Normalize(FORWARD3 - ORIGINPOS3);                                          
            double Comparator;          
            Comparator = Vector3D.Dot(SHIPFORWARD, FORWARDVECTOR3);                                   
            bool same = ( Comparator > 0.5 );                                                          
            return same;                                    
        }                         
                     //C.Davies  RFC 2016         
   bool ISORIENT_BACK(IMyTerminalBlock block)                                
        {                               
            Vector3D ORIGINPOS3 = block.GetPosition();                                   
            var FORWARDPOS3 = block.Position + Base6Directions.     GetIntVector(block.Orientation.TransformDirection(Base6Directions.Direction.Forward));       var FORWARD3 = block.CubeGrid.GridIntegerToWorld(FORWARDPOS3);                                       
            var FORWARDVECTOR3 = -1*Vector3D.Normalize(FORWARD3 - ORIGINPOS3);                                     
            double Comparator;                            
            Comparator = Math.Abs((SHIPBACK - FORWARDVECTOR3).Length());                            
            bool same = ( Comparator < 0.01);                               
            return same;                               
        }                      
              
   bool ISORIENT_UP(IMyTerminalBlock block)                                
        {                               
            Vector3D ORIGINPOS3 = block.GetPosition();                                   
            var FORWARDPOS3 = block.Position + Base6Directions.     GetIntVector(block.Orientation.TransformDirection(Base6Directions.Direction.Forward));       var FORWARD3 = block.CubeGrid.GridIntegerToWorld(FORWARDPOS3);                                       
            var FORWARDVECTOR3 = -1*Vector3D.Normalize(FORWARD3 - ORIGINPOS3);                                     
            double Comparator;                            
            Comparator = Math.Abs((SHIPUP - FORWARDVECTOR3).Length());                            
            bool same = ( Comparator < 0.01 );                               
            return same;                               
        }                      
              
   bool ISORIENT_DOWN(IMyTerminalBlock block)                                 
        {                                
            Vector3D ORIGINPOS3 = block.GetPosition();                                    
            var FORWARDPOS3 = block.Position + Base6Directions.     GetIntVector(block.Orientation.TransformDirection(Base6Directions.Direction.Forward));       var FORWARD3 = block.CubeGrid.GridIntegerToWorld(FORWARDPOS3);                                        
            var FORWARDVECTOR3 = -1*Vector3D.Normalize(FORWARD3 - ORIGINPOS3);                                      
            double Comparator;                             
            Comparator = Math.Abs((SHIPDOWN - FORWARDVECTOR3).Length());                             
            bool same = ( Comparator < 0.01 );                                
            return same;                                
        }                      
              
   bool ISORIENT_LEFT(IMyTerminalBlock block)                                 
        {                                
            Vector3D ORIGINPOS3 = block.GetPosition();                                    
            var FORWARDPOS3 = block.Position + Base6Directions.     GetIntVector(block.Orientation.TransformDirection(Base6Directions.Direction.Forward));       var FORWARD3 = block.CubeGrid.GridIntegerToWorld(FORWARDPOS3);                                        
            var FORWARDVECTOR3 = -1*Vector3D.Normalize(FORWARD3 - ORIGINPOS3);                                      
            double Comparator;                             
            Comparator = Math.Abs((SHIPLEFT - FORWARDVECTOR3).Length());                             
            bool same = ( Comparator < 0.01 );                                
            return same;                                
        }                      
              
   bool ISORIENT_RIGHT(IMyTerminalBlock block)                                  
        {                                 
            Vector3D ORIGINPOS3 = block.GetPosition();                                     
            var FORWARDPOS3 = block.Position + Base6Directions.     GetIntVector(block.Orientation.TransformDirection(Base6Directions.Direction.Forward));       var FORWARD3 = block.CubeGrid.GridIntegerToWorld(FORWARDPOS3);                                         
            var FORWARDVECTOR3 = Vector3D.Normalize(FORWARD3 - ORIGINPOS3);                                       
            double Comparator;                              
            Comparator = Math.Abs((SHIPRIGHT - FORWARDVECTOR3).Length());                              
            bool same = ( Comparator < 0.01 );                                 
            return same;                                 
        }                      
              
   bool ISORIENT_LAUNCH(IMyTerminalBlock block)                                 
        {                                
            Vector3D ORIGINPOS3 = block.GetPosition();                                    
            var FORWARDPOS3 = block.Position + Base6Directions.     GetIntVector(block.Orientation.TransformDirection(Base6Directions.Direction.Forward));       var FORWARD3 = block.CubeGrid.GridIntegerToWorld(FORWARDPOS3);                                        
            var FORWARDVECTOR3 = -1*Vector3D.Normalize(FORWARD3 - ORIGINPOS3);                                      
            double Comparator;                             
            Comparator = Math.Abs((SHIPLAUNCH - FORWARDVECTOR3).Length());                             
            bool same = ( Comparator < 0.01 );                                
            return same;                                
        }                    
//=======================================================================================                           
                           
              
/*=======================================================================================                             
  Function: GyroTurn2                           
  ---------------------------------------                            
  function will: rotate a gyro using overrides to point to a target with damping (up of gyro is forward)                            
  Inputs: GYRO, TARGETPOSITION, MULTIPLIER                            
//=======================================================================================*/                                             
     double VEL_MULTIPLIER = 20;                           
     double PREV_MISSILEELEVATION;                           
     double PREV_MISSILEAZIMUTH;                
     float AZOVER;                
     float ELOVER;                           
void GyroTurn(Vector3D TARGETPOSITION, IMyGyro GYRO, double MULTIPLIER )                               
{                              
             //Get forward  + up unit vector (compressed function)                             
             //-----------------------------------------------------------------------                             
             Vector3D ORIGINPOS = GYRO.GetPosition();                                
             var FORWARDPOS = GYRO.Position + Base6Directions.     GetIntVector(GYRO.Orientation.TransformDirection(Base6Directions.Direction.Forward));       var FORWARD = GYRO.CubeGrid.GridIntegerToWorld(FORWARDPOS);                                    
             var FORWARDVECTOR = Vector3D.Normalize(FORWARD - ORIGINPOS);                                  
             var UPPOS = GYRO.Position + Base6Directions.       GetIntVector(GYRO.Orientation.TransformDirection(Base6Directions.Direction.Up));          var UP = GYRO.CubeGrid.GridIntegerToWorld(UPPOS);                                      
             var UPVECTOR = Vector3D.Normalize(UP - ORIGINPOS);                                  
                             
             //Create inverse quaternion + translate forward direction from forward + up                             
             //----------------------------------------------------------------------------------------------------                             
             Quaternion QUAT_TWO = Quaternion.CreateFromForwardUp(  UPVECTOR	 , FORWARDVECTOR 	 );                                 
             var INVQUAT = Quaternion.Inverse( QUAT_TWO 	);                                
             Vector3D DIRECTIONVECTOR = Vector3D.Normalize(TARGETPOSITION - ORIGINPOS);                             
             Vector3D AZ_EL_VECTOR = Vector3D.Transform( DIRECTIONVECTOR 	, INVQUAT );                             
                             
             //Converting To Azimuth And Elevation + Gyro Overrrides                             
             //-------------------------------------------------------------                                
             double MISSILEAZIMUTH = 0;                             
             double MISSILEELEVATION = 0;                            
             Vector3D.GetAzimuthAndElevation( AZ_EL_VECTOR , 	out  MISSILEAZIMUTH, out MISSILEELEVATION );                            
             //Rdav  RFC 2016            
             double ELEVATION_VEL = VEL_MULTIPLIER*(MISSILEELEVATION - PREV_MISSILEELEVATION)/(double)(ElapsedTime.TotalSeconds);                              
             double AZIMUTH_VEL = VEL_MULTIPLIER*(MISSILEAZIMUTH - PREV_MISSILEAZIMUTH)/(double)(ElapsedTime.TotalSeconds);                            
             PREV_MISSILEELEVATION = MISSILEELEVATION;                           
             PREV_MISSILEAZIMUTH = MISSILEAZIMUTH;                           
                           
             AZOVER = (float)((MISSILEAZIMUTH*MULTIPLIER + AZIMUTH_VEL));                                
             ELOVER = (float)((MISSILEELEVATION*-MULTIPLIER - ELEVATION_VEL));                               
             GYRO.SetValue<float>("Roll", (AZOVER));                                     
             GYRO.SetValue<float>("Pitch", (ELOVER));                                     
             GYRO.SetValue("Override", true); }                            
//=======================================================================================                           
                                     