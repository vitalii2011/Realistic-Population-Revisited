using System.Collections.Generic;
using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement visit count changes for commercial buildings, and supporting methods.
    /// </summary>
    [HarmonyPatch(typeof(CommercialBuildingAI), nameof(CommercialBuildingAI.CalculateVisitplaceCount))]
    public static class RealisticVisitplaceCount
    {
        // Commercial visits modes.
        internal enum ComVisitModes
        {
            popCalcs = 0,
            legacy
        }

        // Array indexes.
        private enum SubServiceIndex
        {
            CommercialLow = 0,
            CommercialHigh,
            CommercialLeisure,
            CommercialTourist,
            CommercialEco,
            NumSubServices
        }

        // Sub-service mapping.
        private static readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.CommercialLow,
            ItemClass.SubService.CommercialHigh,
            ItemClass.SubService.CommercialLeisure,
            ItemClass.SubService.CommercialTourist,
            ItemClass.SubService.CommercialEco
        };

        // Default multiplier.
        internal const int DefaultVisitMult = 70;

        // Maximum multiplier.
        internal const int MaxVisitMult = 100;


        // Arrays for calculation mode and multipliers.
        private static readonly int[] comVisitModes =
        {
            (int)ComVisitModes.popCalcs,
            (int)ComVisitModes.popCalcs,
            (int)ComVisitModes.popCalcs,
            (int)ComVisitModes.popCalcs,
            (int)ComVisitModes.popCalcs
        };
        private static readonly int[] comVisitMults =
        {
            DefaultVisitMult,
            DefaultVisitMult,
            DefaultVisitMult,
            DefaultVisitMult,
            DefaultVisitMult
        };


        /// <summary>
        /// Harmony Prefix patch to ResidentialBuildingAI.CalculateHomeCount to implement mod population calculations.
        /// </summary>
        /// <param name="__result">Original method result</param>
        /// <param name="__instance">Original AI instance reference</param>
        /// <param name="level">Building level</param>
        /// <returns>False (don't execute base game method after this) if new calculatons have been used, true if old calculations are being used</returns>
        public static bool Prefix(ref int __result, CommercialBuildingAI __instance, ItemClass.Level level)
        {
            // Get builidng info.
            BuildingInfo info = __instance.m_info;
            ItemClass.SubService subService = info.GetSubService();

            // Array index.
            int arrayIndex = GetIndex(subService);

            // New or old calculations?
            if (comVisitModes[arrayIndex] == (int)RealisticVisitplaceCount.ComVisitModes.popCalcs)
            {
                // Get cached workplace count and calculate total workplaces.
                int[] workplaces = PopData.instance.WorkplaceCache(info, (int)level);
                float totalWorkers = workplaces[0] + workplaces[1] + workplaces[2] + workplaces[3];
                float multiplier;

                // New settings, based on population.
                switch (subService)
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

                // Multiply total workers by multipler and overall multiplier (from settings) to get result.
                __result = (int)((totalWorkers * comVisitMults[arrayIndex] * multiplier) / 100f);
            }
            else
            {
                // Old settings, based on lot size.
                // Just go to default game method.
                return true;
            }

            // Always set at least one.
            if (__result < 1)
            {
                Logging.Error("invalid visitcount result ", __result.ToString(), " for ", __instance.m_info.name, "; setting to 1");
                __result = 1;
            }

            // Don't execute base method after this.
            return false;
        }


        /// <summary>
        /// Sets the visit mode for all commercial subservices to the specified mode.
        /// </summary>
        internal static int SetVisitModes
        {
            set
            {
                for (int i = 0; i < comVisitModes.Length; ++i)
                {
                    comVisitModes[i] = value;
                }
            }
        }

        /// <summary>
        /// Sets the visit percentage multiplier for all commercial subservices to the specified mode.
        /// </summary>
        internal static int SetVisitMults
        {
            set
            {
                for (int i = 0; i < comVisitMults.Length; ++i)
                {
                    comVisitMults[i] = value;
                }
            }
        }


        /// <summary>
        /// Gets the current commerical visit mode for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Visit calculation mode</returns>
        internal static int GetVisitMode(ItemClass.SubService subService) => comVisitModes[GetIndex(subService)];


        /// <summary>
        /// Sets the current commerical visit mode for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        /// <returns>Visit calculaiton mode</returns>
        internal static void SetVisitMode(ItemClass.SubService subService, int value) => comVisitModes[GetIndex(subService)] = value;


        /// <summary>
        /// Gets the current commerical visit multiplier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Visit multiplier</returns>
        internal static int GetVisitMult(ItemClass.SubService subService) => comVisitMults[GetIndex(subService)];


        /// <summary>
        /// Sets the current commerical visit multipier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        internal static void SetVisitMult(ItemClass.SubService subService, int value) => comVisitMults[GetIndex(subService)] = value;


        /// <summary>
        /// Serializes the current visitor mode settings ready for XML.
        /// </summary>
        /// <returns>New list of visitor mode entries ready for serialization</returns>
        internal static List<SubServiceMode> SerializeVisits()
        {
            List<SubServiceMode> entries = new List<SubServiceMode>();

            for (int i = 0; i < comVisitModes.Length; ++i)
            {
                entries.Add(new SubServiceMode
                {
                    subService = subServices[i],
                    mode = comVisitModes[i],
                    multiplier = comVisitMults[i]
                });
            }

            return entries;
        }


        /// <summary>
        /// Deserializes XML visitor mode entries.
        /// </summary>
        /// <param name="entries">List of visitor mode entries to deserialize</param>
        /// <returns>New list of visitor mode entries ready for serialization</returns>
        internal static void DeserializeVisits(List<SubServiceMode> entries)
        {
            foreach (SubServiceMode entry in entries)
            {
                SetVisitMode(entry.subService, entry.mode);
                SetVisitMult(entry.subService, entry.multiplier);
            }
        }


        /// <summary>
        /// Returns the sub-service array index for the given sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Array index</returns>
        private static int GetIndex(ItemClass.SubService subService)
        {
            switch (subService)
            {
                case ItemClass.SubService.CommercialLow:
                    return (int)SubServiceIndex.CommercialLow;
                case ItemClass.SubService.CommercialHigh:
                    return (int)SubServiceIndex.CommercialHigh;
                case ItemClass.SubService.CommercialLeisure:
                    return (int)SubServiceIndex.CommercialLeisure;
                case ItemClass.SubService.CommercialTourist:
                    return (int)SubServiceIndex.CommercialTourist;
                case ItemClass.SubService.CommercialEco:
                    return (int)SubServiceIndex.CommercialEco;
                default:
                    Logging.Error("invalid subservice ", subService.ToString(), " passed to RealisticVisitplaceCount.GetIndex");
                    return (int)SubServiceIndex.CommercialLow;
            }
        }
    }
}
