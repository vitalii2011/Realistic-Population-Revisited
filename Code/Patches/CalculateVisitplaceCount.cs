using ColossalFramework.Math;
using HarmonyLib;


#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0060 // Remove unused parameter


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement visit count changes for commercial buildings.
    /// </summary>
    [HarmonyPatch(typeof(CommercialBuildingAI))]
    [HarmonyPatch("CalculateVisitplaceCount")]
    public static class RealisticVisitplaceCount
    {
        /// <summary>
        /// Harmony Prefix patch to ResidentialBuildingAI.CalculateHomeCount to implement mod population calculations.
        /// </summary>
        /// <param name="__result">Original method result</param>
        /// <param name="__instance">Original AI instance reference</param>
        /// <param name="level">Building level</param>
        /// <param name="r">Randomizer (unused)</param>
        /// <param name="width">Building lot width (unused)</param>
        /// <param name="length">Building lot length (unused)</param>
        /// <returns>Always false (don't execute base game method after this)</returns>
        public static bool Prefix(ref int __result, CommercialBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length)
        {
            // Get builidng info.
            BuildingInfo info = __instance.m_info;

            // Get cached workplace count and calculate total workplaces.
            int[] workplaces = PopData.instance.WorkplaceCache(info, (int)level);
            float totalWorkers = workplaces[0] + workplaces[1] + workplaces[2] + workplaces[3];
            float multiplier;

            // New or old calculations?
            if (ModSettings.comVisitsMode == 0)
            {
                // New settings, based on population.
                switch (info.GetSubService())
                {
                    case ItemClass.SubService.CommercialLow:
                        switch (info.GetClassLevel())
                        {
                            case ItemClass.Level.Level1:
                                multiplier = 1.8f;
                                break;
                            case ItemClass.Level.Level2:
                                multiplier = 1.333333333f;
                                break;
                            default:
                                multiplier = 1.1f;
                                break;
                        }
                        break;

                    case ItemClass.SubService.CommercialHigh:
                        switch (info.GetClassLevel())
                        {
                            case ItemClass.Level.Level1:
                                multiplier = 2.666666667f;
                                break;
                            case ItemClass.Level.Level2:
                                multiplier = 3f;
                                break;
                            default:
                                multiplier = 3.2f;
                                break;
                        }
                        break;

                    case ItemClass.SubService.CommercialLeisure:
                    case ItemClass.SubService.CommercialTourist:
                        multiplier = 2.5f;
                        break;

                    default:
                        // Commercial eco.
                        multiplier = 1f;
                        break;
                }

                // Multiply total workers by multipler to get result.
                __result = (int)(totalWorkers * multiplier);
            }
            else
            {
                // Old settings, based on lot size.
                // Just go to default game method.
                return true;
            }

            // Always set at least two.
            if (__result < 1)
            {
                Logging.Error("invalid visitcount result ", __result.ToString(), " for ", __instance.m_info.name, "; setting to 1");
                __result = 1;
            }

            Logging.Message("calculated visitors of ", __result.ToString(), " for prefab ", (info?.name) ?? "null");

            // Don't execute base method after this.
            return false;
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
