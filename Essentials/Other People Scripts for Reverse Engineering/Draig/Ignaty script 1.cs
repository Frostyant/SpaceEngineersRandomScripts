#region pre-script
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    public class Program : MyGridProgram
    {
        #endregion
        //To put your code in a PB copy from this comment...
        public Program()
        {

        }

        public void Save()
        {

        }

        //Function of Script//
        /*
        Function of script is to produce a set amount of components
        Not that the Demand 
        */

        /*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/
        //Setup//
        //IMPORTANT//
        //ENSURE BLOCK NAMES BELOW MATCH !
        /*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/

        //Block Names//
        //Text block with info in it (such as ID and passwords to acces this machine)
        public const string INFO_NAME = "info";//needs to be on the LEFT of cntrl
                                               //Text block containing current demand
        public const string DEMAND_NAME = "demand";
        //Name of OUTPUT container
        public const string OUTPUT_NAME = "output";
        //Name of intermediate container (containing stuff while)
        public const string INTERMEDIATE_NAME = "product";
        public const string DEMAND_TYPE = "DMD";

        //Other Variables
        public const char SEP = '\n';
        public const char SEP1 = ':';

        //Components list



        /*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/
        //dont edit unless you know what you are doing !//
        /*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/

        //Methods//

        public List<IMyTerminalBlock> MultiBlockGet(string NAME)
        {
            List<IMyTerminalBlock> output = new List<IMyTerminalBlock>();

            GridTerminalSystem.SearchBlocksOfName(NAME, output);

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cargos"></param>
        /// Cargo containers you are checking
        /// <param name="type"></param>
        /// Type in format "item name"+" "+"item class"
        /// /// <param name="WhichInventory"></param>
        ///  Determins which inventory you check (for Refs and Assemlers which have 2 of them)
        /// <returns></returns>
        public double CheckInventoryForType<T>(T[] cargos, string type, int WhichInventory = 0)
            where T : class, IMyEntity
        {
            double output;

            long total = 0;
            string[] types = type.Split(' ');
            if (types.Length == 1)
            {
                foreach (T cargo in cargos)
                {
                    foreach (IMyInventoryItem item in cargo.GetInventory(WhichInventory).GetItems())
                    {
                        if (item.Content.SubtypeName.Contains(type))
                        {
                            total += item.Amount.RawValue;
                        }
                    }
                }
            }
            else
            {
                foreach (T cargo in cargos)
                {
                    foreach (IMyInventoryItem item in cargo.GetInventory(WhichInventory).GetItems())
                    {

                        if (item.Content.ToString().Contains(types[0]) && item.Content.SubtypeName.Contains(types[1]))
                        {
                            total += item.Amount.RawValue;
                        }
                    }
                }
            }

            output = (double)total;

            return output;
        }

        /// <summary>
        /// Gets Density
        /// </summary>
        /// <param name="type"></param>
        /// 
        /// <returns></returns>
        public float GetDensity(string type)
        {
            float output = 0.0001f;

            return output;
        }



        public IMyInventory[] AccessInventory(IMyCargoContainer[] cargos)
        {
            IMyInventory[] output = new IMyInventory[cargos.Length];
            
            for (int it = 0; it <= cargos.Length-1; it++)
            {
                output[it] = (cargos[it] as IMyEntity).GetInventory(0);
            }

            return output;

        }

        /// <summary>
        /// Does what it says
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cargos"></param>
        /// Cargo you want to access
        /// <param name="WhichInventory"></param>
        /// Which inventory you want to access
        /// <returns></returns>
        public IMyInventory[] AccessInventory<T>(T[] cargos, int WhichInventory = 0) where T : class, IMyEntity
        {
            IMyInventory[] output = new IMyInventory[cargos.Length];

            for (int it = 0; it <= cargos.Length - 1; it++)
            {
                output[it] = (cargos[it] as IMyEntity).GetInventory(WhichInventory);
            }

            return output;
        }

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

        #region LCD-Advanced

        /// <summary>
        /// Sets up selected LCD for printing debug info.
        /// </summary>
        /// <param name="lcdName"></param>
        public void LcdSetupForDebugging(string lcdName)
        {
            IMyTextPanel lcd = getLcd(lcdName);

            // Seems to be unaccessible in-game :(
            //lcd.SetShowOnScreen(VRage.Game.GUI.TextPanel.ShowTextOnScreenFlag.PUBLIC);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="input"></param>
        /// Blocks inputing items
        /// <param name="output"></param>
        /// Blocks Receiveing items
        /// <param name="type"></param>
        /// type of transferred items
        /// <param name="WhichInventory"></param>
        /// Which iventory of input block (for assemblers and refineries)
        /// <param name="WhichOutput"></param>
        /// Which iventory of output block (for assemblers and refineries)
        public void TransferInventoryType<T, U>(T[] input, U[] output, string type, int WhichInventory = 0, int WhichOutput = 0)
            where T : class, IMyEntity
            where U : class, IMyEntity
        {
            //string BACON_NAME = "3";
            //IMyBeacon bacon = GridTerminalSystem.GetBlockWithName("bacon") as IMyBeacon;
            //bacon.SetCustomName("3.0");
            //Accessing Inventory
            IMyInventory[] input_inventory = AccessInventory<T>(input, WhichInventory);
            //bacon.SetCustomName("3.1");
            IMyInventory[] output_inventory = AccessInventory<U>(output, WhichOutput);
            //bacon.SetCustomName("3.2");

            string[] types = type.Split(' ');

            //getting density
            float density = GetDensity(type);
            float MaxTransfer = 100f;

            int it = 0;
            int it2 = 0;
            T[] cargo_temp = new T[input.Length];
            //bacon.SetCustomName("3.3");
            if (types.GetLength(0) == 1)
            {
                foreach (IMyInventory input_temp in input_inventory)
                {

                    //bacon.SetCustomName("3.30");
                    var StorageItems = (input_temp as IMyInventory).GetItems();
                    //bacon.SetCustomName("3.31");
                    var TransferFrom = (input[it] as IMyEntity).GetInventory(WhichInventory);
                    //bacon.SetCustomName("3.32");
                    cargo_temp[it] = input[it];
                    //bacon.SetCustomName("3.33");

                    double amount = CheckInventoryForType<T>(cargo_temp, type, WhichInventory);
                    //bacon.SetCustomName("3.4");
                    for (int j = StorageItems.Count - 1; j >= 0; j--)
                    {

                        if (StorageItems[j].Content.ToString().Contains(type))
                        {
                            it2 = 0;
                            foreach (IMyInventory output_temp in output_inventory)
                            {
                                it2 += 1;
                                //Transfers Items
                                //bacon.SetCustomName("3.5");
                                //var EjectorInv = (output[it2] as IMyEntity).GetInventory(WhichOutput);

                                float output_inventoryFreeVol = (float)output_temp.MaxVolume - (float)output_temp.CurrentVolume;
                                if (output_inventoryFreeVol > MaxTransfer)
                                {
                                    output_inventoryFreeVol = MaxTransfer;
                                }
                                int TransferQty = (int)Math.Floor(output_inventoryFreeVol * (1 / density));
                                //bacon.SetCustomName(((float)output_temp.MaxVolume).ToString());
                                bool isTransferrable = TransferFrom.TransferItemTo(output_temp, j, 1, true, TransferQty);
                                amount -= TransferQty;
                                it2 += 1;

                                if (amount <= 0)
                                {
                                    it2 = 0;
                                    break;
                                }
                            }
                        }
                    }
                    it += 1;
                }
            }
            else
            {
                foreach (IMyInventory input_temp in input_inventory)
                {

                    //bacon.SetCustomName("3.30");
                    var StorageItems = (input_temp as IMyInventory).GetItems();
                    //bacon.SetCustomName("3.31");
                    var TransferFrom = (input[it] as IMyEntity).GetInventory(WhichInventory);
                    //bacon.SetCustomName("3.32");
                    cargo_temp[it] = input[it];
                    //bacon.SetCustomName("3.33");

                    double amount = CheckInventoryForType<T>(cargo_temp, type, WhichInventory);
                    //bacon.SetCustomName("3.4");
                    for (int j = StorageItems.Count - 1; j >= 0; j--)
                    {

                        if (StorageItems[j].Content.SubtypeName == types[0] && StorageItems[j].Content.ToString().Contains(types[1]))
                        {
                            it2 = 0;
                            foreach (IMyInventory output_temp in output_inventory)
                            {
                                it2 += 1;
                                //Transfers Items
                                //bacon.SetCustomName("3.5");
                                //var EjectorInv = (output[it2] as IMyEntity).GetInventory(WhichOutput);

                                float output_inventoryFreeVol = (float)output_temp.MaxVolume - (float)output_temp.CurrentVolume;
                                if (output_inventoryFreeVol > MaxTransfer)
                                {
                                    output_inventoryFreeVol = MaxTransfer;
                                }
                                int TransferQty = (int)Math.Floor(output_inventoryFreeVol * (1 / density));
                                //bacon.SetCustomName(((float)output_temp.MaxVolume).ToString());
                                bool isTransferrable = TransferFrom.TransferItemTo(output_temp, j, 1, true, TransferQty);
                                amount -= TransferQty;
                                it2 += 1;

                                if (amount <= 0)
                                {
                                    it2 = 0;
                                    break;
                                }
                            }
                        }
                    }
                    it += 1;
                }
            }
        }


        /// <summary>
        /// Extracts txt info
        /// </summary>
        /// <param name="txt"></param>
        /// txt panel
        /// <returns></returns>
        public string GetRaw(IMyTextPanel txt)
        {
            string output;
            output = txt.GetPublicText();
            return output;
        }

        /// <summary>
        /// Extracts txt info and splits it
        /// </summary>
        /// <param name="txt"></param>
        /// txt panel
        /// <param name="sep"></param>
        /// separator used in split
        /// <returns></returns>
        public string[] GetRef(IMyTextPanel txt, char sep)
        {
            //Divides RAW into an array for later use
            string raw;
            raw = GetRaw(txt);
            string[] output = raw.Split(sep);
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
        public List<string> GetData(IMyTextPanel txt, string type, char sep = '\n', char InternalSep = ':'){
            string[] FullData = GetRef(txt,sep);
            List<string> output = new List<string>();

            foreach (string DataChunk in FullData){
                string[] Data = DataChunk.Split(InternalSep);
                if (Data[0] == type) {
                    output.Add(DataChunk);
                }
                 
            }
            return output;
        }




        /*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/
        //dont edit unless you know what you are doing !//
        /*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/

        void Main(string argument)
        {
            //IMyTextPanel infotxt = GridTerminalSystem.GetBlockWithName(INFO_NAME) as IMyTextPanel;

            //string[] info = GetRef(infotxt, SEP);

            IMyTextPanel DemandTxt = GridTerminalSystem.GetBlockWithName(DEMAND_NAME) as IMyTextPanel;

            LcdClear();

            if (argument == " ")
            {
                List<IMyTerminalBlock> output = MultiBlockGet(OUTPUT_NAME);

                List<IMyTerminalBlock> TempCargo = MultiBlockGet(INTERMEDIATE_NAME);

                string NewDemand = "";

                List<string> demands = GetData(DemandTxt,DEMAND_TYPE);

                string[] DemandDetail;

                foreach (string demand in demands){

                    

                    DemandDetail = demand.Split(SEP1);

                    IMyTerminalBlock Assembler = GridTerminalSystem.GetBlockWithName(DemandDetail[1]);

                    IMyTerminalBlock[] AssemblerArray = new IMyTerminalBlock[] {Assembler as IMyTerminalBlock};

                    double offer = CheckInventoryForType<IMyTerminalBlock>(TempCargo.ToArray(), DemandDetail[1])/(1000000);

                    LcdPrintln(offer.ToString()+":" +DemandDetail[2]);
                    LcdPrintln(demand);

                    if (offer >= Convert.ToDouble(DemandDetail[2]))
                    {
                        LcdPrintln("if");

                        Assembler.ApplyAction("OnOff_Off");

                        TransferInventoryType<IMyTerminalBlock, IMyTerminalBlock>(TempCargo.ToArray(), output.ToArray(), DemandDetail[1] + " " + "Component");

                    }
                    else
                    {
                        LcdPrintln("else");

                        NewDemand = NewDemand + "\n" + demand;

                        //ITerminalAction TurnOn = Assembler.GetActionWithName("OnOff_On");

                        Assembler.ApplyAction("OnOff_On");
                    }

                    TransferInventoryType<IMyTerminalBlock, IMyTerminalBlock>(AssemblerArray, TempCargo.ToArray(), DemandDetail[1] + " " + "Component",1);
                }

                DemandTxt.WritePublicText(NewDemand);

            }
            else
            {
                string demand = GetRaw(DemandTxt);

                demand = demand + "\n" + argument;

                LcdClear(DEMAND_NAME);

                LcdPrint(demand,DEMAND_NAME);
            }


        }
        //to this comment.
        #region post-script
    }
}
#endregion