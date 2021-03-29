using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement production changes for office buildings, and supporting methods.
    /// </summary>
    [HarmonyPatch(typeof(IndustrialExtractorAI), nameof(IndustrialExtractorAI.CalculateProductionCapacity))]
    public static class RealisticExtractorProduction
    {
        // Extractor production calculation modes.
        internal enum ProdModes
        {
            popCalcs = 0,
            legacy
        }

        // Default multiplier.
        internal const int DefaultProdMult = 100;

        // Maximum multiplier.
        internal const int MaxProdMult = 200;

        // Dictionaries for calculation mode and multipliers.
        private static readonly Dictionary<ItemClass.SubService, int> prodModes = new Dictionary<ItemClass.SubService, int>
        {
            { ItemClass.SubService.IndustrialFarming, (int)ProdModes.legacy },
            { ItemClass.SubService.IndustrialForestry, (int)ProdModes.legacy },
            { ItemClass.SubService.IndustrialOil, (int)ProdModes.legacy },
            { ItemClass.SubService.IndustrialOre, (int)ProdModes.legacy }
        };
        private static readonly Dictionary<ItemClass.SubService, int> prodMults = new Dictionary<ItemClass.SubService, int>
        {
            { ItemClass.SubService.IndustrialFarming, DefaultProdMult },
            { ItemClass.SubService.IndustrialForestry, DefaultProdMult },
            { ItemClass.SubService.IndustrialOil, DefaultProdMult },
            { ItemClass.SubService.IndustrialOre, DefaultProdMult }
        };


        /// <summary>
        /// Harmony Prefix patch to IndustrialBuildingAI.CalculateProductionCapacity to implement mod production calculations.
        /// </summary>
        /// <param name="__result">Original method result</param>
        /// <param name="__instance">Original AI instance reference</param>
        /// <param name="level">Building level</param>
        /// <returns>False (don't execute base game method after this)</returns>
        public static bool Prefix(ref int __result, IndustrialExtractorAI __instance, ItemClass.Level level, int width, int length)
        {
            // Get builidng info.
            BuildingInfo info = __instance.m_info;
            ItemClass.SubService subService = info.GetSubService();

            // New or old method?
            if (prodModes[subService] == (int)ProdModes.popCalcs)
            {
                // New settings, based on population.
                float multiplier;
                switch (subService)
                {
                    case ItemClass.SubService.IndustrialFarming:
                    case ItemClass.SubService.IndustrialForestry:
                        multiplier = 1f;
                        break;
                    default:
                        multiplier = 0.625f;
                        break;
                }

                // Get cached workplace count and calculate total workplaces.
                int[] workplaces = PopData.instance.WorkplaceCache(info, (int)level);

                float totalWorkers = workplaces[0] + workplaces[1] + workplaces[2] + workplaces[3];
                // Multiply total workers by multipler and overall multiplier (from settings) to get result.
                __result = (int)((totalWorkers * multiplier * prodMults[subService]) / 100f);
            }
            else
            {
                // Legacy calcs.
                int[] array = IndustrialExtractorAIMod.GetArray(__instance.m_info, IndustrialExtractorAIMod.EXTRACT_LEVEL);

                // Original method return value.
                __result = Mathf.Max(100, width * length * array[DataStore.PRODUCTION]) / 100;
            }


            // Always set at least one.
            if (__result < 1)
            {
                Logging.Error("invalid production result ", __result.ToString(), " for ", __instance.m_info.name, "; setting to 1");
                __result = 1;
            }

            // Don't execute base method after this.
            return false;
        }


        /// <summary>
        /// Sets the extractor production mode for all commercial subservices to the specified mode.
        /// </summary>
        internal static int SetProdModes
        {
            set
            {
                foreach (ItemClass.SubService subService in prodModes.Keys)
                {
                    prodModes[subService] = value;
                }
            }
        }

        /// <summary>
        /// Sets the extractor production percentage multiplier for all commercial subservices to the specified mode.
        /// </summary>
        internal static int SetProdMults
        {
            set
            {
                foreach (ItemClass.SubService subService in prodMults.Keys)
                {
                    prodMults[subService] = value;
                }
            }
        }


        /// <summary>
        /// Gets the current extractor production calculation mode for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Production calculation mode</returns>
        internal static int GetProdMode(ItemClass.SubService subService)
        {
            if (prodModes.ContainsKey(subService))
            {
                return prodModes[subService];
            }

            Logging.Error("invalid subservice ", subService.ToString(), " passed to extractor GetProdMode");
            return 0;
        }


        /// <summary>
        /// Sets the current extractor production mode for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        internal static void SetProdMode(ItemClass.SubService subService, int value)
        {
            if (prodModes.ContainsKey(subService))
            {
                prodModes[subService] = Mathf.Clamp(0, value, 1);
            }
            else
            {
                Logging.Error("invalid subservice ", subService.ToString(), " passed to extractor SetProdMode");
            }
        }


        /// <summary>
        /// Gets the current extractor production percentage multiplier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Visit mode</returns>
        internal static int GetProdMult(ItemClass.SubService subService)
        {
            if (prodMults.ContainsKey(subService))
            {
                return prodMults[subService];
            }

            Logging.Error("invalid subservice ", subService.ToString(), " passed to extractor GetProdMult");
            return 0;
        }


        /// <summary>
        /// Sets the current extractor production percentage multipier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        /// <returns>Visit mode</returns>
        internal static void SetProdMult(ItemClass.SubService subService, int value)
        {
            if (prodMults.ContainsKey(subService))
            {
                prodMults[subService] = Mathf.Clamp(0, value, MaxProdMult);
            }

            Logging.Error("invalid subservice ", subService.ToString(), " passed to extractor SetProdMult");
        }


        /// <summary>
        /// Serializes the current extractor production mode settings ready for XML.
        /// </summary>
        /// <returns>New list of extractor production mode entries ready for serialization</returns>
        internal static List<SubServiceMode> SerializeProds()
        {
            List<SubServiceMode> entries = new List<SubServiceMode>();

            foreach (KeyValuePair<ItemClass.SubService, int> entry in prodModes)
            {
                entries.Add(new SubServiceMode
                {
                    subService = entry.Key,
                    mode = entry.Value,
                    multiplier = prodMults[entry.Key]
                });
            }

            return entries;
        }


        /// <summary>
        /// Deserializes XML extractor production mode entries.
        /// </summary>
        /// <param name="entries">List of extractor production mode entries to deserialize</param>
        /// <returns>New list of extractor production mode entries ready for serialization</returns>
        internal static void DeserializeProds(List<SubServiceMode> entries)
        {
            foreach (SubServiceMode entry in entries)
            {
                SetProdMode(entry.subService, entry.mode);
                SetProdMult(entry.subService, entry.multiplier);
            }
        }
    }
}
