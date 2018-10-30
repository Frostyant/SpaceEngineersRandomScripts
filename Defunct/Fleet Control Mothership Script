/*====================================================================================    
                                                                               HI THERE!    
   
Welcome! and thank you for downloading my Fleet Command Script!   
    
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
   
    You are currently looking at the 'Main' module used on motherships.   
    It requires:   
    ~A timer loop triggering itself,   
    ~An LCD panel called GPSPANEL for the gps menu   
    ~An LCD panel called MENUPANEL for the main menu   
    ~Any relevantly named docking connectors   
    ~Any long range sensors set to detect enemy targets   
        
    The script as is is in a very WIP state, this is an alpha release simply allowing people to get to grips with the   
    concept, a lot of the things here are highly experimental. If you have any crippling bug reports and issues    
    please report on the workshop page so I can get them fixed soon as possible, along with any requests!     
   
Have fun!    
Also I highly advise using this in a damage-disabled world to get to grips with it, if you set things up badly    
collisions can be somewhat.. destructive,    
     
======================================================================================== */   
   
 
//User Inputs Section:  Used for adjusting formation and fleet sizes  
//------------------------------------------------------------------------------------------  
int FRIGATE_DIST = 100;                                         //Distance that frigates go from the mothership in formation 
int A_CRAFT_DIST_UP = 150;                                //Distance that fighters go 'up' from the mothership in formation 
  
//======================================================================================= 
   
 
 
 
 
/*   
Latest Update (newest at top):    
-----------------------------------------   
Added a user inputs section for larger than normal ships 
Added automatic persistant squadron logging  
Added null handling if hangar block connector gets knocked out  
Added a proper hangar location for squadron docking + launching  
Added a function to allow for Forced squadron assignment  
Added diagnostics mode  
Only detects sensors with an 'RFC' tag  
Error checking for null EN-list  
Revisions to undock distance (default 40m)   
Major optimisations, only running 1 squadron per-tick   
Added a delete function for targeted enemies   
Added followme toolbar input options   
Added basic followme support    
Added correct count information to attack menu   
Added Mask + error readout in-case there are no LCD panels   
Increased sensor setup automation   
*/   
             
/*Output Key:      
======================      
Provides information regarding the output arguments sent to slave programmable blocks      
-------------------------------------------------------------------------------------------------------------------------------------------------------      
String, Order:         COMMAND  #   TARGETPOSITION   #   EXTRA_INFORMATION     
                               (extra information gives heading or docking information to block)     
-----------------------------------------------------------------------------------------------------------------------------------------------------*/     
     
     
/*SQ DATATABLE Layout:     
======================     
Provides an information table for Squad information and contents, every squad has a new matching index,      
-------------------------------------------------------------------------------------------------------------------------------------------------------     
(All are Arrays, SQINDEX, SQ_TYPE, and SQ_MAXCOUNT are constant)    ~CURRENTLY SETUP FOR: 10 SQUADS~     
-------------------------------------------------------------------------------------------------------------------------------------------------------     
  SQINDEX  |   SQ_TYPE   |  SQ_MAXCOUNT  |   SQ_COMMAND   |   SQ_TARGET  | SQ_GROUPIDS | SQ_TARGETID            
     index     |      string      |            int              |            string          |      vector3d    |       list of int()   |          int     
-----------------------------------------------------------------------------------------------------------------------------------------------------*/      
//4 assaultcraft 4 frigates and 2 heavy cruisers     
string[] SQ_TYPE = new string[10]      
{"AssaultCraft","AssaultCraft","AssaultCraft","AssaultCraft","Frigate",     
"Frigate","Frigate","Frigate","Cruiser","Cruiser"};             
new List<int> SQ_MAXCOUNT = new List<int>() {5,5,5,5,1,1,1,1,1,1};      
            
string[] SQ_COMMAND = new string[10];           
Vector3D[] SQ_TARGET = new Vector3D[10];           
new List<int>[] SQ_GROUPIDS = new List<int>[10];           
int[] SQ_TARGETID = new int[10];       
     
     
/*EN DATATABLE Layout:      
=======================      
Provides an information table for Enemy information     
-----------------------------------------------------------------------------------------     
(All are lists to provide for an ulimited tracking facility)      
-----------------------------------------------------------------------------------------       
        ENINDEX  |   EN_SIZE   |  EN_LOCATION  |   EN_GRID_ID   |               
           index     |        int        |     Vector3D       |        gridid         |        
----------------------------------------------------------------------------------------*/     
List<int> EN_SIZE =  new List<int>();      
List<Vector3D> EN_LOCATION =  new List<Vector3D>();      
List<IMyCubeGrid>EN_GRID_ID = new List<IMyCubeGrid>();       
     
     
/*SHIP DATATABLE Layout:       
=======================       
Provides an information table for Each ship and squad reference     
Edit: Changed to singular programmable blocks for each ship     
-----------------------------------------------------------------------------------------      
(All are arrays to help maintain shipcount)       
-------------------------------------------------------------      
    SHINDEX  |    SH_GROUP    |  SH_SQ_ID |                
       index     |          List           |       int()      |         
-------------------------------------------------------------*/      
List<IMyProgrammableBlock> SH_BLOCKS =  new List<IMyProgrammableBlock>();      
List<int> SH_SQ_ID =  new List<int>();      
     
     
/*MOTHERSHIP CONSTANTS:        
=======================      
All one-time perma-logged mothership objects, done so to prevent multi-logging+unecesessary logging     
-------------------------------------------------------------------------------------------------------------------------------------------*/     
IMyRemoteControl MS_RCUNIT;             //Rc unit on mothership used for formation + location information     
List<IMyShipConnector> MS_CONNECTORS = new List<IMyShipConnector>();    // Mothership connectors     
IMyTextPanel MENUPANEL;     
IMyTextPanel GPSPANEL;      
Color Green = new Color(0,255,0);               //Colour for GPS panel    
     
