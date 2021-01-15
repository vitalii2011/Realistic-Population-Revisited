using System;
using ColossalFramework.Math;
using UnityEngine;
using HarmonyLib;


namespace RealisticPopulationRevisited
{
    [HarmonyPatch(typeof(IndustrialBuildingAI))]
    [HarmonyPatch("CalculateWorkplaceCount")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out })]
    class RealisticIndustrialWorkplaceCount
    {
        static bool Prefix(IndustrialBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            ulong seed = r.seed;
            BuildingInfo item = __instance.m_info;

            PrefabEmployStruct output;
            // If not seen prefab, calculate
            if (!DataStore.prefabWorkerVisit.TryGetValue(item.gameObject.GetHashCode(), out output))
            {
                output = PopData.instance.Workplaces(item, (int)level);

                // Store values in cache.
                DataStore.prefabWorkerVisit.Add(item.gameObject.GetHashCode(), output);
            }

            level0 = output.level0;
            level1 = output.level1;
            level2 = output.level2;
            level3 = output.level3;

            // Don't execute base method after this.
            return false;
        }
    }


    [HarmonyPatch(typeof(IndustrialBuildingAI))]
    [HarmonyPatch("GetConsumptionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out })]
    class RealisticIndustrialConsumption
    {
        static bool Prefix(IndustrialBuildingAI __instance, ItemClass.Level level, Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation, out int mailAccumulation)
        {
            ItemClass item = __instance.m_info.m_class;
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
    class RealisticIndustrialPollution
    {

        static bool Prefix(IndustrialBuildingAI __instance, ItemClass.Level level, int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            ItemClass @class = __instance.m_info.m_class;
            groundPollution = 0;
            noisePollution = 0;
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
    class RealisticIndustrialProduction
    {
        static bool Prefix(ref int __result, IndustrialBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length)
        {
            ItemClass @class = __instance.m_info.m_class;
            int[] array = IndustrialBuildingAIMod.GetArray(__instance.m_info, (int)level);

            // Original method return value.
            __result = Mathf.Max(100, width * length * array[DataStore.PRODUCTION]) / 100;

            // Don't execute base method after this.
            return false;
        }
    }
    
    
    class IndustrialBuildingAIMod
    {
        public static int[] GetArray(BuildingInfo item, int level)
        {
            int tempLevel = 0;
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
                Debugging.Message(item.gameObject.name, " attempted to be use ", item.m_class.m_subService.ToString(), " with level ", level.ToString(), ". Returning as level 0");
                return array[0];
            }
        }
    }
}