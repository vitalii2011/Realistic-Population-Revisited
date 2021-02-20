using System;
using ColossalFramework.Math;
using UnityEngine;
using HarmonyLib;


#pragma warning disable IDE0060 // Remove unused parameter


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement population count changes for residential buildings.
    /// </summary>
    [HarmonyPatch(typeof(ResidentialBuildingAI))]
    [HarmonyPatch("CalculateHomeCount")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int) })]
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


    [HarmonyPatch(typeof(ResidentialBuildingAI))]
    [HarmonyPatch("GetConsumptionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) },
       new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out })]
    public static class RealisticResidentialConsumption
    {
        public static bool Prefix(ResidentialBuildingAI __instance, ItemClass.Level level, Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation, out int mailAccumulation)
        {
            int[] array = ResidentialBuildingAIMod.GetArray(__instance.m_info, (int)level);
            electricityConsumption = array[DataStore.POWER];
            waterConsumption = array[DataStore.WATER];
            sewageAccumulation = array[DataStore.SEWAGE];
            garbageAccumulation = array[DataStore.GARBAGE];
            mailAccumulation = array[DataStore.MAIL];

            int landVal = AI_Utils.GetLandValueIncomeComponent(r.seed);
            incomeAccumulation = array[DataStore.INCOME] + landVal;

            electricityConsumption = Mathf.Max(100, productionRate * electricityConsumption) / 100;
            waterConsumption = Mathf.Max(100, productionRate * waterConsumption) / 100;
            sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation) / 100;
            garbageAccumulation = Mathf.Max(100, productionRate * garbageAccumulation) / 100;
            incomeAccumulation = productionRate * incomeAccumulation;
            mailAccumulation = Mathf.Max(100, productionRate * mailAccumulation) / 100;

            // Don't execute base method after this.
            return false;
        }
    }


    [HarmonyPatch(typeof(ResidentialBuildingAI))]
    [HarmonyPatch("GetPollutionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(int), typeof(DistrictPolicies.CityPlanning), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
    public static class RealisticResidentialPollution
    {
        public static bool Prefix(ResidentialBuildingAI __instance, ItemClass.Level level, int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            int[] array = ResidentialBuildingAIMod.GetArray(__instance.m_info, (int)level);

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = array[DataStore.NOISE_POLLUTION];

            // Don't execute base method after this.
            return false;
        }
    }


    public static class ResidentialBuildingAIMod
    {
        public static int[] GetArray(BuildingInfo item, int level)
        {
            int[][] array = DataStore.residentialLow;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.ResidentialHighEco:
                        array = DataStore.resEcoHigh;
                        break;

                    case ItemClass.SubService.ResidentialLowEco:
                        array = DataStore.resEcoLow;
                        break;

                    case ItemClass.SubService.ResidentialHigh:
                        array = DataStore.residentialHigh;
                        break;

                    case ItemClass.SubService.ResidentialLow:
                    default:
                        break;
                }

                return array[level];
            }
            catch (System.Exception)
            {
                Logging.Message(item.gameObject.name, " attempted to be use ", item.m_class.m_subService.ToString(), " with level ", level.ToString(), ". Returning as level 0");
                return array[0];
            }
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter