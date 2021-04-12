using System.Collections.Generic;


namespace RealPop2
{
    /// <summary>
    /// Static class to hold global mod settings.
    /// </summary>
    internal static class ModSettings
    {
        // Legacy settings.
        private static bool thisSaveLegacyRes = false;
        private static bool thisSaveLegacyCom = false;
        private static bool thisSaveLegacyInd = false;
        private static bool thisSaveLegacyOff = false;
        internal static bool newSaveLegacyRes = false;
        internal static bool newSaveLegacyCom = false;
        internal static bool newSaveLegacyInd = false;
        internal static bool newSaveLegacyExt = false;
        internal static bool newSaveLegacyOff = false;

        // Enable additional features.
        internal static bool enableSchoolPop = true;
        internal static bool enableSchoolProperties = true;
        internal static float crimeMultiplier = 50f;

        // Status flags.
        internal static bool isRealPop2Save = false;
        private static float defaultSchoolMult = 1f;

        // What's new notification version.
        internal static string whatsNewVersion = "0.0";
        internal static int whatsNewBetaVersion = 0;

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
        /// Handles current 'use legacy by default for commercial' option changes.
        /// </summary>
        internal static bool ThisSaveLegacyCom
        {
            // Simple getter.
            get => thisSaveLegacyCom;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != thisSaveLegacyCom)
                {
                    // Yes - clear caches.
                    ClearWorkplaceCaches();
                    // Update flag.
                    thisSaveLegacyCom = value;
                }
            }
        }


        /// <summary>
        /// Handles current 'use legacy by default for industrial' option changes.
        /// </summary>
        internal static bool ThisSaveLegacyInd
        {
            // Simple getter.
            get => thisSaveLegacyInd;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != thisSaveLegacyInd)
                {
                    // Yes - clear caches.
                    ClearWorkplaceCaches();
                    // Update flag.
                    thisSaveLegacyInd = value;
                }
            }
        }


        /// <summary>
        /// Handles current 'use legacy by default for industiral' option changes.
        /// </summary>
        internal static bool ThisSaveLegacyOff
        {
            // Simple getter.
            get => thisSaveLegacyOff;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != thisSaveLegacyOff)
                {
                    // Yes - clear caches.
                    ClearWorkplaceCaches();
                    // Update flag.
                    thisSaveLegacyOff = value;
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


        /// <summary>
        /// Clears all workplace caches.
        /// </summary>
        private static void ClearWorkplaceCaches()
        {
            //Clear workplace cache.
            PopData.instance.workplaceCache.Clear();

            // Clear RICO cache too.
            if (ModUtils.ricoClearAllWorkplaces != null)
            {
                ModUtils.ricoClearAllWorkplaces.Invoke(null, null);
            }
        }
    }
}