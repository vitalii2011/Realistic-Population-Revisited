using System;
using ColossalFramework.Math;
using UnityEngine;
using HarmonyLib;


namespace RealPop2
{
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
        public static bool Prefix(ref int __result, CommercialBuildingAI __instance, ItemClass.Level level, int width, int length)
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