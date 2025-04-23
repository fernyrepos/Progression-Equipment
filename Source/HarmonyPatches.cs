using System.Reflection;
using HarmonyLib;
using Verse;

namespace CTB_AT_Equipment;

[StaticConstructorOnStartup]
internal static class HarmonyPatches
{
	static HarmonyPatches()
	{
		Harmony harmony = new Harmony("rimworld.CTB_AT_Equipment");
		harmony.PatchAll(Assembly.GetExecutingAssembly());
	}
}
