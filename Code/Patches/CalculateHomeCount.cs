using ColossalFramework.Math;
using HarmonyLib;


#pragma warning disable IDE0060 // Remove unused parameter


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement population count changes for residential buildings.
    /// </summary>
    [HarmonyPatch(typeof(ResidentialBuildingAI))]
    [HarmonyPatch("CalculateHomeCount")]
    public static class RealisticHomeCount
    {
        /// <summary>
        /// Harmony Prefix patch to ResidentialBuildingAI.CalculateHomeCount to implement mod population calculations.
        /// </summary>
        /// <param name="__result">Original method result</param>
        /// <param name="__instance">Original AI instance reference</param>
        /// <param name="level">Building level</param>
        /// <param name="r">Randomizer (unused)</param>
        /// <param name="width">Building lot width (unused)</param>
        /// <param name="length">Building lot length (unused)</param>
        /// <returns>Always false (don't execute base game method after this)</returns>
        public static bool Prefix(ref int __result, ResidentialBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length)
        {
            // BuildingInfo prefab for this building.
            BuildingInfo info = __instance.m_info;

            // Check to see if there's a record for this prefab in the population cache.
            if (PopData.instance.householdCache.ContainsKey(info))
            {
                // Yes - get cached value for this level for this prefab.
                int returnValue = PopData.instance.householdCache[info][(int)level];

                // Check to see if a valid cached (non-zero) result was returned.
                if (returnValue != 0)
                {
                    // Valid return value - set original method return value to this.
                    __result = returnValue;

                    // Return, and don't execute base method after this.
                    return false;
                }
            }

            // If we got here, there was no valid record in cache - add new cache record and assign original method return value.
            __result = PopData.instance.CacheHouseholds(info, (int)level);

            // Don't execute base method after this.
            return false;
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter
