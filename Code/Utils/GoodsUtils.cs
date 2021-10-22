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
        internal const int DefaultSalesMult = 60;
        internal const int DefaultInventory = 48000;
        internal const int MinInventory = 8000;
        internal const int MaxInventory = 57000;

        // Internal multipliers.
        private static int lowComMult = DefaultSalesMult, highComMult = DefaultSalesMult, ecoComMult = DefaultSalesMult, touristMult = DefaultSalesMult, leisureMult = DefaultSalesMult;

        // Internal inventory caps;
        private static int lowComInv = DefaultInventory, highComInv = DefaultInventory, ecoComInv = DefaultInventory, touristInv = DefaultInventory, leisureInv = DefaultInventory;


        /// <summary>
        /// Returns the current commercial sales multiplier for the given commercial building.
        /// </summary>
        /// <param name="building">Specified building</param>
        /// <returns>Current sales multiplier as integer pecentage</returns>
        public static int GetComMult(ref Building building) => GetComMult(building.Info.GetSubService());


        /// <summary>
        /// Returns the current inventory cap for the given commercial building.
        /// </summary>
        /// <param name="building">Specified building</param>
        /// <returns>Current sales multiplier as integer pecentage</returns>
        public static int GetInventoryCap(ref Building building) => GetInventoryCap(building.Info.GetSubService());


        /// <summary>
        /// Returns the current inventory cap for the given commercial building ai.
        /// </summary>
        /// <param name="building">Specified building</param>
        /// <returns>Current sales multiplier as integer pecentage</returns>
        public static int GetInventoryCap(CommercialBuildingAI ai) => GetInventoryCap(ai.m_info.GetSubService());


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
        /// Returns the current inventory cap for the given commercial subservice.
        /// </summary>
        /// <param name="subService">Specified subservice</param>
        /// <returns>Current inventory cap</returns>
        public static int GetInventoryCap(ItemClass.SubService subService)
        {

            switch (subService)
            {
                case ItemClass.SubService.CommercialHigh:
                    return highComInv;
                case ItemClass.SubService.CommercialEco:
                    return ecoComInv;
                case ItemClass.SubService.CommercialLeisure:
                    return leisureInv;
                case ItemClass.SubService.CommercialTourist:
                    return touristInv;
                default:
                    return lowComInv;
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
        /// Sets the current inventory cap for the given commercial subservice.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        internal static void SetInventoryCap(ItemClass.SubService subService, int value)
        {
            int cleanValue = Mathf.Clamp(value, MinInventory, MaxInventory);

            switch (subService)
            {
                case ItemClass.SubService.CommercialLow:
                    lowComInv = cleanValue;
                    break;
                case ItemClass.SubService.CommercialHigh:
                    highComInv = cleanValue;
                    break;
                case ItemClass.SubService.CommercialEco:
                    ecoComInv = cleanValue;
                    break;
                case ItemClass.SubService.CommercialLeisure:
                    leisureInv = cleanValue;
                    break;
                case ItemClass.SubService.CommercialTourist:
                    touristInv = cleanValue;
                    break;
            }
        }


        /// <summary>
        /// Serializes the current sales multiplier settings ready for XML.
        /// </summary>
        /// <returns>New list of sub-service entries ready for serialization</returns>
        internal static List<SubServiceValue> SerializeSalesMults()
        {
            return new List<SubServiceValue>
            {
                new SubServiceValue
                {
                    subService = ItemClass.SubService.CommercialLow,
                    value = lowComMult
                },
                new SubServiceValue
                {
                    subService = ItemClass.SubService.CommercialHigh,
                    value = highComMult
                },
                new SubServiceValue
                {
                    subService = ItemClass.SubService.CommercialEco,
                    value = ecoComMult
                },
                new SubServiceValue
                {
                    subService = ItemClass.SubService.CommercialLeisure,
                    value = leisureMult
                },
                new SubServiceValue
                {
                    subService = ItemClass.SubService.CommercialTourist,
                    value = touristMult
                }
            };
        }


        /// <summary>
        /// Serializes the current inventory demand caps ready for XML.
        /// </summary>
        /// <returns>New list of sub-service entries ready for serialization</returns>
        internal static List<SubServiceValue> SerializeInvCaps()
        {
            return new List<SubServiceValue>
            {
                new SubServiceValue
                {
                    subService = ItemClass.SubService.CommercialLow,
                    value = lowComInv
                },
                new SubServiceValue
                {
                    subService = ItemClass.SubService.CommercialHigh,
                    value = highComInv
                },
                new SubServiceValue
                {
                    subService = ItemClass.SubService.CommercialEco,
                    value = ecoComInv
                },
                new SubServiceValue
                {
                    subService = ItemClass.SubService.CommercialLeisure,
                    value = leisureInv
                },
                new SubServiceValue
                {
                    subService = ItemClass.SubService.CommercialTourist,
                    value = touristInv
                }
            };
        }


        /// <summary>
        /// Deserializes XML sub-service entries for sales multipliers.
        /// </summary>
        /// <param name="entries">List of sub-service entries to deserialize</param>
        /// <returns>New list of sub-service entries ready for serialization</returns>
        internal static void DeserializeSalesMults(List<SubServiceValue> entries)
        {
            foreach (SubServiceValue entry in entries)
            {
                SetComMult(entry.subService, entry.value);
            }
        }


        /// <summary>
        /// Deserializes XML sub-service entries for inventory demand caps.
        /// </summary>
        /// <param name="entries">List of sub-service entries to deserialize</param>
        /// <returns>New list of sub-service entries ready for serialization</returns>
        internal static void DeserializeInvCaps(List<SubServiceValue> entries)
        {
            foreach (SubServiceValue entry in entries)
            {
                SetInventoryCap(entry.subService, entry.value);
            }
        }
    }
}
