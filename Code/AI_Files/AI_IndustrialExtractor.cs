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
            int[] array = AI_Utils.GetExtractorArray(__instance.m_info);

            groundPollution = (productionRate * array[DataStore.GROUND_POLLUTION]) / 100;
            noisePollution = (productionRate * array[DataStore.NOISE_POLLUTION]) / 100;

            // Don't execute base method after this.
            return false;
        }
    }


    public static class IndustrialExtractorAIMod
    {
    }
}