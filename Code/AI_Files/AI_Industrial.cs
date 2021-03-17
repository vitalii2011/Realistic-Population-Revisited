using System;
using ColossalFramework.Math;
using UnityEngine;
using HarmonyLib;


#pragma warning disable IDE0060 // Remove unused parameter


namespace RealPop2
{
    [HarmonyPatch(typeof(IndustrialBuildingAI))]
    [HarmonyPatch("GetConsumptionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out })]
    public static class RealisticIndustrialConsumption
    {
        public static bool Prefix(IndustrialBuildingAI __instance, ItemClass.Level level, Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation, out int mailAccumulation)
        {
            int[] array = IndustrialBuildingAIMod.GetArray(__instance.m_info, (int)level);

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


    [HarmonyPatch(typeof(IndustrialBuildingAI))]
    [HarmonyPatch("GetPollutionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(int), typeof(DistrictPolicies.CityPlanning), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
    public static class RealisticIndustrialPollution
    {

        public static bool Prefix(IndustrialBuildingAI __instance, ItemClass.Level level, int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            int[] array = IndustrialBuildingAIMod.GetArray(__instance.m_info, (int)level);

            groundPollution = (productionRate * array[DataStore.GROUND_POLLUTION]) / 100;
            noisePollution = (productionRate * array[DataStore.NOISE_POLLUTION]) / 100;

            // Don't execute base method after this.
            return false;
        }
    }


    [HarmonyPatch(typeof(IndustrialBuildingAI))]
    [HarmonyPatch("CalculateProductionCapacity")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int) })]
    public static class RealisticIndustrialProduction
    {
        public static bool Prefix(ref int __result, IndustrialBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length)
        {
            int[] array = IndustrialBuildingAIMod.GetArray(__instance.m_info, (int)level);

            // Original method return value.
            __result = Mathf.Max(100, width * length * array[DataStore.PRODUCTION]) / 100;

            // Don't execute base method after this.
            return false;
        }
    }


    public static class IndustrialBuildingAIMod
    {
        public static int[] GetArray(BuildingInfo item, int level)
        {
            int tempLevel;
            int[][] array = DataStore.industry;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.IndustrialOre:
                        array = DataStore.industry_ore;
                        tempLevel = level + 1;
                        break;

                    case ItemClass.SubService.IndustrialForestry:
                        array = DataStore.industry_forest;
                        tempLevel = level + 1;
                        break;

                    case ItemClass.SubService.IndustrialFarming:
                        array = DataStore.industry_farm;
                        tempLevel = level + 1;
                        break;

                    case ItemClass.SubService.IndustrialOil:
                        array = DataStore.industry_oil;
                        tempLevel = level + 1;
                        break;

                    case ItemClass.SubService.IndustrialGeneric:  // Deliberate fall through
                    default:
                        tempLevel = level;
                        break;
                }

                return array[tempLevel];
            }
            catch (System.Exception)
            {
                Logging.Message(item.gameObject.name, " attempted to use ", item.m_class.m_subService.ToString(), " with level ", level.ToString(), ". Returning as level 0");
                return array[0];
            }
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter