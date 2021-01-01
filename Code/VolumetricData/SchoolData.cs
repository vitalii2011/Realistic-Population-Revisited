using System.Linq;
using System.Collections.Generic;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Centralised store and management of school calculation data.
    /// </summary>
    internal class SchoolData : CalcData
    {
        // Instance reference.
        internal static SchoolData instance;


        /// <summary>
        /// Constructor - initializes inbuilt default calculation packs and performs other setup tasks.
        /// </summary>
        public SchoolData()
        {
            // Vanilla elementary.
            SchoolDataPack newPack = new SchoolDataPack
            {
                name = "vanelem",
                displayName = Translations.Translate("RPR_PCK_SVE_NAM"),
                description = Translations.Translate("RPR_PCK_SVE_DES"),
                version = (int)DataVersion.one,
                level = ItemClass.Level.Level1,
                baseWorkers = new int[] { 1, 2, 1, 0 },
                workersPer = new int[] { 20, 50, 300, 0 }
            };
            calcPacks.Add(newPack);

            // Vanilla community school.
            newPack = new SchoolDataPack
            {
                name = "vancom",
                displayName = Translations.Translate("RPR_PCK_SVC_NAM"),
                description = Translations.Translate("RPR_PCK_SVC_DES"),
                version = (int)DataVersion.one,
                level = ItemClass.Level.Level1,
                baseWorkers = new int[] { 2, 2, 1, 1 },
                workersPer = new int[] { 25, 25, 50, 0 }
            };
            calcPacks.Add(newPack);

            // Vanilla high school.
            newPack = new SchoolDataPack
            {
                name = "vanhigh",
                displayName = Translations.Translate("RPR_PCK_SVH_NAM"),
                description = Translations.Translate("RPR_PCK_SVH_DES"),
                version = (int)DataVersion.one,
                level = ItemClass.Level.Level2,
                baseWorkers = new int[] { 9, 11, 5, 1 },
                workersPer = new int[] { 100, 20, 100, 250 }
            };
            calcPacks.Add(newPack);

            // Vanilla art school.
            newPack = new SchoolDataPack
            {
                name = "vanart",
                displayName = Translations.Translate("RPR_PCK_SVA_NAM"),
                description = Translations.Translate("RPR_PCK_SVA_DES"),
                version = (int)DataVersion.one,
                level = ItemClass.Level.Level2,
                baseWorkers = new int[] { 10, 20, 5, 1 },
                workersPer = new int[] { 80, 20, 80, 200 }
            };
            calcPacks.Add(newPack);
        }


        /// <summary>
        /// Updates our building setting dictionary for the selected building prefab to the indicated calculation pack.
        /// IMPORTANT: make sure student count is called before calling this.
        /// </summary>
        /// <param name="building">Building prefab to update</param>
        /// <param name="pack">New data pack to apply</param>
        internal override void UpdateBuildingPack(BuildingInfo prefab, DataPack pack)
        {
            // Call base to update dictionary.
            base.UpdateBuildingPack(prefab, pack);

            // Apply settings to prefab.
            ApplyPack(prefab, pack as SchoolDataPack);
        }


        /// <summary>
        /// Returns the currently set default calculation pack for the given prefab.
        /// </summary>
        /// <param name="building">Building prefab</param>
        /// <returns>Default calculation data pack</returns>
        internal override DataPack CurrentDefaultPack(BuildingInfo building) => BaseDefaultPack(building.GetClassLevel(), building);


        /// <summary>
        /// Returns the inbuilt default calculation pack for the given school AI level and prefab.
        /// </summary>
        /// <param name="service">School level to check</param
        /// <param name="prefab">Building prefab to check (null if none)</param>
        /// <returns>Default calculation data pack</returns>
        internal DataPack BaseDefaultPack(ItemClass.Level level, BuildingInfo prefab)
        {
            string defaultName;

            // Is it high school?
            if (level == ItemClass.Level.Level2)
            {
                if (prefab?.name != null && prefab.name.Equals("University of Creative Arts"))
                {
                    // Art school.
                    defaultName = "vanart";
                }
                else
                {
                    // Plain high school.
                    defaultName = "vanhigh";
                }
            }
            else
            // If not high school, default to elementary school.
            {
                if (prefab?.name != null && prefab.name.Equals("Community School"))
                {
                    // Community school.
                    defaultName = "vancom";
                }
                else
                {
                    // Plain elementary school.
                    defaultName = "vanelem";
                }
            }

            // Match name to floorpack.
            return calcPacks.Find(pack => pack.name.Equals(defaultName));
        }


        /// <summary>
        /// Returns a list of calculation packs available for the given prefab.
        /// </summary>
        /// <param name="prefab">BuildingInfo prefab</param>
        /// <returns>Array of available calculation packs</returns>
        internal SchoolDataPack[] GetPacks(BuildingInfo prefab)
        {
            // Return list.
            List<SchoolDataPack> list = new List<SchoolDataPack>();

            ItemClass.Level level = prefab.GetClassLevel();

            // Iterate through each floor pack and see if it applies.
            foreach (SchoolDataPack pack in calcPacks)
            {
                // Check for matching service.
                if (pack.level == level)
                {
                    // Service matches; add pack.
                    list.Add(pack);
                }
            }

            return list.ToArray();
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
                string packName = buildingDict[prefabName]?.name;

                // Check to see if our existing list already contains this building.
                if (returnList.ContainsKey(prefabName))
                {
                    // Yes; update that record to include this floor pack.
                    returnList[prefabName].schoolPack = packName;
                }
                else
                {
                    // No; add a new record with this floor pack.
                    BuildingRecord newRecord = new BuildingRecord { prefab = prefabName, schoolPack = packName };
                    returnList.Add(prefabName, newRecord);
                }
            }

            return returnList.Values.ToList();
        }


        /// <summary>
        /// Deserializes a provided building record list.
        /// </summary>
        /// <param name="recordList">List to deserialize</param>
        internal override void DeserializeBuildings(List<BuildingRecord> recordList)
        {
            Debugging.Message("starting deserialization of schools with record count " + recordList.Count);

            // Use base deserialisation to populate dictionary.
            base.DeserializeBuildings(recordList);

            // Now, apply each entry in dictionary.
            foreach (string buildingName in buildingDict.Keys)
            {
                Debugging.Message("Found school settings for " + buildingName);
                ApplyPack(PrefabCollection<BuildingInfo>.FindLoaded(buildingName), buildingDict[buildingName] as SchoolDataPack);
            }
        }


        /// <summary>
        /// Calculates school worker totals by education level, given a school calculatin pack and a total student count.
        /// </summary>
        /// <param name="schoolPack">School calculation pack to use</param>
        /// <param name="students">Student count to use</param>
        /// <returns></returns>
        internal int[] CalcWorkers(SchoolDataPack schoolPack, int students)
        {
            const int WorkerLevels = 4;

            int[] workers = new int[WorkerLevels];

            // Basic checks.  If we fail we just return the zeroed array.
            if (schoolPack != null)
            {

                // Local references.
                int[] baseWorkers = schoolPack.baseWorkers;
                int[] workersPer = schoolPack.workersPer;

                // Calculate extra jobs for X number of students (ensuring divisor is greater than zero).
                for (int i = 0; i < WorkerLevels; ++i)
                {
                    workers[i] = baseWorkers[i] + (workersPer[0] > 0 ? students / workersPer[0] : 0);
                }
            }

            return workers;
        }


        /// <summary>
        /// Applies a school data pack to a school prefab.
        /// </summary>
        /// <param name="prefab">School prefab to apply to</param>
        /// <param name="schoolPack">School data pack to apply</param>
        private void ApplyPack(BuildingInfo prefab, SchoolDataPack schoolPack)
        {
            // Null checks first.
            if (prefab?.name == null)
            {
                Debugging.Message("No prefab found for SchoolPack");
            }

            if (schoolPack == null)
            {
                Debugging.Message("No SchoolPack found for prefab " + prefab.name);
            }

            Debugging.Message("applying school pack " + schoolPack.name + " to prefab " + prefab.name);

            // Apply settings to prefab.
            SchoolAI schoolAI = prefab.GetAI() as SchoolAI;
            if (prefab != null && schoolPack != null)
            {
                // Calculate workers and breakdowns.
                int[] workers = CalcWorkers(schoolPack, schoolAI.StudentCount);

                // Update prefab AI worker count with results (base + extras) per education level.
                schoolAI.m_workPlaceCount0 = workers[0];
                schoolAI.m_workPlaceCount1 = workers[1];
                schoolAI.m_workPlaceCount2 = workers[2];
                schoolAI.m_workPlaceCount3 = workers[3];
            }
        }


        /// <summary>
        /// Extracts the relevant school pack name from a building line record.
        /// </summary>
        /// <param name="buildingRecord">Building record to extract from</param>
        /// <returns>School pack name (if any)</returns>
        protected override string BuildingPack(BuildingRecord buildingRecord) => buildingRecord.schoolPack;
    }
}