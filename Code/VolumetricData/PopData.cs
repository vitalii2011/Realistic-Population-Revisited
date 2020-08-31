using System.Collections.Generic;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Centralised store and management of population calculation data.
    /// </summary>
    internal static class PopData
    {
        // List of data definition packs.
        internal static List<CalcPack> calcPacks;

        // List of building settings.
        internal static Dictionary<string, CalcPack> buildingDict;


        /// <summary>
        /// Returns whether or not legacy calculations are in force for the given prefab.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <returns>True if legacy calculations are in force, false otherwise</returns>
        //internal static bool UseLegacy(BuildingInfo buildingPrefab) => ActivePack(buildingPrefab).name.Equals("legacy");
        //TODO

        /// <summary>
        /// Returns the population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Population</returns>
        internal static int Population(BuildingInfo buildingPrefab, int level) => ActivePack(buildingPrefab).Population(buildingPrefab, level);


        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="prefab">Building prefab</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        internal static PrefabEmployStruct Workplaces(BuildingInfo buildingPrefab, int level) => ActivePack(buildingPrefab).Workplaces(buildingPrefab, level);


        /// <summary>
        /// Returns the currently active calculation data pack record for the given prefab.
        /// </summary>
        /// <param name="building">Selected prefab</param>
        /// <returns>Currently active size pack</returns>
        internal static CalcPack ActivePack(BuildingInfo building)
        {
            // Local reference.
            string buildingName = building.name;

            // Check to see if this building has an entry in the custom settings dictionary.
            if (buildingDict.ContainsKey(buildingName))
            {
                // Custom settings available - use them.
                return buildingDict[buildingName];
            }
            else
            {
                // Use default selection.
                return DefaultPack(building);
            }
        }


        /// <summary>
        /// Calculates the population for the given building using the given LevelData.
        /// </summary>
        /// <param name="buildingInfoGen">Building info record</param>
        /// <param name="levelData">LevelData record to use for calculations</param>
        /// <param name="floorList">Optional precalculated list of calculated floors (to save time; will be generated if not provided)</param>
        /// <param name="totalArea">Optional precalculated total building area  (to save time; will be generated if not provided)</param>
        /// <returns></returns>
        internal static int VolumetricPopulation(BuildingInfoGen buildingInfoGen, LevelData levelData, SortedList<int, float> floorList = null, float totalArea = 0)
        {
            // Return value.
            int totalUnits = 0;


            // See if we're using area calculations for numbers of units, i.e. areaPer is at least one.
            if (levelData.areaPer > 0)
            {
                // Get local references.
                float floorArea = totalArea;
                SortedList<int, float> floors = floorList;

                // Get list of floors and total building area, if one hasn't already been provided.
                if (floors == null || floorArea == 0)
                {
                    floors = VolumetricFloors(buildingInfoGen, levelData, out floorArea);
                }

                // See if we're calculating based on total building floor area, not per floor.
                if (levelData.multiFloorUnits)
                {
                    // Units base on total floor area: calculate number of units in total building (always rounded down).
                    totalUnits = (int)(floorArea / levelData.areaPer);
                }
                else
                {
                    // Calculating per floor.
                    // Iterate through each floor, assigning population as we go.
                    for (int i = 0; i < floors.Count; ++i)
                    {
                        // Number of units on this floor - always rounded down.
                        int floorUnits = (int)(floors[i] / levelData.areaPer);
                        totalUnits += floorUnits;
                    }
                }
            }
            else
            {
                // areaPer is 0 or less; use a fixed number of units.
                totalUnits = -levelData.areaPer;
            }

            // Always have at least one unit, regardless of size.
            if (totalUnits < 1)
            {
                totalUnits = 1;
            }

            return totalUnits;
        }


        /// <summary>
        /// Returns a list of floors and populations for the given building.
        /// </summary>
        /// <param name="buildingInfoGen">Building info record</param>
        /// <param name="levelData">LevelData record to use for calculations</param>
        /// <param name="total">Total area of all floors</param>
        /// <returns>Sorted list of floors (key = floor number, value = floor area)</returns>
        internal static SortedList<int, float> VolumetricFloors(BuildingInfoGen buildingInfoGen, LevelData levelData, out float totalArea)
        {
            // Initialise required fields.
            float floorArea = 0;
            SortedList<int, float> floors = new SortedList<int, float>();

            // Calculate our heighmap grid (16 x 16).
            float gridSizeX = (buildingInfoGen.m_max.x - buildingInfoGen.m_min.x) / 16f;
            float gridSizeY = (buildingInfoGen.m_max.z - buildingInfoGen.m_min.z) / 16f;
            float gridArea = gridSizeX * gridSizeY;

            // Iterate through our heights, adding each area to our floor count.
            float[] heights = buildingInfoGen.m_heights;
            for (int i = 0; i < heights.Length; ++i)
            {
                // Get local reference.
                float thisHeight = heights[i];

                // Check to see if we have at least one floor in this segment.
                if (thisHeight > levelData.firstFloorMin)
                {
                    // Starting number of floors is either 1 or zero, depending on setting of 'ignore first floor' checkbox.
                    int numFloors = levelData.firstFloorEmpty ? 0 : 1;

                    // Calculate any height left over from the maximum first floor height.
                    float surplusHeight = thisHeight - levelData.firstFloorMax;

                    // See if we have more than one floor, i.e. our height is greater than the first floor maximum height.
                    if (surplusHeight > 0)
                    {
                        // Number of floors for this grid segment is the truncated division (rounded down); no partial floors here!
                        numFloors += (int)(surplusHeight / levelData.floorHeight);
                    }

                    // Total incremental floor area is simply the number of floors multipled by the area of this grid segment.
                    floorArea += numFloors * gridArea;

                    // Iterate through each floor in this grid area and add this grid area to each floor.
                    for (int j = 0; j < numFloors; ++j)
                    {
                        // Check to see if we already an entry for this floor in our list.
                        if (floors.ContainsKey(j))
                        {
                            // We already have an entry for this floor; add this segment's area.
                            floors[j] += gridArea;
                        }
                        else
                        {
                            // We don't have an entry for this floor yet: add one with this segment's area as the initial value.
                            floors.Add(j, gridArea);
                        }
                    }
                }
            }

            totalArea = floorArea;
            return floors;
        }


        /// <summary>
        /// Initialises arrays with default values.
        /// </summary>
        internal static void Setup()
        {
            // Initialise list of data packs.
            calcPacks = new List<CalcPack>();

            // Legacy residential.
            LegacyResPack resWG = new LegacyResPack();
            resWG.name = "resWG";
            resWG.displayName = Translations.Translate("RPR_PCK_LEG_NAM");
            resWG.description = Translations.Translate("RPR_PCK_LEG_DES");
            resWG.version = (int)DataVersion.legacy;
            resWG.service = ItemClass.Service.Residential;
            calcPacks.Add(resWG);

            // Legacy industrial.
            LegacyIndPack indWG = new LegacyIndPack();
            indWG.name = "indWG";
            indWG.displayName = Translations.Translate("RPR_PCK_LEG_NAM");
            indWG.description = Translations.Translate("RPR_PCK_LEG_DES");
            indWG.version = (int)DataVersion.legacy;
            indWG.service = ItemClass.Service.Industrial;
            calcPacks.Add(indWG);

            // Legacy commercial.
            LegacyComPack comWG = new LegacyComPack();
            comWG.name = "comWG";
            comWG.displayName = Translations.Translate("RPR_PCK_LEG_NAM");
            comWG.description = Translations.Translate("RPR_PCK_LEG_DES");
            comWG.version = (int)DataVersion.legacy;
            comWG.service = ItemClass.Service.Commercial;
            calcPacks.Add(comWG);

            // Legacy office.
            LegacyOffPack offWG = new LegacyOffPack();
            offWG.name = "offWG";
            offWG.displayName = Translations.Translate("RPR_PCK_LEG_NAM");
            offWG.description = Translations.Translate("RPR_PCK_LEG_DES");
            offWG.version = (int)DataVersion.legacy;
            offWG.service = ItemClass.Service.Office;
            calcPacks.Add(offWG);


            // Low-density residential.
            VolumetricPack resLow = new VolumetricPack();
            resLow.name = "reslow";
            resLow.displayName = Translations.Translate("RPR_PCK_RLS_NAM");
            resLow.description = Translations.Translate("RPR_PCK_RLS_DES");
            resLow.version = (int)DataVersion.one;
            resLow.service = ItemClass.Service.Residential;
            resLow.subServices = new ItemClass.SubService[] { ItemClass.SubService.ResidentialLow, ItemClass.SubService.ResidentialLowEco };

            resLow.levels = new LevelData[5];
            resLow.levels[0] = new LevelData { floorHeight = 3f, areaPer = -1, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = false, multiFloorUnits = true };
            resLow.levels[1] = new LevelData { floorHeight = 3f, areaPer = -1, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = false, multiFloorUnits = true };
            resLow.levels[2] = new LevelData { floorHeight = 3f, areaPer = -1, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = false, multiFloorUnits = true };
            resLow.levels[3] = new LevelData { floorHeight = 3f, areaPer = -1, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = false, multiFloorUnits = true };
            resLow.levels[4] = new LevelData { floorHeight = 3f, areaPer = -1, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = false, multiFloorUnits = true };

            calcPacks.Add(resLow);

            // Medium-density residential.
            VolumetricPack resMed = new VolumetricPack();
            resMed.name = "resmed";
            resMed.displayName = Translations.Translate("RPR_PCK_RMD_NAM");
            resMed.description = Translations.Translate("RPR_PCK_RMD_DES");
            resMed.version = (int)DataVersion.one;
            resMed.service = ItemClass.Service.Residential;
            resMed.subServices = new ItemClass.SubService[] { ItemClass.SubService.ResidentialLow, ItemClass.SubService.ResidentialLowEco, ItemClass.SubService.ResidentialHigh, ItemClass.SubService.ResidentialHighEco };

            resMed.levels = new LevelData[5];
            resMed.levels[0] = new LevelData { floorHeight = 3f, areaPer = 140, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = false, multiFloorUnits = false };
            resMed.levels[1] = new LevelData { floorHeight = 3f, areaPer = 145, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = false, multiFloorUnits = false };
            resMed.levels[2] = new LevelData { floorHeight = 3f, areaPer = 150, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = false, multiFloorUnits = false };
            resMed.levels[3] = new LevelData { floorHeight = 3f, areaPer = 160, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = false, multiFloorUnits = false };
            resMed.levels[4] = new LevelData { floorHeight = 3f, areaPer = 170, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = false, multiFloorUnits = false };

            calcPacks.Add(resMed);

            // High-density residential.
            VolumetricPack resHigh = new VolumetricPack();
            resHigh.name = "reshigh";
            resHigh.displayName = Translations.Translate("RPR_PCK_RHR_NAM");
            resHigh.description = Translations.Translate("RPR_PCK_RHR_DES");
            resHigh.version = (int)DataVersion.one;
            resHigh.service = ItemClass.Service.Residential;
            resHigh.subServices = new ItemClass.SubService[] { ItemClass.SubService.ResidentialHigh, ItemClass.SubService.ResidentialHighEco };

            resHigh.levels = new LevelData[5];
            resHigh.levels[0] = new LevelData { floorHeight = 3f, areaPer = 140, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = true, multiFloorUnits = false };
            resHigh.levels[1] = new LevelData { floorHeight = 3f, areaPer = 145, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = true, multiFloorUnits = false };
            resHigh.levels[2] = new LevelData { floorHeight = 3f, areaPer = 150, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = true, multiFloorUnits = false };
            resHigh.levels[3] = new LevelData { floorHeight = 3f, areaPer = 160, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = true, multiFloorUnits = false };
            resHigh.levels[4] = new LevelData { floorHeight = 3f, areaPer = 170, firstFloorMin = 3f, firstFloorMax = 3f, firstFloorEmpty = true, multiFloorUnits = false };

            calcPacks.Add(resHigh);

            // Low-density commercial.
            // Figures are from Montgomery County round 7.0.
            VolumetricPack comLow = new VolumetricPack();
            comLow.name = "comlow";
            comLow.displayName = Translations.Translate("RPR_PCK_CLS_NAM");
            comLow.description = Translations.Translate("RPR_PCK_CLS_DES");
            comLow.version = (int)DataVersion.one;
            comLow.service = ItemClass.Service.Commercial;
            comLow.subServices = new ItemClass.SubService[] { ItemClass.SubService.CommercialLow, ItemClass.SubService.CommercialEco, ItemClass.SubService.CommercialLeisure, ItemClass.SubService.CommercialTourist };

            comLow.levels = new LevelData[3];
            comLow.levels[0] = new LevelData { floorHeight = 3f, areaPer = 40, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = false, multiFloorUnits = true };
            comLow.levels[1] = new LevelData { floorHeight = 3f, areaPer = 40, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = false, multiFloorUnits = true };
            comLow.levels[2] = new LevelData { floorHeight = 3f, areaPer = 40, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = false, multiFloorUnits = true };

            calcPacks.Add(comLow);

            // High-density commercial.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            // 20% gross-up for conversion from NIA to GIA.
            VolumetricPack comHigh = new VolumetricPack();
            comHigh.name = "comhigh";
            comHigh.displayName = Translations.Translate("RPR_PCK_CHC_NAM");
            comHigh.description = Translations.Translate("RPR_PCK_CHC_DES");
            comHigh.version = (int)DataVersion.one;
            comHigh.service = ItemClass.Service.Commercial;
            comHigh.subServices = new ItemClass.SubService[] { ItemClass.SubService.CommercialHigh, ItemClass.SubService.CommercialLeisure, ItemClass.SubService.CommercialTourist };

            comHigh.levels = new LevelData[3];
            comHigh.levels[0] = new LevelData { floorHeight = 4f, areaPer = 23, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = false, multiFloorUnits = false };
            comHigh.levels[1] = new LevelData { floorHeight = 4f, areaPer = 23, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = false, multiFloorUnits = false };
            comHigh.levels[2] = new LevelData { floorHeight = 4f, areaPer = 23, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = false, multiFloorUnits = false };

            calcPacks.Add(comHigh);

            // Retail warehouses.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            // 20% gross-up for conversion from NIA to GIA.
            VolumetricPack retailWarehouse = new VolumetricPack();
            retailWarehouse.name = "retailware";
            retailWarehouse.displayName = Translations.Translate("RPR_PCK_CLW_NAM");
            retailWarehouse.description = Translations.Translate("RPR_PCK_CLW_DES");
            retailWarehouse.version = (int)DataVersion.one;
            retailWarehouse.service = ItemClass.Service.Commercial;
            retailWarehouse.subServices = new ItemClass.SubService[] { ItemClass.SubService.CommercialLow, ItemClass.SubService.CommercialHigh };

            retailWarehouse.levels = new LevelData[3];
            retailWarehouse.levels[0] = new LevelData { floorHeight = 12f, areaPer = 108, firstFloorMin = 3f, firstFloorMax = 12f, firstFloorEmpty = false, multiFloorUnits = false };
            retailWarehouse.levels[1] = new LevelData { floorHeight = 12f, areaPer = 108, firstFloorMin = 3f, firstFloorMax = 12f, firstFloorEmpty = false, multiFloorUnits = false };
            retailWarehouse.levels[2] = new LevelData { floorHeight = 12f, areaPer = 108, firstFloorMin = 3f, firstFloorMax = 12f, firstFloorEmpty = false, multiFloorUnits = false };

            calcPacks.Add(retailWarehouse);

            // Hotels.
            // Figures are from Montgomery County round 7.0.
            VolumetricPack hotel = new VolumetricPack();
            hotel.name = "hotel";
            hotel.displayName = Translations.Translate("RPR_PCK_THT_NAM");
            hotel.description = Translations.Translate("RPR_PCK_THT_DES");
            hotel.version = (int)DataVersion.one;
            hotel.service = ItemClass.Service.Commercial;
            hotel.subServices = new ItemClass.SubService[] { ItemClass.SubService.CommercialLow, ItemClass.SubService.CommercialTourist };

            hotel.levels = new LevelData[3];
            hotel.levels[0] = new LevelData { floorHeight = 4f, areaPer = 130, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = false, multiFloorUnits = true };
            hotel.levels[1] = new LevelData { floorHeight = 4f, areaPer = 130, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = false, multiFloorUnits = true };
            hotel.levels[2] = new LevelData { floorHeight = 4f, areaPer = 130, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = false, multiFloorUnits = true };

            calcPacks.Add(hotel);

            // Restaurants and cafes.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            // 20% gross-up for conversion from NIA to GIA.
            VolumetricPack restaurant = new VolumetricPack();
            restaurant.name = "restaurant";
            restaurant.displayName = Translations.Translate("RPR_PCK_LFD_NAM");
            restaurant.description = Translations.Translate("RPR_PCK_LFD_DES");
            restaurant.version = (int)DataVersion.one;
            restaurant.service = ItemClass.Service.Commercial;
            restaurant.subServices = new ItemClass.SubService[] { ItemClass.SubService.CommercialLow, ItemClass.SubService.CommercialLeisure };

            restaurant.levels = new LevelData[3];
            restaurant.levels[0] = new LevelData { floorHeight = 4f, areaPer = 22, firstFloorMin = 3f, firstFloorMax = 4f, firstFloorEmpty = false, multiFloorUnits = true };
            restaurant.levels[1] = new LevelData { floorHeight = 4f, areaPer = 22, firstFloorMin = 3f, firstFloorMax = 4f, firstFloorEmpty = false, multiFloorUnits = true };
            restaurant.levels[2] = new LevelData { floorHeight = 4f, areaPer = 22, firstFloorMin = 3f, firstFloorMax = 4f, firstFloorEmpty = false, multiFloorUnits = true };

            calcPacks.Add(restaurant);

            // Entertainment centres.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            // 20% gross-up for conversion from NIA to GIA.
            VolumetricPack entertainment = new VolumetricPack();
            entertainment.name = "entertainment";
            entertainment.displayName = Translations.Translate("RPR_PCK_LEN_NAM");
            entertainment.description = Translations.Translate("RPR_PCK_LEN_DES"); ;
            entertainment.version = (int)DataVersion.one;
            entertainment.service = ItemClass.Service.Commercial;
            entertainment.subServices = new ItemClass.SubService[] { ItemClass.SubService.CommercialLow, ItemClass.SubService.CommercialLeisure };

            entertainment.levels = new LevelData[3];
            entertainment.levels[0] = new LevelData { floorHeight = 6f, areaPer = 108, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = false, multiFloorUnits = true };
            entertainment.levels[1] = new LevelData { floorHeight = 6f, areaPer = 108, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = false, multiFloorUnits = true };
            entertainment.levels[2] = new LevelData { floorHeight = 6f, areaPer = 108, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = false, multiFloorUnits = true };

            calcPacks.Add(entertainment);

            // Light industry.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            VolumetricPack lightIndustry = new VolumetricPack();
            lightIndustry.name = "lightind";
            lightIndustry.displayName = Translations.Translate("RPR_PCK_ILG_NAM");
            lightIndustry.description = Translations.Translate("RPR_PCK_ILG_DES");
            lightIndustry.version = (int)DataVersion.one;
            lightIndustry.service = ItemClass.Service.Industrial;
            lightIndustry.subServices = new ItemClass.SubService[] { ItemClass.SubService.IndustrialGeneric, ItemClass.SubService.IndustrialFarming, ItemClass.SubService.IndustrialForestry, ItemClass.SubService.IndustrialOil, ItemClass.SubService.IndustrialOre };

            lightIndustry.levels = new LevelData[3];
            lightIndustry.levels[0] = new LevelData { floorHeight = 4f, areaPer = 47, firstFloorMin = 3f, firstFloorMax = 9f, firstFloorEmpty = false, multiFloorUnits = false };
            lightIndustry.levels[1] = new LevelData { floorHeight = 4f, areaPer = 47, firstFloorMin = 3f, firstFloorMax = 9f, firstFloorEmpty = false, multiFloorUnits = false };
            lightIndustry.levels[2] = new LevelData { floorHeight = 4f, areaPer = 47, firstFloorMin = 3f, firstFloorMax = 9f, firstFloorEmpty = false, multiFloorUnits = false };

            calcPacks.Add(lightIndustry);

            // Industry factory.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            VolumetricPack indFactory = new VolumetricPack();
            indFactory.name = "factory";
            indFactory.displayName = Translations.Translate("RPR_PCK_IFC_NAM");
            indFactory.description = Translations.Translate("RPR_PCK_IFC_DES");
            indFactory.version = (int)DataVersion.one;
            indFactory.service = ItemClass.Service.Industrial;
            indFactory.subServices = new ItemClass.SubService[] { ItemClass.SubService.IndustrialGeneric, ItemClass.SubService.IndustrialFarming, ItemClass.SubService.IndustrialForestry, ItemClass.SubService.IndustrialOil, ItemClass.SubService.IndustrialOre };

            indFactory.levels = new LevelData[3];
            indFactory.levels[0] = new LevelData { floorHeight = 4f, areaPer = 36, firstFloorMin = 3f, firstFloorMax = 9f, firstFloorEmpty = false, multiFloorUnits = false };
            indFactory.levels[1] = new LevelData { floorHeight = 4f, areaPer = 36, firstFloorMin = 3f, firstFloorMax = 9f, firstFloorEmpty = false, multiFloorUnits = false };
            indFactory.levels[2] = new LevelData { floorHeight = 4f, areaPer = 36, firstFloorMin = 3f, firstFloorMax = 9f, firstFloorEmpty = false, multiFloorUnits = false };

            calcPacks.Add(indFactory);

            // Industry warehouse.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            VolumetricPack indWarehouse = new VolumetricPack();
            indWarehouse.name = "warehouse";
            indWarehouse.displayName = Translations.Translate("RPR_PCK_IGW_NAM");
            indWarehouse.description = Translations.Translate("RPR_PCK_IGW_DES");
            indWarehouse.version = (int)DataVersion.one;
            indWarehouse.service = ItemClass.Service.Industrial;
            indWarehouse.subServices = new ItemClass.SubService[] { ItemClass.SubService.IndustrialGeneric, ItemClass.SubService.IndustrialFarming, ItemClass.SubService.IndustrialForestry, ItemClass.SubService.IndustrialOil, ItemClass.SubService.IndustrialOre };

            indWarehouse.levels = new LevelData[3];
            indWarehouse.levels[0] = new LevelData { floorHeight = 9f, areaPer = 70, firstFloorMin = 3f, firstFloorMax = 9f, firstFloorEmpty = false, multiFloorUnits = false };
            indWarehouse.levels[1] = new LevelData { floorHeight = 9f, areaPer = 70, firstFloorMin = 3f, firstFloorMax = 9f, firstFloorEmpty = false, multiFloorUnits = false };
            indWarehouse.levels[2] = new LevelData { floorHeight = 9f, areaPer = 70, firstFloorMin = 3f, firstFloorMax = 9f, firstFloorEmpty = false, multiFloorUnits = false };

            calcPacks.Add(indWarehouse);

            // Industry high-bay warehouse.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            VolumetricPack highBay = new VolumetricPack();
            highBay.name = "highbay";
            highBay.displayName = Translations.Translate("RPR_PCK_IHB_NAM");
            highBay.description = Translations.Translate("RPR_PCK_IHB_DES");
            highBay.version = (int)DataVersion.one;
            highBay.service = ItemClass.Service.Industrial;
            highBay.subServices = new ItemClass.SubService[] { ItemClass.SubService.IndustrialGeneric, ItemClass.SubService.IndustrialFarming, ItemClass.SubService.IndustrialForestry, ItemClass.SubService.IndustrialOil, ItemClass.SubService.IndustrialOre };

            highBay.levels = new LevelData[3];
            highBay.levels[0] = new LevelData { floorHeight = 12f, areaPer = 80, firstFloorMin = 3f, firstFloorMax = 12f, firstFloorEmpty = false, multiFloorUnits = false };
            highBay.levels[1] = new LevelData { floorHeight = 12f, areaPer = 80, firstFloorMin = 3f, firstFloorMax = 12f, firstFloorEmpty = false, multiFloorUnits = false };
            highBay.levels[2] = new LevelData { floorHeight = 12f, areaPer = 80, firstFloorMin = 3f, firstFloorMax = 12f, firstFloorEmpty = false, multiFloorUnits = false };

            calcPacks.Add(highBay);

            // Low-density office.
            VolumetricPack offLow = new VolumetricPack();
            offLow.name = "offlow";
            offLow.displayName = Translations.Translate("RPR_PCK_OLD_NAM");
            offLow.description = Translations.Translate("RPR_PCK_OLD_DES");
            offLow.version = (int)DataVersion.one;
            offLow.service = ItemClass.Service.Office;
            offLow.subServices = new ItemClass.SubService[] { ItemClass.SubService.OfficeGeneric, ItemClass.SubService.OfficeHightech };

            offLow.levels = new LevelData[3];
            offLow.levels[0] = new LevelData { floorHeight = 4f, areaPer = 34, firstFloorMin = 3f, firstFloorMax = 4f, firstFloorEmpty = false, multiFloorUnits = false };
            offLow.levels[1] = new LevelData { floorHeight = 4f, areaPer = 36, firstFloorMin = 3f, firstFloorMax = 4f, firstFloorEmpty = false, multiFloorUnits = false };
            offLow.levels[2] = new LevelData { floorHeight = 4f, areaPer = 38, firstFloorMin = 3f, firstFloorMax = 4f, firstFloorEmpty = false, multiFloorUnits = false };

            calcPacks.Add(offLow);

            // High-density office.
            // Figures are from Montgomery County round 7.0.
            VolumetricPack offHigh = new VolumetricPack();
            offHigh.name = "offhigh";
            offHigh.displayName = Translations.Translate("RPR_PCK_OHD_NAM");
            offHigh.description = Translations.Translate("RPR_PCK_OHD_DES");
            offHigh.version = (int)DataVersion.one;
            offHigh.service = ItemClass.Service.Office;
            offHigh.subServices = new ItemClass.SubService[] { ItemClass.SubService.OfficeGeneric, ItemClass.SubService.OfficeHightech };

            offHigh.levels = new LevelData[3];
            offHigh.levels[0] = new LevelData { floorHeight = 4f, areaPer = 25, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = true, multiFloorUnits = false };
            offHigh.levels[1] = new LevelData { floorHeight = 4f, areaPer = 25, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = true, multiFloorUnits = false };
            offHigh.levels[2] = new LevelData { floorHeight = 4f, areaPer = 25, firstFloorMin = 3f, firstFloorMax = 6f, firstFloorEmpty = true, multiFloorUnits = false };

            calcPacks.Add(offHigh);

            // Initialise building setting dictionary.
            buildingDict = new Dictionary<string, CalcPack>();
        }


        /// <summary>
        /// Updates our building setting dictionary for the selected building prefab to the indicated calculation pack.
        /// </summary>
        /// <param name="building">Building prefab to update</param>
        /// <param name="pack">New data pack to apply</param>
        internal static void UpdateBuildingPack(BuildingInfo building, CalcPack pack)
        {
            // Local reference.
            string buildingName = building.name;

            // Check to see if this building already has an entry.
            if (buildingDict.ContainsKey(buildingName))
            {
                // Contains an entry - update it.
                buildingDict[buildingName] = pack;
            }
            else
            {
                // No entry yet - add one.
                buildingDict.Add(buildingName, pack);
            }
        }


        /// <summary>
        /// Returns the default calculation pack for the given prefab.
        /// </summary>
        /// <param name="building">Building prefab</param>
        /// <returns>Default calculation data pack</returns>
        internal static CalcPack DefaultPack(BuildingInfo building)
        {
            // Get service and subservice.
            ItemClass.Service service = building.GetService();
            ItemClass.SubService subService = building.GetSubService();

            string defaultName;

            // Manual breakdown.
            switch (service)
            {
                case ItemClass.Service.Residential:
                    switch (subService)
                    {
                        case ItemClass.SubService.ResidentialHigh:
                            defaultName = "reshigh";
                            break;
                        case ItemClass.SubService.ResidentialHighEco:
                            defaultName = "reshigh";
                            break;
                        default:
                            defaultName = "reslow";
                            break;
                    }
                    break;
                case ItemClass.Service.Industrial:
                    defaultName = "factory";
                    break;
                case ItemClass.Service.Office:
                    defaultName = "offlow";
                    break;
                default:
                    // Default is commercial.
                    switch (subService)
                    {
                        case ItemClass.SubService.CommercialHigh:
                            defaultName = "comhigh";
                            break;
                        case ItemClass.SubService.CommercialTourist:
                            defaultName = "hotel";
                            break;
                        case ItemClass.SubService.CommercialLeisure:
                            defaultName = "restaurant";
                            break;
                        default:
                            // Default is low-density commercial.
                            defaultName = "comlow";
                            break;
                    }
                    break;
            }

            // Match name to floorpack.
            return calcPacks.Find(pack => pack.name.Equals(defaultName));
        }


        /// <summary>
        /// Returns a list of calculation packs available for the given prefab.
        /// </summary>
        /// <param name="prefab">BuildingInfo prefab</param>
        /// <returns>Array of available calculation packs</returns>
        internal static CalcPack[] GetPacks(BuildingInfo prefab)
        {
            // Return list.
            List<CalcPack> list = new List<CalcPack>();

            // Get service, subservice, and default name.
            ItemClass.Service service = prefab.GetService();
            ItemClass.SubService subService = prefab.GetSubService();

            // TODO
            // Add legacy selection.
            /*list.Add(new LegacyPack {
                name = "legacy",
                version = (int)DataVersion.legacy,
                displayName = "Old calculations",
                description = "Use older calculation style (as used by versions 1.x of this mod and Whitefang Greytail's original Realistic Population and Consumption mod"
            });*/

            // Iterate through each floor pack and see if it applies.
            foreach (CalcPack pack in calcPacks)
            {
                // Check for matching service.
                if (pack.service == service)
                {
                    // Service matches; 
                    {
                        // If no subservices are listed, then this applies to all subservice types for this service.
                        if (pack.subServices == null)
                        {
                            list.Add(pack);
                        }
                        // Otherwise, iterate through subservices to see if there's a match.
                        else foreach (ItemClass.SubService packSubService in pack.subServices)
                        {
                            if (packSubService == subService)
                            {
                                // Got a match; add this pack to our return list.
                                list.Add(pack);

                                // Already matched this pack; no point in continuing.
                                break;
                            }
                        }
                    }
                }
            }

            return list.ToArray();
        }
    }
}
 