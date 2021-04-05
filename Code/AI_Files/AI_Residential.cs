using System;
using HarmonyLib;


namespace RealPop2
{
    [HarmonyPatch(typeof(ResidentialBuildingAI))]
    [HarmonyPatch("GetPollutionRates")]
    [HarmonyPatch(new Type[] { typeof(ItemClass.Level), typeof(int), typeof(DistrictPolicies.CityPlanning), typeof(int), typeof(int) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
    public static class RealisticResidentialPollution
    {
        public static bool Prefix(ResidentialBuildingAI __instance, ItemClass.Level level, out int groundPollution, out int noisePollution)
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
                Logging.Message(item.gameObject.name, " attempted to use ", item.m_class.m_subService.ToString(), " with level ", level.ToString(), ". Returning as level 0");
                return array[0];
            }
        }
    }
}