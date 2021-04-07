using UnityEngine;
using ColossalFramework.Math;
using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement office building consumption rates.
    /// </summary>
    [HarmonyPatch(typeof(OfficeBuildingAI), nameof(OfficeBuildingAI.GetConsumptionRates))]
    public static class RealisticOfficeConsumption
    {
        /// <summary>
        /// Pre-emptive Harmony Prefix patch for OfficeBuildingAI.GetConsumptionRates, to implement the mod's consumption calculations.
        /// </summary>
        /// <param name="__instance">AI instance reference</param>
        /// <param name="level">Building level</param>
        /// <param name="r">Randomizer</param>
        /// <param name="productionRate">Building production rate</param>
        /// <param name="electricityConsumption">Building electricity consumption</param>
        /// <param name="waterConsumption">Building water consumption</param>
        /// <param name="sewageAccumulation">Building sewage accumulation</param>
        /// <param name="garbageAccumulation">Building garbage accumulation</param>
        /// <param name="incomeAccumulation">Building income accumulation</param>
        /// <param name="mailAccumulation">Building mail accumulation</param>
        /// <returns>Always false (never execute original method)</returns>
        public static bool Prefix(OfficeBuildingAI __instance, ItemClass.Level level, Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation, out int mailAccumulation)
        {
            // Get relevant array from datastore.
            int[] array = AI_Utils.GetOfficeArray(__instance.m_info, (int)level);

            // Get consumption rates from array.
            electricityConsumption = array[DataStore.POWER];
            waterConsumption = array[DataStore.WATER];
            sewageAccumulation = array[DataStore.SEWAGE];
            garbageAccumulation = array[DataStore.GARBAGE];
            mailAccumulation = array[DataStore.MAIL];

            // Calculate land value.
            int landValue = AI_Utils.GetLandValueIncomeComponent(r.seed);
            incomeAccumulation = array[DataStore.INCOME] + landValue;

            // Apply consumption rates.
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
}