// COPY TO HERE   ------------------

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

      You are currently looking at the 'Central Command' Unit, which
      is the main hub of operations for the code, only ONE of these units
      should be used per-fleet, others can be kept as backups, but only one
      should be operational at any one time.

      Rdav 28/08/17

      Installation
      --------------
      The code should come in a Pre-Fab format to make installation a breeze.
      Setup of these modules manually can be achieved however for streamlining
      or otherwise making the components of the module smaller, please refer to
      the manual to do so.

      The Central Command Unit will automatically:
       * Find an antennae on the ship and use it for broadcast
       * Find a command seat used for issuing commands called 'RFC_RC' (only renaming required)
       * Find and use for target detection any turret with

      Bughunting/Problems
      --------------------
      The code will automatically create a bugreport and save it to the custom-data of the
      designated remote block.

      Suggestions Planned Features
      -----------------------------
      Piss off I'm still working on it

       ChangeLog:
       * Update 6
       * 18/01/2017 ln255 Added support for alternate orientations.
       * 18/01/2017 DETECT changed convergance to 100-800m default
       * added command recieved indicator to LCD
       * updated command recieved indicator
       * Fixed UI unresponsiveness
       * Updated to reflect self ship docking
       * Updated to draw a line for a go-to
      */

      #endregion

      #region Syst Constants

      //Display Setup
      //---------------------------------------------------------
      string VERSION = "Ver_006";                         //Script Current Version
      const char P = medBlue; //Primary System Colour (your own ships)
      const char B = ' '; //Background Colour
      const char L1 = black; //Layer1background1colour
      const char L2 = mediumGray; //Layer2background1colour
      public char[,] BDRAW = new char[ROWS_CT, COLUMNS_CT]; //Stores Background
      char[,] DRAW = new char[ROWS_CT, COLUMNS_CT]; //Temporary Assigner

      #region Lettercodes
      const char red = '\uE200';
      const char medRed = '\uE1C0';
      const char darkRed = '\uE180';

      const char green = '\uE120';
      const char medGreen = '\uE118';
      const char darkGreen = '\uE110';

      const char blue = '\uE104';
      const char medBlue = '\uE103';
      const char darkBlue = '\uE102';

      const char yellow = '\uE220';
      const char medYellow = '\uE1D8';
      const char darkYellow = '\uE190';

      const char magenta = '\uE204';
      const char medMagenta = '\uE1C3';
      const char darkMagenta = '\uE182';

      const char cyan = '\uE124';
      const char medCyan = '\uE11B';
      const char darkCyan = '\uE112';

      const char white = '\uE2FF';
      const char lightGray = '\uE1DB';
      const char mediumGray = '\uE192';
      const char darkGray = '\uE149';
      const char black = '\uE100';
      #endregion

      //System Data Management
      //------------------------
      class DC_INF_INFO
      {
          public int SIZE; //Size of target
          public string TYPE; //Type of target
          public Vector3D POSITION; //Position
          public string DIRECT_TO_OUTPUT; //Directly Outputted string
          public int ST_SIZE; //Start Size
          public Vector2D UIPOS; // Position of UI locator
      }
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
          public string EXT_INF; //Drone Extra Info
          public string OUTPUT;   // String Drone Data Output
          public Vector2D UIPOS; // Position of UI locator (UI Only)

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
              DRONE_INF.OUTPUT = string.Join("*", "#" + DRONE_INF.ID, DRONE_INF.COMLOC, DRONE_INF.GLOC, DRONE_INF.LOC, DRONE_INF.VEL, DRONE_INF.TVEl, DRONE_INF.ISDOCKED, DRONE_INF.HEALTH, DRONE_INF.LAST_PING, DRONE_INF.EXT_INF, "#" + DRONE_INF.ID);
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
              if (RUN_ID[1] == true) { Vector3D.TryParse(DCK_INFO[1], out DOCKPT_INF.LOC); }
              if (RUN_ID[2] == true) { DOCKPT_INF.BASE_TAG = DCK_INFO[2]; }
              if (RUN_ID[3] == true) { DOCKPT_INF.ISDOCKED = DCK_INFO[3]; }
              if (RUN_ID[4] == true) { DOCKPT_INF.LAST_PING = DateTime.Parse(DCK_INFO[4]); }
              if (RUN_ID[5] == true) { DOCKPT_INF.OUTPUTROLL = DCK_INFO[5]; }

              DOCKPT_INF.OUTPUT = string.Join("*", "#" + DOCKPT_INF.ID, DOCKPT_INF.LOC, DOCKPT_INF.BASE_TAG, DOCKPT_INF.ISDOCKED, DOCKPT_INF.LAST_PING, DOCKPT_INF.OUTPUTROLL, "#" + DOCKPT_INF.ID);
              return DOCKPT_INF;
          }

          //Standardised DockString Saving Procedure
          public static DOCKPOINT_INFO SAVE_ROUTE_TO_STRING(DOCKPOINT_INFO DOCKPT_INFO)
          {
              List<string> OUTPUT = new List<string>();
              double OFFSET_CONST = 2;
              List<IMyTerminalBlock> DOCKPT_TRAIL = DOCKPT_INFO.ROUTE;

              //Adds First Ordinates (self and forwards position)
              OUTPUT.Add(Vector3D.Round(DOCKPT_TRAIL[0].GetPosition() + DOCKPT_TRAIL[0].WorldMatrix.Forward * (1.5), 2) + "");
              OUTPUT.Add(Vector3D.Round(DOCKPT_TRAIL[0].GetPosition() + DOCKPT_TRAIL[0].WorldMatrix.Forward * (OFFSET_CONST + 2.5), 2) + "");

              //Iterates Through List Of LCD's
              for (int i = 1; i < DOCKPT_TRAIL.Count; i++)
              { var IMYPLACE = DOCKPT_TRAIL[i]; OUTPUT.Add(Vector3D.Round(IMYPLACE.GetPosition() + IMYPLACE.WorldMatrix.Backward * OFFSET_CONST, 2) + ""); }

              //Adds Final Position
              OUTPUT.Add(Vector3D.Round(DOCKPT_TRAIL[DOCKPT_TRAIL.Count - 1].GetPosition() +
                  DOCKPT_TRAIL[DOCKPT_TRAIL.Count - 1].WorldMatrix.Backward * OFFSET_CONST + DOCKPT_TRAIL[DOCKPT_TRAIL.Count - 1].WorldMatrix.Up * 100, 2) + "");


              //Saves To String, Updates Locator, (And Updates OUTPUT)
              DOCKPT_INFO.OUTPUTROLL = string.Join("^", OUTPUT);
              DOCKPT_INFO.LOC = Vector3D.Round(DOCKPT_TRAIL[0].GetPosition(), 2);
              DOCKPT_INFO.OUTPUT = string.Join("*", "#" + DOCKPT_INFO.ID, DOCKPT_INFO.LOC, DOCKPT_INFO.BASE_TAG, DOCKPT_INFO.ISDOCKED, DOCKPT_INFO.LAST_PING, DOCKPT_INFO.OUTPUTROLL, "#" + DOCKPT_INFO.ID);

              return DOCKPT_INFO;
          }

          public static DOCKPOINT_INFO SAVE(DOCKPOINT_INFO DOCKPT_INFO)
          {
              DOCKPT_INFO.OUTPUT = string.Join("*", "#" + DOCKPT_INFO.ID, DOCKPT_INFO.LOC, DOCKPT_INFO.BASE_TAG, DOCKPT_INFO.ISDOCKED, DOCKPT_INFO.LAST_PING, DOCKPT_INFO.OUTPUTROLL, "#" + DOCKPT_INFO.ID);
              return DOCKPT_INFO;
          }

      }
      Dictionary<string, DC_INF_INFO> DECENTIN_INFO = new Dictionary<string, DC_INF_INFO>();
      Dictionary<string, DOCKPOINT_INFO> DOCKPOINTS = new Dictionary<string, DOCKPOINT_INFO>();
      Dictionary<string, DRONE_INFO> DRONES = new Dictionary<string, DRONE_INFO>();

      //Symbology Setup
      //-------------------------------------------
      class FIVE_THREE_NUMBERS
      {
          static char N = blue;
          static char[,] NUM_0 = new char[5, 3] { { N, N, N }, { N, B, N }, { N, B, N }, { N, B, N }, { N, N, N } };
          static char[,] NUM_1 = new char[5, 3] { { N, N, B }, { B, N, B }, { B, N, B }, { B, N, B }, { N, N, N } };
          static char[,] NUM_2 = new char[5, 3] { { N, N, N }, { B, B, N }, { N, N, N }, { N, B, B }, { N, N, N } };
          static char[,] NUM_3 = new char[5, 3] { { N, N, N }, { B, B, N }, { N, N, N }, { B, B, N }, { N, N, N } };
          static char[,] NUM_4 = new char[5, 3] { { N, B, B }, { N, B, B }, { N, B, N }, { N, N, N }, { B, B, N } };
          static char[,] NUM_5 = new char[5, 3] { { N, N, N }, { N, B, B }, { N, N, N }, { B, B, N }, { N, N, N } };
          static char[,] NUM_6 = new char[5, 3] { { N, N, N }, { N, B, B }, { N, N, N }, { N, B, N }, { N, N, N } };
          static char[,] NUM_7 = new char[5, 3] { { N, N, N }, { B, B, N }, { N, N, N }, { B, N, B }, { B, N, B } };
          static char[,] NUM_8 = new char[5, 3] { { N, N, N }, { N, B, N }, { N, N, N }, { N, B, N }, { N, N, N } };
          static char[,] NUM_9 = new char[5, 3] { { N, N, N }, { N, B, N }, { N, N, N }, { B, B, N }, { N, N, N } };

          public char[][,] NUMBERS = new char[][,] { NUM_0, NUM_1, NUM_2, NUM_3, NUM_4, NUM_5, NUM_6, NUM_7, NUM_8, NUM_9 };  //list of Numbers for external accessor
      } // Standard Numbers
      FIVE_THREE_NUMBERS SYST_NBRS = new FIVE_THREE_NUMBERS();

      class CURSOR_SYMBOLOGY
      {
          const char L1 = black;
          const char L2 = darkYellow;
          const char L3 = darkMagenta;
          const char L4 = cyan;
          const char L5 = green;
          const char L6 = red;

          //public char[,] Attack = new char[5, 5] {
          //{B,B,B,B,L1},
          //{L1,B,B,L1,B},
          //{B,L1,L1,B,B},
          //{B,L1,L1,B,B},
          //{L1,B,B,L1,B}};

          public char[,] Attack = new char[5, 5] {
          {L6,L1,L1,L1,B},
          {L1,L6,L1,B,B},
          {L1,L1,L6,B,B},
          {L1,B,B,L1,B},
          {B,B,B,B,L1,}};

          public char[,] GoTo = new char[5, 5] {
          {L1,B,L1,B,B},
          {B,L1,B,L1,B},
          {B,L1,B,L1,B},
          {L1,B,L1,B,B},
          {B,B,B,B,B}};

          public char[,] Cursor = new char[5, 5] {
          {L2,L1,L1,L1,B},
          {L1,L2,L1,B,B},
          {L1,L1,L2,B,B},
          {L1,B,B,L1,B},
          {B,B,B,B,L1,}};

          //public char[,] Select = new char[5, 5] {
          // {L2,L2,L2,L2,B},
          // {L2,L2,B,B,B},
          // {L2,B,L2,L2,L2},
          // {L2,B,L2,L2,B},
          // {B,B,L2,B,L2}};

          public char[,] Select = new char[5, 5] {
          {L5,L1,L1,L1,B},
          {L1,L5,L1,B,B},
          {L1,L1,L5,B,B},
          {L1,B,B,L1,B},
          {B,B,B,B,L1,}};

          public char[,] Dock = new char[5, 5] {
           {L3,L3,L3,L3,L3},
           {L3,B,B,B,L3},
           {L3,B,L3,B,L3},
           {B,B,L3,B,B},
           {B,B,B,B,B}};

          public char[,] StdBrd = new char[5, 5] {
           {L4,L4,L4,L4,L4},
           {L4,L4,B,B,L4},
           {L4,B,L4,B,L4},
           {L4,L4,B,L4,L4},
           {B,L4,L4,L4,B}};


      } // Standard Cursor Details
      CURSOR_SYMBOLOGY SYST_CURSOR = new CURSOR_SYMBOLOGY();

      class SHIP_SYMBOLS
      {
          public static char[,] Cruiser = new char[3, 7] {
          { P, P, P, P, P, P, B, },
          { B, P, B, B, B, P, P, },
          { P, P, P, P, P, P, B, }};

          public static char[,] Carrier = new char[3, 7] {
          { P, P, P, P, P, P, P, },
          { P, B, B, B, B, B, P, },
          { P, P, P, P, P, P, P, }};

          public static char[,] Basic_Frigate = new char[3, 7] {
          { P, P, P, P, P, B, B, },
          { B, P, B, B, B, P, B, },
          { B, B, P, P, P, P, P, }};

          public static char[,] HeavyMissile_Frigate = new char[3, 7] {
          { P, P, P, P, P, B, B, },
          { B, P, P, B, P, P, B, },
          { B, B, P, P, P, P, P, }};

          public static char[,] GravCannon_Frigate = new char[3, 7] {
          { P, P, P, P, P, B, B, },
          { B, P, P, B, P, P, B, },
          { B, B, P, P, P, P, P, }};

          public static char[,] Intercept = new char[3, 7] {
          { B, B, B, P, B, B, B, },
          { B, B, P, B, P, B, B, },
          { B, B, B, B, B, B, B, }};

          public static char[,] PrdCarrier = new char[3, 7] {
          { B, B, B, B, B, B, B, },
          { B, B, B, B, B, B, B, },
          { B, B, B, B, B, B, B, }};

          public static char[,] Bomber = new char[3, 7] {
          { B, B, B, B, B, B, B, },
          { B, B, B, B, B, B, B, },
          { B, B, B, B, B, B, B, }};

          public static char[,] MOTH = new char[3, 7] {
          { B, B, B, B, B, B, B, },
          { B, B, B, B, B, B, B, },
          { B, B, B, B, B, B, B, }};

          public Dictionary<string, char[,]> SYST_SYMBLS_PROCEDURAL = new Dictionary<string, char[,]>()
          {{ "CR",Cruiser},
          { "CA",Carrier},
          { "FR",Basic_Frigate},
          { "FG",GravCannon_Frigate},
          { "FM",HeavyMissile_Frigate},
          { "MOTH",MOTH},
          { "IN",Intercept}};

      } // Standard Ship Symbols (11X5, 6,3 IS CENTRE)
      SHIP_SYMBOLS SYST_SYMBLS = new SHIP_SYMBOLS();

      class LETTERING
      {
          public char[,] SELECT = new char[5, 23] {
          {P,P,P,B,P,P,P,B,P,B,B,B,P,P,P,B,P,P,P,B,P,P,P},
          {P,B,B,B,P,B,B,B,P,B,B,B,P,B,B,B,P,B,B,B,B,P,B},
          {P,P,P,B,P,P,P,B,P,B,B,B,P,P,P,B,P,B,B,B,B,P,B},
          {B,B,P,B,P,B,B,B,P,B,B,B,P,B,B,B,P,B,B,B,B,P,B},
          {P,P,P,B,P,P,P,B,P,P,P,B,P,P,P,B,P,P,P,B,B,P,B}};

          public char[,] GOTO = new char[5, 23] {
          {P,P,P,B,P,P,P,B,P,P,P,B,P,P,P,B,B,B,B,B,B,B,B},
          {P,B,B,B,P,B,P,B,B,P,B,B,P,B,P,B,B,B,B,B,B,B,B},
          {P,B,P,B,P,B,P,B,B,P,B,B,P,B,P,B,B,B,B,B,B,B,B},
          {P,B,P,B,P,B,P,B,B,P,B,B,P,B,P,B,B,B,B,B,B,B,B},
          {P,P,P,B,P,P,P,B,B,P,B,B,P,P,P,B,B,B,B,B,B,B,B}};

          public char[,] FOLLOW = new char[5, 23] {
          {P,P,P,B,P,P,P,B,P,B,B,P,B,B,P,P,P,B,P,B,B,B,P},
          {P,B,B,B,P,B,P,B,P,B,B,P,B,B,P,B,P,B,P,B,B,B,P},
          {P,P,P,B,P,B,P,B,P,B,B,P,B,B,P,B,P,B,P,B,P,B,P},
          {P,B,B,B,P,B,P,B,P,B,B,P,B,B,P,B,P,B,P,P,B,P,P},
          {P,B,B,B,P,P,P,B,P,P,B,P,P,B,P,P,P,B,P,B,B,B,P}};

          public char[,] DOCK = new char[5, 23] {
          {P,P,P,B,P,P,P,B,P,P,P,B,P,B,P,B,B,B,B,B,B,B,B},
          {P,B,P,B,P,B,P,B,P,B,B,B,P,B,P,B,B,B,B,B,B,B,B},
          {P,B,P,B,P,B,P,B,P,B,B,B,P,P,B,B,B,B,B,B,B,B,B},
          {P,B,P,B,P,B,P,B,P,B,B,B,P,B,P,B,B,B,B,B,B,B,B},
          {P,P,P,B,P,P,P,B,P,P,P,B,P,B,P,B,B,B,B,B,B,B,B}};

          public char[,] ATTACK = new char[5, 23] {
          {P,P,P,B,P,P,P,B,P,P,P,B,P,P,P,B,P,P,P,B,P,B,P},
          {P,B,P,B,B,P,B,B,B,P,B,B,P,B,P,B,P,B,B,B,P,B,P},
          {P,P,P,B,B,P,B,B,B,P,B,B,P,P,P,B,P,B,B,B,P,P,B},
          {P,B,P,B,B,P,B,B,B,P,B,B,P,B,P,B,P,B,B,B,P,B,P},
          {P,B,P,B,B,P,B,B,B,P,B,B,P,B,P,B,P,P,P,B,P,B,P}};

      } // Standard Ship Symbols (11X5, 6,3 IS CENTRE)
      LETTERING SYST_LETTERING = new LETTERING();

      //Permanently Logged Blocks
      //-----------------------------------
      IMyTextPanel DISPLAY_PANEL;
      IMyProgrammableBlock COMMAND_MODULE;
      IMyShipController CONTROL;
      List<IMyGyro> GYROS = new List<IMyGyro>();

      //User Interface Stored Data
      //-------------------------------------
      List<string> SELECTED_SQDS = new List<string>();
      int UI_SCALE = 20; //UI scale in meters per pixel
      int OFFSETY = 66; //Initial position of me on screen
      int OFFSETX = 50; //Initial position of me on screen
      bool PREV_CLICK = false; //Was previously Clicking
      char[,] MOUSE_SYMB;
      string FR_HOVER;
      string EN_HOVER;
      char[,] TEXT_OUT;
      Vector2D STARTCLICK; //Starting click position for box dragging
      Vector2D CLICKPOS; //position of last command
      int CLICK_TIMER = 0; //Timer for the clickmarker


      //User Interface Screen Limits
      //--------------------------------
      double POS_Y = 50; //Mouse X pos
      double POS_X = 50; //Mouse YPos
      const int ROWS_CT = 100;  //Res Vertical
      const int COLUMNS_CT = 100; //Res Horizontal
      char[] ALLOC = new char[10101]; //Screen Size
      int ROW_UI_START = 7;
      int ROW_UI_END = 94;
      int ROW_ZOOM_ICON = 4;
      int COL_ZOOM_ICON1 = 78;
      int COL_ZOOM_ICON2 = 96;
      int SYST_PROXIMITY = 3;
      int SYST_NUMBERS_ROW = 2;
      int SYST_NUMBERS_COL = 82;


      //System Initialisation
      //--------------------------
      bool UIINIT_HASRUN = false;
      bool SYSTINIT_HASRUN = false;

      //System Saves
      //---------------------
      //StringBuilder VISUALDATA = new StringBuilder();
      String VISUALDATA = "";

      #endregion

      #region MAIN USER INTERFACE
      /*=======================================================================================
        Function: User Interface
        ---------------------------------------
        function will: Take user inputs and convert them to a usable command structure
      //=======================================================================================*/
      int TIMER;
      void Main(string argument)
      {
          try
          {
              //Initialisation 0.25
              //---------------------------
              if (UIINIT_HASRUN == false)
              { UIINIT(); UIINIT_HASRUN = true; }
              //Echo(Runtime.LastRunTimeMs + "");
              try
              {
                  if (SYSTINIT_HASRUN == false)
                  { FC_INIT(); SYSTINIT_HASRUN = true; }
                  //if (TIMER == 1)
                  { SYST_UPDATE(); }
              }
              catch (Exception e) { Echo(e + ""); }

              OP_BAR();
              Echo(VERSION);

              //All Follow Me Command Issuer
              //------------------------------
              if (argument == "ALLFOLLOWME") //All follow me issued
              {
                  foreach (var DRONE in DRONES)
                  { DRONE.Value.COMLOC = "FOLLOW^ME"; }
                  FC_SAVE();
                  return;
              }

              //Draws Background Elements 0.64
              //--------------------------------
              if (TIMER == 0){DRAW_CHART(); }

              //Preliminary Click Reader 0.69
              //--------------------------------------------------
              foreach (var item in GYROS)
              { item.GyroOverride = false; }

              bool ISLEFT_CLICKING = false;
              bool ISRIGHT_CLICKING = false;

              if (CONTROL.IsUnderControl)  //Only if under control
              {
                  //Sets Gyros to avoid spinning a ship while under control
                  foreach (var item in GYROS)
                  { item.GyroOverride = true; }

                  //Generates Mouse Pos
                  POS_Y = MathHelper.Clamp(POS_Y + (CONTROL.RotationIndicator.X) * 0.1, 0, COLUMNS_CT - 1);
                  POS_X = MathHelper.Clamp(POS_X + (CONTROL.RotationIndicator.Y) * 0.1, 0, ROWS_CT - 1);

                  //Clamps Values
                  MathHelper.Clamp(POS_Y, 0, COLUMNS_CT - 1);
                  MathHelper.Clamp(POS_X, 0, ROWS_CT - 1);

                  //Click Reader
                  ISLEFT_CLICKING = CONTROL.RollIndicator < 0;
                  ISRIGHT_CLICKING = CONTROL.RollIndicator > 0;

              }
              //else if (DateTime.Now.Second != 0 || DateTime.Now.Second != 15 || DateTime.Now.Second != 30 || DateTime.Now.Second != 45) //Out Of Cockpit limiter
              //{ return; }

              //Preliminary Assignment
              //------------------------
              MOUSE_SYMB = SYST_CURSOR.Cursor;
              TEXT_OUT = SYST_LETTERING.SELECT;

              //Primary Drawer
              //----------------------------------------------
              if (TIMER == 0)
              {
                  foreach (var item in DECENTIN_INFO) //Draws Items Of Interest
                  { WRT_DEI(item.Value); }
                  var KEYS = new List<string>(DRONES.Keys);
                  for (int i = 0; i < DRONES.Count; i++) //draws Squadrons
                  { DRONES[KEYS[i]] = WRT_SQD(DRONES[KEYS[i]]); }
              }

              //Runs Map Zoom And Scroll
              //----------------------------------------
              MAP_ZOOM_AND_SCROLL(ISLEFT_CLICKING);

              //Runs Squadron Selection
              //--------------------------------------
              SQUAD_SELECT(ISLEFT_CLICKING);

              //Runs Squadron Logic
              //------------------------
              SQUAD_COMMANDS(ISRIGHT_CLICKING, ISLEFT_CLICKING);

              //Click Logger (Called At End Of Operations
              //---------------------------------------------
              PREV_CLICK = false;
              if (ISLEFT_CLICKING || ISRIGHT_CLICKING) { PREV_CLICK = true; }

              //Writes Standard UI Symbols 1ms
              //--------------------------------------------------
              WRT_SYMB(MOUSE_SYMB, (int)POS_Y, (int)POS_X);
              WRT_SYMB(TEXT_OUT, 3, 2);
              //Arrow Writer
              if (CLICKPOS.X > 4 && CLICKPOS.X < 96 && CLICKPOS.Y > 4 && CLICKPOS.Y < 96)
              {
                  if (CLICK_TIMER > 30)
                  {
                      DRAW[(int)CLICKPOS.Y + 2, (int)CLICKPOS.X] = green;
                      DRAW[(int)CLICKPOS.Y - 2, (int)CLICKPOS.X] = green;
                      DRAW[(int)CLICKPOS.Y, (int)CLICKPOS.X - 2] = green;
                      DRAW[(int)CLICKPOS.Y, (int)CLICKPOS.X + 2] = green;
                  }
                  if (CLICK_TIMER > 0)
                  {
                      DRAW[(int)CLICKPOS.Y + 1, (int)CLICKPOS.X] = green;
                      DRAW[(int)CLICKPOS.Y - 1, (int)CLICKPOS.X] = green;
                      DRAW[(int)CLICKPOS.Y, (int)CLICKPOS.X - 1] = green;
                      DRAW[(int)CLICKPOS.Y, (int)CLICKPOS.X + 1] = green;
                  }
              }
              if (CLICK_TIMER > 0)
              {CLICK_TIMER--; }

              Vector2D ME_LOC_LO = new Vector2D(OFFSETX, OFFSETY);
              RASTER(ref ME_LOC_LO);
              if (ME_LOC_LO.X > 00 && ME_LOC_LO.X < 100 && ME_LOC_LO.Y > 10 && ME_LOC_LO.Y < 80)
              {DRAW[(int)ME_LOC_LO.Y, (int)ME_LOC_LO.X] = blue;
              DRAW[(int)ME_LOC_LO.Y + 1, (int)ME_LOC_LO.X] = blue;
              DRAW[(int)ME_LOC_LO.Y - 1, (int)ME_LOC_LO.X] = blue;
              DRAW[(int)ME_LOC_LO.Y, (int)ME_LOC_LO.X - 1] = blue;
              DRAW[(int)ME_LOC_LO.Y, (int)ME_LOC_LO.X + 1] = blue;}

              //Text Writer (Writes to String) (90% of performance is here)
              //----------------------------------
              for (int j = TIMER * 50; j < TIMER * 50 + 50; j++)
              {
                  for (int i = 0; i < COLUMNS_CT; i++)
                  { ALLOC[j * 100 + (j + 1) + i] = DRAW[j, i]; }
                  ALLOC[j * 101] = '\n';
              }
              Random rand = new Random();
              ALLOC[0] = (char)rand.Next(0, 9);// rgb((rand.Next(0, 240)), rand.Next(0, 240), rand.Next(0, 240));
              string VISUALDATA = new string(ALLOC);
              if (TIMER < 1)
              { TIMER++; return; }

              //Temp Writer (writes To Panel)
              //-------------------------------
              //DISPLAY_PANEL.Enabled = true;
              VISUALDATA = VISUALDATA.Replace(" ", " " + '\uE073' + '\uE072');
              //DISPLAY_PANEL.ShowTextureOnScreen();
              DISPLAY_PANEL.WritePublicText(VISUALDATA);
              DRAW = BDRAW.Clone() as char[,]; //Reassigns after writing
              DISPLAY_PANEL.ShowPublicTextOnScreen();
              DISPLAY_PANEL.SetValue("FontSize", 0.178f); //Value for 100pix
              DISPLAY_PANEL.SetValue<long>("Font", 1147350002); //Sets as monospace
              TIMER = 0;
          }
          catch (Exception e)
          { Echo(e + ""); return; }

      }
      #endregion

      //Drawing Functions (Called in Load Order)

      #region Draw Static data
      /*=======================================================================================
        Function: Symbol Writer  0.4ms
        ---------------------------------------
        function will: Scroll the background and write any static data
      //=======================================================================================*/
      void DRAW_CHART()
      {

          //Generating map Scale
          int TENS = (int)Math.Floor(UI_SCALE / 200.0);
          int UNITS = (int)Math.Floor((UI_SCALE - TENS * 200) / 10.0);

          double SCALE = 60 - (UNITS);

          //Generates Unit Generation
          int TENSY = (int)Math.Floor(OFFSETY / SCALE);
          int UNITSY = (int)Math.Floor((OFFSETY - TENSY * SCALE));

          int TENSX = (int)Math.Floor(OFFSETX / SCALE);
          int UNITSX = (int)Math.Floor((OFFSETX - TENSX * SCALE));

          //Sets Up Line Boundaries
          double X_LINE1 = 50 + SCALE / 2; //*1+ (UNITS+1)/20;
          double X_LINE2 = 50 - SCALE / 2; //*1+ (UNITS + 1) / 20;
          double X_LINE3 = 50 - SCALE * 1.5; //*1+ (UNITS + 1) / 20;
          double X_LINE4 = 50 - SCALE * 2.5; //*1+ (UNITS + 1) / 20;

          double Y_LINE1 = 0; //* 1 + (UNITS + 1) / 20;
          double Y_LINE2 = SCALE; //* 1 + (UNITS + 1) / 20;
          double Y_LINE3 = SCALE * 2; //* 1 + (UNITS + 1) / 20;

          //Draws Line One
          Vector2D COORD_START = new Vector2D(X_LINE1 + UNITSX, ROW_UI_START);
          Vector2D COORD_FINISH = new Vector2D(X_LINE1 + UNITSX, ROW_UI_END);
          RASTER(ref COORD_START);
          RASTER(ref COORD_FINISH);
          line(COORD_START, COORD_FINISH, mediumGray);

          //Draws Vertical Line 2
          COORD_START = new Vector2D(X_LINE2 + UNITSX, ROW_UI_START);
          COORD_FINISH = new Vector2D(X_LINE2 + UNITSX, ROW_UI_END);
          RASTER(ref COORD_START);
          RASTER(ref COORD_FINISH);
          line(COORD_START, COORD_FINISH, mediumGray);

          //Draws Vertical Line 3
          COORD_START = new Vector2D(X_LINE3 + UNITSX, ROW_UI_START);
          COORD_FINISH = new Vector2D(X_LINE3 + UNITSX, ROW_UI_END);
          RASTER(ref COORD_START);
          RASTER(ref COORD_FINISH);
          line(COORD_START, COORD_FINISH, mediumGray);

          //Draws Vertical Line 4
          COORD_START = new Vector2D(X_LINE4 + UNITSX, ROW_UI_START);
          COORD_FINISH = new Vector2D(X_LINE4 + UNITSX, ROW_UI_END);
          RASTER(ref COORD_START);
          RASTER(ref COORD_FINISH);
          line(COORD_START, COORD_FINISH, mediumGray);


          //Draws Horizontal Line 1
          COORD_START = new Vector2D(-15, UNITSY + Y_LINE1);
          COORD_FINISH = new Vector2D(110, UNITSY + Y_LINE1);
          RASTER(ref COORD_START);
          RASTER(ref COORD_FINISH);
          line(COORD_START, COORD_FINISH, mediumGray);

          //Draws Horizontal Line 2
          COORD_START = new Vector2D(-15, UNITSY + Y_LINE2);
          COORD_FINISH = new Vector2D(110, UNITSY + Y_LINE2);
          RASTER(ref COORD_START);
          RASTER(ref COORD_FINISH);
          line(COORD_START, COORD_FINISH, mediumGray);

          //Draws Horizontal Line 3
          COORD_START = new Vector2D(-15, UNITSY + Y_LINE3);
          COORD_FINISH = new Vector2D(110, UNITSY + Y_LINE3);
          RASTER(ref COORD_START);
          RASTER(ref COORD_FINISH);
          line(COORD_START, COORD_FINISH, mediumGray);

          //Writes Current Ship Counts To Bottom of Screen
          //------------------------------------------------

          //Writes Fighter Craft
          int INTER_CT = 0;
          int FRIG_CT = 0;
          int CRUIS_CT = 0;
          int UTIL_CT = 0;

          foreach (var item in DRONES)
          {
              if (item.Value.ID.Substring(0, 1)=="I") { INTER_CT++; }
              if (item.Value.ID.Substring(0, 1)=="F") { FRIG_CT++; }
              if (item.Value.ID.Substring(0, 1)=="C") { CRUIS_CT++; }
              if (item.Value.ID.Substring(0, 1)=="U") { UTIL_CT++; }

          }

          //Writes Interceptors
          int TENS_I = (int)Math.Floor(INTER_CT / 10.0);
          int UNITS_I = (int)Math.Floor((INTER_CT - TENS_I * 10) / 1.0);
          WRT_SYMB(SYST_NBRS.NUMBERS[TENS_I], 95, 13);
          WRT_SYMB(SYST_NBRS.NUMBERS[UNITS_I], 95, 17);

          //Writes Frigates
          int TENS_F = (int)Math.Floor(FRIG_CT / 10.0);
          int UNITS_F = (int)Math.Floor((FRIG_CT - TENS_F * 10) / 1.0);
          WRT_SYMB(SYST_NBRS.NUMBERS[TENS_F], 95, 33);
          WRT_SYMB(SYST_NBRS.NUMBERS[UNITS_F], 95, 37);

          //Writes Cruisers
          int TENS_C = (int)Math.Floor(CRUIS_CT / 10.0);
          int UNITS_C = (int)Math.Floor((CRUIS_CT - TENS_C * 10) / 1.0);
          WRT_SYMB(SYST_NBRS.NUMBERS[TENS_C], 95, 55);
          WRT_SYMB(SYST_NBRS.NUMBERS[UNITS_C], 95, 59);

          //Writes Cruisers
          int TENS_U = (int)Math.Floor(UTIL_CT / 10.0);
          int UNITS_U = (int)Math.Floor((UTIL_CT - TENS_C * 10) / 1.0);
          WRT_SYMB(SYST_NBRS.NUMBERS[TENS_U], 95, 75);
          WRT_SYMB(SYST_NBRS.NUMBERS[UNITS_U], 95, 79);

      }
      #endregion

      #region Map Zoom And Scroll
      /*=======================================================================================
        Function: Symbol Writer
        ---------------------------------------
        function will: Use Information and show and display zoom and map function
      //=======================================================================================*/
      public void MAP_ZOOM_AND_SCROLL(bool ISLEFT_CLICKING)
      {
          //Map Zoom Logger Writer And Functioner
          //------------------------------------------------
          if (Math.Abs(POS_X - COL_ZOOM_ICON1) < 3 && Math.Abs(POS_Y - ROW_ZOOM_ICON) < 4)  //Checks Location + Highlights if so
          {
              MOUSE_SYMB = SYST_CURSOR.Select;
              DRAW[ROW_ZOOM_ICON, COL_ZOOM_ICON1] = yellow; //Highlights icons
              if (ISLEFT_CLICKING && PREV_CLICK == false)
              { UI_SCALE = MathHelper.Clamp(UI_SCALE + 10, 10, 990); OFFSETX = COLUMNS_CT / 2; OFFSETY = COLUMNS_CT / 2; }
          }
          if (Math.Abs(POS_X - COL_ZOOM_ICON2) < 3 && Math.Abs(POS_Y - ROW_ZOOM_ICON) < 4)
          {
              MOUSE_SYMB = SYST_CURSOR.Select;
              DRAW[ROW_ZOOM_ICON, COL_ZOOM_ICON2] = yellow; //Highlights icons
              if (ISLEFT_CLICKING && PREV_CLICK == false)
              { UI_SCALE = MathHelper.Clamp(UI_SCALE - 10, 10, 990); OFFSETX = COLUMNS_CT / 2; OFFSETY = COLUMNS_CT / 2; }
          }

          //Converts To Digital Format And Writes
          //----------------------------------------
          int TENS = (int)Math.Floor(UI_SCALE / 100.0);
          int UNITS = (int)Math.Floor((UI_SCALE - TENS * 100) / 10.0);

          WRT_SYMB(SYST_NBRS.NUMBERS[TENS], SYST_NUMBERS_ROW, SYST_NUMBERS_COL);
          WRT_SYMB(SYST_NBRS.NUMBERS[UNITS], SYST_NUMBERS_ROW, SYST_NUMBERS_COL + 4);

          //Mouse Scroll Converter
          //----------------------------------------
          if ((POS_X < 1))
          { OFFSETX = OFFSETX + 1; CLICKPOS.X++; }
          if ((POS_X == COLUMNS_CT-1))
          { OFFSETX = OFFSETX - 1; CLICKPOS.X--; }

          if ((POS_Y < 1))
          { OFFSETY = OFFSETY + 1; CLICKPOS.Y++; }
          if ((POS_Y == COLUMNS_CT - 1))
          { OFFSETY = OFFSETY - 1; CLICKPOS.Y--; }
      }
      #endregion

      #region Drone Drawer
      /*=======================================================================================
        Function: Symbol Writer
        ---------------------------------------
        function will: Writes a symbol at the given coordinates, system will error check itself
      //=======================================================================================*/
      DRONE_INFO WRT_SQD(DRONE_INFO SHIP)
      {
          try
          {
              ////Generates Case
              ////------------------------------
              //bool IS_DRONE = SHIP == null;
              //bool IS_EN = DEI == null && DEI.TYPE == "EN";
              //bool IS_AST = DEI == null && DEI.TYPE == "AS";
              //bool IS_SMALL_CLASS = SHIP.ID.Substring(0,1) == "I";
              //bool IS_FRIG_CLASS = SHIP.ID.Substring(0, 1) == "F";
              //bool IS_CRUISER_CLASS = SHIP.ID.Substring(0, 1) == "C";



              //If ship is docked to location don't show it
              if (SHIP.COMLOC.Contains("DOCK") == true)
              {
                  if ((SHIP.LOC - DOCKPOINTS[SHIP.ISDOCKED].LOC).Length() < 100)
                  { return SHIP; }
              }

              //Establishes Self Positions
              //---------------------------
              Vector3D MEPOS = Me.GetPosition();
              Vector3D MEPRIGHT = Me.WorldMatrix.Right;
              Vector3D MEPOSUP = Me.WorldMatrix.Up;
              Vector3D MEPDOWN = -Me.WorldMatrix.Forward;

              //Draws Docked Ship Counter At Bottom Of Screen
              //------------------------------------------------
              if (SHIP.ID.Substring(0, 2).Contains("I") == false && SELECTED_SQDS.Count > 0)
              {
                  if (SELECTED_SQDS[0] == SHIP.ID)
                  {
                      List<DOCKPOINT_INFO> TEMP_DOCKS = new List<DOCKPOINT_INFO>();
                      foreach (var item in DOCKPOINTS)
                      {
                          if (item.Value.ISDOCKED == "NC" && item.Value.BASE_TAG == SELECTED_SQDS[0])
                          { TEMP_DOCKS.Add(item.Value); }
                      }

                      int TENS = (int)Math.Floor(TEMP_DOCKS.Count / 10.0);
                      int UNITS = (int)Math.Floor((TEMP_DOCKS.Count - TENS * 10) / 1.0);

                      WRT_SYMB(SYST_NBRS.NUMBERS[TENS], 85, 91);
                      WRT_SYMB(SYST_NBRS.NUMBERS[UNITS], 85, 95);
                  }
              }

              //Initialises Health Data
              //---------------------------
              Vector3D SHIPPOS = SHIP.LOC;
              int SIZE = (int)SHIP.HEALTH;
              double HEALTH = SHIP.HEALTH - (int)SHIP.HEALTH;
              char DISPLAY_COLOUR = lightGray;  //Background of own ships display colour

              //Gets me To Target And Finds Projections
              //------------------------------------------------------
              Vector3D ME_TO_TARG = Vector3D.Normalize(SHIPPOS - MEPOS); //Gets Me To Target
              double ME_TOTARG_LEN = (SHIPPOS - MEPOS).Length();
              double X_ORD = Math.Round(ME_TOTARG_LEN * Vector_Projection(ME_TO_TARG, MEPRIGHT)); //(capped?) //forward (100high) finds projection in forward
              double Y_ORD = Math.Round(ME_TOTARG_LEN * Vector_Projection(ME_TO_TARG, MEPDOWN)); //down, (100 high)
              double Z_ORD = Math.Round(ME_TOTARG_LEN * Vector_Projection(ME_TO_TARG, MEPOSUP)); //height (- value = higher on screen)

              X_ORD = X_ORD / UI_SCALE; //divides by scale
              Y_ORD = Y_ORD / UI_SCALE;
              Z_ORD = Z_ORD / UI_SCALE;

              int BASEX = (int)Math.Round(X_ORD + OFFSETX); //generates base position
              int BASEY = (int)Math.Round(Y_ORD + OFFSETY);
              int BASEZ = 0;
              Vector2D RASTERED_BASE = new Vector2D(BASEX, BASEY);

              //Stops Processing if Out Of Range Prior To Rasterising
              if (RASTERED_BASE.Y < -40 || RASTERED_BASE.Y > 140)
              { return SHIP; }

              RASTER(ref RASTERED_BASE);
              BASEX = (int)RASTERED_BASE.X;
              BASEY = (int)RASTERED_BASE.Y;
              BASEZ = (int)Math.Round(BASEY - Z_ORD);
              Vector2D RASTERED_SQUAD = new Vector2D(BASEX, BASEZ);

              //Draws Symbol
              //---------------------------------
              SHIP.UIPOS = new Vector2D(-100, -100); //Position For Further lookup Operations
              if (BASEZ > 4 && BASEZ < 95)
              {
                  SHIP.UIPOS = new Vector2D(BASEX, BASEZ); //Position For Further lookup Operations
                  DRAW_UI_SHP(RASTERED_BASE, RASTERED_SQUAD, DISPLAY_COLOUR, SIZE, lightGray, SHIP.ID.Substring(0, 2));
              }

              //Retrieves Mouse Loc For Drawing Health
              //-----------------------------------------------
              if ((Math.Abs(POS_X - (BASEX)) < 2 && Math.Abs(POS_Y - (BASEZ)) < 2) || SELECTED_SQDS.Contains(SHIP.ID))
              {
                  for (int i = 0; i < 7; i++)
                  {
                      char VAL = (HEALTH * 7 < i) ? red : green;
                      DRAW[BASEZ - 2, BASEX - 3 + i] = VAL;
                  }
                  MOUSE_SYMB = SYST_CURSOR.Select;
              }
          }
          catch (Exception e) { Echo("Error During Squadron Drawer" + e); }
          return SHIP;
      }
      #endregion

      #region Object Drawer
      /*=======================================================================================
        Function: Symbol Writer
        ---------------------------------------
        function will: Writes a symbol at the given coordinates, system will error check itself
      //=======================================================================================*/
      void WRT_DEI(DC_INF_INFO SQUAD)
      {
          try
          {

              //Establishes Self Positions
              Vector3D MEPOS = Me.GetPosition();
              Vector3D MEPRIGHT = Me.WorldMatrix.Right;
              Vector3D MEPOSUP = Me.WorldMatrix.Up;
              Vector3D MEPDOWN = -Me.WorldMatrix.Forward;

              //Loads Squadron Data
              Vector3D SHIPPOS = SQUAD.POSITION;
              char SYS_COLOUR = (SQUAD.TYPE != "EN") ? darkGray : red;

              //Gets me To Target And Finds Projections
              //------------------------------------------------------
              Vector3D ME_TO_TARG = Vector3D.Normalize(SHIPPOS - MEPOS); //Gets Me To Target
              double ME_TOTARG_LEN = (SHIPPOS - MEPOS).Length();
              double X_ORD = Math.Round(ME_TOTARG_LEN * Vector_Projection(ME_TO_TARG, MEPRIGHT)); //(capped?) //forward (100high) finds projection in forward
              double Y_ORD = Math.Round(ME_TOTARG_LEN * Vector_Projection(ME_TO_TARG, MEPDOWN)); //down, (100 high)
              double Z_ORD = Math.Round(ME_TOTARG_LEN * Vector_Projection(ME_TO_TARG, MEPOSUP)); //height (- value = higher on screen)

              X_ORD = X_ORD / UI_SCALE; //divides by scale
              Y_ORD = Y_ORD / UI_SCALE;
              Z_ORD = Z_ORD / UI_SCALE;

              int BASEX = (int)Math.Round(X_ORD + OFFSETX); //generates base position
              int BASEY = (int)Math.Round(Y_ORD + OFFSETY);
              int BASEZ = 0;
              Vector2D RASTERED_BASE = new Vector2D(BASEX, BASEY);

              //Stops Processing if Out Of Range Prior To Rasterising
              if (RASTERED_BASE.Y < -40 || RASTERED_BASE.Y > 140)
              { return; }

              RASTER(ref RASTERED_BASE);
              BASEX = (int)RASTERED_BASE.X;
              BASEY = (int)RASTERED_BASE.Y;
              BASEZ = (int)Math.Round(BASEY - Z_ORD);
              Vector2D RASTERED_SQUAD = new Vector2D(BASEX, BASEZ);
              SQUAD.UIPOS = new Vector2D(-100, -100); //Position For Further lookup Operations

              //Draws Symbol
              //-----------------------------------------------
              if (BASEZ > 7 && BASEZ < 95)
              {
                  line(RASTERED_SQUAD, RASTERED_BASE, lightGray);
                  SQUAD.UIPOS = new Vector2D(BASEX, BASEZ); //Sets UI position
                  int HEIGHT = MathHelper.Clamp((SQUAD.TYPE == "EN") ? (int)SQUAD.ST_SIZE / UI_SCALE / 2 : SQUAD.ST_SIZE / UI_SCALE,1,30);
                  int LENGTH = MathHelper.Clamp(SQUAD.ST_SIZE / UI_SCALE, 1, 30);
                  for (int j = 0; j < HEIGHT; j++)
                  {
                      for (int i = 0; i < LENGTH; i++)
                      {
                          DRAW[BASEZ - HEIGHT / 2 + j, BASEX - LENGTH / 2 + i] = SYS_COLOUR;
                      }
                  }
              }


              //Draws Enemy Health
              //-----------------------------------------------
              if ((Math.Abs(POS_X - (BASEX)) < 4 && Math.Abs(POS_Y - (BASEZ)) < 4))
              {
                  for (int i = 0; i < 11; i++)
                  {
                      char VAL = ((SQUAD.ST_SIZE / SQUAD.SIZE) * 11 < i) ? red : green;
                      DRAW[BASEZ - 3, BASEX - 5 + i] = VAL;
                  }
              }
          }
          catch (Exception e) { Echo("Error During DEI Drawer" + e); }
      }
      #endregion

      //System Command Setters/Getters

      #region FC Update
      /*=======================================================================================
        Function: FC Update        0.25ms
        ---------------------------------------
        function will: Update commands for fleet command system, regenerate the structs
        Drone input:  //no need for this? as all is stored in unit, yes, ship quantities
      //=======================================================================================*/
      void SYST_UPDATE()
      {
          //Delogger For Detected Entities
          //----------------------------------
          List<string> keys_DEI = new List<string>(DECENTIN_INFO.Keys);
          for (int i = 0; i < keys_DEI.Count; i++)
          { if (DECENTIN_INFO[keys_DEI[i]].TYPE == "EN") { DECENTIN_INFO.Remove(keys_DEI[i]); } }

          //Delogs Dockpoints
          //--------------------------------------------------------
          List<string> DOCKKEYS = new List<string>(DOCKPOINTS.Keys);
          for (int i = 0; i < DOCKPOINTS.Count; i++)
          {
              if (DateTime.Now.Ticks - ((DateTime)(DOCKPOINTS[DOCKKEYS[i]]).LAST_PING).Ticks > 50000000)
              { DOCKPOINTS.Remove(DOCKKEYS[i]); continue; }
          }

          //Delogs Drones
          //------------------------------
          List<string> KEYS = new List<string>(DRONES.Keys);
          for (int i = 0; i < DRONES.Count; i++)
          {
              if (DateTime.Now.Ticks - ((DateTime)(DRONES[KEYS[i]]).LAST_PING).Ticks > 50000000)
              { DRONES.Remove(KEYS[i]); continue; }
          }

          //Loads CustomData
          //---------------------------------
          string CUSTDATA = COMMAND_MODULE.CustomData;
          string SQ_DATA = CUSTDATA.Split(new string[] { "##INFO##" }, StringSplitOptions.None)[0]; //0 = sqdata
          string[] SQ_DATA_LIST = SQ_DATA.Split(new string[] { "\n" }, StringSplitOptions.None);
          string DK_DATA = CUSTDATA.Split(new string[] { "##INFO##" }, StringSplitOptions.None)[1]; //0 = sqdata
          string[] DK_DATA_LIST = DK_DATA.Split(new string[] { "\n" }, StringSplitOptions.None);
          string DEI_DATA = CUSTDATA.Split(new string[] { "##INFO##" }, StringSplitOptions.None)[2];
          string[] DEI_DATA_LIST = DEI_DATA.Split(new string[] { "\n" }, StringSplitOptions.None);

          foreach (var item in SQ_DATA_LIST)
          {
              //Case, Entry Is A Drone
              //-------------------------------------------------
              if (item.Split('*').Length < 7) { continue; }
              string DRONE_ID = item.Split('*')[0].Substring(1, 6);
              if (DRONES.ContainsKey(DRONE_ID) == false && item.Split('*').Length > 8) //Creates New Entry
              {
                  DRONE_INFO NEW_DRONE = new DRONE_INFO();
                  bool[] UPDATE_RUN = new bool[] { false, true, true, true, true, true, true, true, true, false };
                  NEW_DRONE = DRONE_INFO.DRONE_DATA_RS(item, NEW_DRONE, UPDATE_RUN);
                  NEW_DRONE.GLOC = NEW_DRONE.LOC + "";
                  NEW_DRONE.LAST_PING = DateTime.Now;
                  NEW_DRONE.ID = DRONE_ID;
                  DRONES.Add(DRONE_ID, NEW_DRONE);
                  Echo("Sucessfully Added New Drone " + DRONES.Count + "");
              }
              else if (item.Split('*').Length > 8) //Updates Entry
              {
                  bool[] UPDATE_RUN = new bool[] { false, true, true, true, true, true, true, true, true, false };
                  DRONES[DRONE_ID] = DRONE_INFO.DRONE_DATA_RS(item, DRONES[DRONE_ID], UPDATE_RUN);
              }
          }

          foreach (var item in DK_DATA_LIST)
          {
              //Case, Does Contain Dock
              //---------------------------------------------
              if (item.Split('*').Length < 4 ) { continue; }
              string DCK_ID = item.Split('*')[0].Substring(1, 6);

              if (DOCKPOINTS.ContainsKey(DCK_ID) == false && item.Split('*').Length > 4)
              {
                  DOCKPOINT_INFO NEW_DOCKPT = new DOCKPOINT_INFO();
                  bool[] DOCKPOINT_INPUT = new bool[] { false, true, true, true, true, true };
                  NEW_DOCKPT = DOCKPOINT_INFO.DOCK_DATA_RS(item, NEW_DOCKPT, DOCKPOINT_INPUT);
                  NEW_DOCKPT.ID = DCK_ID;
                  NEW_DOCKPT.LAST_PING = DateTime.Now;
                  DOCKPOINTS.Add(DCK_ID, NEW_DOCKPT);
                  Echo("Sucessfully Added New Dockpoint " + DOCKPOINTS.Count + "");
              }
              else if (item.Split('*').Length > 4)
              {
                  bool[] DOCKPOINT_INPUT = new bool[] { false, true, true, true, true, true };
                  DOCKPOINTS[DCK_ID] = DOCKPOINT_INFO.DOCK_DATA_RS(item, DOCKPOINTS[DCK_ID], DOCKPOINT_INPUT);
              }
          }

          foreach (var item in DEI_DATA_LIST)
          {
              //Case, Item is a DEI
              //---------------------------------------------

              if (item.Split('^').Length < 3)
              { continue; } //passes on error

              //DEI^ID^SIZE^TYPE^LOCATION
              var DATA_STR = item.Split('^');
              string ID = DATA_STR[0];
              int SIZE = int.Parse(DATA_STR[1]);
              string TYPE = DATA_STR[2];
              Vector3D TRIAL_POS_AS; Vector3D.TryParse(DATA_STR[3], out TRIAL_POS_AS);

              if (DECENTIN_INFO.ContainsKey(ID) == false) //If unlogged
              {
                  DC_INF_INFO NEW_ENTITY = new DC_INF_INFO();

                  NEW_ENTITY.SIZE = SIZE;
                  NEW_ENTITY.ST_SIZE = SIZE;
                  NEW_ENTITY.TYPE = TYPE;
                  NEW_ENTITY.POSITION = TRIAL_POS_AS;
                  DECENTIN_INFO.Add(ID, NEW_ENTITY);
                  Echo("Sucessfully Added New DEI " + DECENTIN_INFO.Count + "");
              }
              else //if logged
              {
                  DECENTIN_INFO[ID].SIZE = SIZE;
                  DECENTIN_INFO[ID].TYPE = TYPE;
                  DECENTIN_INFO[ID].POSITION = TRIAL_POS_AS;
              }
          }

      }
      #endregion

      #region FC Save
      /*=======================================================================================
        Function: FC Save
        ---------------------------------------
        function will: Update commands for fleet command system, regenerate the structs
        Drone input:  //no need for this? as all is stored in unit, yes, ship quantities
      //=======================================================================================*/
      void FC_SAVE()
      {
          //Retrieves Current Data
          string CUSTDATA = COMMAND_MODULE.CustomData;

          //Saves Data
          StringBuilder CUSTSAVE = new StringBuilder();
          var KEYS = new List<string>(DRONES.Keys);
          for (int i = 0; i < DRONES.Count; i++)
          {
              DRONES[KEYS[i]] = DRONE_INFO.SAVE(DRONES[KEYS[i]]);
              CUSTSAVE.Append(DRONES[KEYS[i]].OUTPUT);
              CUSTSAVE.Append("\n");
          }
          CUSTSAVE.Append("##INFO##");
          CUSTSAVE.Append(CUSTDATA.Split(new string[] { "##INFO##" }, StringSplitOptions.None)[1]);
          CUSTSAVE.Append("##INFO##");
          CUSTSAVE.Append(CUSTDATA.Split(new string[] { "##INFO##" }, StringSplitOptions.None)[2]);

          COMMAND_MODULE.CustomData = CUSTSAVE + "";
      }
      #endregion

      #region Squadron Selection And Assignment
      /*=======================================================================================
        Function: Squadron Assignment
        ---------------------------------------
        function will: Use Information and show and display zoom and map function
      //=======================================================================================*/
      public void SQUAD_SELECT(bool ISLEFT_CLICKING)
      {
          //Select Logger/Clearer
          //---------------------------------------
          if (ISLEFT_CLICKING && PREV_CLICK == false)
          { STARTCLICK = new Vector2D(POS_X, POS_Y); }
          if (ISLEFT_CLICKING)
          {

              //Pre Checks, Does Not Reselect if:
              if (SELECTED_SQDS.Count == 1 && POS_X > 87 && POS_Y > 91 && SELECTED_SQDS[0].Contains("I") == false)
              { MOUSE_SYMB = SYST_CURSOR.Dock; TEXT_OUT = SYST_LETTERING.DOCK; return; }
              SELECTED_SQDS = new List<string>();

              //Draws Box To Current Position
              line(STARTCLICK, new Vector2D(POS_X, STARTCLICK.Y), green);
              line(STARTCLICK, new Vector2D(STARTCLICK.X, POS_Y), green);
              line(new Vector2D(STARTCLICK.X, POS_Y), new Vector2D(POS_X, POS_Y), green);
              line(new Vector2D(POS_X, STARTCLICK.Y), new Vector2D(POS_X, POS_Y), green);

              //Adds To Currently Selected Squads
              foreach (var item in DRONES)
              {
                  //Docking De-Drawer(hides if close/docked)
                  if (item.Value.COMLOC.Contains("DOCK") && DOCKPOINTS.ContainsKey(item.Value.ISDOCKED))
                  {
                      if ((item.Value.LOC - DOCKPOINTS[item.Value.ISDOCKED].LOC).Length() < 100)
                      { continue; }
                  }

                  if (Math.Abs(POS_X - item.Value.UIPOS.X) < 4 && Math.Abs(POS_Y - item.Value.UIPOS.Y) < 4) // || POS_Y > 93 && Math.Abs(POS_X - (4 * item.Key + 2)) < 1
                  {
                      SELECTED_SQDS.Add(item.Value.ID); MOUSE_SYMB = SYST_CURSOR.Select;
                      if (POS_X - STARTCLICK.X < 1 && POS_Y - STARTCLICK.Y < 1 && POS_X - STARTCLICK.X > -1 && POS_Y - STARTCLICK.Y > -1)
                      { break; } continue; //Delogs and allows for only one click if very close
                  }

                  if (Math.Abs(item.Value.UIPOS.X) > STARTCLICK.X && Math.Abs(item.Value.UIPOS.X) < POS_X && Math.Abs(item.Value.UIPOS.Y) < POS_Y && Math.Abs(item.Value.UIPOS.Y) > STARTCLICK.Y && item.Value.ID != "CA0000") // || POS_Y > 93 && Math.Abs(POS_X - (4 * item.Key + 2)) < 1
                  { SELECTED_SQDS.Add(item.Value.ID); MOUSE_SYMB = SYST_CURSOR.Select; } //selects if close
              }
          }

          //Rejiggers List To Sort By Largest To Smallest Ships (Also Coincidentally Alphabetical )
          SELECTED_SQDS.Sort();
      }
      #endregion

      #region Squadron Command Generator
      /*=======================================================================================
        Function: Squadron Assignment
        ---------------------------------------
        function will: Use Information and show and display zoom and map function
      //=======================================================================================*/
      public void SQUAD_COMMANDS(bool ISRIGHT_CLICKING, bool ISLEFTCLICKING)
      {
          //Mouse Command Recogniser (Draws Current Command)
          //-------------------------------------------------------
          if (SELECTED_SQDS.Count > 0) //Only if a squad is selected
          {
              MOUSE_SYMB = SYST_CURSOR.Select;
              FR_HOVER = "999";
              TEXT_OUT = SYST_LETTERING.GOTO;
              foreach (var item in DRONES) //For Each Friendly Squad, dock/follow matrix
              {
                  if (Math.Abs(POS_X - item.Value.UIPOS.X) < 4 && Math.Abs(POS_Y - item.Value.UIPOS.Y) < 4 && item.Value.ID != SELECTED_SQDS[0]) //dock carrier, follow other
                  {
                      MOUSE_SYMB = SYST_CURSOR.StdBrd;
                      TEXT_OUT = SYST_LETTERING.FOLLOW;
                      if ((item.Value.ID.Substring(0, 2) == "CA" || item.Value.ID.Substring(0, 2) == "FR" || item.Value.ID.Substring(0, 2) == "CR") && (DRONES[SELECTED_SQDS[0]].ID.Substring(0, 2) == "IB" || DRONES[SELECTED_SQDS[0]].ID.Substring(0, 2) == "IN"))
                      { MOUSE_SYMB = SYST_CURSOR.Dock; TEXT_OUT = SYST_LETTERING.DOCK; }
                      FR_HOVER = item.Key;
                  }
              }

              foreach (var item in DECENTIN_INFO) //For Each Enemy Squad Mouse Over
              {
                  if (Math.Abs(POS_X - item.Value.UIPOS.X) < 4 && Math.Abs(POS_Y - item.Value.UIPOS.Y) < 4) //dock carrier, follow other
                  { MOUSE_SYMB = SYST_CURSOR.Attack; EN_HOVER = item.Key; TEXT_OUT = SYST_LETTERING.ATTACK; }
              }
          }
          Echo(DOCKPOINTS.Count + " Dockpoint Count");

          //Little Snippet To Draw Symbols For Undocking
          if (SELECTED_SQDS.Count == 1 && POS_X > 87 && POS_Y > 91 && SELECTED_SQDS[0].Contains("I") == false)
          { TEXT_OUT = SYST_LETTERING.DOCK; MOUSE_SYMB = SYST_CURSOR.Dock; }


          //Simple Undock Command Generator
          //---------------------------------
          if (SELECTED_SQDS.Count == 1 && ISLEFTCLICKING && PREV_CLICK == false && POS_X > 87 && POS_Y > 91 )
          {

              //scans through lists of docks for itself
              List<DOCKPOINT_INFO> TEMP_DOCKS = new List<DOCKPOINT_INFO>();
              foreach (var item in DOCKPOINTS)
              {
                  if (item.Value.ISDOCKED == "NC" && item.Value.BASE_TAG == SELECTED_SQDS[0])
                  { TEMP_DOCKS.Add(item.Value); }
              }

              //Finds First Matching Ship And Sets to Undock
              DRONE_INFO UNDOCK_DRONE = new DRONE_INFO();
              if (TEMP_DOCKS.Count > 0)
              {
                  foreach (var item in DRONES)
                  {
                      if (item.Value.COMLOC.Contains(TEMP_DOCKS[0].ID))
                      { UNDOCK_DRONE = item.Value; break; }
                  }
              }
              UNDOCK_DRONE.COMLOC = "FOLLOW^" + SELECTED_SQDS[0];
              FC_SAVE();
          }


          //Mouse Command Generator (Creates Current Command)
          //-------------------------------------------------------
          if (SELECTED_SQDS.Count > 0 && ISRIGHT_CLICKING && PREV_CLICK == false) //Only if a squad is selected and mouse is hovering
          {

              //Runs Docking Scenario
              //-------------------------
              if (MOUSE_SYMB == SYST_CURSOR.Dock && (SELECTED_SQDS[0].Substring(0, 2) == "IN")) //Dock Ships
              {
                  //Runs Through Each Drone To Dock Respective Ships
                  for (int i = 0; i < SELECTED_SQDS.Count; i++)
                  {
                      DRONES[SELECTED_SQDS[i]].COMLOC = "DOCK^" + FR_HOVER;
                  }
                  FC_SAVE();
                  SELECTED_SQDS = new List<string>();
                  return; //Ends after docking (prevents follow scenario)
              }

              //Runs Follow Scenario
              //-------------------------
              if (MOUSE_SYMB == SYST_CURSOR.StdBrd) //Follow
              { DRONES[SELECTED_SQDS[0]].COMLOC = "FOLLOW^" + FR_HOVER; }

              //Runs GOTO Scenario
              //-------------------------
              if (MOUSE_SYMB == SYST_CURSOR.Select) //GoTo
              {
                  CLICK_TIMER = 60;
                  CLICKPOS = new Vector2D(POS_X,POS_Y);
                  Vector2D DERASTE_IN = new Vector2D(POS_X, POS_Y);
                  DERASTER(ref DERASTE_IN);
                  double BASEX = (DERASTE_IN.X - OFFSETX); //generates base position
                  double BASEY = (DERASTE_IN.Y - OFFSETY);

                  Vector3D MEPOS = Me.GetPosition();
                  Vector3D MEPRIGHT = Me.WorldMatrix.Right * UI_SCALE;
                  Vector3D MEPDOWN = -Me.WorldMatrix.Forward * UI_SCALE;

                  Vector3D GOPOS = Vector3D.Round(MEPOS + MEPRIGHT * BASEX + MEPDOWN * BASEY, 2);
                  DRONES[SELECTED_SQDS[0]].COMLOC = "GOTO^" + GOPOS;
              }

              //Runs Attack/Investigate Scenario
              //------------------------------------
              if (MOUSE_SYMB == SYST_CURSOR.Attack) //Attack
              {
                  CLICK_TIMER = 60;
                  CLICKPOS = new Vector2D(POS_X, POS_Y);
                  DISPLAY_PANEL.CustomData = "got here";
                  EN_HOVER = EN_HOVER.Replace("#", String.Empty);
                  //Vector3D GOPOS = Vector3D.Round(DECENTIN_INFO[EN_HOVER].POSITION, 2);
                  for (int i = 0; i < SELECTED_SQDS.Count; i++)
                  { DRONES[SELECTED_SQDS[i]].COMLOC = "ATTACK^" + EN_HOVER; }
                  FC_SAVE();
                  SELECTED_SQDS = new List<string>();
                  return;
              }

              //Issues Follow Command If Multiple Drones Are Selected
              //--------------------------------------------------------
              if (SELECTED_SQDS.Count > 1)
              {
                  for (int i = 1; i < SELECTED_SQDS.Count; i++)
                  { DRONES[SELECTED_SQDS[i]].COMLOC = "FOLLOW^" + DRONES[SELECTED_SQDS[0]].ID; }
              }
              FC_SAVE();
              SELECTED_SQDS = new List<string>();
          }
      }
      #endregion

      //Initialisation Functions Called Once

      #region UI Static Init
      /*=======================================================================================
        Function: Initialises User Interface Background
        ------------------------------------------------
        function will: Writes ui interface background
      //=======================================================================================*/
      void UIINIT()
      {
          //Initialises Any Required Blocks
          DISPLAY_PANEL = GridTerminalSystem.GetBlockWithName("RFC_PANEL") as IMyTextPanel;
          COMMAND_MODULE = GridTerminalSystem.GetBlockWithName("CENTRAL_COMMAND") as IMyProgrammableBlock;
          try
          {
              List<IMyTerminalBlock> TEMP_RC = new List<IMyTerminalBlock>();
              GridTerminalSystem.GetBlocksOfType<IMyShipController>(TEMP_RC, b => b.CubeGrid == Me.CubeGrid && b.CustomName == "RFC_RC");
              CONTROL = TEMP_RC[0] as IMyShipController;
          }
          catch { }

          GridTerminalSystem.GetBlocksOfType<IMyGyro>(GYROS, b => b.CubeGrid == Me.CubeGrid);
          string BCKGRD_RAW = Me.CustomData;

          //Extracts Background Image Layer1
          string BCKGRD = BCKGRD_RAW.Replace("\n", String.Empty);
          BCKGRD = BCKGRD.Replace(" ", String.Empty);
          for (int j = 0; j < ROWS_CT; j++)
          {
              for (int i = 0; i < COLUMNS_CT; i++)
              {
                  char ITEM = (BCKGRD[(j * COLUMNS_CT) + i] == '0') ? ITEM = L1 : ITEM = B;
                  BDRAW[j, i] = ITEM;
              }
          }
          return;
      }
      #endregion

      #region FC Initial Load
      /*=======================================================================================
        Function: FC Inbitialisation
        ---------------------------------------
        function will: Initialise fleet command system, generate the structs and
        Drone input:
      //=======================================================================================*/
      void FC_INIT()
      {
          //Creates From Permanent Storage
          //--------------------------------
          string CUSTDATA = COMMAND_MODULE.CustomData;
          string SQ_DATA = CUSTDATA.Split(new string[] { "##INFO##" }, StringSplitOptions.None)[0]; //0 = sqdata
          string[] SQ_DATA_LIST = SQ_DATA.Split(new string[] { "\n" }, StringSplitOptions.None);

          foreach (var item in SQ_DATA_LIST)
          {
              Echo(item);
              if (item.Split('*').Length < 7) { continue; }
              string DRONE_ID = item.Split('*')[0].Substring(1, 6);
              if (DRONE_ID.Length < 4) { continue; }
              DRONE_INFO NEW_DRONE = new DRONE_INFO();
              bool[] UPDATE_RUN = new bool[] { false, true, false, false, false, false, false, false, false, false };
              NEW_DRONE = DRONE_INFO.DRONE_DATA_RS(item, NEW_DRONE, UPDATE_RUN);
              NEW_DRONE.LAST_PING = DateTime.Now;
              NEW_DRONE.ID = DRONE_ID;
              DRONES.Add(DRONE_ID, NEW_DRONE);
              Echo("Sucessfully Added New Drone " + DRONES.Count + " " + DRONE_ID);
          }
      }
      #endregion

      //Utils Functions

      #region DRAW UI SHP
      /*=======================================================================================
        Function:General UI Drawer
        ---------------------------------------
        function will: Draw A ship of the correct size, draw a line an etc to the top
      //=======================================================================================*/
      public void DRAW_UI_SHP(Vector2D BASE, Vector2D TOP, char PRI_COL, int SIZE, char LINE_COLOUR, string SYMBOL)
      {
          //Draws Symbol Line Faintly
          line(TOP, BASE, LINE_COLOUR);

          //Draws Symbol Core
          int HEIGHT = MathHelper.Clamp(SIZE / UI_SCALE / 2,1,20);
          int LENGTH = MathHelper.Clamp(SIZE / UI_SCALE,3,20);
          for (int j = 0; j < HEIGHT; j++)
          {
              for (int i = 0; i < LENGTH; i++)
              {
                  int y = (int)TOP.Y - HEIGHT / 2 + j;
                  int x = (int)TOP.X - LENGTH / 2 + i;//int x = ((int)TOP.X - (LENGTH / 2 + WIDTH)) + (i + 1);
                  if (x < COLUMNS_CT && y < COLUMNS_CT && y > ROW_UI_START && y < ROW_UI_END && x > -1 && DRAW[y, x] != P)
                  {DRAW[y,x] = PRI_COL;}
              }
          }

          //Draws Symbol Itself

          WRT_SYMB(SYST_SYMBLS.SYST_SYMBLS_PROCEDURAL[SYMBOL], (int)TOP.Y - 1, (int)TOP.X - 3);
          //Echo("got here");
          //Draws Symbol Identifier
          //for (int j = 0; j < HEIGHT; j++)
          //{
          //    for (int i = 0; i < WIDTH; i++)
          //    {
          //        int y = (int)TOP.Y - HEIGHT / 2 + j;
          //        int x = (int)TOP.X - LENGTH / 2 + i;
          //        if (x < COLUMNS_CT && y < COLUMNS_CT && y > 4 && x > 0)
          //        { DRAW[y, x] = ID_COLOUR; }
          //    }
          //}

      }
      #endregion

      #region DRAW LINE
      /*=======================================================================================
        Function: Symbol Writer
        ---------------------------------------
        function will: Draw A Line using Bresenhams algorithm
      //=======================================================================================*/
      public void line(Vector2D IN_COORD, Vector2D OUT_COORD, char color)
      {
          int x = (int)IN_COORD.X;
          int y = (int)IN_COORD.Y;

          int x2 = (int)OUT_COORD.X;
          int y2 = (int)OUT_COORD.Y;

          int w = x2 - x;
          int h = y2 - y;
          int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
          if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
          if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
          if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
          int longest = Math.Abs(w);
          int shortest = Math.Abs(h);
          if (!(longest > shortest))
          {
              longest = Math.Abs(h);
              shortest = Math.Abs(w);
              if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
              dx2 = 0;
          }
          int numerator = longest >> 1;
          for (int i = 0; i <= longest; i++)
          {
              if (!(x < COLUMNS_CT && y < ROW_UI_END)) { return; }
              if (y > ROW_UI_START && x > -1 && DRAW[y, x] != P && DRAW[y, x] != red && DRAW[y, x] != green) //conditions of drawing line
              {DRAW[y, x] = color; }
              numerator += shortest;
              if (!(numerator < longest))
              {
                  numerator -= longest;
                  x += dx1;
                  y += dy1;
              }
              else
              {
                  x += dx2;
                  y += dy2;
              }
          }
          //Echo(x+"");
          //Echo(y+"");
      }
      #endregion

      #region RDAV'S RASTERISER
      /*=======================================================================================
        Function: Rasteriser For a 100x100 Grid
        ---------------------------------------
        function will: Using perspective redo x and y positions to perspective
      //=======================================================================================*/
      int SYST_PERSPECTIVE = -250; //Perspective for the system
      void RASTER(ref Vector2D COORD)
      {
          //COORD.X = MathHelper.Clamp(COORD.X, 0, 120);
          //if (COORD.Y < - 100) { return; }
          //COORD.Y = MathHelper.Clamp(COORD.Y, SYST_PERSPECTIVE, 120);

          //Do X Coordinates
          double GRADIENT = ((50 - COORD.X) / (SYST_PERSPECTIVE - 100.0));
          COORD.X = COORD.X - (100 - COORD.Y) * GRADIENT;

          //Do Y Coordinates
          double ALT_GRAD = (SYST_PERSPECTIVE - COORD.Y) / (SYST_PERSPECTIVE - 100);
          COORD.Y = (COORD.Y) * ALT_GRAD;
      }
      void DERASTER(ref Vector2D RASTEREDCOORD)
      {
          //Do Y Coordinates
          RASTEREDCOORD.Y = (SYST_PERSPECTIVE + Math.Sqrt(SYST_PERSPECTIVE * SYST_PERSPECTIVE - 4 * RASTEREDCOORD.Y * SYST_PERSPECTIVE + 400 * RASTEREDCOORD.Y)) / 2;
          //Do X Coordinates
          RASTEREDCOORD.X = (RASTEREDCOORD.X * SYST_PERSPECTIVE - 100 * RASTEREDCOORD.X - 50 * RASTEREDCOORD.Y + 5000) / (SYST_PERSPECTIVE - RASTEREDCOORD.Y);
      }
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

      #region Texture Writer
      /*=======================================================================================
        Function: Symbol Writer
        ---------------------------------------
        function will: Writes a symbol at the given coordinates, system will error check itself
      //=======================================================================================*/
      void WRT_SYMB(char[,] SYMBOL, int START_ROW, int START_COL)
      {
          //Writes Symbol At Given Input Coords
          for (int j = START_ROW; j < START_ROW + SYMBOL.GetLength(0); j++)
          {
              for (int i = START_COL; i < START_COL + SYMBOL.GetLength(1); i++)
              {
                  if (j > COLUMNS_CT - 1 || i > COLUMNS_CT - 1 || i < 0 || j < 0) { continue; } //does not write if over limits
                  if (SYMBOL[j - START_ROW, i - START_COL] == B || DRAW[j, i] == green) { continue; } //does not overwrite with blank space
                  DRAW[j, i] = SYMBOL[j - START_ROW, i - START_COL];
              }
          }
      }
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

      #region Coloured Character Generator (Thanks Whip)
      /*=======================================================================================
        Function: Symbol Writer
        ---------------------------------------
        function will: Writes a symbol at the given coordinates, system will error check itself
      //=======================================================================================*/
      static char rgb(int R, int G, int B)
      {
          int r = (int)(7.00 / 255.00 * R);
          int g = (int)(7.00 / 255.00 * G);
          int b = (int)(7.00 / 255.00 * B);
          return (char)(0xE100 + (MathHelper.Clamp(r, 0, 7) << 6) + (MathHelper.Clamp(g, 0, 7) << 3) + MathHelper.Clamp(b, 0, 7));
      }
      #endregion

      // COPY TO HERE   ------------------
