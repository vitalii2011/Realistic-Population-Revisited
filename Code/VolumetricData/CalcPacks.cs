namespace RealisticPopulationRevisited
{
    public enum DataVersion
    {
        legacy = 0,
        one,
        customOne
    }


    /// <summary>
    /// Struct holding data for volumetric calculations for a given building level.
    /// </summary>
    public struct LevelData
    {
        // Height per floor in metres.
        public float floorHeight;

        // Empty area (if any) to be subtracted from building before any calculations are made.
        public int emptyArea;

        // Area per unit (household/worker), in square metres.
        public int areaPer;

        // Height needs to be at least this high, in metres, for any floor to exist.
        public float firstFloorMin;

        // Extend first floor height by this additional amount, in metres.
        public float firstFloorExtra;

        // True if the first floor should be excluded from calculations (e.g. for foyers/lobbies).
        public bool firstFloorEmpty;

        // True if unit areas should be calculated as though they extend through all levels (ground to roof - e.g. detached housing, rowhouses, etc.),
        // false if units should only be treated as single-floor entities (e.g. apartment blocks).
        // Generally speaking, true for low-density, false for high-density.
        public bool multiFloorUnits;
    }


    /// <summary>
    /// Calculation data pack - provides parameters for calculating building populations for given services and (optional) subservices.
    /// </summary>
    public class CalcPack
    {
        public int version;
        public string name;
        public string displayName;
        public string description;
        public ItemClass.Service service;
        public ItemClass.SubService[] subServices;


        /// <summary>
        /// Returns the population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Population</returns>
        public virtual int Population(BuildingInfo buildingPrefab, int level) => 0;


        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        public virtual PrefabEmployStruct Workplaces(BuildingInfo buildingPrefab, int level) => new PrefabEmployStruct();
    }


    /// <summary>
    /// Volumetric calculation data pack.
    /// </summary>
    public class VolumetricPack : CalcPack
    {
        // Building level records.
        public LevelData[] levels;

        /// <summary>
        /// Returns the volumetric population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Population</returns>
        public override int Population(BuildingInfo buildingPrefab, int level)// => PopData.VolumetricPopulation(buildingPrefab.m_generatedInfo, levels[level]);
        {
            return PopData.VolumetricPopulation(buildingPrefab.m_generatedInfo, levels[level]);
        }


        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        public override PrefabEmployStruct Workplaces(BuildingInfo buildingPrefab, int level) => EmploymentData.CalculateWorkplaces(buildingPrefab, level);
    }


    /// <summary>
    /// Legacy WG residential calculation pack.
    /// </summary>
    public class LegacyResPack : CalcPack
    {
        /// <summary>
        /// Returns the volumetric population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Population</returns>
        public override int Population(BuildingInfo buildingPrefab, int level)
        {
            int[] array = ResidentialBuildingAIMod.GetArray(buildingPrefab, (int)level);
            return AI_Utils.CalculatePrefabHousehold(buildingPrefab.GetWidth(), buildingPrefab.GetWidth(), ref buildingPrefab, ref array, (int)level);
        }
    }


    /// <summary>
    /// Legacy WG commercial calculation pack.
    /// </summary>
    public class LegacyComPack : CalcPack
    {
        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        public override PrefabEmployStruct Workplaces(BuildingInfo buildingPrefab, int level)
        {
            PrefabEmployStruct output;

            int[] array = CommercialBuildingAIMod.GetArray(buildingPrefab, level);
            AI_Utils.CalculateprefabWorkerVisit(buildingPrefab.GetWidth(), buildingPrefab.GetLength(), ref buildingPrefab, 4, ref array, out output);

            return output;
        }
    }


    /// <summary>
    /// Legacy WG industrial calculation pack.
    /// </summary>
    public class LegacyIndPack : CalcPack
    {
        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        public override PrefabEmployStruct Workplaces(BuildingInfo buildingPrefab, int level)
        {
            PrefabEmployStruct output;
            int[] array;
            int minWorkers;

            // Need to test if we're an extractor or not for this one.
            if (buildingPrefab.GetAI() is IndustrialExtractorAI)
            {
                array = IndustrialExtractorAIMod.GetArray(buildingPrefab, IndustrialExtractorAIMod.EXTRACT_LEVEL);
                minWorkers = 3;
            }
            else
            {
                array = IndustrialBuildingAIMod.GetArray(buildingPrefab, level);
                minWorkers = 4;
            }

            AI_Utils.CalculateprefabWorkerVisit(buildingPrefab.GetWidth(), buildingPrefab.GetLength(), ref buildingPrefab, minWorkers, ref array, out output);


            Debugging.Message("calculating legacy workplace count for industry " + buildingPrefab.name + " with workplaces " + output.level0 + "," + output.level1 + "," + output.level2 + "," + output.level3);

            return output;
        }
    }


    /// <summary>
    /// Legacy WG office calculation pack.
    /// </summary>
    public class LegacyOffPack : CalcPack
    {
        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        public override PrefabEmployStruct Workplaces(BuildingInfo buildingPrefab, int level)
        {
            PrefabEmployStruct output;

            int[] array = OfficeBuildingAIMod.GetArray(buildingPrefab, level);
            AI_Utils.CalculateprefabWorkerVisit(buildingPrefab.GetWidth(), buildingPrefab.GetLength(), ref buildingPrefab, 10, ref array, out output);

            return output;
        }
    }
}