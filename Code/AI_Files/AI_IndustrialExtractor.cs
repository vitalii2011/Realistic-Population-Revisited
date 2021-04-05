using System;
using HarmonyLib;


namespace RealPop2
{
    [HarmonyPatch(typeof(IndustrialExtractorAI))]
    [HarmonyPatch("GetPollutionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(int), typeof(DistrictPolicies.CityPlanning), typeof(int), typeof(int) },
    new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
    public static class RealisticExtractorPollution
    {

        public static bool Prefix(IndustrialExtractorAI __instance, int productionRate, out int groundPollution, out int noisePollution)
        {
            int[] array = IndustrialExtractorAIMod.GetArray(__instance.m_info, IndustrialExtractorAIMod.EXTRACT_LEVEL);

            groundPollution = (productionRate * array[DataStore.GROUND_POLLUTION]) / 100;
            noisePollution = (productionRate * array[DataStore.NOISE_POLLUTION]) / 100;

            // Don't execute base method after this.
            return false;
        }
    }


    public static class IndustrialExtractorAIMod
    {
        // Extracting is always level 1 (To make it easier to code)
        public const int EXTRACT_LEVEL = 0;


        public static int[] GetArray(BuildingInfo item, int level)
        {
            int[][] array = DataStore.industry;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.IndustrialOre:
                        array = DataStore.industry_ore;
                        break;

                    case ItemClass.SubService.IndustrialForestry:
                        array = DataStore.industry_forest;
                        break;

                    case ItemClass.SubService.IndustrialFarming:
                        array = DataStore.industry_farm;
                        break;

                    case ItemClass.SubService.IndustrialOil:
                        array = DataStore.industry_oil;
                        break;

                    case ItemClass.SubService.IndustrialGeneric:  // Deliberate fall through
                    default:
                        break;
                }

                return array[level];
            }
            catch (System.Exception)
            {
                return array[0];
            }
        } // end getArray
    }
}