/*CONTINUOUS CONSTANTS:         
========================       
Dynamically Logged And Updated Blocklists     
-------------------------------------------------------------------------------------------------------------------------------------------*/     
List<IMySensorBlock> SENSORS = new List<IMySensorBlock>();      //Global list of sensors     
//List<IMySensorBlock> LOGGEDBLOCKS = new List<IMySensorBlock>();    //Permanently logged ship-blocks     
int SQ_TIMER = 0;                                                                                          //A timer used to initiate and run command   
     
//Resetable Variables For User Interface      
int MENUINDEX_SQ = 0;                //Currently selected squadron            (menu 1)      
int MENUINDEX_COM = 0;                //Currently selected command            (menu 2)      
int MENUINDEX_TARGET = 0;       //Currently detected target                 (menu 3)      
int MENUPAGE = 0;                       //Currently Displayed menu page      
String COMMAN  = "";                    //Output String Command For User Interface      
Vector3D GOTOPOS;                     //Go-to position if required      
      
//Constant Strings For User-Interface      
String CommandGoToString_all = ("     ++ Please Select Command ++          \n============================= \n<--Prev  [GoTo] Attack  Dock   Next--> \n                  \n            GoTo          \n            --------------------                                     \n            Selected squadron will go  \n            to a location                \n               \n        \n\n------------------------------------------------ \nRecent Events:");         
String CommandAttackString_all = ("     ++ Please Select Command ++           \n=============================  \n<--Prev   GoTo [Attack] Dock   Next-->  \n                   \n            Attack          \n            --------------------                                      \n            Selected squadron will \n            attack selected enemy \n            target \n                                  \n\n------------------------------------------------  \nRecent Events:");        
String CommandDockString_all = ("     ++ Please Select Command ++            \n=============================   \n<--Prev   GoTo  Attack [Dock]  Next-->   \n                    \n            Dock            \n            --------------------                                       \n            Selected squadron will   \n            attempt to dock on  \n            allocated connectors  \n                 \n\n------------------------------------------------   \nRecent Events:");        
String MainMenuString1_2 = "      ++ Please Select Squadron ++          \n=============================";        
String AttackMenuString = "       ++  Please Select Target  ++          \n============================= \n<--Up                                      Down--> \n \nEnemy Targets At: ";       
String GoToMenuString = "       ++  Please Select Target  ++           \n=============================  \n<--Up                                      Down-->  \n  \nStored Locations:  ";       
String[] GoToMenuOptions = new String[] {"1500m In Front Of Mothership", "1500m Left Of Mothership","1500m Right Of Mothership","1500m Above Mothership", "Select From GPS"};       
String[] GoToMenuGeneration = new String[] {"FORWARD", "LEFT", "RIGHT","UP"};      
     
     
//################################################################################     
//################################################################################     
//################################################################################     
//################################################################################     
     
     
bool setup = false;     
     
     
void Main(String argument)     
{	     
     
    //First time setup              
    //------------------------------------------------------------------------------------------------------------------------------------------         
        if(setup == false)                                                                          //first time setup required to initialise         
        {   
        for (int i = 0; i < SQ_GROUPIDS.Length; i++)          
            { SQ_GROUPIDS[i] = new List<int>();}       
      
        //Initially Detect LCD Panel       
        //------------------------------------      
        MENUPANEL = GridTerminalSystem.GetBlockWithName( 	"MENUPANEL" ) as IMyTextPanel;      
        GPSPANEL = GridTerminalSystem.GetBlockWithName( 	"GPSPANEL" ) as IMyTextPanel;      
              
        if(MENUPANEL == null || GPSPANEL == null)   
        {Echo("Missing Required LCD Panels"); return;}   
        GPSPANEL.WritePublicText("         @@ GPS Input Panel @@            \n=============================" + "\n Please write in GPS coords \n on the next line between the  \n" +" symbols, Format should be; \n"+"              {X:0 Y:0 Z:0} \n \n*{X:10 Y:10 Z:10}* \n"+"               \n        When written select  \n        'Select from GPS' \n        on main-menu panel \n");   
        GPSPANEL.ShowPrivateTextOnScreen();      
        GPSPANEL.ShowPublicTextOnScreen();    
        GPSPANEL.SetValue("FontColor",Green);    
         setup = true;    
        }   
    //------------------------------------------------------------------------------------------------------------------------------------------         
    
   
//Issuing All follow me command in scenario of following   
//---------------------------------------------------------------------------   
if(argument == "All Follow Me")   
    {   
    for (int i = 0; i < SQ_TYPE.Length; i++)                         //(i = squadindex)     
        {   
        if(SQ_COMMAND[i] != "Dock" && SQ_COMMAND[i] != "")   
        {SQ_COMMAND[i] = "Follow";}   
        }   
    }   
   
DeleteDestroyed();    
SensorScan();    
UpdateSQ_DATATABLE();        //   (33 runs)     
GroupAllocate();        //Will Return function upon sucessfully finding new matching group         //   (41 runs)     
Command();                                      //   (76 runs)     
UserInterface(argument);                // (8 runs)     
//Echo(Runtime.CurrentInstructionCount.ToString());    
    
}     
     
