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
            // Get population value from cache.
            __result = PopData.instance.HouseholdCache(__instance.m_info, (int)level);

            // Always set at least one.
            if (__result < 0)
            {
                __result = 1;
            }

            // Don't execute base method after this.
            return false;
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter
