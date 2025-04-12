using System;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Reflection;


namespace CTB_AT_Equipment
{
	[StaticConstructorOnStartup]
	static class HarmonyPatches
	{
		static HarmonyPatches()
		{
			Harmony harmony = new Harmony("rimworld.CTB_AT_Equipment");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

}

