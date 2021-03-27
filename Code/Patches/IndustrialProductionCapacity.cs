using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement production changes for office buildings, and supporting methods.
    /// </summary>
    [HarmonyPatch(typeof(IndustrialBuildingAI), nameof(IndustrialBuildingAI.CalculateProductionCapacity))]
    public static class RealisticIndustrialProduction
    {
        // Industrial production calculation modes.
        internal enum ProdModes
        {
            popCalcs = 0,
            legacy
        }

        // Default multiplier.
        internal const int DefaultProdMult = 100;

        // Maximum multiplier.
        internal const int MaxProdMult = 200;

        // Generic industrial settings.
        private static int prodMode = (int)ProdModes.legacy;
        private static int prodMult = DefaultProdMult;


        /// <summary>
        /// Harmony Prefix patch to IndustrialBuildingAI.CalculateProductionCapacity to implement mod production calculations.
        /// </summary>
        /// <param name="__result">Original method result</param>
        /// <param name="__instance">Original AI instance reference</param>
        /// <param name="level">Building level</param>
        /// <returns>False (don't execute base game method after this)</returns>
        public static bool Prefix(ref int __result, IndustrialBuildingAI __instance, ItemClass.Level level, int width, int length)
        {
            // Get builidng info.
            BuildingInfo info = __instance.m_info;

            // New or old method?
            if (prodMode == (int)ProdModes.popCalcs)
            {
                // New settings, based on population.
                float multiplier;
                switch (info.GetClassLevel())
                {
                    case ItemClass.Level.Level1:
                        multiplier = 1f;
                        break;
                    case ItemClass.Level.Level2:
                        multiplier = 0.933333f;
                        break;
                    default:
                        multiplier = 0.8f;
                        break;
                }

                // Get cached workplace count and calculate total workplaces.
                int[] workplaces = PopData.instance.WorkplaceCache(info, (int)level);

                float totalWorkers = workplaces[0] + workplaces[1] + workplaces[2] + workplaces[3];
                // Multiply total workers by multipler and overall multiplier (from settings) to get result.
                __result = (int)((totalWorkers * multiplier * prodMult) / 100f);
            }
            else
            {
                // Legacy calcs.
                int[] array = IndustrialBuildingAIMod.GetArray(__instance.m_info, (int)level);

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
        /// Gets the current industrial production calculation mode.
        /// </summary>
        /// <returns>Visit mode</returns>
        internal static int GetProdMode() => prodMode;


        /// <summary>
        /// Sets the current industrial production calculation mode.
        /// </summary>
        /// <param name="value">Value to set</param>
        /// <returns>Visit mode</returns>
        internal static void SetProdMode(int value)
        {
            prodMode = value;
        }

        /// <summary>
        /// Gets the current industrial production multiplier.
        /// </summary>
        /// <returns>Visit mode</returns>
        internal static int GetProdMult() => prodMult;


        /// <summary>
        /// Sets the current industrial production multipier.
        /// </summary>
        /// <param name="value">Value to set</param>
        /// <returns>Visit mode</returns>
        internal static void SetProdMult(int value)
        {
            prodMult = Mathf.Clamp(0, value, MaxProdMult);
        }


        /// <summary>
        /// Serializes the current industrial production mode settings ready for XML.
        /// </summary>
        /// <returns>New list of industrial production mode entries ready for serialization</returns>
        internal static List<SubServiceMode> SerializeProds()
        {
            return new List<SubServiceMode>
            {
                new SubServiceMode
                {
                    subService = ItemClass.SubService.IndustrialGeneric,
                    mode = prodMode,
                    multiplier = prodMult
                }
            };
        }


        /// <summary>
        /// Deserializes XML industrial production mode entries.
        /// </summary>
        /// <param name="entries">List of industrial mode entries to deserialize</param>
        /// <returns>New list of industrial mode entries ready for serialization</returns>
        internal static void DeserializeProds(List<SubServiceMode> entries)
        {
            foreach (SubServiceMode entry in entries)
            {
                if (entry.subService == ItemClass.SubService.IndustrialGeneric)
                {
                    SetProdMode(entry.mode);
                    SetProdMult(entry.multiplier);
                }
            }
        }
    }
}
