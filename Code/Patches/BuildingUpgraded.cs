using ColossalFramework.Math;
using HarmonyLib;


namespace RealPop2
{
	/// <summary>
	/// Harmony Postfix patch to handle *reductions* in homecounts when a building upgrades.
	/// </summary>
	[HarmonyPatch(typeof(PrivateBuildingAI))]
	[HarmonyPatch("BuildingUpgraded")]
	[HarmonyBefore("com.github.algernon-A.csl.ablc")]
	public static class BuildingUpgradedPatch
	{
		/// <summary>
		/// Harmony Postfix patch to handle *reductions* in homecounts when a building upgrades.
		/// </summary>
		/// <param name="__instance">Instance reference</param>
		/// <param name="buildingID">Building instance ID</param>
		/// <param name="data">Building data</param>
		public static void Postfix(PrivateBuildingAI __instance, ushort buildingID, ref Building data)
		{
			// Only interested in residential builidngs.
			if (__instance is ResidentialBuildingAI)
			{
				// Recalculate homecount.
				int homeCount = (__instance.CalculateHomeCount((ItemClass.Level)data.m_level, new Randomizer(buildingID), data.Width, data.Length));

				Logging.Message("residential building ", buildingID.ToString(), " (", data.Info.name, ") upgraded to level ", (data.m_level + 1).ToString(), "; calculated homecount is ", homeCount.ToString());

				// Remove any extra households.
				RealisticCitizenUnits.RemoveHouseHold(ref data, homeCount);
			}
		}
	}
}
