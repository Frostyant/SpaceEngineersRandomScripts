//FROSTYANT'S ESSENTIALS//



        #region Frostyant's Essential Method Library 

        /// <summary>
        /// Makes Vessel Go To Vector 3 position
        /// Alternative to Draig's Movement Script, innacurate but still usefull for tasks such as scouting
        /// Has Built-in Collision Avoidance from KSH
        /// </summary>
        /// <param name="cntrl"></param>
        /// Remote Control
        /// <param name="Pos"></param>
        /// Target Position
        /// <param name="Relative"></param>
        /// Whether Coords are relative or not
        public void GoTo(IMyRemoteControl cntrl, Vector3 Pos, bool Relative = false)
        {
            //orders ship to move to Vector location
            Vector3 obj;

            if (Relative == true)
            {
                obj = Pos + cntrl.GetPosition();
            }
            else
            {
                obj = Pos;
            }

            cntrl.ClearWaypoints();
            cntrl.AddWaypoint(obj, "obj");
            cntrl.SetAutoPilotEnabled(true);
        }

        //Extracts "raw" info from text panel
        public string GetRaw(IMyTextPanel txt)
        {
            string output;
            output = txt.GetPublicText();
            return output;
        }

        public string[] GetRef(IMyTextPanel txt, char sep)
        {
            //Divides RAW into an array for later use
            string raw;
            raw = GetRaw(txt);
            string[] output = raw.Split(sep);
            return output;
        }

        public void RemoveDataType(IMyTextPanel txt, string type, char sep = '\n', char InternalSep = ':')
        {

            string[] FullData = GetRef(txt, sep);

            //convert to list
            List<string> output = new List<string>(FullData);

            output.RemoveAll(x => x.Split(InternalSep).First().Equals(type));

            LcdClear(DATABASE_NAME);

            LcdPrint(DATABASE_NAME, output);

        }

        /// <summary>
        /// really, do I need to do this ?
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public Vector3 GetBlockPosition(string BLOCK_NAME)
        {
            //Returns Block Position//

            Vector3 output;

            IMyTerminalBlock block = GridTerminalSystem.GetBlockWithName(BLOCK_NAME) as IMyTerminalBlock;

            output = block.GetPosition();

            return output;
        }

        /// <summary>
        /// does what is says on the tin
        /// </summary>
        /// <param name="gps"></param>
        /// in GPS format (same as if you would copy past it from a GPS coords in-game)
        /// <returns></returns>
        public Vector3 GPStoVector(string gps)
        {

            string[] coords;

            //Getting Target Position//

            coords = gps.Split(':');

            //Pending, need to change indices

            Vector3 gpsvector = new Vector3(Convert.ToSingle(coords[1]),
                                     Convert.ToSingle(coords[2]),
                                     Convert.ToSingle(coords[3]));

            return gpsvector;
        }

        /// <summary>
        /// Does what it says on the tin
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public string VectorToGPS(Vector3 vec)
        {
            string output;

            output = "GPS:location:"
                + Convert.ToString(vec.X) + ":"
                + Convert.ToString(vec.Y) + ":"
                + Convert.ToString(vec.Z) + ":";

            return output;
        }

        /// <summary>
        /// Extracts txt info, splits it and gets relevant DataChunks
        /// </summary>
        /// <param name="txt"></param>
        /// txt panel
        /// <param name="sep"></param>
        /// separator used in split, which distinguishes between the different data chunks
        /// <param name="type"></param>
        /// type of data chunk you want
        /// <param name="InternalSep"></param>
        /// separator used in split, which distinguishes between the info within a data chunk
        /// <returns></returns>
        public List<string> GetData(IMyTextPanel txt, string type, char sep = '\n', char InternalSep = ':')
        {
            string[] FullData = GetRef(txt, sep);
            List<string> output = new List<string>();

            foreach (string DataChunk in FullData)
            {
                string[] Data = DataChunk.Split(InternalSep);
                if (Data[0] == type)
                {
                    output.Add(DataChunk);
                }

            }
            return output;
        }

        /// <summary>
        /// Extracts txt info, splits it and gets relevant DataChunks
        /// </summary>
        /// <param name="txt"></param>
        /// txt panel
        /// <param name="sep"></param>
        /// separator used in split, which distinguishes between the different data chunks
        /// <param name="type"></param>
        /// type of data chunk you want
        /// <param name="InternalSep"></param>
        /// separator used in split, which distinguishes between the info within a data chunk
        /// <returns></returns>
        public List<string> GetData(string txtname, string type, char sep = '\n', char InternalSep = ':')
        {

            IMyTextPanel txt = GridTerminalSystem.GetBlockWithName(txtname) as IMyTextPanel;

            string[] FullData = GetRef(txt, sep);
            List<string> output = new List<string>();

            foreach (string DataChunk in FullData)
            {
                string[] Data = DataChunk.Split(InternalSep);
                if (Data[0] == type)
                {
                    //Adds full Data chunk
                    output.Add(DataChunk);
                }

            }
            return output;
        }

        /// <summary>
        /// Starts a timer block
        /// </summary>
        /// <param name="TIMER_NAME"></param>
        /// name of timer block
        public void Execute(string TIMER_NAME)
        {
            //Starts a Timer Block

            IMyTimerBlock timer = GridTerminalSystem.GetBlockWithName(TIMER_NAME) as IMyTimerBlock;


            if (timer != null)
            {


                ITerminalAction start;

                start = timer.GetActionWithName("Start");

                start.Apply(timer);
            }
        }

        /// <summary>
        /// Starts Target timer block
        /// </summary>
        /// <param name="timer"></param>
        /// timer block
        public void Execute(IMyTimerBlock timer)
        {
            //Starts a Timer Block
            if (timer != null)
            {


                ITerminalAction start;

                start = timer.GetActionWithName("Start");

                start.Apply(timer);
            }
        }

        /// <summary>
        /// Launches action on timer block
        /// </summary>
        /// <param name="TIMER_NAME"></param>
        /// timer block name
        /// <param name="ACTION_NAME"></param>
        /// action name
        public void Execute(string TIMER_NAME, string ACTION_NAME)
        {
            //Executes Timer Action

            IMyTimerBlock timer = GridTerminalSystem.GetBlockWithName(TIMER_NAME) as IMyTimerBlock;

            if (timer != null)
            {

                ITerminalAction start;

                start = timer.GetActionWithName(ACTION_NAME);

                start.apply(timer);
            }
        }


        /// <summary>
        /// Extracts elements of list starting with given string
        /// </summary>
        /// <param name="input"></param>
        /// input list to extract from
        /// <param name="Start"></param>
        /// input string starting the required strings in the list
        /// <returns></returns>
        public List<String> ListStringStartWith(List<String> input, String Start)
        {
            List<String> output = new List<string>();

            foreach (string element in input)
            {
                if (element.StartsWith(Start) == true)
                {
                    output.Add(element);
                }
            }

            return output;
        }


        /// <summary>
        /// Used for orders requiring a sequence of actions.
        /// 
        /// This Sequence is numbered & the script will save this number in a database along with the specs of the action.
        /// 
        /// The specs are also saved in the event that one might receive an interrupting order or a later one,
        /// as to not get mixed up (although this method does not deal with that).
        /// 
        /// This script extracts, from a list of possibilities, the latest action in the sequence taken.
        /// </summary>
        /// <param name="input"></param>
        /// The input list is the list of all datachunks in the database with the same order specs as required
        /// <param name="location"></param>
        /// The Location describes where the numbering of the sequencing takes place in the datachunk, default assume
        /// this is a scouting order.
        /// <returns></returns>
        public string GetHighest(List<string> input, int location = 4)
        {
            string output = "none";

            int ord = -1;

            int x = -9001;

            foreach (string element in input)
            {
                string[] ref0 = element.Split(SEP_INT);



                Int32.TryParse(ref0[location], out x);

                if (x > ord)
                {
                    ord = x;

                    output = element;
                }
            }

            //Gotta deal with the empty exception outside methode
            return output;
        }

        /// <summary>
        /// General Method used to check if we have reached a destination.
        /// 
        /// This NEEDS a specialized GetTargetLocation() method to work.
        /// This method is NOT included in my default library.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="Sensitivity"></param>
        /// <returns></returns>
        public bool AreWeThereYet(float Sensitivity)
        {
            IMyRemoteControl cntrl = GridTerminalSystem.GetBlockWithName(REMOTE_CONTROL) as IMyRemoteControl;

            Vector3 Target = GetTargetLocation();

            Vector3 Position = cntrl.GetPosition();

            Vector3 RelativeVec = Position - Target;

            if(RelativeVec.Magnitude <= Sensitivity)
            {
                return true;
            }else
            {
                return false;
            }
        }
        #endregion



















        
        #region LCD-Print-Basic

        public void LcdPrintln(string msg)
        {
            LcdPrint(msg + '\n');
        }

        public void LcdPrint(string msg)
        {
            LcdPrint(msg, "VarPanel");
        }

        public void LcdPrintln(string msg, string lcdName = "VarPanel")
        {
            LcdPrint(msg + '\n', lcdName);
        }

        public void LcdPrint(string msg, string lcdName = "VarPanel")
        {
            IMyTextPanel lcd =
              GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
            lcd.WritePublicText(lcd.GetPublicText() + msg);
        }

        public void LcdPrint(List<string> msg, string lcdName = "VarPanel")
        {
            IMyTextPanel lcd =
              GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
            lcd.WritePublicText(lcd.GetPublicText() + msg);
        }

        /// <summary>
        /// Clears selected LCD's public text and returns it.
        /// </summary>
        /// <param name="lcdName">Block Name of LCD (not the Title!)</param>
        /// <returns></returns>
        public string LcdClear(string lcdName)
        {
            IMyTextPanel lcd =
              GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
            string text = lcd.GetPublicText();
            lcd.WritePublicText("");
            return text;

        }

        /// <summary>
        /// Clears the default "VarPanel" LCD.
        /// </summary>
        /// <returns></returns>
        public string LcdClear()
        {
            return LcdClear("VarPanel");
        }

        /// <summary>
        /// Finds an LCD with the specified string as its block name (not its Title!)
        /// </summary>
        /// <param name="lcdName"></param>
        /// <returns></returns>
        public IMyTextPanel getLcd(string lcdName)
        {
            return GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
        }

        public IMyTextPanel getLcd()
        {
            return getLcd("VarPanel");
        }

        #endregion