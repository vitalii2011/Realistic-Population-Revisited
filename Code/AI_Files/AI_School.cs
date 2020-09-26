using HarmonyLib;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Harmony patch for realistic student counts.
    /// </summary>
    [HarmonyPatch(typeof(SchoolAI), "StudentCount", MethodType.Getter)]
    public static class StudentCountPatch
    {
        /// <summary>
        /// Harmony SchoolAI.StudentCount Prefix getter patch to return realistic student counts (if enabled).
        /// </summary>
        /// <param name="__instance">SchoolAI instance</param>
        /// <param name="__result">Method result</param>
        /// <returns></returns>
        public static bool Prefix(SchoolAI __instance, ref int __result)
        {
            // Check to see if we're using realistic school populations.
            if (ModSettings.enableSchools)
            {
                // We are - set the result to our realistic population lookup.
                __result = PopData.Population(__instance.m_info, (int)__instance.m_info.GetClassLevel());

                // Don't continue on to original method.
                return false;
            }

            // Not using realistic school populations - continue on to original method.
            return true;
        }
    }
}