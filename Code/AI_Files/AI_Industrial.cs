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
            int[] array = LegacyAIUtils.GetIndustryArray(__instance.m_info, (int)level);

            groundPollution = (productionRate * array[DataStore.GROUND_POLLUTION]) / 100;
            noisePollution = (productionRate * array[DataStore.NOISE_POLLUTION]) / 100;

            // Don't execute base method after this.
            return false;
        }
    }
}