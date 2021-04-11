using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class CalculationsPanel
    {
        // Instance reference.
        internal static CalculationsPanel Instance { get; private set; }


        // Components.
        private readonly ResidentialTab resTab;
        private readonly CommercialTab comTab;
        private readonly OfficeTab offTab;
        private readonly IndustrialTab indTab;
        private readonly ExtractorTab extTab;
        private readonly SchoolTab schTab;


        /// <summary>
        /// Adds education options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CalculationsPanel(UITabstrip parentTabStrip, int tabIndex)
        {
            // Instance reference.
            Instance = this;

            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTextTab(parentTabStrip, Translations.Translate("RPR_PCK_NAM"), tabIndex, out UIButton _);
            panel.autoLayout = false;

            // Add tabstrip.
            UITabstrip childTabStrip = panel.AddUIComponent<UITabstrip>();
            childTabStrip.relativePosition = new Vector3(0, 0);
            childTabStrip.size = new Vector2(744f, 725f);

            // Tab container (the panels underneath each tab).
            UITabContainer tabContainer = panel.AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0, 30f);
            tabContainer.size = new Vector3(744f, 720);
            childTabStrip.tabPages = tabContainer;

            // Add child tabs.
            int tab = 0;
            resTab = new ResidentialTab(childTabStrip, tab++);
            comTab = new CommercialTab(childTabStrip, tab++);
            offTab = new OfficeTab(childTabStrip, tab++);
            indTab = new IndustrialTab(childTabStrip, tab++);
            schTab = new SchoolTab(childTabStrip, tab++);
            new PopulationPanel(childTabStrip, tab++);
            new FloorPanel(childTabStrip, tab++);
            new LegacyPanel(childTabStrip, tab);
        }


        /// <summary>
        /// Updates default calculation pack selection menu options.
        /// </summary>
        internal void UpdateDefaultMenus()
        {
            // Update for each defaults panel.
            resTab.UpdateControls();
            comTab.UpdateControls();
            offTab.UpdateControls();
            indTab.UpdateControls();
            schTab.UpdateControls();
        }
    }
}