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
//Name of OUTPUT container
public const string OUTPUT_NAME = "Ingots";

//Other Variables
public const char SEP = ';';
public const char SEP1 = ',';

//Components list



/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/
//dont edit unless you know what you are doing !//
/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/

//Methods//

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
public double CheckInventoryForType<T>(T[] cargos, string type,int WhichInventory = 0)
    where T:class,IMyEntity
{
    double output;

    long total = 0;
    string[] types = type.Split(' ');
    if (types.Length == 1) {
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
    }else
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


/// <summary>
/// Does what it says
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="cargos"></param>
/// Cargo you want to access
/// <param name="WhichInventory"></param>
/// Which inventory you want to access
/// <returns></returns>
public IMyInventory[] AccessInventory<T>(T[] cargos,int WhichInventory = 0) where T : class, IMyEntity
{
    IMyInventory[] output = new IMyInventory[cargos.Length];

    for (int it = 0; it <= cargos.Length - 1; it++)
    {
        output[it] = (cargos[it] as IMyEntity).GetInventory(WhichInventory);
    }

    return output;
}



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
public void TransferInventoryType<T,U>(T[] input, U[] output, string type,int WhichInventory = 0,int WhichOutput = 0)
    where T: class, IMyEntity
    where U: class, IMyEntity
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
    if(types.GetLength(0) == 1) {
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
    }else
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

    //bacon.SetCustomName("3.6");
}
//Extracts "raw" info from text panel
public string GetRaw(IMyTextPanel txt)
{
    string output;
    output = txt.GetPublicText();
    return output;
}

 /// <summary>
 /// Self-Explanatory
 /// Basic Exctraction method I used since forever ago
 /// </summary>
 /// <param name="txt"></param>
 ///
 /// <param name="sep"></param>
 /// Separator used
 /// <returns></returns>
public string[] GetRef(IMyTextPanel txt, char sep)
{
    //Divides RAW into an array for later use
    string raw;
    raw = GetRaw(txt);
    string[] output = raw.Split(sep);
    return output;
}






/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/
//dont edit unless you know what you are doing !//
/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/

void Main(string argument)
{
    //string BACON_NAME = "bacon";
    //IMyBeacon bacon = GridTerminalSystem.GetBlockWithName(BACON_NAME) as IMyBeacon;
    List<IMyRefinery> Refs = new List<IMyRefinery>();
    //bacon.SetCustomName("0");
    List<IMyTerminalBlock> output = new List<IMyTerminalBlock>();
    //bacon.SetCustomName("1");
    GridTerminalSystem.GetBlocksOfType<IMyRefinery>(Refs);
    //bacon.SetCustomName("2");

    GridTerminalSystem.SearchBlocksOfName(OUTPUT_NAME, output);
    //bacon.SetCustomName("3");


    TransferInventoryType<IMyRefinery,IMyTerminalBlock>(Refs.ToArray(), output.ToArray(), "Ingot",1);
    //bacon.SetCustomName("4");




}
//to this comment.
