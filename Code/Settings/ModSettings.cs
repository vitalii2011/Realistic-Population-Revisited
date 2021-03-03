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
        private static bool thisSaveLegacyRes = false;
        private static bool thisSaveLegacyWrk = false;

        // What's new notification version.
        internal static string whatsNewVersion = "0.0";
        internal static string whatsNewBeta = "";


        /// <summary>
        /// Handles current 'use legacy by default for residential' option changes.
        /// </summary>
        internal static bool ThisSaveLegacyRes
        {
            // Simple getter.
            get => thisSaveLegacyRes;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != thisSaveLegacyRes)
                {
                    // Yes - clear caches.
                    PopData.instance.householdCache.Clear();

                    // Update flag.
                    thisSaveLegacyRes = value;
                }
            }
        }


        /// <summary>
        /// Handles current 'use legacy by default for workplaces' option changes.
        /// </summary>
        internal static bool ThisSaveLegacyWrk
        {
            // Simple getter.
            get => thisSaveLegacyWrk;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != thisSaveLegacyWrk)
                {
                    // Yes - clear caches.
                    PopData.instance.workplaceCache.Clear();

                    // Clear RICO cache too.
                    if (ModUtils.ricoClearAllWorkplaces != null)
                    {
                        ModUtils.ricoClearAllWorkplaces.Invoke(null, null);
                    }

                    // Update flag.
                    thisSaveLegacyWrk = value;
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