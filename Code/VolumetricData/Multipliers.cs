using System.Collections.Generic;
using ColossalFramework;


namespace RealisticPopulationRevisited
{
    internal class Multipliers
    {
        // Default multiplier.
        private const float DefaultMultiplier = 1.0f;


        // Instance reference.
        internal static Multipliers instance;

        // List of building settings.
        private Dictionary<string, float> buildingDict;

        /// <summary>
        /// Constructor - initializes dictionary and performs other setup tasks.
        /// </summary>
        public Multipliers()
        {
            // Initialise building dictionary.
            buildingDict = new Dictionary<string, float>();
        }

        /// <summary>
        /// Returns the currently active multipler for the given prefab (default 1.0).
        /// </summary>
        /// <param name="buildingName">Selected prefab name</param>
        /// <returns>Currently active multiplier, or 1.0 by default if no override in place</returns>
        internal float ActiveMultiplier(string buildingName)
        {
            // Check to see if we have a multiplier override in effect.
            if (buildingName != null && buildingDict.ContainsKey(buildingName))
            {
                // Yes - return the mutlplier.
                return buildingDict[buildingName];
            }

            // If we got here, we don't have a multiplier override; return the default.
            return DefaultMultiplier;
        }


        /// <summary>
        /// Changes (adding or updating) the currently set multiplier for the given building prefab.
        /// </summary>
        /// <param name="prefab">Selected prefab</param>
        /// <param name="multiplier">New multiplier to apply</param>
        internal void UpdateMultiplier(BuildingInfo prefab, float multiplier)
        {
            string buildingName = prefab.name;

            // Check to see if we have an existing entry.
            if (buildingDict.ContainsKey(buildingName))
            {
                // We have an existing key - is the new value the default 1.0f?
                if (multiplier == DefaultMultiplier)
                {
                    // Yes, applying the default multiplier - simply remove default entry.
                    buildingDict.Remove(buildingName);
                }
                else
                {
                    // No, applying a different modifier - update dictionary entry.
                    buildingDict[buildingName] = multiplier;
                }
            }
            else
            {
                // No existing entry - if we're only trying to apply the default, just return without doing anything.
                if (multiplier == DefaultMultiplier)
                {
                    return;
                }

                // Otherwise, create a new dictionary entry.
                buildingDict.Add(buildingName, multiplier);
            }
        }


        /// <summary>
        /// Deserializes a provided building record list.
        /// </summary>
        /// <param name="recordList">List to deserialize</param>
        internal void DeserializeBuildings(List<BuildingRecord> recordList)
        {
            // Iterate through each record in list.
            for (int i = 0; i < recordList.Count; ++i)
            {
                BuildingRecord buildingRecord = recordList[i];

                // Get multiplier.
                float multiplier = buildingRecord.multiplier;

                // Ignore invalid or default records.
                if (buildingRecord.prefab.IsNullOrWhiteSpace() || multiplier <= 1.0f)
                {
                    continue;
                }

                // Add building to our dictionary.
                buildingDict.Add(buildingRecord.prefab, multiplier);
            }
        }


        /// <summary>
        /// Serializes building pack settings to XML.
        /// </summary>
        /// <param name="existingList">Existing list to modify, from population pack serialization (null if none)</param>
        /// <returns>New list of building pack settings ready for XML</returns>
        internal SortedList<string, BuildingRecord> SerializeBuildings(SortedList<string, BuildingRecord> existingList)
        {
            // Return list.
            SortedList<string, BuildingRecord> returnList = existingList ?? new SortedList<string, BuildingRecord>();

            // Iterate through each key (BuildingInfo) in our dictionary and serialise it into a BuildingRecord.
            foreach (string prefabName in buildingDict.Keys)
            {
                // Get multiplier.
                float multiplier = buildingDict[prefabName];

                // Check to see if our existing list already contains this building.
                if (returnList.ContainsKey(prefabName))
                {
                    // Yes; update that record to include this multiplier.
                    returnList[prefabName].multiplier = multiplier;
                }
                else
                {
                    // No; add a new record with this multiplier.
                    BuildingRecord newRecord = new BuildingRecord { prefab = prefabName, multiplier = multiplier };
                    returnList.Add(prefabName, newRecord);
                }
            }

            return returnList;
        }
    }
}