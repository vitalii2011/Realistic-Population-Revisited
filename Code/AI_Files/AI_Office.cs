using System;
using ColossalFramework.Math;
using UnityEngine;
using HarmonyLib;


namespace RealisticPopulationRevisited
{
    [HarmonyPatch(typeof(OfficeBuildingAI))]
    [HarmonyPatch("CalculateWorkplaceCount")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out })]
    class RealisticOfficeWorkplaceCount
    {
        static bool Prefix(OfficeBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            ulong seed = r.seed;
            BuildingInfo item = __instance.m_info;

            PrefabEmployStruct output;
            // If not seen prefab, calculate
            if (!DataStore.prefabWorkerVisit.TryGetValue(item.gameObject.GetHashCode(), out output))
            {
                output = PopData.Workplaces(item, (int)level);

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


    [HarmonyPatch(typeof(OfficeBuildingAI))]
    [HarmonyPatch("GetConsumptionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out })]
    class RealisticOfficeConsumption
    {
        static bool Prefix(OfficeBuildingAI __instance, ItemClass.Level level, Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation, out int mailAccumulation)
        {
            ItemClass item = __instance.m_info.m_class;
            int[] array = OfficeBuildingAIMod.GetArray(__instance.m_info, (int)level);

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


    [HarmonyPatch(typeof(OfficeBuildingAI))]
    [HarmonyPatch("GetPollutionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(int), typeof(DistrictPolicies.CityPlanning), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
    class RealisticOfficePollution
    {
        static bool Prefix(OfficeBuildingAI __instance, ItemClass.Level level, int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            ItemClass @class = __instance.m_info.m_class;
            int[] array = OfficeBuildingAIMod.GetArray(__instance.m_info, (int) level);

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = array[DataStore.NOISE_POLLUTION];

            // Don't execute base method after this.
            return false;
        }
    }


    [HarmonyPatch(typeof(OfficeBuildingAI))]
    [HarmonyPatch("CalculateProductionCapacity")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(Randomizer), typeof(int), typeof(int) })]
    class RealisticOfficeProduction
    {

        static bool Prefix(ref int __result, OfficeBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length)
        {
            ulong seed = r.seed;
            BuildingInfo item = __instance.m_info;
            PrefabEmployStruct worker;

            if (DataStore.prefabWorkerVisit.TryGetValue(item.gameObject.GetHashCode(), out worker))
            {
                // Employment is available
                int workers = worker.level0 + worker.level1 + worker.level2 + worker.level3;
                int[] array = OfficeBuildingAIMod.GetArray(__instance.m_info, (int)level);

                // Original method return value.
                __result = Mathf.Max(1, workers / array[DataStore.PRODUCTION]);
            }
            else
            {
                // Original method return value.
                // Return minimum to be safe.
                __result = 1;
            }

            // Don't execute base method after this.
            return false;
        }
    }


    class OfficeBuildingAIMod : OfficeBuildingAI
    {
        public static int[] GetArray(BuildingInfo item, int level)
        {
            int[][] array = DataStore.office;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.OfficeHightech:
                        array = DataStore.officeHighTech;
                        break;

                    case ItemClass.SubService.OfficeGeneric:
                    default:
                        break;
                }

                return array[level];
            }
            catch (System.Exception)
            {
                // Prevent unnecessary log spamming due to 'level-less' buildings returning level 1 instead of level 0.
                if (level != 1)
                {
                    Debugging.Message(item.gameObject.name + " attempted to be use " + item.m_class.m_subService.ToString() + " with level " + level + ". Returning as level 0");
                }
                return array[0];
            }
        }
    }
}