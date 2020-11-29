using System.Collections.Generic;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Centralised store and management of population calculation data.
    /// </summary>
    internal class PopData : CalcData
    {
        // Instance reference.
        internal static PopData instance;


        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="prefab">Building prefab</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        internal PrefabEmployStruct Workplaces(BuildingInfo buildingPrefab, int level) => ((PopDataPack)ActivePack(buildingPrefab)).Workplaces(buildingPrefab, level);


        /// <summary>
        /// Returns the population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Population</returns>
        internal int Population(BuildingInfo buildingPrefab, int level)
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
            return ((PopDataPack)ActivePack(buildingPrefab)).Population(buildingPrefab, level);
        }


        /// <summary>
        /// Calculates the population for the given building using the given LevelData and FloorDataPack.
        /// </summary>
        /// <param name="buildingInfoGen">Building info record</param>
        /// <param name="levelData">LevelData record to use for calculations</param>
        /// <param name="floorData">FloorDataPack record to use for calculations</param>
        /// <param name="floorList">Optional precalculated list of calculated floors (to save time; will be generated if not provided)</param>
        /// <param name="totalArea">Optional precalculated total building area  (to save time; will be generated if not provided)</param>
        /// <returns></returns>
        internal int VolumetricPopulation(BuildingInfoGen buildingInfoGen, LevelData levelData, FloorDataPack floorData, SortedList<int, float> floorList = null, float totalArea = 0)
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
                    floors = VolumetricFloors(buildingInfoGen, levelData, floorData, out floorArea);
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
        /// Returns a list of floors and populations for the given building.
        /// </summary>
        /// <param name="buildingInfoGen">Building info record</param>
        /// <param name="levelData">LevelData record to use for calculations</param>
        /// <param name="floorData">Floor calculation pack to use for calculations</param>
        /// <param name="total">Total area of all floors</param>
        /// <returns>Sorted list of floors (key = floor number, value = floor area)</returns>
        internal SortedList<int, float> VolumetricFloors(BuildingInfoGen buildingInfoGen, LevelData levelData, FloorDataPack floorData, out float totalArea)
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
                if (thisHeight > floorData.firstFloorMin)
                {
                    // Starting number of floors is either 1 or zero, depending on setting of 'ignore first floor' checkbox.
                    int numFloors = floorData.firstFloorEmpty ? 0 : 1;

                    // Calculate any height left over from the maximum (minimum plus extra) first floor height.
                    float surplusHeight = thisHeight - floorData.firstFloorMin - floorData.firstFloorExtra;

                    // See if we have more than one floor, i.e. our height is greater than the first floor maximum height.
                    if (surplusHeight > 0)
                    {
                        // Number of floors for this grid segment is the truncated division (rounded down); no partial floors here!
                        numFloors += (int)(surplusHeight / floorData.floorHeight);
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
        /// Constructor - initializes inbuilt default calculation packs and performs other setup tasks.
        /// </summary>
        public PopData()
        {
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
            VolumetricPopPack newPack = new VolumetricPopPack
            {
                name = "reslow",
                displayName = Translations.Translate("RPR_PCK_RLS_NAM"),
                description = Translations.Translate("RPR_PCK_RLS_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Residential,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.ResidentialLow, ItemClass.SubService.ResidentialLowEco },
                levels = new LevelData[5]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -1f, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -1f, multiFloorUnits = true };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -1f, multiFloorUnits = true };
            newPack.levels[3] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -1f, multiFloorUnits = true };
            newPack.levels[4] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -1f, multiFloorUnits = true };
            calcPacks.Add(newPack);

            // Medium-density residential.
            newPack = new VolumetricPopPack
            {
                name = "resmed",
                displayName = Translations.Translate("RPR_PCK_RMD_NAM"),
                description = Translations.Translate("RPR_PCK_RMD_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Residential,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.ResidentialLow, ItemClass.SubService.ResidentialLowEco, ItemClass.SubService.ResidentialHigh, ItemClass.SubService.ResidentialHighEco },
                levels = new LevelData[5]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 120f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 135f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 150f, multiFloorUnits = false };
            newPack.levels[3] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 165f, multiFloorUnits = false };
            newPack.levels[4] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 180f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // High-density residential.
            newPack = new VolumetricPopPack
            {
                name = "reshigh",
                displayName = Translations.Translate("RPR_PCK_RHR_NAM"),
                description = Translations.Translate("RPR_PCK_RHR_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Residential,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.ResidentialHigh, ItemClass.SubService.ResidentialHighEco },
                levels = new LevelData[5]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 110f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 125f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 140f, multiFloorUnits = false };
            newPack.levels[3] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 155f, multiFloorUnits = false };
            newPack.levels[4] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 170f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // European apartments (medium-density).
            newPack = new VolumetricPopPack
            {
                name = "resMedEU",
                displayName = Translations.Translate("RPR_PCK_REM_NAM"),
                description = Translations.Translate("RPR_PCK_REM_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Residential,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.ResidentialHigh, ItemClass.SubService.ResidentialHighEco },
                levels = new LevelData[5]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 10, areaPer = 85f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 12, areaPer = 90f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 16, areaPer = 100f, multiFloorUnits = false };
            newPack.levels[4] = new LevelData { emptyArea = 0f, emptyPercent = 18, areaPer = 105f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // European apartments (high-density).
            newPack = new VolumetricPopPack()
            {
                name = "resHighEU",
                displayName = Translations.Translate("RPR_PCK_REH_NAM"),
                description = Translations.Translate("RPR_PCK_REH_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Residential,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.ResidentialHigh, ItemClass.SubService.ResidentialHighEco },
                levels = new LevelData[5],
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 75f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 1, areaPer = 80f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 2, areaPer = 85f, multiFloorUnits = false };
            newPack.levels[3] = new LevelData { emptyArea = 0f, emptyPercent = 3, areaPer = 90f, multiFloorUnits = false };
            newPack.levels[4] = new LevelData { emptyArea = 0f, emptyPercent = 4, areaPer = 95f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // High-density residential US - empty percentages from AssetInsights.net, areas from 2018 RentCafe.
            newPack = new VolumetricPopPack()
            {
                name = "reshighUS",
                displayName = Translations.Translate("RPR_PCK_RUH_NAM"),
                description = Translations.Translate("RPR_PCK_RUH_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Residential,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.ResidentialHigh, ItemClass.SubService.ResidentialHighEco },
                levels = new LevelData[5]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 36, areaPer = 70f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 37, areaPer = 78f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 38, areaPer = 86f, multiFloorUnits = false };
            newPack.levels[3] = new LevelData { emptyArea = 0f, emptyPercent = 39, areaPer = 94f, multiFloorUnits = false };
            newPack.levels[4] = new LevelData { emptyArea = 0f, emptyPercent = 40, areaPer = 102f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Low-density commercial.
            // Figures are from Montgomery County round 7.0.
            newPack = new VolumetricPopPack()
            {
                name = "comlow",
                displayName = Translations.Translate("RPR_PCK_CLS_NAM"),
                description = Translations.Translate("RPR_PCK_CLS_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Commercial,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.CommercialLow, ItemClass.SubService.CommercialEco, ItemClass.SubService.CommercialLeisure, ItemClass.SubService.CommercialTourist },
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0, emptyPercent = 0, areaPer = 40, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 0, emptyPercent = 0, areaPer = 40, multiFloorUnits = true };
            newPack.levels[2] = new LevelData { emptyArea = 0, emptyPercent = 0, areaPer = 40, multiFloorUnits = true };
            calcPacks.Add(newPack);

            // High-density commercial.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            // 20% gross-up for conversion from NIA to GIA.
            newPack = new VolumetricPopPack()
            {
                name = "comhigh",
                displayName = Translations.Translate("RPR_PCK_CHC_NAM"),
                description = Translations.Translate("RPR_PCK_CHC_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Commercial,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.CommercialHigh, ItemClass.SubService.CommercialLeisure, ItemClass.SubService.CommercialTourist },
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 23f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 23f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 23f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Retail warehouses.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            // 20% gross-up for conversion from NIA to GIA.
            newPack = new VolumetricPopPack()
            {
                name = "retailware",
                displayName = Translations.Translate("RPR_PCK_CLW_NAM"),
                description = Translations.Translate("RPR_PCK_CLW_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Commercial,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.CommercialLow, ItemClass.SubService.CommercialHigh },
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 108f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 108f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 108f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Hotels.
            // Figures are from Montgomery County round 7.0.
            newPack = new VolumetricPopPack()
            {
                name = "hotel",
                displayName = Translations.Translate("RPR_PCK_THT_NAM"),
                description = Translations.Translate("RPR_PCK_THT_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Commercial,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.CommercialLow, ItemClass.SubService.CommercialTourist },
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 130f, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 130f, multiFloorUnits = true };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 130f, multiFloorUnits = true };
            calcPacks.Add(newPack);

            // Restaurants and cafes.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            // 20% gross-up for conversion from NIA to GIA.
            newPack = new VolumetricPopPack()
            {
                name = "restaurant",
                displayName = Translations.Translate("RPR_PCK_LFD_NAM"),
                description = Translations.Translate("RPR_PCK_LFD_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Commercial,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.CommercialLow, ItemClass.SubService.CommercialLeisure },
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 22f, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 22f, multiFloorUnits = true };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 22f, multiFloorUnits = true };
            calcPacks.Add(newPack);

            // Entertainment centres.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            // 20% gross-up for conversion from NIA to GIA.
            newPack = new VolumetricPopPack()
            {
                name = "entertainment",
                displayName = Translations.Translate("RPR_PCK_LEN_NAM"),
                description = Translations.Translate("RPR_PCK_LEN_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Commercial,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.CommercialLow, ItemClass.SubService.CommercialLeisure },
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 108f, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 108f, multiFloorUnits = true };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 108f, multiFloorUnits = true };
            calcPacks.Add(newPack);

            // Light industry.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            newPack = new VolumetricPopPack()
            {
                name = "lightind",
                displayName = Translations.Translate("RPR_PCK_ILG_NAM"),
                description = Translations.Translate("RPR_PCK_ILG_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Industrial,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.IndustrialGeneric, ItemClass.SubService.IndustrialFarming, ItemClass.SubService.IndustrialForestry, ItemClass.SubService.IndustrialOil, ItemClass.SubService.IndustrialOre },
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 47f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 47f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 47f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Industry factory.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            newPack = new VolumetricPopPack()
            {
                name = "factory",
                displayName = Translations.Translate("RPR_PCK_IFC_NAM"),
                description = Translations.Translate("RPR_PCK_IFC_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Industrial,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.IndustrialGeneric, ItemClass.SubService.IndustrialFarming, ItemClass.SubService.IndustrialForestry, ItemClass.SubService.IndustrialOil, ItemClass.SubService.IndustrialOre },
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 36f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 36f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 36f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Industry warehouse.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            newPack = new VolumetricPopPack()
            {
                name = "warehouse",
                displayName = Translations.Translate("RPR_PCK_IGW_NAM"),
                description = Translations.Translate("RPR_PCK_IGW_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Industrial,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.IndustrialGeneric, ItemClass.SubService.IndustrialFarming, ItemClass.SubService.IndustrialForestry, ItemClass.SubService.IndustrialOil, ItemClass.SubService.IndustrialOre },
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 70f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 70f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 70f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Industry high-bay warehouse.
            // Densities from 2010 OffPAT/Homes & Communities Agency/Drivers Jonas Deloitte 'Employment Densities Guide' 2nd Edition.
            newPack = new VolumetricPopPack()
            {
                name = "highbay",
                displayName = Translations.Translate("RPR_PCK_IHB_NAM"),
                description = Translations.Translate("RPR_PCK_IHB_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Industrial,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.IndustrialGeneric, ItemClass.SubService.IndustrialFarming, ItemClass.SubService.IndustrialForestry, ItemClass.SubService.IndustrialOil, ItemClass.SubService.IndustrialOre },
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 80f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 80f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 80f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Low-density office.
            newPack = new VolumetricPopPack()
            {
                name = "offlow",
                displayName = Translations.Translate("RPR_PCK_OLD_NAM"),
                description = Translations.Translate("RPR_PCK_OLD_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Office,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.OfficeGeneric, ItemClass.SubService.OfficeHightech },
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 34f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 36f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 38f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // High-density office.
            // Figures are from Montgomery County round 7.0.
            newPack = new VolumetricPopPack()
            {
                name = "offhigh",
                displayName = Translations.Translate("RPR_PCK_OHD_NAM"),
                description = Translations.Translate("RPR_PCK_OHD_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Office,
                subServices = new ItemClass.SubService[] { ItemClass.SubService.OfficeGeneric, ItemClass.SubService.OfficeHightech },
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 25f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 25f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 25f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Suburban schools.
            // Level 1 is elementary, level 2 is high school.
            // Figures are from NSW Department of Education 2013 targets.
            newPack = new VolumetricPopPack()
            {
                name = "schoolsub",
                displayName = Translations.Translate("RPR_PCK_SSB_NAM"),
                description = Translations.Translate("RPR_PCK_SSB_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Education,
                subServices = null,
                levels = new LevelData[2]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 8f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 8f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Suburban schools.
            // Figures are from MN Department of Education Guide for Planning School Construction Projects (lowest density).
            newPack = new VolumetricPopPack()
            {
                name = "schoolmnlow",
                displayName = Translations.Translate("RPR_PCK_SML_NAM"),
                description = Translations.Translate("RPR_PCK_SML_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Education,
                subServices = null,
                levels = new LevelData[2]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 14f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 30f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Suburban schools.
            // Figures are from MN Department of Education Guide for Planning School Construction Projects (middle density).
            newPack = new VolumetricPopPack()
            {
                name = "schoolmnmed",
                displayName = Translations.Translate("RPR_PCK_SMM_NAM"),
                description = Translations.Translate("RPR_PCK_SMM_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Education,
                subServices = null,
                levels = new LevelData[2]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 12f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 23f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Suburban schools.
            // Figures are from MN Department of Education Guide for Planning School Construction Projects (highest density).
            newPack = new VolumetricPopPack()
            {
                name = "schoolmnhigh",
                displayName = Translations.Translate("RPR_PCK_SMH_NAM"),
                description = Translations.Translate("RPR_PCK_SMH_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Education,
                subServices = null,
                levels = new LevelData[2]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 9f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 14f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // UK schools.
            // Figures are from Planning Statement - Warwickshire County Council.
            newPack = new VolumetricPopPack()
            {
                name = "schoolukhigh",
                displayName = Translations.Translate("RPR_PCK_SUK_NAM"),
                description = Translations.Translate("RPR_PCK_SUK_DES"),
                version = (int)DataVersion.one,
                service = ItemClass.Service.Education,
                subServices = null,
                levels = new LevelData[2]
            };
            newPack.levels[0] = new LevelData { emptyArea = 350f, emptyPercent = 0, areaPer = 4.1f, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 1400f, emptyPercent = 0, areaPer = 6.3f, multiFloorUnits = true };
            calcPacks.Add(newPack);
        }


        /// <summary>
        /// Adds or updates a calculation pack entry to our list.
        /// </summary>
        /// <param name="calcPack">Calculation pack to add</param>
        internal void AddCalculationPack(PopDataPack calcPack)
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
        internal PopDataPack[] GetPacks(BuildingInfo prefab) => GetPacks(prefab.GetService(), prefab.GetSubService());


        /// <summary>
        /// Returns a list of calculation packs available for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="subService">Sub-service</param>
        /// <returns>Array of available calculation packs</returns>
        internal PopDataPack[] GetPacks(ItemClass.Service service, ItemClass.SubService subService)
        {
            // Return list.
            List<PopDataPack> list = new List<PopDataPack>();

            // Iterate through each floor pack and see if it applies.
            foreach (PopDataPack pack in calcPacks)
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
        /// Serializes building pack settings to XML.  Intended to be passed directly to FloorData.SerializeBuildings.
        /// </summary>
        /// <returns>New sorted list with building pack settings</returns>
        internal SortedList<string, BuildingRecord> SerializeBuildings()
        {
            // Return list.
            SortedList<string, BuildingRecord> returnList =  new SortedList<string, BuildingRecord>();

            // Iterate through each key (BuildingInfo) in our dictionary.
            foreach (string prefabName in buildingDict.Keys)
            {
                // Serialise it into a BuildingRecord and add it to the list.
                BuildingRecord newRecord = new BuildingRecord { prefab = prefabName, popPack = buildingDict[prefabName].name };
                returnList.Add(prefabName, newRecord);
            }

            return returnList;
        }


        /// <summary>
        /// Extracts the relevant pack name (floor or pop) from a building line record.
        /// </summary>
        /// <param name="buildingRecord">Building record to extract from</param>
        /// <returns>Floor pack name (if any)</returns>
        protected override string BuildingPack(BuildingRecord buildingRecord) => buildingRecord.popPack;

    }
}
 