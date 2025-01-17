﻿using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Harmony Postfix patch to toggle Realistic Population building info panel button visibility when building selection changes.
    /// </summary>
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
    internal static class BuildingPanelPatch
    {
        /// <summary>
        /// Harmony Postfix patch to toggle Realistic Population building info panel button visibility when building selection changes.
        /// </summary>
        public static void Postfix()
        {
            BuildingDetailsPanel.UpdateServicePanelButton();
        }
    }
}