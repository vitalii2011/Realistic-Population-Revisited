namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Static class to hold global mod settings.
    /// </summary>
    internal static class ModSettings
    {
        internal static bool defaultLegacy = false;
        internal static bool enableSchoolPop = true;
        internal static bool enableSchoolProperties = true;
        internal static float crimeMultiplier = 50f;

        private static float defaultSchoolMult = 1f;

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