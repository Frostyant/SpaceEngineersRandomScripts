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
public const string DEMAND_NAME = "demand";
//Name of OUTPUT container
public const string OUTPUT_NAME = "output";
//Name of intermediate container (containing stuff while)
public const string INTERMEDIATE_NAME = "product";

//Other Variables
public const string SEP = ";"




/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/
//dont edit unless you know what you are doing !//
/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/

//Methods//

public double CheckInventoryFor(IMyCargoContainer cargo,string type){
  double output;

  long total;

  string[] types = type.Split(" ")

  foreach (IMyInventoryItem item in ReactorEB.GetInventory(0).GetItems()){
    // We know that reactors only hold uranium ingots, but I've included type checking if you want to use this code for something else like cargo bins
    if (item.Content.ToString().Contains(types[0]) && item.Content.SubtypeName.Contains(types[1]))
    {
        total += item.Amount.RawValue;
    }
  }


  double output = (double)total / 1000000;

  return output;
}

//Extracts "raw" info from text panel
public string GetRaw(IMyTextPanel txt){
  string output;
  output =txt.GetPublicText();
  return output;
}

public string[] GetRef(IMyTextPanel txt,char sep){
	//Divides RAW into an array for later use
	string raw;
	raw=GetRaw(txt);
	string[] output =raw.Split(sep);
	return output;
}




/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/
//dont edit unless you know what you are doing !//
/*==#=====#====#=====#=====#=====#=====#=====#====#=====#=====#=====#=====#=====#====#===================*/

void Main(string argument)
{
IMyTextPanel infotxt =  GridTerminalSystem.GetBlockWithName(INFO_NAME) as IMyTextPanel;

string info = GetRef(infotxt,SEP);

if(argument = null){
  IMyTextPanel demandtxt = GridTerminalSystem.GetBlockWithName(DEMAND_NAME) as IMyTextPanel;

  string demand = GetRef(demandtxt,SEP);

  
}


}