//========================================================================================           
//##########################    ###    ###   ###   ###   #############################################          
//##########################    ###    ###   ###   ###  #############################################            
//=======================================================================================      
     
     
/*=========================================Command===========================================3=======3============                     
  Function: Command                   
  ---------------------------------------                    
  Function will: Run each programmable block with relevant string command    (runs once for each ship)     
  Inputs: None, self allocating      
~theorised secondaries~     
    run for formation input and ship     
    dock command interface     
    leaderinfo     
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/       
void Command()         
{        
int i = SQ_TIMER;                                                       //Sets 'i' as squadron index to run   
   
    if(SQ_GROUPIDS[i].Count > 0)                                //Check to make sure it can't run for a negative squadron     
        {     
          String COMMAND = SQ_COMMAND[i];      //extract command     
          Vector3D TARGETPOS = SQ_TARGET[i];           //extract location     
   //---+------+------+------+--------+-------+-------+-----+-------+--------+-------+-------+-----+-------               
          if(COMMAND == "GoTo" || COMMAND == "Follow" )     
                {    
                if((TARGETPOS - SH_BLOCKS[SQ_GROUPIDS[i][0]].GetPosition()).Length() < 1500)     
                    {DefendPosition(i,TARGETPOS);}     
                else    
                    {GoToCommand(i,TARGETPOS);}    
                }     
  //---+------+------+------+--------+-------+-------+-----+-------+--------+-------+-------+-----+-------             
          if(COMMAND == "Dock")     
                {     
                DockCommand(i);    
                }     
  //---+------+------+------+--------+-------+-------+-----+-------+--------+-------+-------+-----+-------        
          if(COMMAND == "Attack")    
                {    
                if((TARGETPOS - SH_BLOCKS[SQ_GROUPIDS[i][0]].GetPosition()).Length() < 1000)    
                    {AttackCommand(i,TARGETPOS);}    
                else    
                    {GoToCommand(i,TARGETPOS);}    
                }    
  //---+------+------+------+--------+-------+-------+-----+-------+--------+-------+-------+-----+-------        
        }     
SQ_TIMER ++;                //function to increase or reset timer   
if(SQ_TIMER > 9)   
{SQ_TIMER = 0;}   
}     
//====================================Command===================================================     
     
    
     
/*====================================UserInterface==========================================                      
  Function: UserInterface                    
  ---------------------------------------                     
  Function will: Create and allow use of a user interface to control SQ_DICT      
  Inputs: None, self allocating       
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/       
     
void UserInterface(String argument)          
{      
     
//Reset Menuwrite At Start Of Iteration     
String MENUWRITE = "";     
     
//Run Enter And Output System     
//-----------------------------------------     
if(argument == "Enter")     
    {MENUPAGE ++;}     
if(argument == "Clear")      
    {MENUPAGE = 0;MENUINDEX_SQ = 0;MENUINDEX_COM = 0;MENUINDEX_TARGET = 0; COMMAN  = ""; GOTOPOS = new Vector3D(0,0,0); }     
if(argument == "Enter" && MENUPAGE == 3)      
    {   
    if(EN_LOCATION.Count == 0 && COMMAN == "Attack" )   {MENUPAGE = 0;MENUINDEX_SQ = 0;MENUINDEX_COM = 0;MENUINDEX_TARGET = 0; COMMAN  = ""; GOTOPOS = new Vector3D(0,0,0); }      
    WriteTo_SQ_DATATABLE(MENUINDEX_SQ,MENUINDEX_TARGET,COMMAN,GOTOPOS);     
    MENUPAGE = 0;MENUINDEX_SQ = 0;MENUINDEX_COM = 0;MENUINDEX_TARGET = 0; COMMAN  = ""; GOTOPOS = new Vector3D(0,0,0);     
    }     
     
//---+------+------+------+--------+-------+-------+-----+-------+--------+-------+-------+-----+-------      
if(MENUPAGE == 0)     
    {     
    //Generating Command     
    if(argument == "Left"&&MENUINDEX_SQ>0)     
        {     
        MENUINDEX_SQ --;     
        }     
    if(argument == "Right"&&MENUINDEX_SQ<8)      
        {      
        MENUINDEX_SQ ++;      
        }     
     
    //Writing Tracker Line     
    //------------------------------------------------        
    MENUWRITE = MainMenuString1_2;     
    MENUWRITE = MENUWRITE + "\n<--Prev   ";     
    for(int i = 0; i < 9; i++)         
        {     
        if(MENUINDEX_SQ == i)     
            {MENUWRITE = MENUWRITE+" ["+i+"]";}     
        else     
            {MENUWRITE = MENUWRITE+" "+i;}     
        }     
    MENUWRITE = MENUWRITE + "    Next-->";     
     
    //Writing SquadronInfo Lines     
    //------------------------------------------------       
    MENUWRITE  = MENUWRITE+"\n\n          Squadron  "+((MENUINDEX_SQ).ToString()) +"       \n"    +"          --------------------                    \n";     
    MENUWRITE = MENUWRITE+"          [Class: "+SQ_TYPE[MENUINDEX_SQ]+"]\n";     
    MENUWRITE = MENUWRITE+"          [Command: "+SQ_COMMAND[MENUINDEX_SQ]+"]\n";     
    MENUWRITE = MENUWRITE+"          [Ships In Squadron: "+ SQ_GROUPIDS[MENUINDEX_SQ].Count+"/"+SQ_MAXCOUNT[MENUINDEX_SQ]+"]               \n";      
    MENUWRITE = MENUWRITE+"\n\n"+"------------------------------------------------\n"+"Recent Events:"+"\n"+"\n"+"                                      [RFC 2016]  ";     
    }     
//---+------+------+------+--------+-------+-------+-----+-------+--------+-------+-------+-----+-------      
if(MENUPAGE == 1)      
    {      
    //Generating Command      
    if(argument == "Left"&&MENUINDEX_COM>0)      
        {      
        MENUINDEX_COM --;      
        }      
    if(argument == "Right"&&MENUINDEX_COM<2)       
        {       
        MENUINDEX_COM ++;       
        }     
    //Writing Pre-Determined Menus     
    if(MENUINDEX_COM  == 0)     
    {MENUWRITE = CommandGoToString_all; COMMAN = "GoTo";}     
    if(MENUINDEX_COM  == 1)      
    {MENUWRITE = CommandAttackString_all; COMMAN = "Attack";}     
    if(MENUINDEX_COM  == 2)      
    {MENUWRITE = CommandDockString_all; COMMAN = "Dock";}     
    }     
//---+------+------+------+--------+-------+-------+-----+-------+--------+-------+-------+-----+-------      
if(MENUPAGE == 2)       
    {       
   // Generating Command     
   //------------------------------------------------          
   if(argument == "Left"&&MENUINDEX_TARGET>0)       
        {       
        MENUINDEX_TARGET --;       
        }       
    if(argument == "Right"&&MENUINDEX_TARGET<4)        
        {        
        MENUINDEX_TARGET ++;        
        }     
     
    //Writing Menu For Attack     
    //------------------------------------------------        
    if(COMMAN == "Attack")      
    {     
    if(MENUINDEX_TARGET > EN_LOCATION.Count -1)                //Rewrites menu to ensure no blank space selection   
    {MENUINDEX_TARGET = EN_LOCATION.Count -1;}   
    if(MENUINDEX_TARGET < 0)                  
    {MENUINDEX_TARGET = 0;}   
   
    MENUWRITE = AttackMenuString +"\n";     
     for(int i = 0; i < (EN_LOCATION.Count) ; i++)          
        {      
        if(MENUINDEX_TARGET == i)      
            {MENUWRITE = MENUWRITE+"\n     >"+ EN_LOCATION[i] + "<";}      
        else      
            {MENUWRITE = MENUWRITE+"\n     "+ EN_LOCATION[i];}      
        }     
    }     
     
    //Writing Menu For GoTo Menu      
    //------------------------------------------------        
    if(COMMAN == "GoTo")       
    {      
    MENUWRITE = GoToMenuString;      
     for(int i = 0; i < 5; i++)           
        {       
        if(MENUINDEX_TARGET == i && MENUINDEX_TARGET < 4)       
            {     
            MENUWRITE = MENUWRITE+"\n     >"+ GoToMenuOptions[i] + "<";     
            GOTOPOS = (Vector3D)Me.GetPosition() + 1500*(ExtractDirection(GoToMenuGeneration[i], Me));     
            }       
        else if(MENUINDEX_TARGET == 4 && i == 4 )   
            {   
            GOTOPOS = new Vector3D(0,0,0);   
            String[] STORED_DATA = (GPSPANEL.GetPublicText()).Split('*');    
            if(STORED_DATA.Length >= 1)    
                {Vector3D.TryParse(STORED_DATA[1], out GOTOPOS);}     
            MENUWRITE = MENUWRITE+"\n     >"+ GoToMenuOptions[i] + "<";      
            }   
        else       
            {MENUWRITE = MENUWRITE+"\n     "+ GoToMenuOptions[i];}       
        }      
    MENUWRITE = MENUWRITE+"\n\n------------------------------------------------  \nRecent Events:";      
    }     
    if(COMMAN == "Dock")     
    {MENUWRITE = "\n\n    INITIATING DOCK,\n    PLEASE ATTEMPT TO MAINTAIN\n    A CONSTANT VELOCITY\n\n        (Press enter to continue)";}     
    }     
//---+------+------+------+--------+-------+-------+-----+-------+--------+-------+-------+-----+-------      
    //Finish All By Writing Menu     
    //-------------------------------------   
    if(Me.DisplayNameText == "Diagnostics Mode")   
    {MENUWRITE = "Diagnostics Mode Activated \n" +  
    "\n" + "Runtime Indicator:" + SQ_TIMER.ToString() +   
    "\n" +"Logged Sensors On Ship:"+ SENSORS.Count.ToString() +   
    "\n" +"Currently Logged Friendly Ships: " + SH_BLOCKS.Count.ToString() +  
    "\n" +" \n To return to main menu, \n change p-block name \n Do not press any command keys \n when in this menu";}  
  
    MENUPANEL.WritePublicText(MENUWRITE);     
    MENUPANEL.ShowPrivateTextOnScreen();     
    MENUPANEL.ShowPublicTextOnScreen();   
}     
//====================================UserInterface==========================================         
     
    
   
    
//=====================================FollowMe=========================================          
//Low Level Function: FollowMe generation          
//Function will: Rename squadron targetposition, if follow me is selected                
// ------------------------------------------------------------------------------------------------------             
Vector3D FollowMe(int i)      
{                
        Vector3D VECTOR = new Vector3D(0,0,0);   
            
        //If it is a  Frigateclass, on same level   
        if(SQ_TYPE[i] == "Frigate")   
        {   
        VECTOR = Me.GetPosition() + Math.Pow(-1, (i-3))*ExtractDirection("LEFT", Me)*((i-3)*FRIGATE_DIST);   
        SQ_TARGET[i] = VECTOR;    
        }   
   
        //If it is a  Fighterclass, above   
        if(SQ_TYPE[i] == "AssaultCraft")    
        {    
        VECTOR = Me.GetPosition() + Math.Pow(-1, (i))*ExtractDirection("LEFT", Me)*((i)*70) + ExtractDirection("UP", Me)*A_CRAFT_DIST_UP;    
        SQ_TARGET[i] = VECTOR;     
        }   
   
        return VECTOR;   
}          
//=====================================FollowMe=======================================        
     
      
      
      
      
      
      
      
      
      
     
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
     
     
     
     
     
     
     
     
     
//Finished Functions:     
/*-----------------------------     
Contents:     
                   ~DefendPosition (inputs)   
                   ~AttackCommand (inputs)    
                   ~DockCommand (inputs)    
                   ~ExtractDirection  (inputs)     
                   ~UpdateSQ_DATATABLE     
                   ~GroupAllocate     
                   ~WriteTo_SQ_DATATABLE (inputs)     
                   ~GoToCommand  (inputs)     
                   ~DeleteDestroyed    
                   ~SensorScan  (well, I say completed...)   
*/     
     
   
     
