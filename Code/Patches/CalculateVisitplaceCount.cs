using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;


#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0060 // Remove unused parameter


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

        // Default multiplier.
        internal const int DefaultVisitMult = 40;

        // Maximum multiplier.
        internal const int MaxVisitMult = 100;


        // Dictionaries for calculation mode and multipliers.
        private static readonly Dictionary<ItemClass.SubService, int> comVisitModes = new Dictionary<ItemClass.SubService, int>
        {
            { ItemClass.SubService.CommercialLow, (int)ComVisitModes.legacy },
            { ItemClass.SubService.CommercialHigh, (int)ComVisitModes.legacy },
            { ItemClass.SubService.CommercialLeisure, (int)ComVisitModes.legacy },
            { ItemClass.SubService.CommercialTourist, (int)ComVisitModes.legacy },
            { ItemClass.SubService.CommercialEco, (int)ComVisitModes.legacy }
        };
        private static readonly Dictionary<ItemClass.SubService, int> comVisitMults = new Dictionary<ItemClass.SubService, int>
        {
            { ItemClass.SubService.CommercialLow, DefaultVisitMult },
            { ItemClass.SubService.CommercialHigh, DefaultVisitMult },
            { ItemClass.SubService.CommercialLeisure, DefaultVisitMult },
            { ItemClass.SubService.CommercialTourist, DefaultVisitMult },
            { ItemClass.SubService.CommercialEco, DefaultVisitMult }
        };


        /// <summary>
        /// Sets the visit mode for all commercial subservices to the specified mode.
        /// </summary>
        /// <param name="visitMode">Visit mode to set</param>
        internal static int SetVisitModes
        {
            set
            {
                foreach (ItemClass.SubService subService in comVisitModes.Keys)
                {
                    comVisitModes[subService] = value;
                }
            }
        }

        /// <summary>
        /// Sets the visit percentage multiplier for all commercial subservices to the specified mode.
        /// </summary>
        /// <param name="visitMode">Visit mode to set</param>
        internal static int SetVisitMults
        {
            set
            {
                foreach (ItemClass.SubService subService in comVisitMults.Keys)
                {
                    comVisitMults[subService] = value;
                }
            }
        }


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
            ItemClass.SubService subService = info.m_class.m_subService;

            // New or old calculations?
            if (comVisitModes[subService] == (int)RealisticVisitplaceCount.ComVisitModes.popCalcs)
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
                __result = (int)((totalWorkers * comVisitMults[subService] * multiplier) / 100f);
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
        /// Gets the current commerical visit mode for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Visit mode</returns>
        internal static int GetVisitMode(ItemClass.SubService subService)
        {
            if (comVisitModes.ContainsKey(subService))
            {
                return comVisitModes[subService];
            }

            Logging.Error("invalid subservice passed to GetVisitMode");
                return 0;
        }


        /// <summary>
        /// Sets the current commerical visit mode for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        /// <returns>Visit mode</returns>
        internal static void SetVisitMode(ItemClass.SubService subService, int value)
        {
            if (comVisitModes.ContainsKey(subService))
            {
                comVisitModes[subService] = Mathf.Clamp(0, value, 1);
            }

            Logging.Error("invalid subservice passed to SetVisitMode");
        }


        /// <summary>
        /// Gets the current commerical visit multiplier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Visit mode</returns>
        internal static int GetVisitMult(ItemClass.SubService subService)
        {
            if (comVisitModes.ContainsKey(subService))
            {
                return comVisitMults[subService];
            }

            Logging.Error("invalid subservice passed to GetVisitMult");
            return 0;
        }


        /// <summary>
        /// Sets the current commerical visit multipier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        /// <returns>Visit mode</returns>
        internal static void SetVisitMult(ItemClass.SubService subService, int value)
        {
            if (comVisitMults.ContainsKey(subService))
            {
                comVisitMults[subService] = Mathf.Clamp(0, value, MaxVisitMult);
            }

            Logging.Error("invalid subservice passed to SetVisitMult");
        }


        /// <summary>
        /// Serializes the current visitor mode settings ready for XML.
        /// </summary>
        /// <returns>New list of visitor mode entries ready for serialization</returns>
        internal static List<VisitorMode> SerializeVisits()
        {
            List<VisitorMode> entries = new List<VisitorMode>();

            foreach(KeyValuePair<ItemClass.SubService, int> entry in comVisitModes)
            {
                entries.Add(new VisitorMode
                {
                    subService = entry.Key,
                    mode = entry.Value,
                    multiplier = comVisitMults[entry.Key]
                });
            }

            return entries;
        }


        /// <summary>
        /// Deserializes XML visitor mode entries.
        /// </summary>
        /// <param name="entries">List of visitor mode entries to deserialize</param>
        /// <returns>New list of visitor mode entries ready for serialization</returns>
        internal static void DeserializeVisits(List<VisitorMode> entries)
        {
            foreach (VisitorMode entry in entries)
            {
                SetVisitMode(entry.subService, entry.mode);
                SetVisitMult(entry.subService, entry.multiplier);
            }
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
