using System.Collections.Generic;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// External interfaces that can be used by other mods.
    /// Also used by the building details UI to modify buildings.
    /// </summary>
    class ExternalCalls
    {
        /// <summary>
        /// Returns the customised number of households for a given prefab.
        /// Returns 0 if no custom settings exist.
        /// </summary>
        /// <param name="prefab">The prefab (BuldingInfo) to query</param>
        /// <returns>The custom household count (0 if no settings)</returns>
        public static int GetResidential(BuildingInfo prefab)
        {
            int returnValue;
            if (DataStore.householdCache.TryGetValue(prefab.name, out returnValue))
            {
                return returnValue;
            }
            return 0;
        }


        /// <summary>
        /// Sets the customised number of households for a given prefab.
        /// If a record doesn't already exist, a new one will be created.
        /// </summary>
        /// <param name="prefab">The prefab (BuildingInfo) to set</param>
        /// <param name="houses">The updated households</param>
        public static void SetResidential(BuildingInfo prefab, int houses)
        {
            // Update or add entry to configuration file cache.
            if (DataStore.householdCache.ContainsKey(prefab.name))
            {
                // Prefab already has a record; update.
                DataStore.householdCache[prefab.name] = houses;
            }
            else
            {
                // Prefab doesn't already have a record; create.
                DataStore.householdCache.Add(prefab.name, houses);
            }

            // Save the updated configuration files.
            ConfigUtils.SaveSettings();

            // Get current building hash (for updating prefab dictionary).
            var prefabHash = prefab.gameObject.GetHashCode();

            // Update entry in 'live' settings.
            if (DataStore.prefabHouseHolds.ContainsKey(prefabHash))
            {
                // Prefab already has a record; update.
                DataStore.prefabHouseHolds[prefabHash] = houses;
            }
            else
            {
                // Prefab doesn't already have a record; create.
                DataStore.prefabHouseHolds.Add(prefabHash, houses);
            }
        }


        /// <summary>
        /// Removes the custom household record (if any) for a given prefab.
        /// </summary>
        /// <param name="prefab">The prefab (BuildingInfo) to remove the record from</param>
        public static void RemoveResidential(BuildingInfo prefab)
        {
            // Remove the entry from the configuration file cache.
            DataStore.householdCache.Remove(prefab.name);

            // Save the updated configuration files.
            XMLUtilsWG.WriteToXML();
            ConfigUtils.SaveSettings();

            // Remove current building's record from 'live' dictionary.
            DataStore.prefabHouseHolds.Remove(prefab.gameObject.GetHashCode());
        }

        /// <summary>
        /// Returns the customised number of workers for a given prefab.
        /// Returns 0 if no custom settings exist.
        /// </summary>
        /// <param name="prefab">The custom worker count (0 if no settings)</param>
        /// <returns></returns>
        public static int GetWorker(BuildingInfo prefab)
        {
            int returnValue;
            if (DataStore.workerCache.TryGetValue(prefab.name, out returnValue))
            {
                return returnValue;
            }
            return 0;
        }


        /// <summary>
        /// Sets the customised number of workers for a given prefab.
        /// If a record doesn't already exist, a new one will be created.
        /// </summary>
        /// <param name="prefab">The prefab (BuildingInfo) to set</param>
        /// <param name="workers">The updated worker count</param>
        public static void SetWorker(BuildingInfo prefab, int workers)
        {
            // Update or add entry to configuration file cache.
            if (DataStore.workerCache.ContainsKey(prefab.name))
            {
                // Prefab already has a record; update.
                DataStore.workerCache[prefab.name] = workers;
            }
            else
            {
                // Prefab doesn't already have a record; create.
                DataStore.workerCache.Add(prefab.name, workers);
            }

            // Save the updated configuration files.
            ConfigUtils.SaveSettings();

            // Get current building hash (for updating prefab dictionary).
            var prefabHash = prefab.gameObject.GetHashCode();

            // Calculate employment breakdown.
            int[] array = CommercialBuildingAIMod.GetArray(prefab, (int)prefab.GetClassLevel());
            PrefabEmployStruct output = new PrefabEmployStruct();
            AI_Utils.CalculateprefabWorkerVisit(prefab.GetWidth(), prefab.GetLength(), ref prefab, 4, ref array, out output);

            // Update entry in 'live' settings.
            if (DataStore.prefabWorkerVisit.ContainsKey(prefabHash))
            {
                // Prefab already has a record; update.
                DataStore.prefabWorkerVisit[prefabHash] = output;
            }
            else
            {
                // Prefab doesn't already have a record; create.
                DataStore.prefabWorkerVisit.Add(prefabHash, output);
            }
        }


        /// <summary>
        /// Removes the custom household record (if any) for a given prefab.
        /// </summary>
        /// <param name="prefab">The prefab (BuildingInfo) to remove the record from</param>
        public static void RemoveWorker(BuildingInfo prefab)
        {
            // Remove the entry from the configuration file cache.
            DataStore.workerCache.Remove(prefab.name);

            // Save the updated configuration files.
            XMLUtilsWG.WriteToXML();
            ConfigUtils.SaveSettings();

            // Remove current building's record from 'live' dictionary.
            DataStore.prefabWorkerVisit.Remove(prefab.gameObject.GetHashCode());
        }
    }
}