//====================================DefendPosition=======================================================         
//Low Level Function: DefendPosition        
//Function will: Rename squadron to attack targets if an enemy is in range               
// ------------------------------------------------------------------------------------------------------            
void DefendPosition(int i, Vector3D TARGETPOS)         
{         
        //Finding First Enemy Target and attacking if in range     
        //-----------------------------------------------------------------------     
        for (int j = 0; j < EN_LOCATION.Count; j++)          
              {        
              Vector3D BLANK =  new Vector3D(0,0,0);        
              double BLANKCHECK = (EN_LOCATION[j]-BLANK).Length();        
              if(BLANKCHECK > 3)                                                                        //only run for actual Enemydetails        
                    {        
                    Vector3D POS = EN_LOCATION[j];        
                    double DISTANCE = Math.Abs((POS - TARGETPOS).Length());         
                    if(DISTANCE < 1000)         
                         {     
                         Echo("currently writing for targets");     
                         for (int k = 0; k < SQ_GROUPIDS[i].Count; k++)                                          //(k= individual ship)            
                             {         
                             //Generating Hangar Location + Sending coords about launch sequence    
                             IMyTerminalBlock HANGAR_LOC = GridTerminalSystem.GetBlockWithName( 	i+"0" ) as IMyTerminalBlock;      
                             if(HANGAR_LOC == null) {HANGAR_LOC = Me;}  
  
                             var SHIP_TO_COMMAND =  SH_BLOCKS[SQ_GROUPIDS[i][k]];        
                             SHIP_TO_COMMAND.TryRun("Attack" + "#" + (EN_LOCATION[j].ToString()) + "#" + (new Vector3D(0,0,0)).ToString()+ "#" + (SHIP_TO_COMMAND.GetPosition()-HANGAR_LOC.GetPosition()).Length().ToString()  );          
     
                             if((SHIP_TO_COMMAND.GetPosition()-HANGAR_LOC.GetPosition()).Length() < 40)  //terminates if prev is not clear of hangar         
                             {return;}                               
                             }       
                         return;     
                         }                   
                   }        
            }      
            GoToCommand(i,TARGETPOS);      
}         
//=====================================DefendPosition========================================        
      
   
   
   
//===================================AttackCommand =========================================         
//Low Level Function: AttackCommand        
//Function will: Rename squadron to attack a specified target               
// ------------------------------------------------------------------------------------------------------            
void AttackCommand(int i, Vector3D TARGETPOS )     
{         
  for (int j = 0; j < SQ_GROUPIDS[i].Count; j++)                                                        //(j= individual ship)         
        {      
        //Generating Hangar Location + Sending coords about launch sequence    
        IMyTerminalBlock HANGAR_LOC = GridTerminalSystem.GetBlockWithName( 	i+"0" ) as IMyTerminalBlock;   
        if(HANGAR_LOC == null) {HANGAR_LOC = Me;}  
  
        var SHIP_TO_COMMAND =  SH_BLOCKS[SQ_GROUPIDS[i][j]];     
        SHIP_TO_COMMAND.TryRun("Attack" + "#" + (TARGETPOS.ToString()) + "#" + (new Vector3D(0,0,0)).ToString()+ "#" + (SHIP_TO_COMMAND.GetPosition()-HANGAR_LOC.GetPosition()).Length().ToString()  );       
             
        if((SHIP_TO_COMMAND.GetPosition()-HANGAR_LOC.GetPosition()).Length() < 40)  //terminates if prev is not clear of hangar        
        {return;}    
        }       
}         
//====================================AttackCommand ======================================       
     
       
/*================================DockCommand======================================================                           
  Secondary Function: DockCommand                      
  -----------------------------------------------------                          
  Function will: Dock a squadron at relevant connectors           
  Inputs: i, (i = Squadron-index)       
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/          
void DockCommand(int i)     
{     
for (int j = 0; j < SQ_GROUPIDS[i].Count; j++)                        //For every ship in fighter squadron         
        {         
        var ACTIVE_BLOCK = SH_BLOCKS[SQ_GROUPIDS[i][j]];         
        string SHIP_ID = i.ToString() + j.ToString();       
        var CON = GridTerminalSystem.GetBlockWithName(SHIP_ID);       //find connector for ship        
        if(CON != null)                                                //only run dock if there is a connector         
             {      
             Vector3D ORIGINPOS4 = CON.GetPosition();                                
             var FORWARDPOS4 = CON.Position + Base6Directions.     GetIntVector(CON.Orientation.TransformDirection(Base6Directions.Direction.Forward));       var FORWARD4 = CON.CubeGrid.GridIntegerToWorld(FORWARDPOS4);                                    
             var FORWARDVECTOR4 = Vector3D.Normalize(FORWARD4 - ORIGINPOS4);                                     
          
             var DOCKREADYPOS  =  FORWARDVECTOR4*100+ORIGINPOS4;        
             ACTIVE_BLOCK.TryRun("Dock" + "#" + ORIGINPOS4.ToString() +  "#" + ExtractDirection("UP", CON).ToString()+"#"+ DOCKREADYPOS.ToString() );          
                 
             }        
     
        }     
}            
//==================================DockCommand===========================================           
    
      
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
      
     
     
     
     
     
/*================================UpdateSQ_DATATABLE=========================================================                     
  Function: UpdateSQ_DATATABLE                   
  ---------------------------------------                    
  Function will: Update, and rename SQ_TARGET, for current values of vectors    (runs 10 times max)      
  Inputs: None, self allocating             
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/      
void UpdateSQ_DATATABLE()         
{         
      
//Iterating Through Squadrons       
//----------------------------------------         
for (int i = 0; i < SQ_TYPE.Length; i++)                         //(i = squadindex)      
    {         
    String COMMAND = SQ_COMMAND[i];      
    if(COMMAND == "Follow")                              //Running For Friendly Commands         
        {       
        var REFPOS = FollowMe(i);   
        //int REF = SQ_TARGETID[i];                                        //extracting target referece      
        //var REFPOS = SH_BLOCKS[REF].GetPosition();              
        SQ_TARGET[i] =   REFPOS;                                       //writing targetposition to Sq_table      
        }         
    if(COMMAND == "Attack")                               //Running For Enemy Commands          
        {          
        int REF = SQ_TARGETID[i];                                        //extracting target referece       
        if(EN_SIZE[REF]>3 && (EN_LOCATION[REF] - (new Vector3D(0,0,0))).Length() >10)    
            {   
            var REFPOS = EN_LOCATION[REF];                           //(also deletes any destryed enemy targets)   
            SQ_TARGET[i] =   REFPOS;                                       //writing targetposition to Sq_table       
            }   
        else   
            {   
            SQ_COMMAND[i] = "GoTo";                                     //Deletes any small or destroyed enemy grids   
            EN_SIZE.Remove(EN_SIZE[REF]);   
            EN_LOCATION.Remove(EN_LOCATION[REF]);   
            EN_GRID_ID.Remove(EN_GRID_ID[REF]);   
            }   
        }         
    }         
}         
//=================================UpdateSQ_DATATABLE====================================================        
     
     
     
     
      
/*====================================GroupAllocate====================================================                    
  Function: GroupAllocate                  
  ---------------------------------------                   
  Function Will: Detect new blocks of three names "AssaultCraft" "Frigate" "Cruiser" and put them into new sqd's                                      
  Inputs: None, Self Allocating            
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/      
void GroupAllocate()       
{	      
      
//Scanning For Programmable Blocks (PERF HEAVY)      
//-----------------------------------------------------------------------      
List<IMyTerminalBlock> UNLOGGED_BLOCKS =  new List<IMyTerminalBlock>();                                       
GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock> (UNLOGGED_BLOCKS);           
      
// Sqd Allocation      
//---------------------------------      
for (int i = 0; i < UNLOGGED_BLOCKS.Count; i++)                      //This runs through every just found block (i=BLOCK)          
        {                            
        IMyProgrammableBlock TESTBLOCK = UNLOGGED_BLOCKS[i] as IMyProgrammableBlock;      
        String SHIP_TYPE =  TESTBLOCK.DisplayNameText;  
  
        if(TESTBLOCK.DisplayNameText.Split('#').Length > 1)         //Runs if user specifies squadron  
        {  
        SHIP_TYPE = (TESTBLOCK.DisplayNameText.Split('#'))[0];      
        int SHIP_SQUAD_ID;  
        int.TryParse((TESTBLOCK.DisplayNameText.Split('#'))[1], out SHIP_SQUAD_ID);      
  
        if(SH_BLOCKS.Contains(TESTBLOCK) == false && (TESTBLOCK.DisplayNameText.Split('#'))[1] != "" && SQ_GROUPIDS[SHIP_SQUAD_ID].Count < SQ_MAXCOUNT[SHIP_SQUAD_ID] && SHIP_TYPE == SQ_TYPE[SHIP_SQUAD_ID])     
             {    
             int NEWINDEX = SH_BLOCKS.Count;             //generates a new index + adds to first non-full list                           
             SH_BLOCKS.Add(TESTBLOCK);                     //Adds to loggedblocks        
             SQ_GROUPIDS[SHIP_SQUAD_ID].Add(NEWINDEX);               //Adds index to sq_group        
             SH_SQ_ID.Add((int)SHIP_SQUAD_ID);                                           //Creates new associated index for relevant squadid        
             return;                                                             //finishes the programme     
             }     
         }  
  
         if(SH_BLOCKS.Contains(TESTBLOCK) == false)              
              {       
              for (int j = 0; j < SQ_TYPE.Length; j++)           //searches for first squadron of matching name  (j=sqindex)       
                  {      
                  if(SHIP_TYPE == SQ_TYPE[j] && SQ_GROUPIDS[j].Count < SQ_MAXCOUNT[j])      
                       {      
                        int NEWINDEX = SH_BLOCKS.Count;             //generates a new index + adds to first non-full list                         
                        SH_BLOCKS.Add(TESTBLOCK);                     //Adds to loggedblocks      
                        SQ_GROUPIDS[j].Add(NEWINDEX);               //Adds index to sq_group      
                        SH_SQ_ID.Add(j);                                           //Creates new associated index for relevant squadid    
                        TESTBLOCK.SetCustomName(SHIP_TYPE.ToString() + "#" + j);  
                        return;                                                             //finishes the programme      
                        }         
                  }      
              }    
        }      
}      
//====================================GroupAllocate===================================================      
     
     
     
     
      
/*================================WriteTo_SQ_DATATABLE======================================================                         
  Secondary Function: WriteTo_SQ_DATATABLE                     
  ---------------------------------------------------------------                        
  Function will: Given a squadindex and two values, update the datatable      
  Inputs: SQUADID, TARGET, COMM, VECTOR      
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/      
void WriteTo_SQ_DATATABLE(int SQUADID, int TARGET, String COMM, Vector3D VECTOR)           
{          
SQ_COMMAND[SQUADID]  = COMM;      
SQ_TARGETID [SQUADID] = TARGET;     
SQ_TARGET[SQUADID] = VECTOR;     
}      
//================================WriteTo_SQ_DATATABLE===========================================      
     
     
     
/*================================GoToCommand======================================================                         
  Secondary Function: GoToCommand                    
  -----------------------------------------------------                        
  Function will: Run the standard formation and go-to command for a squadron         
  Inputs: i, TARGETPOS   (i = Squadron-index)     
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/        
void GoToCommand(int i, Vector3D TARGETPOS )           
{        
  //Running Commands For SQ-leader      
  //-----------------------------------------------     
  IMyProgrammableBlock LEADER = SH_BLOCKS[SQ_GROUPIDS[i][0]];       
  LEADER.TryRun("GoTo" + "#" + (TARGETPOS.ToString()) + "#" + (ExtractDirection("FORWARD", Me)).ToString()+   
 "#" + (LEADER.GetPosition()-Me.GetPosition()).Length().ToString() + "#" + "LEADER"  );     
                     
   //Getting leader information       
   //------------------------------------       
   var UP_DIRECT = ExtractDirection("UP", LEADER);                  //extract forward, left and up data       
   var LEFT_DIRECT = ExtractDirection("LEFT", LEADER);        
   var FORWARD_DIRECT = ExtractDirection("FORWARD", LEADER);       
   Vector3D LEADERPOS = LEADER.GetPosition();        
       
   //Generating Hangar Location + Sending coords about launch sequence  
   IMyTerminalBlock HANGAR_LOC = GridTerminalSystem.GetBlockWithName( 	i+"0" ) as IMyTerminalBlock;   
   if(HANGAR_LOC == null) {HANGAR_LOC = Me;}  
   if((LEADER.GetPosition()-HANGAR_LOC.GetPosition()).Length() < 40)  //terminates if prev is not clear of hangar       
   {return;}    
    
  //Applying Information for Squadron       
  //------------------------------------------------       
  for (int j = 1; j < SQ_GROUPIDS[i].Count; j++)                                                        //(j= individual ship)       
        {         
        var SHIP_TO_COMMAND =  SH_BLOCKS[SQ_GROUPIDS[i][j]];                     //(Line formation)       
        SHIP_TO_COMMAND.TryRun("GoTo" + "#" +        
        (LEADERPOS + Math.Pow(-1, j)*LEFT_DIRECT*j*15).ToString()       
        + "#" + FORWARD_DIRECT.ToString() + "#" + (SHIP_TO_COMMAND.GetPosition()-HANGAR_LOC.GetPosition()).Length().ToString() + "# false");       
            
        if((SHIP_TO_COMMAND.GetPosition()-HANGAR_LOC.GetPosition()).Length() < 40)  //terminates if prev is not clear of hangar      
        {return;}    
        }      
}      
//===================================GoToCommand====================================================       
     
    
    
/*================================DeleteDestroyed======================================================                         
  Secondary Function:DeleteDestroyed                   
  -----------------------------------------------------                        
  Function will: Remove any destroyed blocks from lists and such         
  Inputs: None Self Allocating        
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/        
void DeleteDestroyed()     
{     
for(int i = 0; i < SH_BLOCKS.Count; i++)             
        {      
        if(SH_BLOCKS[i].GetPosition() == (new Vector3D(0,0,0)))     
            {     
            var SQUAD_IND = SH_SQ_ID[i];     
            SQ_GROUPIDS[SQUAD_IND].Remove(i);     
            }     
        }     
}     
//=================================DeleteDestroyed==========================================           
     
    
/*====================================SensorScan===================================================                   
  High Level Function: SensorScan                  
  ----------------------------------------------                  
  function will: Collect every sensor on a grid and output any newly detected targets into a list of enemies                 
  Inputs: (Collects all automatically)          
  Outputs: EN_SIZE, EN_LOCATION, EN_GRID_ID                    
//-=--------------=-----------=-----------=-------------------=-------------------=----------------------=-----------------------=------------------*/         
void SensorScan()            
{            
//Adding New Sensors To List Of Known Sensors            
//-----------------------------------------------------------------            
List<IMyTerminalBlock> UNLOGGEDSENSORS = new List<IMyTerminalBlock>();            
GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(UNLOGGEDSENSORS);            
for (int i = 0; i < UNLOGGEDSENSORS.Count; i++)            
    {            
    IMySensorBlock TESTSENSOR = UNLOGGEDSENSORS[i] as IMySensorBlock;            
    if(SENSORS.Contains(TESTSENSOR) == false && TESTSENSOR.DisplayNameText.Contains("RFC") && TESTSENSOR.IsActive == false)            
        {            
        TESTSENSOR.ApplyAction("Detect Players_Off");           //Detecting any of these will result in bad things   
        TESTSENSOR.ApplyAction("Detect Friendly_Off");        
        TESTSENSOR.ApplyAction("Detect Enemy_On");    
        TESTSENSOR.ApplyAction("Detect Neutral_Off");    
        TESTSENSOR.ApplyAction("Detect Owner_Off");    
        TESTSENSOR.ApplyAction("Detect Asteroids_Off");    
        TESTSENSOR.ApplyAction("Detect Floating Objects_Off");    
        SENSORS.Add(TESTSENSOR);     
        TESTSENSOR.SetValue<Single>("Front", 1000);          
        return;            
        }            
    }             
            
//Scanning Through Sensors To Sense Grids            
//-----------------------------------------------------------------            
if(SENSORS != null)                                                                         // stops if no sensors in list                
            {                
                 for (int i = 0; i < SENSORS.Count; i++)                                      // for every sensor in list                
                 {               
                    IMySensorBlock CHECK_SENSOR = SENSORS[i] as IMySensorBlock;                
                    if ((CHECK_SENSOR.IsActive) == true)                
                    {                
                    var THIS_GRID = Me.CubeGrid as IMyCubeGrid;              
                    var TEST_GRID = CHECK_SENSOR.LastDetectedEntity as IMyCubeGrid;              
                    if ( THIS_GRID != TEST_GRID && 	 	EN_GRID_ID.Contains(TEST_GRID) == false && ((TEST_GRID.Min - TEST_GRID.Max).Length()) > 3  )          //if grid is not mothership  and is larger than 3m       
                        {                                      
                        	EN_GRID_ID.Add(TEST_GRID);                                          //Assigning new index in list     
                        EN_LOCATION.Add(new Vector3D());     
                        EN_SIZE.Add(0);            
                        }              
                    }                
                 }                   
            }                    
     
//Extracting Data Coordinates For Each Grid             
//---------------------------------------------------------            
for (int i = 0; i <  	EN_GRID_ID.Count; i++)                                      // for every detected grid  update en_men (i = en_id)   
                 {      
                  var DETECTED_GRID = EN_GRID_ID[i];            
                  EN_LOCATION[i] = Vector3D.Round(DETECTED_GRID.GridIntegerToWorld((DETECTED_GRID.Min + DETECTED_GRID.Max) / 2), 2);                  
                  EN_SIZE[i] = ((DETECTED_GRID.Min - DETECTED_GRID.Max).Length());          
                /*if(EN_SIZE[i] < 3)   
                      {         
                      EN_SIZE.Remove(EN_SIZE[i]);                           //Deletes any small or destroyed enemy grids    
                      EN_LOCATION.Remove(EN_LOCATION[i]);    
                      EN_GRID_ID.Remove(EN_GRID_ID[i]);         
                    }  */          
                }          
}               
//====================================SensorScan====================================================      
  