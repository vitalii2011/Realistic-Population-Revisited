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
            int[] array = AI_Utils.GetResidentialArray(__instance.m_info, (int)level);

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = array[DataStore.NOISE_POLLUTION];

            // Don't execute base method after this.
            return false;
        }
    }
}