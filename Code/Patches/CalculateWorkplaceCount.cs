using System.Collections.Generic;
using System.Reflection;
using ColossalFramework.Math;
using HarmonyLib;


#pragma warning disable IDE0060 // Remove unused parameter


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement population count changes for workplace buildings.
    /// </summary>
    [HarmonyPatch]
    public static class CalculateWorkplaceCountPatch
    {
        /// <summary>
        /// Target method to patch - CalculateWorkplaceCount methods of base-game workplace AIs.
        /// </summary>
        /// <returns>List of methods to patch</returns>
        public static IEnumerable<MethodBase> TargetMethods()
        {
            string methodName = "CalculateWorkplaceCount";

            Logging.Message("patching ", methodName);

            yield return typeof(CommercialBuildingAI).GetMethod(methodName);
            yield return typeof(OfficeBuildingAI).GetMethod(methodName);
            yield return typeof(IndustrialBuildingAI).GetMethod(methodName);
            yield return typeof(IndustrialExtractorAI).GetMethod(methodName);
        }


        /// <summary>
        /// Harmony pre-emptive Prefix patch to replace game workplace calculations with the mod's.
        /// </summary>
        /// <param name="__instance">Original AI instance reference</param>
        /// <param name="level">Building level</param>
        /// <param name="r">Randomizer (unused)</param>
        /// <param name="width">Building lot width (unused)</param>
        /// <param name="length">Building lot length (unused)</param>
        /// <param name="level0">Uneducated worker count output</param>
        /// <param name="level1">Educated worker count output</param>
        /// <param name="level2">Well-educated worker count output</param>
        /// <param name="level3">Highly-educated worker count output</param>
        /// <returns></returns>
        public static bool Prefix(PrivateBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            // BuildingInfo prefab for this building.
            BuildingInfo info = __instance.m_info;

            // Get cached workplace count.
            PrefabEmployStruct workplaces = PopData.instance.WorkplaceCache(info, (int)level);

            // Set return values.
            level0 = workplaces.level0;
            level1 = workplaces.level1;
            level2 = workplaces.level2;
            level3 = workplaces.level3;

            // Don't execute base method after this.
            return false;
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter