using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Math;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Centralised store and management of population calculation data.
    /// </summary>
    internal static class PopData
    {
        // Initialised status flag.
        internal static bool ready = false;

        // List of data definition packs.
        internal static List<CalcPack> calcPacks;

        // List of building settings.
        internal static Dictionary<string, CalcPack> buildingDict;

        // List of (sub)service default settings.
        internal static Dictionary<ItemClass.Service, Dictionary<ItemClass.SubService, CalcPack>> serviceDict;


        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="prefab">Building prefab</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        internal static PrefabEmployStruct Workplaces(BuildingInfo buildingPrefab, int level) => ActivePack(buildingPrefab).Workplaces(buildingPrefab, level);


        /// <summary>
        /// Returns the population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Population</returns>
        internal static int Population(BuildingInfo buildingPrefab, int level)
        {
            // First, check for population override.
            int population;

            // Residential?
            if (buildingPrefab.GetService() == ItemClass.Service.Residential)
            {
                // Residential - see if we have a household override for this prefab.
                if (DataStore.householdCache.TryGetValue(buildingPrefab.name, out population))
                {
                    // Yes - return override.
                    return population;
                }
            }
            // Not residential - see if we have a workplace override for this prefab.
            else if (DataStore.workerCache.TryGetValue(buildingPrefab.name, out population))
            {
                // Yes - return override.
                return population;
            }

            // If we got here, there's no override; return pack default.
            return ActivePack(buildingPrefab).Population(buildingPrefab, level);
        }


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
                return CurrentDefaultPack(building);
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
                float emptyArea = levelData.emptyArea;
                SortedList<int, float> floors = floorList;

                // Get list of floors and total building area, if one hasn't already been provided.
                if (floors == null || floorArea == 0)
                {
                    floors = VolumetricFloors(buildingInfoGen, levelData, out floorArea);
                }

                // Determine area percentage to use for calculations (inverse of empty area percentage).
                float areaPercent = 1 - (levelData.emptyPercent / 100f);

                // See if we're calculating based on total building floor area, not per floor.
                if (levelData.multiFloorUnits)
                {
                    // Units base on total floor area: calculate number of units in total building (always rounded down), after subtracting exmpty space.
                    totalUnits = (int)(((floorArea - emptyArea) * areaPercent) / levelData.areaPer);
                }
                else
                {
                    // Calculating per floor.
                    // Iterate through each floor, assigning population as we go.
                    for (int i = 0; i < floors.Count; ++i)
                    {
                        // Subtract any unallocated empty space.
                        if (emptyArea > 0)
                        {
                            // Get the space to be allocated against this floor - minimum of remaining (unallocated) empty space and this floor size.
                            float emptyAllocation = UnityEngine.Mathf.Min(emptyArea, floors[i]);

                            // Subtract empty space to be allocated from both this floor area and our unallocated empty space (because it's now allocated).
                            floors[i] -= emptyAllocation;
                            emptyArea -= emptyAllocation;
                        }

                        // Number of units on this floor - always rounded down.
                        int floorUnits = (int)((floors[i] * areaPercent) / levelData.areaPer);
                        totalUnits += floorUnits;
                    }
                }
            }
            else
            {
                // areaPer is 0 or less; use a fixed number of units.
                totalUnits = (int)-levelData.areaPer;
            }

            // Always have at least one unit, regardless of size.
            if (totalUnits < 1)
            {
                totalUnits = 1;
            }

            return totalUnits;
        }



        /// <summary>
        /// Adds/replaces service dictionary entry for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="subService">Sub-service</param>
        /// <param name="calcPack">New default calculation pack to apply</param>
        internal static void AddService(ItemClass.Service service, ItemClass.SubService subService, CalcPack calcPack)
        {
            // Check for existing key in our services dictionary for this service.
            if (!serviceDict.ContainsKey(service))
            {
                // No existing entry - add one.
                serviceDict.Add(service, new Dictionary<ItemClass.SubService, CalcPack>());
            }

            // Check for existing sub-service key.
            if (PopData.serviceDict[service].ContainsKey(subService))
            {
                // Existing key found - update entry.
                PopData.serviceDict[service][subService] = calcPack;
            }
            else
            {
                // No existing key found - add entry.
                PopData.serviceDict[service].Add(subService, calcPack);
            }
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

                    // Calculate any height left over from the maximum (minimum plus extra) first floor height.
                    float surplusHeight = thisHeight - levelData.firstFloorMin - levelData.firstFloorExtra;

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
            resLow.levels[0] = new LevelData { floorHeight = 3f, emptyArea = 0f,  areaPer = -1f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = false, multiFloorUnits = true };
            resLow.levels[1] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = -1f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = false, multiFloorUnits = true };
            resLow.levels[2] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = -1f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = false, multiFloorUnits = true };
            resLow.levels[3] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = -1f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = false, multiFloorUnits = true };
            resLow.levels[4] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = -1f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = false, multiFloorUnits = true };
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
            resMed.levels[0] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = 140f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = false, multiFloorUnits = false };
            resMed.levels[1] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = 145f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = false, multiFloorUnits = false };
            resMed.levels[2] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = 150f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = false, multiFloorUnits = false };
            resMed.levels[3] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = 160f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = false, multiFloorUnits = false };
            resMed.levels[4] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = 170f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = false, multiFloorUnits = false };
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
            resHigh.levels[0] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = 140f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = true, multiFloorUnits = false };
            resHigh.levels[1] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = 145f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = true, multiFloorUnits = false };
            resHigh.levels[2] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = 150f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = true, multiFloorUnits = false };
            resHigh.levels[3] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = 160f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = true, multiFloorUnits = false };
            resHigh.levels[4] = new LevelData { floorHeight = 3f, emptyArea = 0f, areaPer = 170f, firstFloorMin = 3f, firstFloorExtra = 0f, firstFloorEmpty = true, multiFloorUnits = false };
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
            comLow.levels[0] = new LevelData { floorHeight = 3f, emptyArea = 0, areaPer = 40, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = false, multiFloorUnits = true };
            comLow.levels[1] = new LevelData { floorHeight = 3f, emptyArea = 0, areaPer = 40, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = false, multiFloorUnits = true };
            comLow.levels[2] = new LevelData { floorHeight = 3f, emptyArea = 0, areaPer = 40, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = false, multiFloorUnits = true };
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
            comHigh.levels[0] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 23f, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = false, multiFloorUnits = false };
            comHigh.levels[1] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 23f, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = false, multiFloorUnits = false };
            comHigh.levels[2] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 23f, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = false, multiFloorUnits = false };
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
            retailWarehouse.levels[0] = new LevelData { floorHeight = 12f, emptyArea = 0f, areaPer = 108f, firstFloorMin = 3f, firstFloorExtra = 9f, firstFloorEmpty = false, multiFloorUnits = false };
            retailWarehouse.levels[1] = new LevelData { floorHeight = 12f, emptyArea = 0f, areaPer = 108f, firstFloorMin = 3f, firstFloorExtra = 9f, firstFloorEmpty = false, multiFloorUnits = false };
            retailWarehouse.levels[2] = new LevelData { floorHeight = 12f, emptyArea = 0f, areaPer = 108f, firstFloorMin = 3f, firstFloorExtra = 9f, firstFloorEmpty = false, multiFloorUnits = false };
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
            hotel.levels[0] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 130f, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = false, multiFloorUnits = true };
            hotel.levels[1] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 130f, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = false, multiFloorUnits = true };
            hotel.levels[2] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 130f, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = false, multiFloorUnits = true };
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
            restaurant.levels[0] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 22f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = true };
            restaurant.levels[1] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 22f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = true };
            restaurant.levels[2] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 22f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = true };
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
            entertainment.levels[0] = new LevelData { floorHeight = 6f, emptyArea = 0f, areaPer = 108f, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = false, multiFloorUnits = true };
            entertainment.levels[1] = new LevelData { floorHeight = 6f, emptyArea = 0f, areaPer = 108f, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = false, multiFloorUnits = true };
            entertainment.levels[2] = new LevelData { floorHeight = 6f, emptyArea = 0f, areaPer = 108f, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = false, multiFloorUnits = true };
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
            lightIndustry.levels[0] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 47f, firstFloorMin = 3f, firstFloorExtra = 6f, firstFloorEmpty = false, multiFloorUnits = false };
            lightIndustry.levels[1] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 47f, firstFloorMin = 3f, firstFloorExtra = 6f, firstFloorEmpty = false, multiFloorUnits = false };
            lightIndustry.levels[2] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 47f, firstFloorMin = 3f, firstFloorExtra = 6f, firstFloorEmpty = false, multiFloorUnits = false };
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
            indFactory.levels[0] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 36f, firstFloorMin = 3f, firstFloorExtra = 6f, firstFloorEmpty = false, multiFloorUnits = false };
            indFactory.levels[1] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 36f, firstFloorMin = 3f, firstFloorExtra = 6f, firstFloorEmpty = false, multiFloorUnits = false };
            indFactory.levels[2] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 36f, firstFloorMin = 3f, firstFloorExtra = 6f, firstFloorEmpty = false, multiFloorUnits = false };
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
            indWarehouse.levels[0] = new LevelData { floorHeight = 9f, emptyArea = 0f, areaPer = 70f, firstFloorMin = 3f, firstFloorExtra = 6f, firstFloorEmpty = false, multiFloorUnits = false };
            indWarehouse.levels[1] = new LevelData { floorHeight = 9f, emptyArea = 0f, areaPer = 70f, firstFloorMin = 3f, firstFloorExtra = 6f, firstFloorEmpty = false, multiFloorUnits = false };
            indWarehouse.levels[2] = new LevelData { floorHeight = 9f, emptyArea = 0f, areaPer = 70f, firstFloorMin = 3f, firstFloorExtra = 6f, firstFloorEmpty = false, multiFloorUnits = false };
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
            highBay.levels[0] = new LevelData { floorHeight = 12f, emptyArea = 0f, areaPer = 80f, firstFloorMin = 3f, firstFloorExtra = 9f, firstFloorEmpty = false, multiFloorUnits = false };
            highBay.levels[1] = new LevelData { floorHeight = 12f, emptyArea = 0f, areaPer = 80f, firstFloorMin = 3f, firstFloorExtra = 9f, firstFloorEmpty = false, multiFloorUnits = false };
            highBay.levels[2] = new LevelData { floorHeight = 12f, emptyArea = 0f, areaPer = 80f, firstFloorMin = 3f, firstFloorExtra = 9f, firstFloorEmpty = false, multiFloorUnits = false };
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
            offLow.levels[0] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 34f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = false };
            offLow.levels[1] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 36f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = false };
            offLow.levels[2] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 38f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = false };
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
            offHigh.levels[0] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 25f, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = true, multiFloorUnits = false };
            offHigh.levels[1] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 25f, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = true, multiFloorUnits = false };
            offHigh.levels[2] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 25f, firstFloorMin = 3f, firstFloorExtra = 3f, firstFloorEmpty = true, multiFloorUnits = false };
            calcPacks.Add(offHigh);

            // Suburban schools.
            // Level 1 is elementary, level 2 is high school.
            // Figures are from NSW Department of Education 2013 targets.
            VolumetricPack suburbanSchool = new VolumetricPack();
            suburbanSchool.name = "schoolsub";
            suburbanSchool.displayName = Translations.Translate("RPR_PCK_SSB_NAM");
            suburbanSchool.description = Translations.Translate("RPR_PCK_SSB_DES");
            suburbanSchool.version = (int)DataVersion.one;
            suburbanSchool.service = ItemClass.Service.Education;
            suburbanSchool.subServices = null;
            suburbanSchool.levels = new LevelData[2];
            suburbanSchool.levels[0] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 8f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = false };
            suburbanSchool.levels[1] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 8f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = false };
            calcPacks.Add(suburbanSchool);

            // Suburban schools.
            // Figures are from MN Department of Education Guide for Planning School Construction Projects (lowest density).
            VolumetricPack mnLow = new VolumetricPack();
            mnLow.name = "schoolmnlow";
            mnLow.displayName = Translations.Translate("RPR_PCK_SML_NAM");
            mnLow.description = Translations.Translate("RPR_PCK_SML_DES");
            mnLow.version = (int)DataVersion.one;
            mnLow.service = ItemClass.Service.Education;
            mnLow.subServices = null;
            mnLow.levels = new LevelData[2];
            mnLow.levels[0] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 14f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = false };
            mnLow.levels[1] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 30f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = false };
            calcPacks.Add(mnLow);

            // Suburban schools.
            // Figures are from MN Department of Education Guide for Planning School Construction Projects (middle density).
            VolumetricPack mnMed = new VolumetricPack();
            mnMed.name = "schoolmnmed";
            mnMed.displayName = Translations.Translate("RPR_PCK_SMM_NAM");
            mnMed.description = Translations.Translate("RPR_PCK_SMM_DES");
            mnMed.version = (int)DataVersion.one;
            mnMed.service = ItemClass.Service.Education;
            mnMed.subServices = null;
            mnMed.levels = new LevelData[2];
            mnMed.levels[0] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 12f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = false };
            mnMed.levels[1] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 23f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = false };
            calcPacks.Add(mnMed);

            // Suburban schools.
            // Figures are from MN Department of Education Guide for Planning School Construction Projects (highest density).
            VolumetricPack mnHigh = new VolumetricPack();
            mnHigh.name = "schoolmnhigh";
            mnHigh.displayName = Translations.Translate("RPR_PCK_SMH_NAM");
            mnHigh.description = Translations.Translate("RPR_PCK_SMH_DES");
            mnHigh.version = (int)DataVersion.one;
            mnHigh.service = ItemClass.Service.Education;
            mnHigh.subServices = null;
            mnHigh.levels = new LevelData[2];
            mnHigh.levels[0] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 9f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = false };
            mnHigh.levels[1] = new LevelData { floorHeight = 4f, emptyArea = 0f, areaPer = 14f, firstFloorMin = 3f, firstFloorExtra = 1f, firstFloorEmpty = false, multiFloorUnits = false };
            calcPacks.Add(mnHigh);

            // UK schools.
            // Figures are from Planning Statement - Warwickshire County Council.
            VolumetricPack ukHigh = new VolumetricPack();
            ukHigh.name = "schoolukhigh";
            ukHigh.displayName = Translations.Translate("RPR_PCK_SUK_NAM");
            ukHigh.description = Translations.Translate("RPR_PCK_SUK_DES");
            ukHigh.version = (int)DataVersion.one;
            ukHigh.service = ItemClass.Service.Education;
            ukHigh.subServices = null;
            ukHigh.levels = new LevelData[2];
            ukHigh.levels[0] = new LevelData { floorHeight = 3.5f, emptyArea = 350f, areaPer = 4.1f, firstFloorMin = 3f, firstFloorExtra = 0.5f, firstFloorEmpty = false, multiFloorUnits = true };
            ukHigh.levels[1] = new LevelData { floorHeight = 3.5f, emptyArea = 1400f, areaPer = 6.3f, firstFloorMin = 3f, firstFloorExtra = 0.5f, firstFloorEmpty = false, multiFloorUnits = true };
            calcPacks.Add(ukHigh);

            // Initialise building and service dictionaries.
            serviceDict = new Dictionary<ItemClass.Service, Dictionary<ItemClass.SubService, CalcPack>>();
            buildingDict = new Dictionary<string, CalcPack>();

            // Set status flag.
            ready = true;
        }


        /// <summary>
        /// Adds or updates a calculation pack entry to our list.
        /// </summary>
        /// <param name="calcPack">Calculation pack to add</param>
        internal static void AddCalculationPack(CalcPack calcPack)
        {
            // Iterate through the list of packs, looking for a name match.
            for (int i = 0; i < calcPacks.Count; ++i)
            {
                if (calcPacks[i].name.Equals(calcPack.name))
                {
                    // Found a match - replace with our new entry and return.
                    calcPacks[i] = calcPack;
                    return;
                }
            }

            // If we got here, we didn't find a match; add this pack to the list.
            calcPacks.Add(calcPack);
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

            // Check to see if this pack matches the default.
            bool isDefault = pack == CurrentDefaultPack(building);

            // Check to see if this building already has an entry.
            if (buildingDict.ContainsKey(buildingName))
            {
                // Contains an entry - check to see if this pack matches the default.
                if (isDefault)
                {
                    // Matches the default - just remove the custom entry.
                    buildingDict.Remove(buildingName);
                }
                else
                {
                    // Doesn't match the default - update the existing entry.
                    buildingDict[buildingName] = pack;
                }
            }
            else if (!isDefault)
            {
                // No entry yet and the pack isn't the default - add a custom entry.
                buildingDict.Add(buildingName, pack);
            }

            // Clear out any cached calculations for households.workplaces (depending on whether or not this is residential).
            if (building.GetService() == ItemClass.Service.Residential)
            {
                // Remove from household cache.
                DataStore.prefabHouseHolds.Remove(building.gameObject.GetHashCode());

                // Update household counts for existing instances of this building - only needed for residential buildings.
                // Workplace counts will update automatically with next call to CalculateWorkplaceCount; households require more work (tied to CitizenUnits).
                UpdateHouseholds(buildingName);
            }
            else
            {
                // Remove from workplace cache.
                DataStore.prefabWorkerVisit.Remove(building.gameObject.GetHashCode());
            }
        }


        /// <summary>
        /// Returns the currently set default calculation pack for the given prefab's service/subservice.
        /// </summary>
        /// <param name="building">Building prefab</param>
        /// <returns>Default calculation data pack</returns>
        internal static CalcPack CurrentDefaultPack(BuildingInfo building) => CurrentDefaultPack(building.GetService(), building.GetSubService());



        /// <summary>
        /// Returns the currently set default calculation pack for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="subService">Sub-service</param>
        /// <returns>Default calculation data pack</returns>
        internal static CalcPack CurrentDefaultPack(ItemClass.Service service, ItemClass.SubService subService)
        {
            // See if we've got an entry for this service.
            if (serviceDict.ContainsKey(service))
            {
                // We do; check for sub-service entry.
                if (serviceDict[service].ContainsKey(subService))
                {
                    // Got an entry!  Return it.
                    return serviceDict[service][subService];
                }
            }

            // If we got here, we didn't get a match; return base default entry.
            return BaseDefaultPack(service, subService);
        }


        /// <summary>
        /// Returns the inbuilt default calculation pack for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="subService">Sub-service</param>
        /// <returns>Default calculation data pack</returns>
        internal static CalcPack BaseDefaultPack(ItemClass.Service service, ItemClass.SubService subService)
        {
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
                case ItemClass.Service.Education:
                    defaultName = "schoolsub";
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
        internal static CalcPack[] GetPacks(BuildingInfo prefab) => GetPacks(prefab.GetService(), prefab.GetSubService());


        /// <summary>
        /// Returns a list of calculation packs available for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="subService">Sub-service</param>
        /// <returns>Array of available calculation packs</returns>
        internal static CalcPack[] GetPacks(ItemClass.Service service, ItemClass.SubService subService)
        {
            // Return list.
            List<CalcPack> list = new List<CalcPack>();

            // Iterate through each floor pack and see if it applies.
            foreach (CalcPack pack in calcPacks)
            {
                // Check for matching service.
                if (pack.service == service)
                {
                    // Service matches; check subservices.
                    // If no subservices are listed, then this applies to all subservice types for this service.
                    if (pack.subServices == null)
                    {
                        list.Add(pack);
                    }
                    // Otherwise, iterate through subservices to see if there's a match.
                    else
                    {
                        foreach (ItemClass.SubService packSubService in pack.subServices)
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




        /// <summary>
        /// Updates the household numbers of already existing (placed/grown) residential building instances to the current prefab value.
        /// Called after updating a residential prefab's household count in order to apply changes to existing buildings.
        /// </summary>
        /// <param name="prefabName">The (raw BuildingInfo) name of the prefab</param>
        internal static void UpdateHouseholds(string prefabName)
        {
            // Get building manager instance.
            var instance = Singleton<BuildingManager>.instance;

            // Iterate through each building in the scene.
            for (ushort i = 0; i < instance.m_buildings.m_buffer.Length; i++)
            {
                // Get current building instance.
                Building thisBuilding = instance.m_buildings.m_buffer[i];

                // Only interested in residential buildings.
                BuildingAI thisAI = thisBuilding.Info?.GetAI() as ResidentialBuildingAI;
                if (thisAI != null)
                {
                    // Residential building; check for name match.
                    if (thisBuilding.Info.name.Equals(prefabName))
                    {
                        // Got one!  Recalculate home and visit counts.
                        int homeCount = ((ResidentialBuildingAI)thisAI).CalculateHomeCount((ItemClass.Level)thisBuilding.m_level, new Randomizer(i), thisBuilding.Width, thisBuilding.Length);
                        int visitCount = ((ResidentialBuildingAI)thisAI).CalculateVisitplaceCount((ItemClass.Level)thisBuilding.m_level, new Randomizer(i), thisBuilding.Width, thisBuilding.Length);

                        // Apply changes via direct call to EnsureCitizenUnits prefix patch from this mod.
                        RealisticCitizenUnits.Prefix(ref thisAI, i, ref thisBuilding, homeCount, 0, visitCount, 0);
                    }
                }
            }
        }
    }
}
 