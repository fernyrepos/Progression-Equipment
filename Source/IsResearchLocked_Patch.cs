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
	
    //[HarmonyPatch(typeof(EquipmentUtility), "CanEquip", new Type[] {typeof(Thing), typeof(Pawn), typeof(string), typeof(bool)},
	//													new ArgumentType[] {ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal}  )]
	[HarmonyPatch(typeof(Base), "IsResearchLocked", new Type[] {typeof(ThingDef), typeof(Pawn)} )]
	public class IsResearchLocked
	{
		[HarmonyPrefix]
		static bool Prefix(ref CanEquip_PatchState __state, ref bool __result, ThingDef thingDef, Pawn pawn)
		{
			__state = new CanEquip_PatchState();
			__state.rdefName = null;
			__state.techLevel = thingDef.techLevel;
			
			if (pawn == null) return true;
				
			TraitDef def = TraitDef.Named("TechBackground");
			if (def == null)
			{
				Log.Message("Couldn't find Trait TechBackground");
				return true;
			}
			
			int pawn_tech_level = 0;
			bool can = false;

            //if (pawn.story.traits.HasTrait(def))
            //{
            //	Trait tr = pawn.story.traits.GetTrait(def);
            //	pawn_tech_level = tr.Degree;
            //}

            if (pawn.story?.traits?.HasTrait(def) ?? false)
            {
                Trait tr = pawn.story.traits.GetTrait(def);
                if (tr != null)
                {
                    pawn_tech_level = tr.Degree;
                }
                else
                {
                    //optional else in case we want to log
                    Log.Warning($"{pawn} - Null in pawn.story.traits.HasTrait(def)");
                }
            }

            ItemLevelRule rule;
			
			// No special rules
			if (!EquipmentRestrictions.itemList.ContainsKey(thingDef.defName)) return true;
			rule = EquipmentRestrictions.itemList.TryGetValue(thingDef.defName);
			
			TechLevel techLevel = rule.techLevelNoResearch;
			
			can = TechLevelMatcher.Match(pawn_tech_level, techLevel);
			
			//Log.Message("Trying to equip without research:" + pawn_tech_level + ", " + techLevel + ": " + can);
			
			if (can)
			{
				// can equip without research, temporarily add the required tech to the list of exemptions
				ResearchProjectDef rdef;
				
				thingDef.techLevel = rule.techLevelNoResearch;
				
				// if thing doesn't have research requirements no need to do anything
				if (!Base.thingDic.TryGetValue(thingDef, out rdef)) return true;
				
				// research project is already exempt, no need to do anything
				if (GearAssigner.exemptProjects.Contains(rdef.defName)) return true;
				
				//Log.Message("Adding temporary exemption: " + rdef.defName);
				GearAssigner.exemptProjects.Add(rdef.defName);
				__state.rdefName = rdef.defName;
			}
			else
			{
				// can't equip without research, temporarily change the required Tech Background
				thingDef.techLevel = rule.techLevelResearched;
			}
			
			return true;
		}
		
	    [HarmonyPostfix]
		static void Postfix(ref CanEquip_PatchState __state, ref bool __result, ThingDef thingDef, Pawn pawn)
		{
			thingDef.techLevel = __state.techLevel;
			if (__state.rdefName != null)
			{
				GearAssigner.exemptProjects.Remove(__state.rdefName);
			}
		}
	}
}
