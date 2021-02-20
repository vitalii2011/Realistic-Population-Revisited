namespace RealPop2
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
            if (DataStore.householdCache.TryGetValue(prefab.name, out int returnValue))
            {
                return returnValue;
            }
            return 0;
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
            PopData.instance.householdCache.Remove(prefab);
        }

        /// <summary>
        /// Returns the customised number of workers for a given prefab.
        /// Returns 0 if no custom settings exist.
        /// </summary>
        /// <param name="prefab">The custom worker count (0 if no settings)</param>
        /// <returns></returns>
        public static int GetWorker(BuildingInfo prefab)
        {
            if (DataStore.workerCache.TryGetValue(prefab.name, out int returnValue))
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

            // Remove current building's record from 'live' dictionary.
            PopData.instance.workplaceCache.Remove(prefab);
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
            PopData.instance.workplaceCache.Remove(prefab);
        }
    }
}
