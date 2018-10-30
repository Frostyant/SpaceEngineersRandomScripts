/* 
Written by quaixor 

Script moves Stone from the first inventory of all containers (except connectors) into connectors. 
If you have multiple connectors they all get filled up with Stone. 

*/ 

void Main() 
{ 
int connectorIndex = 0; 
var containers = new List<IMyTerminalBlock>(); 
var connectors = new List<IMyTerminalBlock>(); 
GridTerminalSystem.GetBlocksOfType<IMyInventoryOwner>(containers); 
GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(connectors); 

for (int i = 0; i < containers.Count; i++) 
{ 

	connectorLabel: 
	if (connectorIndex > connectors.Count - 1) 
		return; 

	var connector = connectors[connectorIndex]; 
	var connectorInventoryOwner = connector as IMyInventoryOwner; 
	var connectorInventory = connectorInventoryOwner.GetInventory(0); 

	if ((double)connectorInventory.MaxVolume - (double)connectorInventory.CurrentVolume < 0.01) 
		{ 
		connectorIndex++; 
		goto connectorLabel; 
		} 

	var containerInventoryOwner = containers as IMyInventoryOwner; 
	if (containerInventoryOwner == null) 
		continue; 
	if ((containers as IMyCubeBlock).DefinitionDisplayNameText == "Connector") 
		{ 
		continue; 
		} 
	var containerInventory = containerInventoryOwner.GetInventory(0); 
	var containerItems = containerInventory.GetItems(); 
	for (int j = containerItems.Count - 1; j >= 0; j--) 
		{ 
		if (containerItems[j].Content.SubtypeName != "Stone") 
			{ 
			continue; 
			} 
		connectorInventory.TransferItemFrom(containerInventory,j,null,true,null); 
		} 
} 
}