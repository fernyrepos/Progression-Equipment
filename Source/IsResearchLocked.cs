using System;
using DArcaneTechnology;
using HarmonyLib;
using RimWorld;
using TechBackground;
using Verse;

namespace CTB_AT_Equipment;

[HarmonyPatch(typeof(Base), "IsResearchLocked", new Type[]
{
	typeof(ThingDef),
	typeof(Pawn)
})]
public class IsResearchLocked
{
	[HarmonyPrefix]
	private static bool Prefix(ref CanEquip_PatchState __state, ref bool __result, ThingDef thingDef, Pawn pawn)
	{
		__state = new CanEquip_PatchState();
		__state.rdefName = null;
		__state.techLevel = thingDef.techLevel;
		if (pawn == null)
		{
			return true;
		}
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
		if (!EquipmentRestrictions.itemList.ContainsKey(thingDef.defName))
		{
			return true;
		}
		ItemLevelRule itemLevelRule = EquipmentRestrictions.itemList.TryGetValue(thingDef.defName);
		TechLevel techLevelNoResearch = itemLevelRule.techLevelNoResearch;
		if (TechLevelMatcher.Match(num, techLevelNoResearch))
		{
			thingDef.techLevel = itemLevelRule.techLevelNoResearch;
			if (!Base.thingDic.TryGetValue(thingDef, out var value))
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
			thingDef.techLevel = itemLevelRule.techLevelResearched;
		}
		return true;
	}

	[HarmonyPostfix]
	private static void Postfix(ref CanEquip_PatchState __state, ref bool __result, ThingDef thingDef, Pawn pawn)
	{
		thingDef.techLevel = __state.techLevel;
		if (__state.rdefName != null)
		{
			GearAssigner.exemptProjects.Remove(__state.rdefName);
		}
	}
}
