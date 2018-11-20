#region Introduction
  /*
  Introduction
  ----------------
  Hello and thank you for downloading Rdav's Fleet Command Beta
  Rdav's fleet command is a total-conversion code for automatic fleets.
  This code allows for artificially intelligent drone operation, drones
  adopt unique and intelligent behaviour allowing them to engage targets
  follow commands, dock, undock along with giving players advanced
  control capabilities for an entire amarda.
  Please see the workshop page for more details;

  You are currently looking at the 'Drone AI' Unit, which
  depending on the title contained within the customdata controls
  the behaviour and mentality of the drone

  Rdav 28/08/17

  Bughunting/Problems
  --------------------
  The code will automatically create a bugreport and save it to the custom-data of the
  designated remote block.

  Suggestions Planned Features
  -----------------------------
  - Let me know what you think needs adding to code!

   ChangeLog:
   * Antennae turns on 10/10/17
   * collision detection on large frigates adjusted 10/10/17
   * Allows for carrier on own grid

  */

  #endregion

  #region SETUP
  //STORED VARIABLES
  //----------------------------------------------------------------------------------------------------------------------

  //SUBCATEGORY PERMANENT ASSIGNMENTS:
  string VERSION = "Ver_006";                         //Script Current Version
  string SHIP_CLASS = "FRIGATE";                        //AI Type used for intergral commands
  DRONE_INFO MEINFO = new DRONE_INFO();
  bool[] FC_CENTRAL_RUN = new bool[] { false, true, true, false, false, true, false, false, false, true }; //Bools used for conditional operation
  int RUN_ASSIGNMENT;                                         //Used For broadcasting a request enable message
  string CONNECTOR_PLANE;                                     //Docking Direction
  double SHIP_ANGULAR_ACCEL = 0.6;                           //Ships angular acceleration
  double MAX_SPEED = 80;                                     //Maximum cruising Ship speed
  double SHIP_DOCK_ANGULAR_SPEED = 0.3;                      //Default minimum docking rotational velocity to begin docking sequence
  int INITIAL_HEALTH;
  int PROJECTILE_VELOCITY = 300;

  //SUBCATEGORY STORED BLOCKS
  IMyRemoteControl RC;
  IMyShipConnector CONNECTOR;
  IMyLargeTurretBase DIRECTOR;
  List<IMyLargeTurretBase> DIRECTORS = new List<IMyLargeTurretBase>();
  IMyRadioAntenna RADIO;
  IMyGyro GYRO;
  IMyTimerBlock HW_TIMER;
  List<IMyTerminalBlock> CONTROLLERS = new List<IMyTerminalBlock>();
  List<IMyTerminalBlock> CAMERAS = new List<IMyTerminalBlock>();
  List<IMyTerminalBlock> DIRECTIONAL_FIRE = new List<IMyTerminalBlock>();  //Directional ship weaponry

  //SUBCATEGORY CLASSES
  class DRONE_INFO
  {
      public string ID; //Drone Identifier (contains classification)
      public string COMLOC; //Drone Command & locator
      public string GLOC; //Generated Locator
      public Vector3D LOC; //Current Drone Location
      public Vector3D VEL; //Drone Velocity
      public Vector3D TVEl; //Target Velocity vector
      public string ISDOCKED; //id of docked ship (own Id if not docked)
      public double HEALTH; //Health of the ship
      public DateTime LAST_PING; //Last recieved ping from the ship
      public string EXT_INF;   // String Drone Extra Information
      public string OUTPUT;   // String Drone Data Output

      //Standardised System Of Updating And Saving Drone Data
      public static DRONE_INFO DRONE_DATA_RS(string IN_ARG, DRONE_INFO DRONE_INF, bool[] RUN_ID)
      {
          //Retrieves Data From Store
          string[] DRN_INFO = IN_ARG.Split('*');
          DRONE_INF.ID = (RUN_ID[0] != true) ? DRONE_INF.ID : DRN_INFO[0];
          DRONE_INF.COMLOC = (RUN_ID[1] != true) ? DRONE_INF.COMLOC : DRN_INFO[1];
          DRONE_INF.GLOC = (RUN_ID[2] != true) ? DRONE_INF.GLOC : DRN_INFO[2];
          if (RUN_ID[3] == true) { Vector3D.TryParse(DRN_INFO[3], out DRONE_INF.LOC); }
          if (RUN_ID[4] == true) { Vector3D.TryParse(DRN_INFO[4], out DRONE_INF.VEL); }
          if (RUN_ID[5] == true) { Vector3D.TryParse(DRN_INFO[5], out DRONE_INF.TVEl); }
          if (RUN_ID[6] == true) { DRONE_INF.ISDOCKED = DRN_INFO[6]; }
          if (RUN_ID[7] == true) { DRONE_INF.HEALTH = double.Parse(DRN_INFO[7]); }
          if (RUN_ID[8] == true) { DRONE_INF.LAST_PING = DateTime.Parse(DRN_INFO[8]); }
          if (RUN_ID[9] == true) { DRONE_INF.EXT_INF = DRN_INFO[9]; }
          return DRONE_INF;
      }

      public static DRONE_INFO SAVE(DRONE_INFO DRONE_INF)
      {
          DRONE_INF.OUTPUT = string.Join("*", "#" + DRONE_INF.ID, DRONE_INF.COMLOC, DRONE_INF.GLOC, DRONE_INF.LOC, DRONE_INF.VEL, DRONE_INF.TVEl, DRONE_INF.ISDOCKED, DRONE_INF.HEALTH, DRONE_INF.LAST_PING,DRONE_INF.EXT_INF, "#" + DRONE_INF.ID);
          return DRONE_INF;
      }
  }
  class DOCKPOINT_INFO
  {
      public string ID; //Dockpoint Identifier (contains docktype classification)
      public Vector3D LOC; //Current Dockpoint Location
      public string BASE_TAG; //ID of base ship
      public string ISDOCKED; //Id of docked ship (own Id if not docked)
      public DateTime LAST_PING; //Last recieved ping from the dockpoint
      public string OUTPUTROLL; //Coordinates package for drones to interperate
      public string OUTPUT;   // String Drone Data Output

      public List<IMyTerminalBlock> ROUTE; //List of route (drone ship only, not updated)

      //Standardised System Of Updating And Saving Drone Data
      public static DOCKPOINT_INFO DOCK_DATA_RS(string IN_ARG, DOCKPOINT_INFO DOCKPT_INF, bool[] RUN_ID)
      {
          //Retrieves Data From Store
          string[] DCK_INFO = IN_ARG.Split('*');
          if (RUN_ID[0] == true) { DOCKPT_INF.ID = DCK_INFO[0]; }
          if (RUN_ID[1] == true) { Vector3D.TryParse(DCK_INFO[1], out DOCKPT_INF.LOC);}
          if (RUN_ID[2] == true) { DOCKPT_INF.BASE_TAG = DCK_INFO[2];}
          if (RUN_ID[3] == true) { DOCKPT_INF.ISDOCKED = DCK_INFO[3];}
          if (RUN_ID[4] == true) { DOCKPT_INF.LAST_PING = DateTime.Parse(DCK_INFO[4]); }
          if (RUN_ID[5] == true) { DOCKPT_INF.OUTPUTROLL = DCK_INFO[5];}

          DOCKPT_INF.OUTPUT = string.Join("*", "#" + DOCKPT_INF.ID, DOCKPT_INF.LOC, DOCKPT_INF.BASE_TAG, DOCKPT_INF.ISDOCKED, DOCKPT_INF.LAST_PING, DOCKPT_INF.OUTPUTROLL, "#" + DOCKPT_INF.ID);
          return DOCKPT_INF;
      }

      //Standardised DockString Saving Procedure
      public static DOCKPOINT_INFO SAVE_ROUTE_TO_STRING(DOCKPOINT_INFO DOCKPT_INFO)
      {
          List<string> OUTPUT = new List<string>();
          double OFFSET_CONST = 4;
          List<IMyTerminalBlock> DOCKPT_TRAIL = DOCKPT_INFO.ROUTE;

          //Adds First Ordinates (self and forwards position)
          OUTPUT.Add(Vector3D.Round(DOCKPT_TRAIL[0].GetPosition() + DOCKPT_TRAIL[0].WorldMatrix.Forward * (1.5), 2)+"");
          OUTPUT.Add(Vector3D.Round(DOCKPT_TRAIL[0].GetPosition() + DOCKPT_TRAIL[0].WorldMatrix.Forward * OFFSET_CONST, 2)+"");

          //Iterates Through List Of LCD's
          for (int i = 1; i < DOCKPT_TRAIL.Count; i++)
          { var IMYPLACE = DOCKPT_TRAIL[i]; OUTPUT.Add(Vector3D.Round(IMYPLACE.GetPosition() + IMYPLACE.WorldMatrix.Backward * OFFSET_CONST, 2)+""); }

          //Adds Final Position
          OUTPUT.Add(Vector3D.Round(DOCKPT_TRAIL[DOCKPT_TRAIL.Count - 1].GetPosition() +
              DOCKPT_TRAIL[DOCKPT_TRAIL.Count - 1].WorldMatrix.Backward * OFFSET_CONST + DOCKPT_TRAIL[DOCKPT_TRAIL.Count-1].WorldMatrix.Up*100,2)+"");


          //Saves To String, Updates Locator, (And Updates OUTPUT)
          OUTPUT.Reverse();
          DOCKPT_INFO.OUTPUTROLL = string.Join("^", OUTPUT);
          DOCKPT_INFO.LOC = Vector3D.Round(DOCKPT_TRAIL[0].GetPosition(),2);
          DOCKPT_INFO.OUTPUT = string.Join("*", "#" + DOCKPT_INFO.ID, DOCKPT_INFO.LOC, DOCKPT_INFO.BASE_TAG, DOCKPT_INFO.ISDOCKED, DOCKPT_INFO.LAST_PING, DOCKPT_INFO.OUTPUTROLL, "#" + DOCKPT_INFO.ID);

          return DOCKPT_INFO;
      }

  }
  Dictionary<string, DOCKPOINT_INFO> DOCKPOINTS = new Dictionary<string, DOCKPOINT_INFO>();

  //SUBCATEGORY TEMPORARY ASSIGNMENTS:
  bool FIRSTSETUP_HASRUN = false;                             //Whether Or Not Code has Been Setup
  List<Vector3D> COORDINATES = new List<Vector3D>();          //A list of the ships current coordinates
  int COORD_ID = 0;                                           //Waypoint Mode Counter
  bool ENEMY_DETECTED;                                        //Currently Tracking an Enemy

  //SUBCATEGORY DOCKING SETUP
  bool DOCKING_INIT;

  #endregion

  //Primary Operators

  #region MAIN METHOD (unfinished)
  /*====================================================================================================================
  Function: MAIN METHOD
  ---------------------------------------------
  function will: Main Method, Timing Calculator And Diagnostics
  Performance Cost:
 //======================================================================================================================*/
  void Main(string argument)
  {
      try
      {
          //Only Recieves Calls From The Hub
          //---------------------------------------
          if (argument.Contains("BRDCST") == false)
          { return; }

          //System Error Readouts And Diag
          //---------------------------------------
          #region Error Readouts
          OP_BAR();
          Echo(VERSION);
          if (RC == null || RC.CubeGrid.GetCubeBlock(RC.Position) == null)
          { FIRSTSETUP_HASRUN = false; Echo("No RFC_RC Found"); RC = null; }
          if (CAMERAS.Count == 0)
          { FIRSTSETUP_HASRUN = false; Echo("No CAMERAS Found"); }
          if (CONNECTOR == null || CONNECTOR.CubeGrid.GetCubeBlock(CONNECTOR.Position) == null)
          { FIRSTSETUP_HASRUN = false; Echo("No RFC_CONNECTOR Found"); CONNECTOR = null; }
          if (DIRECTOR == null || DIRECTOR.CubeGrid.GetCubeBlock(DIRECTOR.Position) == null)
          { FIRSTSETUP_HASRUN = false; Echo("No RFC_TURRET Found"); DIRECTOR = null; }
          if (RADIO == null || RADIO.CubeGrid.GetCubeBlock(RADIO.Position) == null)
          { FIRSTSETUP_HASRUN = false; Echo("No RADIO  Found"); RADIO = null; }
          if (GYRO == null || GYRO.CubeGrid.GetCubeBlock(GYRO.Position) == null)
          { FIRSTSETUP_HASRUN = false; Echo("No RFC_GYRO  Found"); GYRO = null; }
          #endregion

          //System Initialisation
          //---------------------------------------
          if (FIRSTSETUP_HASRUN == false)
          { FIRST_TIME_SETUP(); FIRSTSETUP_HASRUN = true; Echo("System Booting"); return; }

          //System Filters For Outputting && Running
          //---------------------------------------
          if ((double)DateTime.Now.Second / RUN_ASSIGNMENT % 1 == 0 && !argument.Contains("#" + MEINFO.ID)) // every random seconds
          { MEINFO.LAST_PING = DateTime.Now; MEINFO = DRONE_INFO.SAVE(MEINFO); RADIO.TransmitMessage(MEINFO.OUTPUT, MyTransmitTarget.Owned); }
          if (!argument.Contains("#" + MEINFO.ID))
          { Echo("nocall"); return; }
          int START_POS = argument.IndexOf("#" + MEINFO.ID);
          int ENDPOS = argument.LastIndexOf("#" + MEINFO.ID);
          MEINFO = DRONE_INFO.DRONE_DATA_RS(argument.Substring(START_POS + 1, ENDPOS - START_POS), MEINFO, FC_CENTRAL_RUN);
          if (MEINFO.COMLOC.Contains("DOCK") == false)
          { COORD_ID = 0; }   //Resets COORD Iter if not dock or undock command

          //Runs System Primary Logic
          //---------------------------------------
          //Handles Carrier On Own Grid P1
          var CENTRAL_COMMAND = GridTerminalSystem.GetBlockWithName("CENTRAL_COMMAND") as IMyProgrammableBlock;
          bool ALL_RUN = true;
          foreach (var item in CONTROLLERS)
          { if ((item as IMyShipController).IsUnderControl || CENTRAL_COMMAND != null) { ALL_RUN = false; } }
          DRONE_PRI_LOGIC_ST1();
          if (ALL_RUN)
          { DRONE_PRI_LOGIC_ST2(); }

          //Runs Docking System
          //---------------------------------------
          string DOCKOUTPUT = "";
          if (SHIP_CLASS.Contains("IN") == false && SHIP_CLASS.Contains("IB") == false)
          { DOCKOUTPUT = DOCK_SYST(); }

          //System Logic Update And Send
          //----------------------------------
          Me.CustomData = COORD_ID+"#"+SHIP_CLASS;
          MEINFO.HEALTH = Math.Round(((int)((Me.CubeGrid.Max - Me.CubeGrid.Min).Length()))+((Me.CubeGrid.Max - Me.CubeGrid.Min).Length() - 0.1 / INITIAL_HEALTH),4);
          MEINFO = DRONE_INFO.SAVE(MEINFO);
          string DEI_OUT ="";
          if (SCAN_HARDLOCK)
          { DEI_OUT = "#" + "DEI" + SCAN_LOCKED_SHIP.ID + "^" + (int)SCAN_LOCKED_SHIP.SIZE + "^" + "EN" + "^" + Vector3D.Round(SCAN_LOCKED_SHIP.POSITION,2); }
          RADIO.TransmitMessage(MEINFO.OUTPUT + DOCKOUTPUT + DEI_OUT, MyTransmitTarget.Owned);
          Echo(MEINFO.OUTPUT + DOCKOUTPUT + DEI_OUT);

          //Handles Carrier On Own Grid P2
          if (CENTRAL_COMMAND != null) { CENTRAL_COMMAND.TryRun((MEINFO.OUTPUT + DOCKOUTPUT + DEI_OUT)+""); }
      }
      catch (Exception e)
      { Echo(e+""); }
  }

  #endregion

  #region First Time Setup #RFC#
  /*====================================================================================================================
  Function: FIRST_TIME_SETUP
  ---------------------------------------
  function will: Initiates Systems and initiasing Readouts to LCD
  Performance Cost:
 //======================================================================================================================*/
  void FIRST_TIME_SETUP()
  {
      //Gathers Key Components
      //-----------------------------------

      //Welding Test
      try
      {
          var TESTBOOL = GridTerminalSystem.GetBlockWithName("RFC_CONSTRUCT").CubeGrid == Me.CubeGrid;
          Echo("Ship Still Under Construction, Detatch To Init");
          return;
      }
      catch { } //only allows through init on exception

      //Updates Type
      try
      {
          SHIP_CLASS = Me.CustomData.Split('#')[1];
      }
      catch { }

      //GathersConnector
      try
      {
          List<IMyTerminalBlock> TEMP_CON = new List<IMyTerminalBlock>();
          GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(TEMP_CON, b => b.CubeGrid == Me.CubeGrid);
          CONNECTOR = TEMP_CON[0] as IMyShipConnector;
      }
      catch { }

      //GathersSystemCameras
      try
      {
          GridTerminalSystem.GetBlocksOfType<IMyCameraBlock>(CAMERAS); //, b => b.CubeGrid == Me.CubeGrid
          foreach (var item in CAMERAS)
          { (item as IMyCameraBlock).EnableRaycast = true; }
      }
      catch { }

      //Gathers Remote Control
      try
      {
          List<IMyTerminalBlock> TEMP_RC = new List<IMyTerminalBlock>();
          GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(TEMP_RC, b => b.CubeGrid == Me.CubeGrid && b.CustomName == "RFC_RC");
          RC = TEMP_RC[0] as IMyRemoteControl;
      }
      catch { }

      //Gathers Director Turret
      try
      {
          List<IMyTerminalBlock> TEMP_TUR = new List<IMyTerminalBlock>();
          GridTerminalSystem.GetBlocksOfType<IMyLargeTurretBase>(DIRECTORS, b => b.CubeGrid == Me.CubeGrid);
          DIRECTOR = DIRECTORS[0] as IMyLargeTurretBase;
      }
      catch { }

      //Gathers Antennae
      try
      {
          List<IMyTerminalBlock> TEMP = new List<IMyTerminalBlock>();
          GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(TEMP, b => b.CubeGrid == Me.CubeGrid && b.CustomData.Contains("RFC_ANT"));
          RADIO = TEMP[0] as IMyRadioAntenna;
          RADIO.Enabled = true;
      }
      catch { }

      //Sets Gyro
      try
      {
          List<IMyTerminalBlock> TEMP_GYRO = new List<IMyTerminalBlock>();
          GridTerminalSystem.GetBlocksOfType<IMyGyro>(TEMP_GYRO, b => b.CubeGrid == Me.CubeGrid && b.CustomName == "RFC_GYRO");
          GYRO = TEMP_GYRO[0] as IMyGyro;
      }
      catch { }

      //GathersControllers
      try
      {
          GridTerminalSystem.GetBlocksOfType<IMyShipController>(CONTROLLERS, b => b.CubeGrid == Me.CubeGrid);
      }
      catch { }

      //Gathers Timer
      if (SHIP_CLASS == "FMISSILE" || SHIP_CLASS == "FGRAV")
      {
          try
          {
             HW_TIMER =  GridTerminalSystem.GetBlockWithName("HW_TIMER") as IMyTimerBlock;
          }
          catch { }
      }

      //Finishes Setup & Sets ID
      //-----------------------------------
      Random rnd = new Random();              //Declares And Assigns permanent unique ID
      MEINFO.ID = SHIP_CLASS.Substring(0, 2) + Me.CubeGrid.EntityId.ToString().Substring(0, 4);
      MEINFO.ISDOCKED = MEINFO.ID;
      try
      { COORD_ID = int.Parse(Me.CustomData.Split('#')[0]); } //Sets Coord ID
      catch
      { COORD_ID = 0; }
      RADIO.CustomName = (MEINFO.ID + "");
      RUN_ASSIGNMENT = rnd.Next(3, 7);
      RETURN_CONNECTOR_DIRECTION();       //Runs Connector Setup Systems
      GridTerminalSystem.GetBlocksOfType<IMyUserControllableGun>(DIRECTIONAL_FIRE, (block => block.GetType().Name == "MySmallMissileLauncher" || block.GetType().Name == "MySmallGatlingGun" || block.GetType().Name == "MySmallMissileLauncherReload")); //Collects the directional weaponry (in a group)

      //Runs Thruster Setup
      COLLECT_AND_FIRE(new Vector3D(), 0, 0, RC.GetPosition());
      for (int j = 0; j < CAF2_THRUST.Count; j++)
      { CAF2_THRUST[j].SetValue<float>("Override", 0.0f); CAF2_THRUST[j].ApplyAction("OnOff_On"); }
      INITIAL_HEALTH = (int)((Me.CubeGrid.Max - Me.CubeGrid.Min)).Length();

      //Docking Status Enable (if it comes pre docked)
      MEINFO.COMLOC = "GOTO^" + Vector3D.Round(Me.GetPosition(),2);
      if (CONNECTOR.Status == MyShipConnectorStatus.Connected && MEINFO.ID.Substring(0,1)== "I") //if interceptor preallocate to dock
      { MEINFO.ISDOCKED = "DK" + CONNECTOR.OtherConnector.EntityId.ToString().Substring(0, 4); MEINFO.COMLOC = "DOCK^" + "DK" + CONNECTOR.OtherConnector.EntityId.ToString().Substring(0, 4); }

  }
  //----------==--------=------------=-----------=---------------=------------=-------==--------=------------=-----------=----------
  #endregion

  #region RFC Primary Logic st1 #RFC#
  /*=================================================
    Function: RFC Primary Logic #RFC#
    ---------------------------------------     */
  void DRONE_PRI_LOGIC_ST1()
  {

      //Update Subroutine
      //===================================================

      //Resets Overrides
      //--------------------------------------
      //Resetting Gyros Specifically
      GYRO.SetValue<float>("Roll", 0);
      GYRO.SetValue<float>("Yaw", 0);
      GYRO.SetValue<float>("Pitch", 0);
      GYRO.SetValue("Override", false);
      RC.SetAutoPilotEnabled(false);

      //Desetting Thrusters
      for (int j = 0; j < CAF2_THRUST.Count; j++)
      { CAF2_THRUST[j].SetValue<float>("Override", 0.0f); CAF2_THRUST[j].ApplyAction("OnOff_On"); }

      //Sets Information Systems
      //---------------------------
      MEINFO.LOC = Vector3D.Round(Me.GetPosition(), 2);
      MEINFO.VEL = Vector3D.Round(RC.GetShipVelocities().LinearVelocity+(RC.WorldMatrix.Forward * 2));
      MEINFO.HEALTH = 0.9; // (NEEDS ADJUSTING)
      MEINFO.LAST_PING = DateTime.Now;
      if (MEINFO.EXT_INF == "1") //Sets Radio For Unit Selection
      { RADIO.CustomName = MEINFO.ID; }
      else
      { RADIO.CustomName = ""; }

      //Docking Turnoff
      //-------------------------------
      if (MEINFO.COMLOC.Contains("DOCK")) { CONNECTOR.Connect(); CONNECTOR.ApplyAction("OnOff_On"); }
      if (MEINFO.COMLOC.Contains("UNDOCK")) { CONNECTOR.Disconnect(); CONNECTOR.ApplyAction("OnOff_Off"); }
      if (CONNECTOR.Status == MyShipConnectorStatus.Connected && MEINFO.COMLOC.Contains("DOCK"))
      {
          for (int j = 0; j < CAF2_THRUST.Count; j++)
          { CAF2_THRUST[j].SetValue<float>("Override", 0.0f); CAF2_THRUST[j].ApplyAction("OnOff_Off"); }
          GYRO.SetValue("Override", false);
          return;
      }

      //Updates Coordinates In
      //--------------------------
      COORDINATES = new List<Vector3D>();
      for (int i = 0; i < MEINFO.GLOC.Split('^').Length; i++)             //for loop to add coordinates
      {
          Vector3D TEMP_COORD;
          Vector3D.TryParse(MEINFO.GLOC.Split('^')[i], out TEMP_COORD);
          COORDINATES.Add(TEMP_COORD);
      }

      //Updates Director
      //----------------------------
      RC.ApplyAction("AutoPilot_Off");                   //RC toggle
      RC.DampenersOverride = true;

      //Updates Target Detection
      //-----------------------------}
      SCAN_MANAGER(out ENEMY_DETECTED);

      //Desets Primary Weaponry
      //------------------------------
      for (int i = 0; i < DIRECTIONAL_FIRE.Count; i++)
      { DIRECTIONAL_FIRE[i].ApplyAction("Shoot_Off"); }

  }
  #endregion

  #region RFC Primary Logic st2 #RFC#
  /*=================================================
    Function: RFC Primary Logic #RFC#
    ---------------------------------------     */
  void DRONE_PRI_LOGIC_ST2()
  {

      //Runtime Subroutine
      //===================================================

      //Specialist UnDock Command
      //------------------------------
      if (MEINFO.COMLOC.Contains("UNDOCK") && COORDINATES.Count > 1)
      { DOCK_ITER("UDOCK"); return; }

      //Specialist Dock Command
      //---------------------------
      if (MEINFO.COMLOC.Contains("DOCK") && COORDINATES.Count > 1)
      { DOCK_ITER("DOCK"); return; }

      //Attack Command Routine
      //--------------------------------------
      if (MEINFO.COMLOC.Contains("ATTACK") || ENEMY_DETECTED && ((COORDINATES[0] - SCAN_LOCKED_SHIP.POSITION).Length() < 1600)) //close to- or if
      {
          //Function Specific Inputs
          //--------------------------------------
          Echo("System Engaging");
          switch (SHIP_CLASS)
          {
              case "INTERCEPTOR":
                  //Runs INTERCEPTOR AI
                  Attack_Location_Interceptor(); return;
              case "IBOMBER":
                  //Runs Bomber AI
                  Attack_Location_Interceptor(); return; //currently moot
              case "FRIGATE":
                  //Runs Frigate AI
                  Attack_Location_Frigate(); return;
              case "CRUISER":
                  //Runs Cruisers AI
                  Attack_Location_Cruiser(); return;
              case "FMISSILE":
                  //Runs Heavy AI
                  Attack_Location_Heavy(); return;
              case "FGRAV":
                  //Runs Heavy AI
                  Attack_Location_Heavy(); return;
          }
      }

      //Standard GOTO Command
      //--------------------------------------
      if (MEINFO.COMLOC.Contains("GOTO") && (COORDINATES[0]-RC.GetPosition()).Length() > 20) //stops floaty boatyness
      {
          Echo("Going To Target");
          GetFreeDestination(COORDINATES[0]);
          //DOGPILOT(GFD_FREE_DESTINATION, MEINFO.TVEl.Length(), false);
          //RC.SetValue<Single>("SpeedLimit", (float)MAX_SPEED); RC_MANAGER(COORDINATES[0]);
          COLLECT_AND_FIRE(GFD_FREE_DESTINATION, 0, MAX_SPEED, RC.GetPosition());
          GyroTurn4(COORDINATES[0], GYRO, 1, new Vector3D(), "FORWARD");
          return;
      }  //Fly Straight Mode max speed limited

      //Standard FOLLOW Command
      //--------------------------------------
      if (MEINFO.COMLOC.Contains("FOLLOW"))
      {
          Echo("Following Target");
          RC.SetValue<Single>("SpeedLimit", ((float)MAX_SPEED+20));  //Sets Max Speed as standard +20

          //Sets Autopilot/Dogpilot According To Follow Conditions
          if ((COORDINATES[0] - RC.GetPosition()).Length() < 100 && (COORDINATES[0] - RC.GetPosition()).Length() > 5)
          {
              COLLECT_AND_FIRE(COORDINATES[0], MEINFO.TVEl.Length(), MAX_SPEED, RC.GetPosition());
              GyroTurn4(COORDINATES[0] + Vector3D.Normalize(MEINFO.TVEl) * 300, GYRO, 2, new Vector3D(), "FORWARD");
          }
          else if ((COORDINATES[0] - RC.GetPosition()).Length() > 5)
          {
              if (!Me.CubeGrid.ToString().Contains("Large")) //Dogpilot for smaller ships
              { GetFreeDestination(COORDINATES[0]); DOGPILOT(GFD_FREE_DESTINATION, MEINFO.TVEl.Length(), false); }
              else
              {
                  RC_MANAGER(COORDINATES[0]);
                  //GetFreeDestination(COORDINATES[0]);
                  //COLLECT_AND_FIRE(GFD_FREE_DESTINATION, MEINFO.TVEl.Length(), MAX_SPEED, RC.GetPosition());
                  //GyroTurn4(COORDINATES[0] + Vector3D.Normalize(MEINFO.TVEl) * 300, GYRO, 2, new Vector3D(), "FORWARD");
              }
          } //Remote control for larger
      }

  }
  #endregion

  //Ship Logic Systems

  #region RC_MANAGER #RFC#
  /*======================================================================================================================================
    Function: RC_MANAGER
    ---------------------------------------
    Function will: Manage RC to make it do what you want, and when you want it. Needs desetting
    Inputs: GYRO, TARGETPOSITION, MULTIPLIER, nullable ROLLVECTOR
  //======================================================================================================================================*/
  void RC_MANAGER(Vector3D TARGETPOSITION)
  {
      //Assign To RC, Clear And Refresh Command
      //--------------------------------------------
      RC.ClearWaypoints();
      RC.AddWaypoint(TARGETPOSITION, "1");
      RC.AddWaypoint(TARGETPOSITION, "cc1");
      RC.ApplyAction("AutoPilot_On");                   //RC toggle
      RC.ApplyAction("DockingMode_Off");                //Precision Mode
      RC.ApplyAction("Forward");                        //Forward Direction
      RC.ApplyAction("CollisionAvoidance_On");          //Col avoidance
  }
  //----------==--------=------------=-----------=---------------=------------=-------==--------=------------=-----------=-
  #endregion

  #region GetPredictedPosition2 #RFC#
  /*=============================================================================================================
    Function: GetPredictedPosition2
    ---------------------------------------
    function will: A secondary modified verison of Keen-code, a bit more effective, (I'm too lazy to write my own)
    Inputs:
  //=============================================================================================================*/
  Vector3D GetPredictedTargetPosition2(IMyRemoteControl shooter, DEC_INFO target)
  {
      Vector3D predictedPosition = target.POSITION;
      Vector3D dirToTarget = Vector3D.Normalize(predictedPosition - shooter.GetPosition());

      //Standardised for gatling fire
      float shotSpeed = PROJECTILE_VELOCITY;

      //Run Setup Calculations
      Vector3 targetVelocity = target.VELOCITY;
      targetVelocity -= shooter.GetShipVelocities().LinearVelocity;
      Vector3 targetVelOrth = Vector3.Dot(targetVelocity, dirToTarget) * dirToTarget;
      Vector3 targetVelTang = targetVelocity - targetVelOrth;
      Vector3 shotVelTang = targetVelTang;
      float shotVelSpeed = shotVelTang.Length();

      if (shotVelSpeed > shotSpeed)
      {
          // Shot is too slow
          return Vector3.Normalize(target.VELOCITY) * shotSpeed;
      }
      else
      {
          // Run Calculations
          float shotSpeedOrth = (float)Math.Sqrt(shotSpeed * shotSpeed - shotVelSpeed * shotVelSpeed);
          Vector3 shotVelOrth = dirToTarget * shotSpeedOrth;
          float timeDiff = shotVelOrth.Length() - targetVelOrth.Length();
          var timeToCollision = timeDiff != 0 ? ((shooter.GetPosition() - target.POSITION).Length()) / timeDiff : 0;
          Vector3 shotVel = shotVelOrth + shotVelTang;
          predictedPosition = timeToCollision > 0.01f ? shooter.GetPosition() + (Vector3D)shotVel * timeToCollision : predictedPosition;
          return predictedPosition;
      }
  }
  //----------==--------=------------=-----------=---------------=------------=-----------=-------=----------------*/
  #endregion

  #region DOCK_ITERATOR
  /*====================================================================================================================================
  Secondary Function: DOCK_ITERATOR
  -----------------------------------------------------
  Function will: Operate docking & undocking sequences for ships
  //-=--------------=-----------=-----------=-------------------=-------------------=----------------------=----------------------------*/
  void DOCK_ITER(string DUD)
  {
      //Logic Check To Check Coords Are Within Limits
      if (COORDINATES.Count < 3) { return; }

      //Changes Iterer Based on Requirement
      int ITER_CURRENT = 0;
      int ITER_PREV = 0;
      int ITERER = 0;
      if (DUD == "DOCK")
      { ITER_CURRENT = 1; ITER_PREV = 0; ITERER = +1; }
      if (DUD == "UDOCK")
      { ITER_CURRENT = 0; ITER_PREV = 1; ITERER = -1; }

      //Setting Up a Few Constants
      Vector3D ROLL_ORIENTER = Vector3D.Normalize(COORDINATES[COORDINATES.Count - 1] - COORDINATES[COORDINATES.Count - 2]);

      //Running for takeoff/landing
      if (COORD_ID == COORDINATES.Count - 1)
      {
          Vector3D DOCKING_HEADING = Vector3D.Normalize(COORDINATES[COORDINATES.Count - 3] - COORDINATES[COORDINATES.Count - 2]) * 9000000; //Heading
          GyroTurn4(DOCKING_HEADING, GYRO, 2, ROLL_ORIENTER, CONNECTOR_PLANE); //Turn to heading
          if ((RC.GetShipVelocities().AngularVelocity).Length() < SHIP_DOCK_ANGULAR_SPEED) //Error check for small rotational velocity
          { VECTOR_THRUST_MANAGER(COORDINATES[COORD_ID - ITER_CURRENT], COORDINATES[COORD_ID - ITER_PREV], CONNECTOR.GetPosition(), 5, 0.7); }  //Thrusts to point
      }
      else
      {
          if (COORD_ID == 0)
          { DOGPILOT(COORDINATES[0],0,false); }  //Standard Auto for first location Ends To Prevent Out Of Range Exception
          else
          {
              var HEADING = Vector3D.Normalize(COORDINATES[COORD_ID - ITER_PREV] - COORDINATES[COORD_ID - ITER_CURRENT]) * 9000000;
              VECTOR_THRUST_MANAGER(COORDINATES[COORD_ID - ITER_CURRENT], COORDINATES[COORD_ID - ITER_PREV], CONNECTOR.GetPosition(), 5, 1); //Runs docking sequence
              GyroTurn4(HEADING, GYRO, 2, ROLL_ORIENTER, CONNECTOR_PLANE);
          }
      }

      //Logic checks and statement to prevent Errors
      if (DUD == "UDOCK" && COORD_ID == 0) { return; };
      if ((CONNECTOR.GetPosition() - COORDINATES[COORD_ID - ITER_PREV]).Length() < 1 || ((RC.GetPosition() - COORDINATES[COORD_ID - ITER_PREV]).Length() < 10 && COORD_ID == 0))
      {
          COORD_ID = COORD_ID + ITERER;
          if (COORD_ID == COORDINATES.Count)
          { COORD_ID = COORDINATES.Count - 1; }
          if (COORD_ID < 0)
          { COORD_ID = 0; }
      }

      //Logic Restraints On Command
      MathHelper.Clamp(COORD_ID, 0, COORDINATES.Count - 1);

  }
  //----------==--------=------------=-----------=---------------=------------=-------==--------=------------=-----------=----------

  #endregion

  //Advanced Ship Control Mechanisms

  #region GyroTurn4 #RFC#
  /*======================================================================================================================================
    Function: GyroTurn4
    ---------------------------------------
    Function will: rotate a gyro using overrides to point to a target with damping (forward of gyro is forward)
                   gyroturn3 is the latest edition of gyroturn, capable of being used for greater Timeframes and slower refresh speeds.
                   gyroturn will also permit gyroroll using an upward facing alignment vector.
    Inputs: GYRO, TARGETPOSITION, MULTIPLIER, nullable ROLLVECTOR
  //======================================================================================================================================*/
  double PREV_MISSILEELEVATION;
  double PREV_MISSILEAZIMUTH;
  double PREV_MISSILEROLL = 0;
  void GyroTurn4(Vector3D TARGETPOSITION, IMyGyro GYRO, double MULTIPLIER, Vector3D ROLLVECTOR, string ROLLORIENT)
  {
      //Get forward  + up unit vector (compressed function)
      //-----------------------------------------------------------------------
      Vector3D ORIGINPOS = RC.GetPosition();
      var FORWARDPOS = RC.Position + Base6Directions.GetIntVector(RC.Orientation.TransformDirection(Base6Directions.Direction.Forward)); var FORWARD = RC.CubeGrid.GridIntegerToWorld(FORWARDPOS);
      var FORWARDVECTOR = Vector3D.Normalize(FORWARD - ORIGINPOS);
      var UPPOS = RC.Position + Base6Directions.GetIntVector(RC.Orientation.TransformDirection(Base6Directions.Direction.Up)); var UP = RC.CubeGrid.GridIntegerToWorld(UPPOS);
      var UPVECTOR = Vector3D.Normalize(UP - ORIGINPOS);

      //Create inverse quaternion + translate forward direction from forward + up
      //--------------------------------------------------------------------------------
      Quaternion QUAT_TWO = Quaternion.CreateFromForwardUp(FORWARDVECTOR, UPVECTOR);
      var INVQUAT = Quaternion.Inverse(QUAT_TWO);
      Vector3D DIRECTIONVECTOR = Vector3D.Normalize(TARGETPOSITION - ORIGINPOS);
      Vector3D AZ_EL_VECTOR = Vector3D.Transform(DIRECTIONVECTOR, INVQUAT);

      //Converting To Azimuth And Elevation + Gyro Overrrides
      //--------------------------------------------------------
      double MISSILEAZIMUTH = 0;
      double MISSILEELEVATION = 0;
      Vector3D.GetAzimuthAndElevation(AZ_EL_VECTOR, out  MISSILEAZIMUTH, out MISSILEELEVATION);
      //Rdav  RFC 2016
      double ROT_VEL = RC.GetShipVelocities().AngularVelocity.Length();
      PREV_MISSILEELEVATION = MISSILEELEVATION;
      PREV_MISSILEAZIMUTH = MISSILEAZIMUTH;

      double AZ_ST_DIST = (ROT_VEL * ROT_VEL) / (2 * SHIP_ANGULAR_ACCEL);
      double EL_ST_DIST = (ROT_VEL * ROT_VEL) / (2 * SHIP_ANGULAR_ACCEL);
      if (Math.Abs(MISSILEAZIMUTH) > AZ_ST_DIST)
      { GYRO.SetValue<float>("Yaw", (float)(((MISSILEAZIMUTH) * MULTIPLIER)) * -1); }
      else
      { GYRO.SetValue<float>("Yaw", (float)0); }

      if (Math.Abs(MISSILEELEVATION) > EL_ST_DIST)
      { GYRO.SetValue<float>("Pitch", (float)(((MISSILEELEVATION) * MULTIPLIER))); }
      else
      { GYRO.SetValue<float>("Pitch", (float)0); }
      GYRO.SetValue("Override", true);

      //Uses GyroRoll Function To Roll Ship Towards Heading
      //-------------------------------------------------------
      if (ROLLVECTOR != null)
      {
          float ROLLANGLE = (float)(0.6 * (Vector3D.Dot(ROLLVECTOR, Vector3D.Normalize(ExtractDirection(ROLLORIENT, RC)))));
          PREV_MISSILEROLL = ROLLANGLE;
          GYRO.SetValue<float>("Roll", (ROLLANGLE));
      }
      else
      { GYRO.SetValue<float>("Roll", (float)0); }

      //Calculatory Error Readouts
      //-------------------------------------------
      if (Me.DisplayNameText == "DIAGNOSTICS")
      {
          Echo("\n~Running GYT~");
          Echo(Math.Round(MISSILEAZIMUTH, 3) + " Azimuth");
          Echo(Math.Round(MISSILEELEVATION, 3) + " Elevation");
          Echo(Math.Round(ROT_VEL, 3) + " Rot Vel");
          Echo(Math.Round(AZ_ST_DIST, 3) + " St Dist");
      }
  }
  //----------==--------=------------=-----------=---------------=------------=-------==--------=------------=-----------=-
  #endregion

  #region COLLECT_AND_FIRE #RFC#
  /*=======================================================================================
    Function: COLLECT_AND_FIRE
    ---------------------------------------
    function will: Collect thrust pointing in a direction inputted and fire said thrust
                   towards that point, remember to deset if not called.
    Inputs: Vector3D INPUT_POINT, double INPUT_VELOCITY,double INPUT_MAX_VELOCITY
    Requires: none
  //----------==--------=------------=-----------=---------------=------------=-----=-----*/
  class Thrust_info                   //Basic Information For Axial Thrust
  {
      public double PositiveMaxForce;
      public double NegativeMaxForce;
      public List<IMyThrust> PositiveThrusters;
      public List<IMyThrust> NegativeThrusters;
      public double VCF;
      public Thrust_info(Vector3D DIRECT, IMyGridTerminalSystem GTS, IMyCubeGrid MEGRID)
      {
          PositiveThrusters = new List<IMyThrust>(); NegativeThrusters = new List<IMyThrust>();
          List<IMyTerminalBlock> TEMP_RC = new List<IMyTerminalBlock>();
          GTS.GetBlocksOfType<IMyThrust>(PositiveThrusters, block => Vector3D.Dot(-1 * block.WorldMatrix.Forward, DIRECT) > 0.7 && block.CubeGrid == MEGRID);
          GTS.GetBlocksOfType<IMyThrust>(NegativeThrusters, block => Vector3D.Dot(block.WorldMatrix.Forward, DIRECT) > 0.7 && block.CubeGrid == MEGRID);
          double POWER_COUNT = 0;
          foreach (var item in PositiveThrusters)
          { POWER_COUNT = POWER_COUNT + item.MaxEffectiveThrust; }
          PositiveMaxForce = POWER_COUNT;
          POWER_COUNT = 0;
          foreach (var item in NegativeThrusters)
          { POWER_COUNT = POWER_COUNT + item.MaxEffectiveThrust; }
          NegativeMaxForce = POWER_COUNT;
      }
  }
  Thrust_info CAF2_FORWARD;
  Thrust_info CAF2_UP;
  Thrust_info CAF2_RIGHT;
  List<Thrust_info> CAFTHI = new List<Thrust_info>();

  List<IMyTerminalBlock> CAF2_THRUST = new List<IMyTerminalBlock>();
  bool C_A_F_HASRUN = false;
  double CAF2_BRAKING_COUNT = 99999999;

  double CAF_SHIP_DECELLERATION;                        //Outputs current decelleration
  double CAF_STOPPING_DIST;                             //Outputs current stopping distance
  double CAF_DIST_TO_TARGET;                            //Outputs distance to target

  void COLLECT_AND_FIRE(Vector3D INPUT_POINT, double INPUT_VELOCITY, double INPUT_MAX_VELOCITY, Vector3D REFPOS)
  {
      //Function Initialisation
      //--------------------------------------------------------------------
      if (C_A_F_HASRUN == false)
      {
          //Initialise Classes And Basic System
          CAF2_FORWARD = new Thrust_info(RC.WorldMatrix.Forward, GridTerminalSystem, Me.CubeGrid);
          CAF2_UP = new Thrust_info(RC.WorldMatrix.Up, GridTerminalSystem, Me.CubeGrid);
          CAF2_RIGHT = new Thrust_info(RC.WorldMatrix.Right, GridTerminalSystem, Me.CubeGrid);
          CAFTHI = new List<Thrust_info>() { CAF2_FORWARD, CAF2_UP, CAF2_RIGHT };
          GridTerminalSystem.GetBlocksOfType<IMyThrust>(CAF2_THRUST, block => block.CubeGrid == Me.CubeGrid);
          C_A_F_HASRUN = true;

          //Initialises Braking Component
          foreach (var item in CAFTHI)
          {
              CAF2_BRAKING_COUNT = (item.PositiveMaxForce < CAF2_BRAKING_COUNT) ? item.PositiveMaxForce : CAF2_BRAKING_COUNT;
              CAF2_BRAKING_COUNT = (item.NegativeMaxForce < CAF2_BRAKING_COUNT) ? item.PositiveMaxForce : CAF2_BRAKING_COUNT;
          }
      }

      //Generating Maths To Point and decelleration information etc.
      //--------------------------------------------------------------------
      double SHIPMASS = Convert.ToDouble(RC.CalculateShipMass().PhysicalMass);
      Vector3D INPUT_VECTOR = Vector3D.Normalize(INPUT_POINT - REFPOS);
      double VELOCITY = RC.GetShipSpeed();
      CAF_DIST_TO_TARGET = (REFPOS - INPUT_POINT).Length();
      CAF_SHIP_DECELLERATION = 0.75 * (CAF2_BRAKING_COUNT / SHIPMASS);
      CAF_STOPPING_DIST = (((VELOCITY * VELOCITY) - (INPUT_VELOCITY * INPUT_VELOCITY))) / (2 * CAF_SHIP_DECELLERATION);

      //Calculatory Error Readouts
      //-------------------------------------------
      if (Me.DisplayNameText == "DIAGNOSTICS")
      {
          Echo("\n~Running CAF~");
          Echo(Math.Round(VELOCITY, 2) + " Vel");
          Echo(Math.Round(CAF_SHIP_DECELLERATION, 2) + " Decel");
          Echo(Math.Round(CAF_DIST_TO_TARGET, 2) + " Dist");
          Echo(Math.Round(CAF_STOPPING_DIST, 2) + " St Dist");
      }

      //If Within Stopping Distance Halts Programme
      //--------------------------------------------
      if (!(CAF_DIST_TO_TARGET > (CAF_STOPPING_DIST + 0.25)) || CAF_DIST_TO_TARGET < 0.25 || VELOCITY > INPUT_MAX_VELOCITY)
      { return; }
      //dev notes, this is the most major source of discontinuity between theorised system response

      //Reflects Vector To Cancel Orbiting
      //------------------------------------
      Vector3D DRIFT_VECTOR = Vector3D.Normalize(RC.GetShipVelocities().LinearVelocity + RC.WorldMatrix.Forward * 0.00001);
      Vector3D R_DRIFT_VECTOR = -1 * Vector3D.Normalize(Vector3D.Reflect(DRIFT_VECTOR, INPUT_VECTOR));
      R_DRIFT_VECTOR = ((Vector3D.Dot(R_DRIFT_VECTOR, INPUT_VECTOR) < -0.3)) ? 0 * R_DRIFT_VECTOR : R_DRIFT_VECTOR;
      INPUT_VECTOR = Vector3D.Normalize((4 * R_DRIFT_VECTOR) + INPUT_VECTOR);

      //Components Of Input Vector In FUR Axis
      //----------------------------------------
      double F_COMP_IN = Vector_Projection(INPUT_VECTOR, RC.WorldMatrix.Forward);
      double U_COMP_IN = Vector_Projection(INPUT_VECTOR, RC.WorldMatrix.Up);
      double R_COMP_IN = Vector_Projection(INPUT_VECTOR, RC.WorldMatrix.Right);

      //Calculate MAX Allowable in Each Axis & Length
      //-----------------------------------------------
      double F_COMP_MAX = (F_COMP_IN > 0) ? CAF2_FORWARD.PositiveMaxForce : -1 * CAF2_FORWARD.NegativeMaxForce;
      double U_COMP_MAX = (U_COMP_IN > 0) ? CAF2_UP.PositiveMaxForce : -1 * CAF2_UP.NegativeMaxForce;
      double R_COMP_MAX = (R_COMP_IN > 0) ? CAF2_RIGHT.PositiveMaxForce : -1 * CAF2_RIGHT.NegativeMaxForce;
      double MAX_FORCE = Math.Sqrt(F_COMP_MAX * F_COMP_MAX + U_COMP_MAX * U_COMP_MAX + R_COMP_MAX * R_COMP_MAX);

      //Apply Length to Input Components and Calculates Smallest Multiplier
      //--------------------------------------------------------------------
      double F_COMP_PROJ = F_COMP_IN * MAX_FORCE;
      double U_COMP_PROJ = U_COMP_IN * MAX_FORCE;
      double R_COMP_PROJ = R_COMP_IN * MAX_FORCE;
      double MULTIPLIER = 1;
      MULTIPLIER = (F_COMP_MAX / F_COMP_PROJ < MULTIPLIER) ? F_COMP_MAX / F_COMP_PROJ : MULTIPLIER;
      MULTIPLIER = (U_COMP_MAX / U_COMP_PROJ < MULTIPLIER) ? U_COMP_MAX / U_COMP_PROJ : MULTIPLIER;
      MULTIPLIER = (R_COMP_MAX / R_COMP_PROJ < MULTIPLIER) ? R_COMP_MAX / R_COMP_PROJ : MULTIPLIER;

      //Calculate Multiplied Components
      //---------------------------------
      CAF2_FORWARD.VCF = ((F_COMP_PROJ * MULTIPLIER) / F_COMP_MAX) * Math.Sign(F_COMP_MAX);
      CAF2_UP.VCF = ((U_COMP_PROJ * MULTIPLIER) / U_COMP_MAX) * Math.Sign(U_COMP_MAX);
      CAF2_RIGHT.VCF = ((R_COMP_PROJ * MULTIPLIER) / R_COMP_MAX) * Math.Sign(R_COMP_MAX);

      //Runs System Thrust Application
      //----------------------------------
      foreach (var THRUSTSYSTM in CAFTHI)
      {
          List<IMyThrust> POSTHRUST = THRUSTSYSTM.PositiveThrusters;
          List<IMyThrust> NEGTHRUST = THRUSTSYSTM.NegativeThrusters;
          if (THRUSTSYSTM.VCF < 0) { POSTHRUST = THRUSTSYSTM.NegativeThrusters; NEGTHRUST = THRUSTSYSTM.PositiveThrusters; }
          foreach (var thruster in POSTHRUST) { thruster.ThrustOverridePercentage = (float)Math.Abs(THRUSTSYSTM.VCF); }
          foreach (var thruster in NEGTHRUST) { thruster.SetValue<float>("Override", (1.001f)); }
      }

      #region Specialised Error Readouts
      //Error Readouts
      //---------------------------------
      if (Me.DisplayNameText == "CAFDIAGNOSTICS")
      {

          Echo(Vector3D.Round(Vector3D.Normalize(INPUT_POINT - REFPOS), 3) + " Input Vector  ");
          Echo(Vector3D.Round(R_DRIFT_VECTOR, 3) + " RDrift Vector  ");
          Echo(Vector3D.Round(INPUT_VECTOR, 3) + " Conv Input Vector  ");

          Echo(Math.Round(F_COMP_IN, 3) + " fcomp  ");
          Echo(Math.Round(U_COMP_IN, 3) + " ucomp  ");
          Echo(Math.Round(R_COMP_IN, 3) + " Rcomp  \n");

          Echo(Math.Round(F_COMP_MAX, 3) + " fcompmax  ");
          Echo(Math.Round(U_COMP_MAX, 3) + " ucompmax  ");
          Echo(Math.Round(R_COMP_MAX, 3) + " Rcompmax  \n");

          Echo(Math.Round(MAX_FORCE, 3) + " Maxforce  \n");

          Echo(Math.Round(F_COMP_MAX / F_COMP_PROJ, 3) + " fcompratio  ");
          Echo(Math.Round(U_COMP_MAX / U_COMP_PROJ, 3) + " ucompratio  ");
          Echo(Math.Round(R_COMP_MAX / R_COMP_PROJ, 3) + " rcompratio  \n");

          Echo(Math.Round(MULTIPLIER, 3) + " multi\n");

          Echo(Math.Round(CAF2_FORWARD.VCF, 3) + " fcomp_force  ");
          Echo(Math.Round(CAF2_UP.VCF, 3) + " Ucomp_force  ");
          Echo(Math.Round(CAF2_RIGHT.VCF, 3) + " Rcomp_force  \n");

          //System Tester
          double X_ = INPUT_VECTOR.X;//F_COMP_IN; //F_COMP_ * F_COMP_MAX
          double Y_ = INPUT_VECTOR.Y;//U_COMP_IN;
          double Z_ = INPUT_VECTOR.Z;//R_COMP_IN;

      }
      #endregion

  }
  //----------==--------=------------=-----------=---------------=------------=-------==--------=-----
  #endregion

  #region Vector Projection #RFC#
  /*=================================================
    Function: Vector Projection
    ---------------------------------------     */
  double Vector_Projection(Vector3D IN, Vector3D Axis)
  {
      double OUT = 0;
      OUT = Vector3D.Dot(IN, Axis) / IN.Length();
      if (OUT + "" == "NaN")
      { OUT = 0; }
      return OUT;
  }
  #endregion

  #region VECTOR_THRUST_MANAGER #RFC#
  /*====================================================================================================================================
  Secondary Function: PRECISION MANAGER
  -----------------------------------------------------
  Function will: Given two inputs manage vector-based thrusting
  Inputs: DIRECTION, BLOCK
  //-=--------------=-----------=-----------=-------------------=-------------------=----------------------=----------------------------*/
  void VECTOR_THRUST_MANAGER(Vector3D PM_START, Vector3D PM_TARGET, Vector3D PM_REF, double PR_MAX_VELOCITY, double PREC)
  {
      Vector3D VECTOR = Vector3D.Normalize(PM_START - PM_TARGET);
      Vector3D GOTOPOINT = PM_TARGET + VECTOR * MathHelper.Clamp((((PM_REF - PM_TARGET).Length() - 0.2)),0,(PM_START - PM_TARGET).Length());
      double DIST_TO_POINT = MathHelper.Clamp((GOTOPOINT - PM_REF).Length(), 0, (PM_START - PM_TARGET).Length());

      if (DIST_TO_POINT > PREC)
      { COLLECT_AND_FIRE(GOTOPOINT, 0, 5, PM_REF); }
      else
      { COLLECT_AND_FIRE(PM_TARGET, 0, 5, PM_REF); }
  }
  //----------==--------=------------=-----------=---------------=------------=-------==--------=------------=-----------=----------
  #endregion

  //Docking Systems

  #region DOCK_SYSTEM
  /*====================================================================================================================================
  Secondary Function: DOCK_SYSTEM
  -----------------------------------------------------
  Function will: Operate docking & undocking route setup and outputs
  //-=--------------=-----------=-----------=-------------------=-------------------=----------------------=----------------------------*/
  string DOCK_SYST()
  {
      //Initialises Self And Docking Routes
      if (DOCKING_INIT == false)
      { DOCK_INIT(); DOCKING_INIT = true; }

      //Generates System Dock Outputs
      StringBuilder DOCKOUTPUT = new StringBuilder();
      List<string> KEYS = new List<string>(DOCKPOINTS.Keys);
      for (int i = 0; i < DOCKPOINTS.Count; i++)
      {
          //Updates Docking Location
          DOCKPOINTS[DOCKPOINTS[KEYS[i]].ID].LAST_PING = DateTime.Now;
          DOCKPOINTS[DOCKPOINTS[KEYS[i]].ID] = DOCKPOINT_INFO.SAVE_ROUTE_TO_STRING(DOCKPOINTS[KEYS[i]]);

          //Appends Location To String
          DOCKOUTPUT.Append(DOCKPOINTS[KEYS[i]].OUTPUT);
      }
      return DOCKOUTPUT+"";
  }
  #endregion

  #region Connector Trail Setup
  /*=======================================================================================
    Function: Connector Trail Setup
    ---------------------------------------
    Function will: Setup A Connector-LCD Trail
  //=======================================================================================*/
  List<IMyTerminalBlock> TRAIL_SETUP(IMyShipConnector CONN)
  {

      //Sets Up Temporary
      List<IMyTerminalBlock> TEMP_D = new List<IMyTerminalBlock>();
      List<IMyTerminalBlock> ROUTE_BLOCKS = new List<IMyTerminalBlock>();

      //Retrieves And Stores First Text Panel
      GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(TEMP_D, block => (block.CubeGrid == Me.CubeGrid && block != CONNECTOR));
      if (TEMP_D.Count == 0) { return ROUTE_BLOCKS; }
      var INITIAL_LCD = default(IMyTextPanel);
      double DIST = 5;
      foreach (var item in TEMP_D)
      {
          var ME_DIST = (item.GetPosition() - CONN.GetPosition()).Length();
          if (ME_DIST < DIST)
          { INITIAL_LCD = item as IMyTextPanel; DIST = ME_DIST; }
      }

      //Logic Check To Ensure No Enpty Routes Are Output
      if (INITIAL_LCD == null) { Echo("Non Route Detected"); return new List<IMyTerminalBlock>(); }
      ROUTE_BLOCKS.Add(CONN);
      ROUTE_BLOCKS.Add(INITIAL_LCD);

      //Iterates Through Panels Until Path Is Open
      for (int i = 1; i < 7; i++)
      {
          List<IMyTerminalBlock> TEMP_E = new List<IMyTerminalBlock>();
          GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(TEMP_E, block => Vector3D.Dot(ROUTE_BLOCKS[i].WorldMatrix.Up, Vector3D.Normalize(block.GetPosition() - ROUTE_BLOCKS[i].GetPosition())) > 0.999);
          if (TEMP_E.Count == 0) { break; } //ends if nothing above
          IMyTextPanel TEMP_PANEL = default(IMyTextPanel);
          DIST = 300;
          foreach (var item in TEMP_E)
          {
              var ME_DIST = (item.GetPosition() - CONN.GetPosition()).Length();
              if (ME_DIST < DIST)
              { TEMP_PANEL = item as IMyTextPanel; DIST = ME_DIST; }
          }
          ROUTE_BLOCKS.Add(TEMP_PANEL);
      }

      //Outputs Panels And Connector
      Echo("Iteration Through Panels Complete");
      return ROUTE_BLOCKS;
  }
  #endregion

  #region Dock Initialisation
  /*=======================================================================================
    Function: DOCK INIT
    ---------------------------------------
    function will: Initialise the docking manager
  //=======================================================================================*/
  void DOCK_INIT()
  {
      //Runs Through All Docking Routes, Forms New Docking routes as terminal block lists
      List<IMyTerminalBlock> TEMP = new List<IMyTerminalBlock>();
      GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(TEMP, b => b.CubeGrid == Me.CubeGrid);
      foreach (IMyTerminalBlock TERMINAL_CONNECTOR in TEMP)
      {
          Echo("started loop");
          List<IMyTerminalBlock> TRAIL_LIST = TRAIL_SETUP(TERMINAL_CONNECTOR as IMyShipConnector);
          Echo("ran coord init");
          if (TRAIL_LIST.Count > 0)
          {
              DOCKPOINT_INFO NEW_DOCKPT = new DOCKPOINT_INFO();
              NEW_DOCKPT.ID = "DK" + TERMINAL_CONNECTOR.EntityId.ToString().Substring(0, 4);
              NEW_DOCKPT.BASE_TAG = MEINFO.ID;
              NEW_DOCKPT.LAST_PING = DateTime.Now;
              NEW_DOCKPT.ROUTE = TRAIL_LIST;
              List<string> ROUTE_POSITIONS = new List<string>();
              foreach (var item in NEW_DOCKPT.ROUTE)
              {ROUTE_POSITIONS.Add(item.GetPosition()+"");}
              NEW_DOCKPT.OUTPUTROLL = string.Join("^", ROUTE_POSITIONS);

              DOCKPOINTS.Add(NEW_DOCKPT.ID, NEW_DOCKPT);
          }
      }
  }

  #endregion

  //Specialist AI Functions

  #region I_Attack_Location
  /*=======================================================================================
    Function: Attack Location Interceptor
    ---------------------------------------
    function will: run the primary autopliot with necessary commands + fire weapons
    also contains collision avoidance for close targets
    Inputs: none
  //=======================================================================================*/
  Vector3D ATTACK_RUN_POS;
  void Attack_Location_Interceptor()
  {
      Echo("In Attack Situation\n");
      Echo(ENEMY_DETECTED+"");

      //Generates Aiming And Fighting Positions
      //------------------------------------------
      var aimpos = COORDINATES[0];
      Echo("Using Default Tracking");
      if (ENEMY_DETECTED)
      {
          aimpos = SCAN_LOCKED_SHIP.POSITION;
          Echo("Using Turret Tracking");
          if ((aimpos - COORDINATES[0]).Length() > 1600)
          { aimpos = COORDINATES[0]; Echo("REASSIGNING COORDS"); } //Reassigns to prevent the anna bug
      }
      if (SCAN_HARDLOCK == true)
      {
          Echo("Overridden, using Raycast Tracking");
          aimpos = GetPredictedTargetPosition2(RC, SCAN_LOCKED_SHIP);
          if ((aimpos - COORDINATES[0]).Length() > 1600)
          { aimpos = COORDINATES[0]; Echo("REASSIGNING COORDS"); } //Reassigns to prevent the anna bug
      }

      //Applies Actions to Autopilot Function depending on target velocity
      //--------------------------------------------------------------------
      if (SCAN_LOCKED_SHIP.VELOCITY.Length() > 20 && SCAN_HARDLOCK)
      {
          Echo("target fast, engaging");
          if ((RC.GetPosition() - aimpos).Length() < SCAN_LOCKED_SHIP.SIZE / 2)
          { GetFreeDestination(aimpos); DOGPILOT(GFD_FREE_DESTINATION, SCAN_LOCKED_SHIP.VELOCITY.Length(), true); }
          if ((RC.GetPosition() - aimpos).Length() < SCAN_LOCKED_SHIP.SIZE)  //only thrusts to if really close
          { GyroTurn4(aimpos, GYRO, 7, new Vector3D(), "FORWARD"); }
          else if ((RC.GetPosition() - aimpos).Length() < 600 || SCAN_HARDLOCK == false)   //if distance is small trigger a targeting ovverride
          { DOGPILOT(aimpos, SCAN_LOCKED_SHIP.VELOCITY.Length(), true); }
          else
          { DOGPILOT(aimpos, SCAN_LOCKED_SHIP.VELOCITY.Length(), false); }  //otherwise just points and flies
      }
      else if (SCAN_LOCKED_SHIP.VELOCITY.Length() < 20 && SCAN_HARDLOCK)
      {
          Echo("target slow, engaging ");
          //Setup GFD
          Vector3D TARG_VECT = Vector3D.Normalize(aimpos - RC.GetPosition()) * 2000 + RC.GetPosition();
          GetFreeDestination(TARG_VECT);

          // Sets target according to situation
          if ((RC.GetPosition() - aimpos).Length() < 800 && ATTACK_RUN_POS == new Vector3D())
          { ATTACK_RUN_POS = Vector3D.Normalize(GFD_FREE_DESTINATION - RC.GetPosition()) * 2000 + RC.GetPosition(); }
          if ((RC.GetPosition() - aimpos).Length() > 800)
          { ATTACK_RUN_POS = Vector3D.Normalize(GFD_FREE_DESTINATION - RC.GetPosition()) * 2000 + RC.GetPosition(); Echo("setting pos"); }

          //Attacks And Controls Based On Situation
          double VEL_COMP = MathHelper.Clamp(Vector3D.Dot(Vector3D.Normalize(RC.GetShipVelocities().LinearVelocity), Vector3D.Normalize(ATTACK_RUN_POS - RC.GetPosition())),-1,1);
          Echo(VEL_COMP + " vel component");
          if (VEL_COMP > 0.975 && (RC.GetPosition() - aimpos).Length() < 800 && RC.GetShipSpeed() > 60)
          {
              Echo("attacking pos");
              RC.DampenersOverride = false;
              GyroTurn4(aimpos, GYRO, 7, new Vector3D(), "FORWARD");
          }
          else
          {
              Echo("going to far away pos");
              DOGPILOT(ATTACK_RUN_POS, SCAN_LOCKED_SHIP.VELOCITY.Length(), true); //else fly to to speed up
              return; //ends to prevent shooting
          }

      }
      else if ((RC.GetPosition() - aimpos).Length() < 800) //otherwise dogpilot to coordinates small distance for fire
      {
          DOGPILOT(aimpos, SCAN_LOCKED_SHIP.VELOCITY.Length(), true);
      }
       else //otherwise dogpilot to coordinates long distance allows drift cancellation
      {
          DOGPILOT(aimpos, SCAN_LOCKED_SHIP.VELOCITY.Length(), false);
      }

      //Fires Weaponry If Applicable
      if (Vector3D.Dot(RC.WorldMatrix.Forward, Vector3D.Normalize(aimpos - RC.GetPosition())) > 0.975 && (RC.GetPosition() - aimpos).Length() < 800)
      {
          //if distance is small dampeners off
          for (int i = 0; i < DIRECTIONAL_FIRE.Count; i++)
          { DIRECTIONAL_FIRE[i].ApplyAction("ShootOnce"); DIRECTIONAL_FIRE[i].ApplyAction("Shoot_On"); }
      }
  }
  #endregion

  #region FR_Attack_Location
  /*=======================================================================================
    Function: Attack Location Frigate
    ---------------------------------------
    function will: run the primary autopliot with necessary commands + fire weapons
    also contains collision avoidance for close targets
    Inputs: none
  //=======================================================================================*/
  int FIGHT_TIMER = 0; //Timer for frigate manouvers
  int SEQUUENCE_TIMER = 0;
  void Attack_Location_Frigate()
  {
      Echo("In Attack Situation\n");
      PROJECTILE_VELOCITY = 200;

      //Generates Aiming And Fighting Positions
      //------------------------------------------
      var aimpos = COORDINATES[0];
      if (ENEMY_DETECTED)
      {
          aimpos = SCAN_LOCKED_SHIP.POSITION;
          if ((aimpos - COORDINATES[0]).Length() > 1400)
          { aimpos = COORDINATES[0]; } //Reassigns to prevent the anna bug
      }
      if (SCAN_HARDLOCK == true)
      {
          aimpos = GetPredictedTargetPosition2(RC, SCAN_LOCKED_SHIP);
          if ((aimpos - COORDINATES[0]).Length() > 1400)
          { aimpos = COORDINATES[0]; } //Reassigns to prevent the anna bug
      }


      //Follows Ship If It's Fast, Static and 600m if it's slow
      //--------------------------------------------------------------------
      if ((RC.GetPosition() - aimpos).Length() > 800) //Distance is high, go to
      {
          if (SCAN_HARDLOCK)
          {
              aimpos = aimpos - Vector3D.Normalize(aimpos - RC.GetPosition()) * 500; //prevents collision
              COLLECT_AND_FIRE(aimpos, 0, MAX_SPEED, RC.GetPosition()); //SCAN_LOCKED_SHIP.VELOCITY.Length() no velocity makes chasing harder but stops collisions
              GyroTurn4(aimpos, GYRO, 5, new Vector3D(), "FORWARD");
          }
          else
          {
              COLLECT_AND_FIRE(aimpos, 0, MAX_SPEED, RC.GetPosition());
              GyroTurn4(aimpos, GYRO, 5, new Vector3D(), "FORWARD");
          }
      }
      else if ((RC.GetPosition() - aimpos).Length() < 400) //Target is too close back off
      {
          COLLECT_AND_FIRE(RC.GetPosition() + 200 * RC.WorldMatrix.Backward, 0, 30, RC.GetPosition()); //backs off
          GyroTurn4(aimpos, GYRO, 5, new Vector3D(), "FORWARD"); //Aims with Forward direction and fires if close
      }
      else if (SCAN_HARDLOCK && SCAN_LOCKED_SHIP.VELOCITY.Length() > 5 ) //Target is travelling away(and further than 200m), follow and lay down fire
      {
          COLLECT_AND_FIRE(aimpos, 0, MAX_SPEED, RC.GetPosition()); //Follows

          GyroTurn4(aimpos, GYRO, 5, new Vector3D(), "FORWARD"); //Aims with Forward direction and fires if close
      }
      else if (SCAN_HARDLOCK) //Target is close and static, attack and strafe up and down
      {
          GyroTurn4(aimpos, GYRO, 5, new Vector3D(), "FORWARD"); //Aims with Forward direction

          if (FIGHT_TIMER < 100)
          { COLLECT_AND_FIRE(RC.GetPosition() + 100 * RC.WorldMatrix.Up, 0, 10, RC.GetPosition()); } //Strafes up
          if (FIGHT_TIMER > 100)
          { COLLECT_AND_FIRE(RC.GetPosition() + 100 * RC.WorldMatrix.Down, 0, 10, RC.GetPosition()); } //Strafes down
          if (FIGHT_TIMER == 200)
          { FIGHT_TIMER = 0; }
          FIGHT_TIMER++;
      }
      else if (ENEMY_DETECTED && DIRECTOR.IsShooting) //Basic Non-locking Systems Override
      {
          GyroTurn4(aimpos, GYRO, 5, new Vector3D(), "FORWARD");
      }

      //Fires Weaponry If Applicable Automatically Sequences
      if (Vector3D.Dot(RC.WorldMatrix.Forward, Vector3D.Normalize(aimpos - RC.GetPosition())) > 0.975 && (RC.GetPosition() - aimpos).Length() < 800 && DIRECTOR.HasTarget)
      {
          DIRECTIONAL_FIRE[SEQUUENCE_TIMER].ApplyAction("ShootOnce"); DIRECTIONAL_FIRE[SEQUUENCE_TIMER].ApplyAction("Shoot_On");
          SEQUUENCE_TIMER++; //sequenced launchers for maximum effect
          if (SEQUUENCE_TIMER > DIRECTIONAL_FIRE.Count-1)
          { SEQUUENCE_TIMER = 0; }
      }
  }
  #endregion

  #region CR_Attack_Location
  /*=======================================================================================
    Function: Attack Location Frigate
    ---------------------------------------
    function will: run the primary autopliot with necessary commands + fire weapons
    also contains collision avoidance for close targets
    Inputs: none
  //=======================================================================================*/
  void Attack_Location_Cruiser()
  {
      Echo("In Attack Situation\n");
      Echo(SCAN_LOCKED_SHIP.AIMPOS + "\n");

      //Generates Aiming And Fighting Positions
      //------------------------------------------
      var aimpos = COORDINATES[0];
      if (ENEMY_DETECTED)
      {
          aimpos = SCAN_LOCKED_SHIP.POSITION;
          if ((aimpos - COORDINATES[0]).Length() > 1200)
          { aimpos = COORDINATES[0]; } //Reassigns to prevent the anna bug
      }
      if (SCAN_HARDLOCK == true)
      {
          aimpos = GetPredictedTargetPosition2(RC, SCAN_LOCKED_SHIP);
          if ((aimpos - COORDINATES[0]).Length() > 1200)
          { aimpos = COORDINATES[0]; } //Reassigns to prevent the anna bug
      }

      //Follows Ship If It's Fast, Static and 600m if it's slow
      //--------------------------------------------------------------------

      if ((RC.GetPosition() - aimpos).Length() < 400) //Target is too close back off
      {

          COLLECT_AND_FIRE(RC.GetPosition() + 200 * RC.WorldMatrix.Backward, 0, 10, RC.GetPosition()); //backs off

          GyroTurn4(aimpos, GYRO, 3, new Vector3D(), "FORWARD"); //Aims with Forward direction and fires if close
      }

      else if ((RC.GetPosition() - aimpos).Length() > 700) //Target is too far approach
      {
          RC_MANAGER(COORDINATES[0]);
          //COLLECT_AND_FIRE(aimpos, 0, 10, RC.GetPosition()); //slowly approach target, guns first
      }
      //else if (SCAN_LOCKED_SHIP.VELOCITY.Length() > 5) //Target is travelling away(and further than 200m), follow and lay down fire
      //{
      //    COLLECT_AND_FIRE(aimpos, 0, 80, RC.GetPosition()); //Follows

      //    GyroTurn4(aimpos, GYRO, 3, new Vector3D(), "FORWARD"); //Aims with Forward direction and fires if close

      //}
      else if (SCAN_HARDLOCK) //Target is close and static, attack directional
      {
          GyroTurn4(aimpos, GYRO, 3, new Vector3D(), "FORWARD"); //Aims with Forward direction
      }


      //Fires Weaponry If Applicable
      if (Vector3D.Dot(RC.WorldMatrix.Forward, Vector3D.Normalize(aimpos - RC.GetPosition())) > 0.975 && (RC.GetPosition() - aimpos).Length() < 800)
      {
          //if distance is small dampeners off
          for (int i = 0; i < DIRECTIONAL_FIRE.Count; i++)
          { DIRECTIONAL_FIRE[i].ApplyAction("ShootOnce"); DIRECTIONAL_FIRE[i].ApplyAction("Shoot_On"); }
      }
  }
  #endregion

  #region HM_Attack_Location
  /*=======================================================================================
    Function: Attack Location Heavy Missile/Grav Cannon   (FMISSILE/FGRAV)
    ---------------------------------------
    function will: run the primary autopliot with necessary commands + fire weapons
    also contains collision avoidance for close targets
    Inputs: none
  //=======================================================================================*/
  double ENGAGE_DIST = 4000;
  int FIRETIMER = 0;
  int FIRE_TIME = 30;
  void Attack_Location_Heavy()
  {
      Echo("In Attack Situation\n");
      Echo(SCAN_LOCKED_SHIP.AIMPOS + "\n");

      if (SHIP_CLASS == "FGRAV")
      {PROJECTILE_VELOCITY = int.Parse(HW_TIMER.CustomData);}

      //Generates Aiming And Fighting Positions
      //------------------------------------------
      var aimpos = COORDINATES[0];
      if (ENEMY_DETECTED)
      {
          aimpos = SCAN_LOCKED_SHIP.POSITION;
          if ((aimpos - COORDINATES[0]).Length() > 1000)
          { aimpos = COORDINATES[0]; } //Reassigns to prevent the anna bug
      }
      if (SCAN_HARDLOCK == true)
      {
          aimpos = GetPredictedTargetPosition2(RC, SCAN_LOCKED_SHIP);
          if ((aimpos - COORDINATES[0]).Length() > 1000)
          { aimpos = COORDINATES[0]; } //Reassigns to prevent the anna bug
      }

      //Read If statements for logic processing, order is sequential
      //--------------------------------------------------------------------

      if ((RC.GetPosition() - aimpos).Length() < ENGAGE_DIST) //Target is too close back off
      {
          COLLECT_AND_FIRE(RC.GetPosition() + 200 * RC.WorldMatrix.Backward, 0, 10, RC.GetPosition()); //backs off
          GyroTurn4(aimpos, GYRO, 2, new Vector3D(), "FORWARD"); //Aims with Forward direction and fires if close
      }
      else if (SCAN_LOCKED_SHIP.VELOCITY.Length() > 5 || (RC.GetPosition() - aimpos).Length() > ENGAGE_DIST) //Target is travelling away(and further than engage), follow and lay down fire
      {
          aimpos = aimpos - Vector3D.Normalize(aimpos - RC.GetPosition()) * 500;
          COLLECT_AND_FIRE(aimpos, 0, MAX_SPEED, RC.GetPosition()); //Follows
          GyroTurn4(aimpos, GYRO, 2, new Vector3D(), "FORWARD"); //Aims with Forward direction and fires if close
      }
      else //Target is at angagement distance and static, attack directional
      {
          GyroTurn4(aimpos, GYRO, 2, new Vector3D(), "FORWARD"); //Aims with Forward direction
      }
      //else if (ENEMY_DETECTED) //tries to fire in blind oanic if enemy is close
      //{
      //    //aimpos = aimpos - Vector3D.Normalize(aimpos - RC.GetPosition()) * 500; //prevents collision
      //    //COLLECT_AND_FIRE(aimpos, SCAN_LOCKED_SHIP.VELOCITY.Length(), MAX_SPEED, RC.GetPosition());
      //    GyroTurn4(aimpos, GYRO, 5, new Vector3D(), "FORWARD");
      //}

      //Fires At Target And Sets External BlockData
      //---------------------------------------------
      if (SCAN_HARDLOCK && SHIP_CLASS == "FMISSILE") //only runs missile fire if target is hardlocked (prevents unecessarry spam)
      { HW_TIMER.CustomData = SCAN_LOCKED_SHIP.POSITION + "#" + (SCAN_LOCKED_SHIP.VELOCITY/60); }
      if (Vector3D.Dot(RC.WorldMatrix.Forward, Vector3D.Normalize(aimpos - RC.GetPosition())) > 0.6 && (RC.GetPosition() - aimpos).Length() < ENGAGE_DIST && FIRETIMER > FIRE_TIME && SCAN_HARDLOCK)
      { HW_TIMER.Trigger(); FIRETIMER = 0; } //Triggers Primary Gun
      FIRETIMER++;
  }
  #endregion

  #region DogPilot #RFC#
  /*=============================================================================================================
    Function: Dogpilot
    ---------------------------------------
    function will: Pilot a small craft priorotising fast manouvers over collision avoidance
    Inputs:
  //=============================================================================================================*/
  bool DP_HASRUN = false;
  List<IMyTerminalBlock> DP_THRUST = new List<IMyTerminalBlock>();
  List<IMyTerminalBlock> DP_BRAKE = new List<IMyTerminalBlock>();
  List<IMyTerminalBlock> DP_UP = new List<IMyTerminalBlock>();
  double DP_THRUST_MULTIPLIER;
  void DOGPILOT(Vector3D TARGETPOS, double TARG_VEL, bool OVERRIDE)
  {
      //Function Initialisation
      //--------------------------------------------------------------------
      if (DP_HASRUN == false)
      {
          GridTerminalSystem.GetBlocksOfType<IMyThrust>(DP_THRUST, block => Vector3D.Dot(-1 * block.WorldMatrix.Forward, RC.WorldMatrix.Forward) > 0.5);
          GridTerminalSystem.GetBlocksOfType<IMyThrust>(DP_BRAKE, block => Vector3D.Dot(1 * block.WorldMatrix.Forward, RC.WorldMatrix.Forward) > 0.5);
          GridTerminalSystem.GetBlocksOfType<IMyThrust>(DP_UP, block => Vector3D.Dot(-1 * block.WorldMatrix.Forward, RC.WorldMatrix.Up) > 0.5);

          if (Me.CubeGrid.ToString().Contains("Large")) { DP_THRUST_MULTIPLIER = 288; }
          else { DP_THRUST_MULTIPLIER = 12; }
          DP_HASRUN = true;
      }

      //Generating Maths To Point and decelleration information etc.
      //--------------------------------------------------------------------
      double VELOCITY = RC.GetShipSpeed();
      Vector3D POSITION = RC.GetPosition();
      double SHIPMASS = Convert.ToDouble(RC.CalculateShipMass().PhysicalMass);
      var DIST_TO_TARGET = (RC.GetPosition() - TARGETPOS).Length();
      var SHIP_DECELLERATION = (DP_BRAKE.Count * DP_THRUST_MULTIPLIER * 1000) / SHIPMASS;
      var STOPPING_DIST = (((VELOCITY * VELOCITY) - (TARG_VEL * TARG_VEL))) / (2 * SHIP_DECELLERATION);
      //Drift Functions
      Vector3D MIS_TO_TARGET = Vector3D.Normalize(TARGETPOS - POSITION);
      Vector3D DRIFT_VECTOR = Vector3D.Normalize(RC.GetShipVelocities().LinearVelocity);
      Vector3D REFLECTED_DRIFT_VECTOR = Vector3D.Negate(Vector3D.Normalize(Vector3D.Reflect(DRIFT_VECTOR, MIS_TO_TARGET)));

      //Disabling Drift
      if (Vector3D.Dot(REFLECTED_DRIFT_VECTOR, MIS_TO_TARGET) < -0.2 || VELOCITY < 10 || OVERRIDE)
      { REFLECTED_DRIFT_VECTOR = REFLECTED_DRIFT_VECTOR * 0; }

      Vector3D CORRECTED_VECTOR = Vector3D.Normalize((4 * REFLECTED_DRIFT_VECTOR) + MIS_TO_TARGET);
      Vector3D AIMPINGPOS = CORRECTED_VECTOR * 300 + RC.GetPosition();

      //Running Operational System
      //--------------------------------------------------------------------
      GyroTurn4(AIMPINGPOS, GYRO, 7, new Vector3D(), "FORWARD");

      if (DIST_TO_TARGET > STOPPING_DIST)
      {
          foreach (var THRUSTER in DP_THRUST)
          { (THRUSTER as IMyThrust).ThrustOverride = (THRUSTER as IMyThrust).MaxThrust; }
          foreach (var THRUSTER in DP_BRAKE)
          { THRUSTER.SetValue<float>("Override", 1.001f); }
      }

      //Calculatory Error Readouts
      //-------------------------------------------
      if (Me.DisplayNameText == "DIAGNOSTICS")
      {
          Echo("\n~Running DP~");
          Echo(DP_BRAKE.Count + " Brake");
          Echo(Math.Round(VELOCITY, 2) + " Vel");
          Echo(Math.Round(SHIP_DECELLERATION, 2) + " Decel");
          Echo(Math.Round(DIST_TO_TARGET, 2) + " Dist");
          Echo(Math.Round(STOPPING_DIST, 2) + " St Dist");
      }

  }
  //----------==--------=------------=-----------=---------------=------------=-----------=-------=----------------*/
  #endregion

  //Utils Functions

  #region Connector Direction #RFC#
  /*=======================================================================================
    Function: Connector Direction
    ---------------------------------------
    function will: return a string for the RC to use for docking procedures
  //=======================================================================================*/
  void RETURN_CONNECTOR_DIRECTION()
  {
      if (CONNECTOR.Orientation.Forward == RC.Orientation.TransformDirection(Base6Directions.Direction.Down))
      { CONNECTOR_PLANE = "LEFT"; }  //Connector is the bottom of ship
      if (CONNECTOR.Orientation.Forward == RC.Orientation.TransformDirection(Base6Directions.Direction.Up))
      { CONNECTOR_PLANE = "RIGHT"; }  //Connector is on the top of the ship
      if (CONNECTOR.Orientation.Forward == RC.Orientation.TransformDirection(Base6Directions.Direction.Right))
      { CONNECTOR_PLANE = "UP"; }  //Connector is on the left of the ship
      if (CONNECTOR.Orientation.Forward == RC.Orientation.TransformDirection(Base6Directions.Direction.Left))
      { CONNECTOR_PLANE = "DOWN"; }  //Connector is on the right of the ship
  }
  #endregion

  #region SCAN
  /*====================================================================================================================================
  Secondary Function: SCAN_MANAGER
  -----------------------------------------------------
  Function will: Given two inputs manage vector-based thrusting
  Outputs: Position for shot convergence, if there is a target in the way, if that target is friendly, information reagrding locked ship
  //-=--------------=-----------=-----------=-------------------=-------------------=----------------------=----------------------------*/
  struct DEC_INFO { public Vector3D POSITION; public Vector3D VELOCITY; public Vector3D AIMPOS; public double SIZE; public string ID;}
  bool SCAN_HARDLOCK = false;
  DEC_INFO SCAN_LOCKED_SHIP;
  int SCAN_PREDICTIVE = 0;
  int GOTO_SCAN_TIMER = 0;
  void SCAN_MANAGER(out bool ISTARGETING)
  {
      //Sets Initial Outputs
      ISTARGETING = false;
      DEC_INFO TEMP_DETECT_INFO = SCAN_LOCKED_SHIP; //Reassignable output
      MyDetectedEntityInfo TEMP_INFO = new MyDetectedEntityInfo();

      //Checks All Turrets For Shooting For Best Target Aquisition
      //--------------------------------------------------------------
      foreach (var item in DIRECTORS)
      {
          if (item.IsShooting && item.HasTarget)
          { DIRECTOR = item; break; }
      }

      //Turret Shooting Lock Override
      //-------------------------------
      if (DIRECTOR.HasTarget)
      {
          ISTARGETING = true;
          TEMP_INFO = DIRECTOR.GetTargetedEntity();
      }

      //Runs A Small Scan Every 5 Ticks To GOTO Location
      //--------------------------------------------------
      if (GOTO_SCAN_TIMER > 5)
      {
          foreach (var ITEM in CAMERAS)
          {
              IMyCameraBlock CAMERA = (ITEM as IMyCameraBlock);
              if (CAMERA.CanScan(COORDINATES[0]) && CAMERA.IsWorking)
              {
                  Vector3D DIRECTION = Vector3D.Normalize(COORDINATES[0] - CAMERA.GetPosition());
                  double DISTANCE = (COORDINATES[0] - CAMERA.GetPosition()).Length() + 40;
                  Vector3D POS = DIRECTION * DISTANCE + CAMERA.GetPosition();
                  TEMP_INFO = CAMERA.Raycast(POS);
                  break;
              }
          }
          GOTO_SCAN_TIMER = 0;
      }
      GOTO_SCAN_TIMER++;

      //Runs Locked System Scan
      //-------------------------
      if (SCAN_HARDLOCK == true && TEMP_INFO.IsEmpty())
      {
          Vector3D TESTPOS = SCAN_LOCKED_SHIP.POSITION + (SCAN_LOCKED_SHIP.VELOCITY) *0.13333333 *SCAN_PREDICTIVE; //Runtime.TimeSinceLastRun.Milliseconds/1000
          Vector3D DIRECTION = Vector3D.Normalize(TESTPOS - RC.GetPosition());
          double DISTANCE = (TESTPOS - RC.GetPosition()).Length() + 40;
          Vector3D POS = DIRECTION * DISTANCE + RC.GetPosition();
          foreach (var ITEM in CAMERAS)
          {
              IMyCameraBlock CAMERA = (ITEM as IMyCameraBlock);
              if (CAMERA.CanScan(POS) && CAMERA.IsWorking)
              {
                  Echo(CAMERA.CustomName);

                  TEMP_INFO = CAMERA.Raycast(POS);
                  break;
              }
          }
      }

      //Detects enemy entity info and outputs
      //Echo((!TEMP_INFO.IsEmpty()) +""+ (TEMP_INFO.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies) + "");
      if (!TEMP_INFO.IsEmpty() && TEMP_INFO.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies)
      {
          ISTARGETING = true;
          Echo("SYSTEM LOCK");
          SCAN_HARDLOCK = true;
          SCAN_PREDICTIVE = 0;
          TEMP_DETECT_INFO.POSITION = TEMP_INFO.Position;
          //if (!DIRECTOR.HasTarget)
          TEMP_DETECT_INFO.AIMPOS = TEMP_INFO.Position;
          TEMP_DETECT_INFO.SIZE = (TEMP_INFO.BoundingBox.Max - TEMP_INFO.BoundingBox.Min).Length();
          TEMP_DETECT_INFO.VELOCITY = TEMP_INFO.Velocity;
          TEMP_DETECT_INFO.ID = TEMP_INFO.EntityId+"";
      }

      //Resets Scanner If Target Is Lost For too Long
      if (SCAN_PREDICTIVE > 10)
      {SCAN_HARDLOCK = false; SCAN_PREDICTIVE = 0; Echo("Target Lost, Clearing Lock"); }

      //Outputs information
      SCAN_LOCKED_SHIP = TEMP_DETECT_INFO;
      SCAN_PREDICTIVE++;

  }
  //----------==--------=------------=-----------=---------------=------------=-------==--------=------------=-----------=----------

  #endregion

  #region RFC Function bar #RFC#
  /*=================================================
    Function: RFC Function bar #RFC#
    ---------------------------------------     */
  string[] FUNCTION_BAR = new string[] { "", " ===||===", " ==|==|==", " =|====|=", " |======|", "  ======" };
  int FUNCTION_TIMER = 0;                                     //For Runtime Indicator
  void OP_BAR()
  {
      FUNCTION_TIMER++;
      Echo("     ~ MKII RFC AI Running~  \n               " + FUNCTION_BAR[FUNCTION_TIMER] + "");
      if (FUNCTION_TIMER == 5) { FUNCTION_TIMER = 0; }
  }
  #endregion

  #region EXTRACTDIRECTION
  /*====================================================================================================================================
  Secondary Function: ExtractDirection
  -----------------------------------------------------
  Function will: Given a direction and a block, return forward, up or left vectors
  Inputs: DIRECTION, BLOCK
  //-=--------------=-----------=-----------=-------------------=-------------------=----------------------=----------------------------*/
  Vector3D ExtractDirection(String DIRECTION, IMyTerminalBlock BLOCK)
  {
      Vector3D OUTPUTDIRECTION = new Vector3D();

      if (DIRECTION == "UP")
      { OUTPUTDIRECTION = BLOCK.WorldMatrix.Up; }

      if (DIRECTION == "LEFT")
      {  OUTPUTDIRECTION = BLOCK.WorldMatrix.Left;  }

      if (DIRECTION == "FORWARD")
      { OUTPUTDIRECTION = BLOCK.WorldMatrix.Forward; }

      if (DIRECTION == "RIGHT")
      { OUTPUTDIRECTION = BLOCK.WorldMatrix.Right;}

      return OUTPUTDIRECTION;
  }
  //----------==--------=------------=-----------=---------------=------------=-------==--------=------------=-----------=----------
  #endregion

  #region GET_FREE_DESTINATION (prototype)
  /*====================================================================================================================================
  Secondary Function: Get Free Destination
  -----------------------------------------------------
  Function will: Get the free destination for a current required travel, and also output whether or not the ship is currently safe travel
  Inputs: TargetPosition (can be nulled if you have a lock)
  //-=--------------=-----------=-----------=-------------------=-------------------=----------------------=----------------------------*/
  Vector3D GFD_FREE_DESTINATION;
  int SCAN_CTIMER = 6;
  void GetFreeDestination(Vector3D TARGET)
  {
      //Only Runs If Scan is larger than
      if (SCAN_CTIMER > 5)
      {
          //Sets Everything Up Anew
          Vector3D SHIP_VEl = Vector3D.Normalize(RC.GetShipVelocities().LinearVelocity);
          Vector3D TO_TARG = Vector3D.Normalize(TARGET - RC.GetPosition());
          MyDetectedEntityInfo TEMP_INFO = new MyDetectedEntityInfo();
          GFD_FREE_DESTINATION = TARGET;

          //If Detected Target Then Run Free Position (overriding any other free position)
          if (SCAN_HARDLOCK && (COORDINATES[0] - SCAN_LOCKED_SHIP.POSITION).Length() < 950) //prevents use of hardlock if not attacking
          {
              //Generates a Free Destination
              double DIST_TO = (SCAN_LOCKED_SHIP.POSITION - RC.GetPosition()).Length();
              Vector3D FREE_DIRECT_P1 = SHIP_VEl * DIST_TO + RC.GetPosition();
              Vector3D FREE_DIRECT_P2 = SCAN_LOCKED_SHIP.POSITION;
              Vector3D FREE_DIRECT = Vector3D.Normalize(FREE_DIRECT_P1 - FREE_DIRECT_P2);
              GFD_FREE_DESTINATION = SCAN_LOCKED_SHIP.POSITION + FREE_DIRECT * (SCAN_LOCKED_SHIP.SIZE / 2 + 15);
              Vector3D TARG = Vector3D.Normalize(SCAN_LOCKED_SHIP.POSITION - RC.GetPosition());
              return;
          }

          //Otherwise Initialises Randomiser
          Random RND = new Random();
          double x = RND.Next(20) * Math.Pow(-1, RND.Next(2));
          double y = RND.Next(20) * Math.Pow(-1, RND.Next(2));
          double z = RND.Next(20) * Math.Pow(-1, RND.Next(2));
          Vector3D RANDOMISER = new Vector3D(x, y, z);

          Vector3D RAYPOS = MathHelper.Clamp(((TARGET - RC.GetPosition()).Length()), 0f, 600f) * TO_TARG + RC.GetPosition() + RANDOMISER;

          //Raycasts Towards Target
          foreach (var ITEM in CAMERAS)
          {
              IMyCameraBlock CAMERA = (ITEM as IMyCameraBlock);
              if (CAMERA.CanScan(RAYPOS))
              { TEMP_INFO = CAMERA.Raycast(RAYPOS); SCAN_CTIMER = -1; break; }
          }

          //If Obstructed Generate a Free Destination
          if (!TEMP_INFO.IsEmpty() && TEMP_INFO.Name != Me.CubeGrid.DisplayName)
          {
              Echo("collision detected");
              //Generates a Free Destination
              double DIST_TO = (TEMP_INFO.Position - RC.GetPosition()).Length();
              Vector3D FREE_DIRECT_P1 = SHIP_VEl * DIST_TO + RC.GetPosition();
              Vector3D FREE_DIRECT_P2 = TEMP_INFO.Position;
              Vector3D FREE_DIRECT = Vector3D.Normalize(FREE_DIRECT_P1 - FREE_DIRECT_P2);
              GFD_FREE_DESTINATION = TEMP_INFO.Position + FREE_DIRECT * TEMP_INFO.BoundingBox.Size.Length() / 2;
          }
      }
      //Iterates Up
      SCAN_CTIMER++;
  }
  //----------==--------=------------=-----------=---------------=------------=-------==--------=------------=-----------=----------
  #endregion
