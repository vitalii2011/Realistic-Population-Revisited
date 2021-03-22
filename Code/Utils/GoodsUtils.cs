using UnityEngine;
using System.Collections.Generic;


namespace RealPop2
{
    /// <summary>
    /// Static utilities class for handling goods production and consumption.
    /// </summary>
    public static class GoodsUtils
    {
        // Defaults.
        internal const int DefaultSalesMult = 100;


        // Internal multipliers.
        private static int lowComMult = DefaultSalesMult, highComMult = DefaultSalesMult, ecoComMult = DefaultSalesMult, touristMult = DefaultSalesMult, leisureMult = DefaultSalesMult;


        /// <summary>
        /// Returns the current commercial sales multiplier for the given commercial building.
        /// </summary>
        /// <param name="building">Specified building</param>
        /// <returns>Current sales multiplier as integer pecentage</returns>
        public static int GetComMult(ref Building building) => GetComMult(building.Info.GetSubService());


        /// <summary>
        /// Returns the current commercial sales multiplier for the given commercial subservice.
        /// </summary>
        /// <param name="subService">Specified subservice</param>
        /// <returns>Current sales multiplier as integer pecentage</returns>
        internal static int GetComMult(ItemClass.SubService subService)
        {
            switch (subService)
            {
                case ItemClass.SubService.CommercialHigh:
                    return highComMult;
                case ItemClass.SubService.CommercialEco:
                    return ecoComMult;
                case ItemClass.SubService.CommercialLeisure:
                    return leisureMult;
                case ItemClass.SubService.CommercialTourist:
                    return touristMult;
                default:
                    return lowComMult;
            }
        }


        /// <summary>
        /// Sets the current commercial sales multiplier for the given commercial subservice.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        internal static void SetComMult(ItemClass.SubService subService, int value)
        {
            int cleanValue = Mathf.Clamp(value, 0, 100);

            switch (subService)
            {
                case ItemClass.SubService.CommercialLow:
                    lowComMult = cleanValue;
                    break;
                case ItemClass.SubService.CommercialHigh:
                    highComMult = cleanValue;
                    break;
                case ItemClass.SubService.CommercialEco:
                    ecoComMult = cleanValue;
                    break;
                case ItemClass.SubService.CommercialLeisure:
                    leisureMult = cleanValue;
                    break;
                case ItemClass.SubService.CommercialTourist:
                    touristMult = cleanValue;
                    break;
            }
        }


        /// <summary>
        /// Serializes the current settings ready for XML.
        /// </summary>
        /// <returns>New list of sub-service entries ready for serialization</returns>
        internal static List<SubServiceEntry> SerializeSalesMults()
        {
            return new List<SubServiceEntry>
            {
                new SubServiceEntry
                {
                    SubService = ItemClass.SubService.CommercialLow,
                    Value = lowComMult
                },
                new SubServiceEntry
                {
                    SubService = ItemClass.SubService.CommercialHigh,
                    Value = highComMult
                },
                new SubServiceEntry
                {
                    SubService = ItemClass.SubService.CommercialEco,
                    Value = ecoComMult
                },
                new SubServiceEntry
                {
                    SubService = ItemClass.SubService.CommercialLeisure,
                    Value = leisureMult
                },
                new SubServiceEntry
                {
                    SubService = ItemClass.SubService.CommercialTourist,
                    Value = touristMult
                }
            };
        }


        /// <summary>
        /// Deserializes XML sub-service entries.
        /// </summary>
        /// <param name="entries">List of sub-service entries to deserialize</param>
        /// <returns>New list of sub-service entries ready for serialization</returns>
        internal static void DeSerializeSalesMults(List<SubServiceEntry> entries)
        {
            foreach (SubServiceEntry entry in entries)
            {
                SetComMult(entry.SubService, entry.Value);
            }
        }
    }
}
