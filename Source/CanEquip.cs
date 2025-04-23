using System;
using DArcaneTechnology;
using HarmonyLib;
using RimWorld;
using TechBackground;
using Verse;

namespace CTB_AT_Equipment;

[HarmonyPatch(typeof(EquipmentUtility), "CanEquip", new Type[]
{
	typeof(Thing),
	typeof(Pawn),
	typeof(string),
	typeof(bool)
}, new ArgumentType[]
{
	ArgumentType.Normal,
	ArgumentType.Normal,
	ArgumentType.Out,
	ArgumentType.Normal
})]
public class CanEquip
{
	[HarmonyPrefix]
	private static bool Prefix(ref CanEquip_PatchState __state, ref bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
	{
		__state = new CanEquip_PatchState();
		__state.rdefName = null;
		__state.techLevel = thing.def.techLevel;
		TraitDef traitDef = TraitDef.Named("TechBackground");
		if (traitDef == null)
		{
			Log.Message("Couldn't find Trait TechBackground");
			return true;
		}
		int num = 0;
		bool flag = false;
		if (pawn.story.traits.HasTrait(traitDef))
		{
			Trait trait = pawn.story.traits.GetTrait(traitDef);
			num = trait.Degree;
		}
		if (!EquipmentRestrictions.itemList.ContainsKey(thing.def.defName))
		{
			return true;
		}
		ItemLevelRule itemLevelRule = EquipmentRestrictions.itemList.TryGetValue(thing.def.defName);
		TechLevel techLevelNoResearch = itemLevelRule.techLevelNoResearch;
		if (TechLevelMatcher.Match(num, techLevelNoResearch))
		{
			thing.def.techLevel = itemLevelRule.techLevelNoResearch;
			if (!Base.thingDic.TryGetValue(thing.def, out var value))
			{
				return true;
			}
			if (GearAssigner.exemptProjects.Contains(value.defName))
			{
				return true;
			}
			GearAssigner.exemptProjects.Add(value.defName);
			__state.rdefName = value.defName;
		}
		else
		{
			thing.def.techLevel = itemLevelRule.techLevelResearched;
		}
		return true;
	}

	[HarmonyPostfix]
	private static void Postfix(ref CanEquip_PatchState __state, ref bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
	{
		thing.def.techLevel = __state.techLevel;
		if (__state.rdefName != null)
		{
			GearAssigner.exemptProjects.Remove(__state.rdefName);
		}
	}
}
