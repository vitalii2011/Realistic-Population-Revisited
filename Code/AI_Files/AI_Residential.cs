using System;
using ColossalFramework.Math;
using UnityEngine;
using HarmonyLib;


namespace RealisticPopulationRevisited
{
    [HarmonyPatch(typeof(ResidentialBuildingAI))]
    [HarmonyPatch("CalculateHomeCount")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int) })]
    class RealisticHomeCount
    {
        static bool Prefix(ref int __result, ResidentialBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length)
        {
            BuildingInfo item = __instance.m_info;
            int returnValue = 0;

            if (!DataStore.prefabHouseHolds.TryGetValue(item.gameObject.GetHashCode(), out returnValue))
            {
                returnValue = PopData.Population(item, (int)level);

                // Store values in cache.
                DataStore.prefabHouseHolds.Add(item.gameObject.GetHashCode(), returnValue);
            }

            // Original method return value.
            __result = returnValue;

            // Don't execute base method after this.
            return false;
        }
    }


    [HarmonyPatch(typeof(ResidentialBuildingAI))]
    [HarmonyPatch("GetConsumptionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) },
       new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out })]
    class RealisticResidentialConsumption
    {
        static bool Prefix(ResidentialBuildingAI __instance, ItemClass.Level level, Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation, out int mailAccumulation)
        {
            ItemClass item = __instance.m_info.m_class;

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
    class RealisticResidentialPollution
    {
        static bool Prefix(ResidentialBuildingAI __instance, ItemClass.Level level, int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            ItemClass @class = __instance.m_info.m_class;
            int[] array = ResidentialBuildingAIMod.GetArray(__instance.m_info, (int)level);

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = array[DataStore.NOISE_POLLUTION];

            // Don't execute base method after this.
            return false;
        }
    }


    class ResidentialBuildingAIMod
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
                Debugging.Message(item.gameObject.name + " attempted to be use " + item.m_class.m_subService.ToString() + " with level " + level + ". Returning as level 0");
                return array[0];
            }
        }
    }
}