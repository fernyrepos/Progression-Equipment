using System.Collections.Generic;
using System.Xml;
using Verse;

namespace CTB_AT_Equipment;

public class EquipmentRestrictionsPatcher : PatchOperation
{
	public List<ItemLevelRule> itemLevelRules;

	protected override bool ApplyWorker(XmlDocument xml)
	{
		//Log.Message("CTB_AT_Equipment: Patching equipment tech level requirements.");
		foreach (ItemLevelRule itemLevelRule in itemLevelRules)
		{
			EquipmentRestrictions.itemList.SetOrAdd(itemLevelRule.thingDef, itemLevelRule);
		}
		return true;
	}
}
