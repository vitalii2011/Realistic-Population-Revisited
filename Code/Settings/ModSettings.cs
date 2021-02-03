namespace RealPop2
{
    /// <summary>
    /// Static class to hold global mod settings.
    /// </summary>
    internal static class ModSettings
    {
        // User settings.
        internal static bool newSaveLegacy = false;
        internal static bool enableSchoolPop = true;
        internal static bool enableSchoolProperties = true;
        internal static float crimeMultiplier = 50f;

        // Status flags.
        internal static bool isRealPop2Save = false;
        private static float defaultSchoolMult = 1f;
        private static bool thisSaveLegacy = false;

        // What's new notification version.
        internal static string whatsNewVersion = "0.0";
        internal static string whatsNewBeta = "";


        /// <summary>
        /// Handles current 'use legacy by default' option changes.
        /// </summary>
        internal static bool ThisSaveLegacy
        {
            // Simple getter.
            get => thisSaveLegacy;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != thisSaveLegacy)
                {
                    // Yes - clear caches.
                    DataStore.prefabHouseHolds.Clear();
                    DataStore.prefabWorkerVisit.Clear();

                    // Clear RICO cache too.
                    if (ModUtils.ricoClearAllWorkplaces != null)
                    {
                        ModUtils.ricoClearAllWorkplaces.Invoke(null, null);
                    }    

                    // Update flag.
                    thisSaveLegacy = value;
                }
            }
        }


        /// <summary>
        /// Handles current default multiplier for schools.
        /// </summary>
        internal static float DefaultSchoolMult
        {
            // Simple getter.
            get => defaultSchoolMult;

            // Setter needs to update schools if SchoolData instance is loaded (i.e. after game load), otherwise don't.
            set
            {
                defaultSchoolMult = value;
                if (SchoolData.instance != null)
                {
                    SchoolData.instance.UpdateSchools();
                }
            }
        }
    }
}