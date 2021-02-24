using UnityEngine;


namespace RealPop2
{
    /// <summary>
    /// Centralised store and management of employment calculation data.
    /// </summary>
    internal static class EmploymentData
    {
        // Arrays for employment percentages by eduction level.
        private static int[][] commercialLow, commercialHigh, office, industry, industryFarm, industryForest, industryOre, industryOil;
        private static int[] commercialEco, commercialLeisure, commercialTourist, officeHightech;


        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="prefab">Building prefab</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        internal static PrefabEmployStruct CalculateWorkplaces(BuildingInfo prefab, int level)
        {
            PrefabEmployStruct employStruct;

            // Get total jobs and distribution.
            int totalJobs = PopData.instance.Population(prefab, level);
            int[] distribution = WorkplaceDistribution(prefab.GetService(), prefab.GetSubService(), (ItemClass.Level)level);

            // Allocate jobs according to distribution (percentages).  Division after multiplication to reduce intermediate rounding errors.
            employStruct.level1 = (totalJobs * distribution[1]) / 100;
            employStruct.level2 = (totalJobs * distribution[2]) / 100;
            employStruct.level3 = (totalJobs * distribution[3]) / 100;

            // Level 0 is the remainder.
            employStruct.level0 = totalJobs - employStruct.level1 - employStruct.level2 - employStruct.level3;

            // Visit count - vanilla calculation, but using number of jobs multiplied by vanilla jobs:visitor ratio.
            if (distribution[5] != 0)
            {
                employStruct.visitors = Mathf.Max(200, (totalJobs * distribution[4]) / distribution[5]) / 100;
            }
            else
            {
                employStruct.visitors = 0;
            }

            return employStruct;
        }


        /// <summary>
        /// Initialises arrays with default values.
        /// First four values are employment percentages by education level, the fifth (index 4) value is VisitCount loading (from vanilla), the sixth value is vanilla population loading (we have it here for visit count conversion; zero means unused).
        /// </summary>
        internal static void Setup()
        {
            commercialLow = new int[][]
            {
                new int[] { 70, 20, 10, 0, 90, 50 },
                new int[] { 30, 45, 20, 5, 100, 75 },
                new int[] { 5, 30, 55, 10, 110, 100 }
            };

            commercialHigh = new int[][]
            {
                new int[] { 10, 45, 40, 5, 200, 75 },
                new int[] { 7, 32, 43, 18, 300, 100 },
                new int[] { 5, 25, 45, 25, 400, 125 }
            };

            office = new int[][]
            {
                new int[] { 2, 8, 20, 70, 0, 0 },
                new int[] { 1, 5, 14, 80, 0, 0 },
                new int[] { 1, 3, 6, 90, 0, 0 }
            };

            industry = new int[][]
            {
                new int[] { 70, 20, 10, 0, 0, 0 },
                new int[] { 20, 45, 25, 10, 0, 0 },
                new int[] { 5, 20, 45, 30, 0, 0 }
            };

            industryFarm = new int[][]
            {
                new int[] { 90, 10,  0, 0, 0, 0 },
                new int[] { 30, 60, 10, 0, 0, 0 }
            };

            industryForest = new int[][]
            {
                new int[] { 90, 10,  0, 0, 0, 0 },
                new int[] { 30, 60, 10, 0, 0, 0 }
            };

            industryOil = new int[][]
            {
                new int[] { 15, 60, 23, 2, 0, 0 },
                new int[] { 10, 35, 45, 10, 0, 0 }
            };

            industryOre = new int[][]
            {
                new int[] { 18, 60, 20, 2, 0, 0 },
                new int[] { 15, 40, 35, 10, 0, 0 }
            };

            commercialEco = new int[] { 50, 40, 10, 0, 100, 100 };

            commercialTourist = new int[] { 15, 35, 35, 15, 250, 100 };

            commercialLeisure = new int[] { 15, 40, 35, 10, 250, 100 };

            officeHightech = new int[] { 1, 2, 3, 94, 0, 0 };
        }


        /// <summary>
        /// Returns the workplace distribution for the given service/subservice/level combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="subService">Sub-service</param>
        /// <param name="level">Level (ignored if a single-level service/subservice combination has been provided)</param>
        /// <returns>Workplace distribution by level as an array of four percentages (cumulative 100%)</returns>
        private static int[] WorkplaceDistribution(ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level)
        {
            // Maximum levels for generic workplaces and specialized industry (zero-based).
            const ItemClass.Level MaxWorkplaceLevel = ItemClass.Level.Level3;
            const ItemClass.Level MaxSpecIndLevel = ItemClass.Level.Level2;

            switch (service)
            {
                case ItemClass.Service.Office:
                    if (subService == ItemClass.SubService.OfficeHightech)
                    {
                        return officeHightech;
                    }
                    // Default is generic office.
                    return office[CheckBuildingLevel(level, MaxWorkplaceLevel)];
                case ItemClass.Service.Industrial:
                    switch (subService)
                    {
                        case ItemClass.SubService.IndustrialForestry:
                            return industryForest[CheckBuildingLevel(level, MaxSpecIndLevel)];
                        case ItemClass.SubService.IndustrialFarming:
                            return industryFarm[CheckBuildingLevel(level, MaxSpecIndLevel)];
                        case ItemClass.SubService.IndustrialOil:
                            return industryOil[CheckBuildingLevel(level, MaxSpecIndLevel)];
                        case ItemClass.SubService.IndustrialOre:
                            return industryOre[CheckBuildingLevel(level, MaxSpecIndLevel)];
                        default:
                            // Default is generic industry.
                            return industry[CheckBuildingLevel(level, MaxWorkplaceLevel)];
                    }
                default:
                    // Default is commercial.
                    switch (subService)
                    {
                        case ItemClass.SubService.CommercialHigh:
                            return commercialHigh[CheckBuildingLevel(level, MaxWorkplaceLevel)];
                        case ItemClass.SubService.CommercialLeisure:
                            return commercialLeisure;
                        case ItemClass.SubService.CommercialTourist:
                            return commercialTourist;
                        case ItemClass.SubService.CommercialEco:
                            return commercialEco;
                        default:
                            // Default is commercial low.
                            return commercialLow[CheckBuildingLevel(level, MaxWorkplaceLevel)];
                    }
            }
        }


        /// <summary>
        /// Checks the level of a building to make sure it's valid.
        /// </summary>
        /// <param name="level">Building level to check</param>
        /// <param name="maxLevel">Maximum level permitted</param>
        /// <returns>Minimum of provided building level or maximem level permitted</returns>
        private static int CheckBuildingLevel(ItemClass.Level level, ItemClass.Level maxLevel)
        {
            ItemClass.Level checkedLevel = level;

            if (checkedLevel > maxLevel)
            {
                Logging.Error("invalid building level ", level.ToString(), " for workplace with maximum level ", maxLevel.ToString());
                checkedLevel = maxLevel;
            }

            return (int)checkedLevel;
        }
    }
}