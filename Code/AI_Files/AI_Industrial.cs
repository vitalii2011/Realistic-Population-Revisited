using System;
using HarmonyLib;


namespace RealPop2
{
    [HarmonyPatch(typeof(IndustrialBuildingAI))]
    [HarmonyPatch("GetPollutionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(int), typeof(DistrictPolicies.CityPlanning), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
    public static class RealisticIndustrialPollution
    {

        public static bool Prefix(IndustrialBuildingAI __instance, ItemClass.Level level, int productionRate, out int groundPollution, out int noisePollution)
        {
            int[] array = IndustrialBuildingAIMod.GetArray(__instance.m_info, (int)level);

            groundPollution = (productionRate * array[DataStore.GROUND_POLLUTION]) / 100;
            noisePollution = (productionRate * array[DataStore.NOISE_POLLUTION]) / 100;

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