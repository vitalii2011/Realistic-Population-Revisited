using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;


#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0060 // Remove unused parameter


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement production changes for office buildings, and supporting methods.
    /// </summary>
    [HarmonyPatch(typeof(OfficeBuildingAI), nameof (OfficeBuildingAI.CalculateProductionCapacity))]
    public static class RealisticOfficeProduction
    {
        // Default multiplier.
        internal const int DefaultOfficeMult = 100;

        // Maximum multiplier.
        internal const int MaxProdMult = 200;

        // Settings per subservice.
        private static int genericOfficeProdMult = DefaultOfficeMult;
        private static int highTechOfficeProdMult = DefaultOfficeMult;


        /// <summary>
        /// Sets the production percentage multiplier for all office subservices to the specified mode.
        /// </summary>
        /// <param name="visitMode">Visit mode to set</param>
        internal static int SetVisitMults
        {
            set
            {
                genericOfficeProdMult = DefaultOfficeMult;
                highTechOfficeProdMult = DefaultOfficeMult;
            }
        }


        /// <summary>
        /// Harmony Prefix patch to OfficeBuildingAI.CalculateProductionCapacity to implement mod production calculations.
        /// </summary>
        /// <param name="__result">Original method result</param>
        /// <param name="__instance">Original AI instance reference</param>
        /// <param name="level">Building level</param>
        /// <returns>False (don't execute base game method after this)</returns>
        public static bool Prefix(ref int __result, OfficeBuildingAI __instance, ItemClass.Level level, int width, int length)
        {
            // Get builidng info.
            BuildingInfo info = __instance.m_info;
            ItemClass.SubService subService = info.m_class.m_subService;

                // Get cached workplace count and calculate total workplaces.
                int[] workplaces = PopData.instance.WorkplaceCache(info, (int)level);
                float totalWorkers = workplaces[0] + workplaces[1] + workplaces[2] + workplaces[3];

                // Multiply total workers by multipler and overall multiplier (from settings) to get result; divisor is 1,000 to match original mod 1/10th when at 100% production.
                __result = (int)((totalWorkers * (subService == ItemClass.SubService.OfficeHightech ? highTechOfficeProdMult : genericOfficeProdMult)) / 1000f);


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
        /// Gets the current office production multiplier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Visit mode</returns>
        internal static int GetProdMult(ItemClass.SubService subService)
        {
            switch (subService)
            {
                case ItemClass.SubService.OfficeGeneric:
                    return genericOfficeProdMult;
                case ItemClass.SubService.OfficeHightech:
                    return highTechOfficeProdMult;
                default:
                    Logging.Error("invalid subservice passed to GetProdMult");
                    return 0;
            }
        }


        /// <summary>
        /// Sets the current office production multipier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        /// <returns>Visit mode</returns>
        internal static void SetProdMult(ItemClass.SubService subService, int value)
        {
            int cleanValue = Mathf.Clamp(0, value, MaxProdMult);

            switch (subService)
            {
                case ItemClass.SubService.OfficeGeneric:
                    genericOfficeProdMult = cleanValue;
                    break;
                case ItemClass.SubService.OfficeHightech:
                    highTechOfficeProdMult = cleanValue;
                    break;
                default:
                    Logging.Error("invalid subservice passed to SetProdMult");
                    break;
            }
        }


        /// <summary>
        /// Serializes the current office production multiplier settings ready for XML.
        /// </summary>
        /// <returns>New list of office production multiplier entries ready for serialization</returns>
        internal static List<SubServiceEntry> SerializeProdMults()
        {
            return new List<SubServiceEntry>()
            {
                new SubServiceEntry
                {
                    subService = ItemClass.SubService.OfficeGeneric,
                    value = genericOfficeProdMult
                },
                new SubServiceEntry
                {
                    subService = ItemClass.SubService.OfficeHightech,
                    value = highTechOfficeProdMult
                }
            };
        }


        /// <summary>
        /// Deserializes XML visitor mode entries.
        /// </summary>
        /// <param name="entries">List of office production multiplier entries to deserialize</param>
        /// <returns>New list of voffice production multiplier entries ready for serialization</returns>
        internal static void DeserializeProdMults(List<SubServiceEntry> entries)
        {
            foreach (SubServiceEntry entry in entries)
            {
                SetProdMult(entry.subService, entry.value);
            }
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression
