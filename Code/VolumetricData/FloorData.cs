using System.Linq;
using System.Collections.Generic;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Centralised store and management of floor calculation data.
    /// </summary>
    internal class FloorData : CalcData
    {
        // Instance reference.
        internal static FloorData instance;


        /// <summary>
        /// Returns a list of available calculation packs.
        /// </summary>
        /// <returns>Array of available calculation packs</returns>
        internal DataPack[] GetPacks() => calcPacks.ToArray();


        /// <summary>
        /// Constructor - initializes inbuilt default calculation packs and performs other setup tasks.
        /// </summary>
        public FloorData()
        {
            // Default; standard 3m stories.
            FloorDataPack newPack = new FloorDataPack
            {
                name = "default",
                displayName = Translations.Translate("RPR_PCK_FDF_NAM"),
                description = Translations.Translate("RPR_PCK_FDF_DES"),
                version = (int)DataVersion.one,
                floorHeight = 3f,
                firstFloorMin = 3f,
                firstFloorExtra = 0f,
                firstFloorEmpty = false
            };
            calcPacks.Add(newPack);

            // Standalone houses.
            newPack = new FloorDataPack
            {
                name = "house",
                displayName = Translations.Translate("RPR_PCK_FHO_NAM"),
                description = Translations.Translate("RPR_PCK_FHO_DES"),
                version = (int)DataVersion.one,
                floorHeight = 3f,
                firstFloorMin = 3f,
                firstFloorExtra = 0f,
                firstFloorEmpty = false
            };
            calcPacks.Add(newPack);

            // Buildings with lobbies.
            newPack = new FloorDataPack
            {
                name = "lobbies",
                displayName = Translations.Translate("RPR_PCK_FDL_NAM"),
                description = Translations.Translate("RPR_PCK_FDL_DES"),
                version = (int)DataVersion.one,
                floorHeight = 3f,
                firstFloorMin = 3f,
                firstFloorExtra = 1f,
                firstFloorEmpty = true
            };
            calcPacks.Add(newPack);

            // Commercial buildings
            newPack = new FloorDataPack
            {
                name = "commercial",
                displayName = Translations.Translate("RPR_PCK_FCM_NAM"),
                description = Translations.Translate("RPR_PCK_FCM_DES"),
                version = (int)DataVersion.one,
                floorHeight = 4f,
                firstFloorMin = 3f,
                firstFloorExtra = 3f,
                firstFloorEmpty = false
            };
            calcPacks.Add(newPack);

            // Warehouses (commercial and industrial)
            newPack = new FloorDataPack
            {
                name = "warehouse",
                displayName = Translations.Translate("RPR_PCK_FWH_NAM"),
                description = Translations.Translate("RPR_PCK_FWH_DES"),
                version = (int)DataVersion.one,
                floorHeight = 9f,
                firstFloorMin = 3f,
                firstFloorExtra = 6f,
                firstFloorEmpty = false
            };
            calcPacks.Add(newPack);

            // High-bay warehouses (commercial and industrial)
            newPack = new FloorDataPack
            {
                name = "highbay",
                displayName = Translations.Translate("RPR_PCK_FHB_NAM"),
                description = Translations.Translate("RPR_PCK_FHB_DES"),
                version = (int)DataVersion.one,
                floorHeight = 12f,
                firstFloorMin = 3f,
                firstFloorExtra = 9f,
                firstFloorEmpty = false
            };
            calcPacks.Add(newPack);

            // Extractors and processors
            newPack = new FloorDataPack
            {
                name = "extractor",
                displayName = Translations.Translate("RPR_PCK_FEX_NAM"),
                description = Translations.Translate("RPR_PCK_FEX_DES"),
                version = (int)DataVersion.one,
                floorHeight = 99f,
                firstFloorMin = 3f,
                firstFloorExtra = 9f,
                firstFloorEmpty = false
            };
            calcPacks.Add(newPack);
        }


        /// <summary>
        /// Returns the inbuilt default calculation pack for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="subService">Sub-service</param>
        /// <returns>Default calculation data pack</returns>
        internal override DataPack BaseDefaultPack(ItemClass.Service service, ItemClass.SubService subService)
        {
            string defaultName;

            // Manual breakdown.
            switch (service)
            {
                case ItemClass.Service.Residential:
                    switch (subService)
                    {
                        case ItemClass.SubService.ResidentialHigh:
                        case ItemClass.SubService.ResidentialHighEco:
                            defaultName = "default";
                            break;
                        default:
                            defaultName = "house";
                            break;
                    }
                    break;
                case ItemClass.Service.Industrial:
                    switch (subService)
                    {
                        case ItemClass.SubService.IndustrialFarming:
                        case ItemClass.SubService.IndustrialForestry:
                        case ItemClass.SubService.IndustrialOil:
                        case ItemClass.SubService.IndustrialOre:
                            defaultName = "extractor";
                            break;
                        default:
                            defaultName = "commercial";
                            break;
                    }
                    break;
                default:
                    // Default is commercial.
                    defaultName = "commercial";
                    break;
            }

            // Match name to floorpack.
            return calcPacks.Find(pack => pack.name.Equals(defaultName));
        }


        /// <summary>
        /// Serializes building pack settings to XML.
        /// </summary>
        /// <param name="existingList">Existing list to modify, from population pack serialization (null if none)</param>
        /// <returns>New list of building pack settings ready for XML</returns>
        internal List<BuildingRecord> SerializeBuildings(SortedList<string, BuildingRecord> existingList)
        {
            // Return list.
            SortedList<string, BuildingRecord> returnList = existingList ?? new SortedList<string, BuildingRecord>();

            // Iterate through each key (BuildingInfo) in our dictionary and serialise it into a BuildingRecord.
            foreach (string prefabName in buildingDict.Keys)
            {
                string packName = buildingDict[prefabName].name;

                // Check to see if our existing list already contains this building.
                if (returnList.ContainsKey(prefabName))
                {
                    // Yes; update that record to include this floor pack.
                    returnList[prefabName].floorPack = packName;
                }
                else
                {
                    // No; add a new record with this floor pack.
                    BuildingRecord newRecord = new BuildingRecord { prefab = prefabName, floorPack = packName };
                    returnList.Add(prefabName, newRecord);
                }
            }

            return returnList.Values.ToList();
        }


        /// <summary>
        /// Extracts the relevant pack name (floor or pop pack) from a building line record.
        /// </summary>
        /// <param name="buildingRecord">Building record to extract from</param>
        /// <returns>Floor pack name (if any)</returns>
        protected override string BuildingPack(BuildingRecord buildingRecord) => buildingRecord.floorPack;
    }
}