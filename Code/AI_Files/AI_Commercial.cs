using System;
using ColossalFramework.Math;
using UnityEngine;
using HarmonyLib;


#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0060 // Remove unused parameter


namespace RealPop2
{
    [HarmonyPatch(typeof(CommercialBuildingAI))]
    [HarmonyPatch("GetConsumptionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out })]
    public static class RealisticCommercialConsumption
    {
        public static bool Prefix(CommercialBuildingAI __instance, ItemClass.Level level, Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation, out int mailAccumulation)
        {
            int[] array = CommercialBuildingAIMod.GetArray(__instance.m_info, (int)level);

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


    [HarmonyPatch(typeof(CommercialBuildingAI))]
    [HarmonyPatch("GetPollutionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(int), typeof(DistrictPolicies.CityPlanning), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
    public static class RealisticCommercialPollution
    {
        public static bool Prefix(CommercialBuildingAI __instance, ItemClass.Level level, int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            ItemClass item = __instance.m_info.m_class;
            int[] array = CommercialBuildingAIMod.GetArray(__instance.m_info, (int)level);

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = (productionRate * array[DataStore.NOISE_POLLUTION]) / 100;
            if (item.m_subService == ItemClass.SubService.CommercialLeisure)
            {
                if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.NoLoudNoises) != DistrictPolicies.CityPlanning.None)
                {
                    noisePollution /= 2;
                }
            }

            // Don't execute base method after this.
            return false;
        }
    }


    [HarmonyPatch(typeof(CommercialBuildingAI))]
    [HarmonyPatch("CalculateProductionCapacity")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int) })]
    public static class RealisticCommercialProduction
    {
        public static bool Prefix(ref int __result, CommercialBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length)
        {
            int[] array = CommercialBuildingAIMod.GetArray(__instance.m_info, (int)level);

            // Original method return value.
            __result = Mathf.Max(100, width * length * array[DataStore.PRODUCTION]) / 100;

            // Don't execute base method after this.
            return false;
        }
    }


    public static class CommercialBuildingAIMod
    {
        public static int[] GetArray(BuildingInfo item, int level)
        {
            int[][] array = DataStore.commercialLow;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.CommercialLeisure:
                        array = DataStore.commercialLeisure;
                        break;
                
                    case ItemClass.SubService.CommercialTourist:
                        array = DataStore.commercialTourist;
                        break;

                    case ItemClass.SubService.CommercialEco:
                        array = DataStore.commercialEco;
                        break;

                    case ItemClass.SubService.CommercialHigh:
                        array = DataStore.commercialHigh;
                        break;

                    case ItemClass.SubService.CommercialLow:
                    default:
                        break;
                }

                return array[level];
            }
            catch (System.Exception)
            {
                return array[0];
            }
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression