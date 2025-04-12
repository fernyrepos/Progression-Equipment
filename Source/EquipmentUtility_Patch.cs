using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using DArcaneTechnology;
using TechBackground;

namespace CTB_AT_Equipment
{
	public class CanEquip_PatchState
	{
		public string rdefName;
		public TechLevel techLevel;
	}
	
    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip", new Type[] {typeof(Thing), typeof(Pawn), typeof(string), typeof(bool)},
														new ArgumentType[] {ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal}  )]
	public class CanEquip
	{
		[HarmonyPrefix]
		static bool Prefix(ref CanEquip_PatchState __state, ref bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
		{
			__state = new CanEquip_PatchState();
			__state.rdefName = null;
			__state.techLevel = thing.def.techLevel;
				
			TraitDef def = TraitDef.Named("TechBackground");
			if (def == null)
			{
				Log.Message("Couldn't find Trait TechBackground");
				return true;
			}
			
			int pawn_tech_level = 0;
			bool can = false;
			
			if (pawn.story.traits.HasTrait(def))
			{
				Trait tr = pawn.story.traits.GetTrait(def);
				pawn_tech_level = tr.Degree;
			}
			
			ItemLevelRule rule;
			
			// No special rules
			if (!EquipmentRestrictions.itemList.ContainsKey(thing.def.defName)) return true;
			rule = EquipmentRestrictions.itemList.TryGetValue(thing.def.defName);
			
			TechLevel techLevel = rule.techLevelNoResearch;
			
			can = TechLevelMatcher.Match(pawn_tech_level, techLevel);
			
			//Log.Message("Trying to equip without research:" + pawn_tech_level + ", " + techLevel + ": " + can);
			
			if (can)
			{
				// can equip without research, temporarily add the required tech to the list of exemptions
				ResearchProjectDef rdef;
				
				thing.def.techLevel = rule.techLevelNoResearch;
				
				// if thing doesn't have research requirements no need to do anything
				if (!Base.thingDic.TryGetValue(thing.def, out rdef)) return true;
				
				// research project is already exempt, no need to do anything
				if (GearAssigner.exemptProjects.Contains(rdef.defName)) return true;
				
				//Log.Message("Adding temporary exemption: " + rdef.defName);
				GearAssigner.exemptProjects.Add(rdef.defName);
				__state.rdefName = rdef.defName;
			}
			else
			{
				// can't equip without research, temporarily change the required Tech Background
				thing.def.techLevel = rule.techLevelResearched;
			}
			
			return true;
		}
		
	    [HarmonyPostfix]
		static void Postfix(ref CanEquip_PatchState __state, ref bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
		{
			thing.def.techLevel = __state.techLevel;
			if (__state.rdefName != null)
			{
				GearAssigner.exemptProjects.Remove(__state.rdefName);
			}
		}
	}
}
