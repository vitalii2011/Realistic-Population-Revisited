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
        private readonly ResDefaultsPanel resDefaults;
        private readonly EmpDefaultsPanel comDefaults, offDefaults, indDefaults;
        private readonly SchDefaultsPanel schDefaults;


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
            UIPanel panel = PanelUtils.AddTab(parentTabStrip, Translations.Translate("RPR_PCK_NAM"), tabIndex);
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
            resDefaults = new ResDefaultsPanel(childTabStrip, tab++);
            comDefaults = new ComDefaultsPanel(childTabStrip, tab++);
            indDefaults = new IndDefaultsPanel(childTabStrip, tab++);
            offDefaults = new OffDefaultsPanel(childTabStrip, tab++);
            schDefaults = new SchDefaultsPanel(childTabStrip, tab++);
            new PopulationPanel(childTabStrip, tab++);
            new FloorPanel(childTabStrip, tab++);
            new ConsumptionPanel(childTabStrip, tab++);
            new LegacyPanel(childTabStrip, tab);
        }


        /// <summary>
        /// Updates default calculation pack selection menu options.
        /// </summary>
        internal void UpdateDefaultMenus()
        {
            // Update for each defaults panel.
            resDefaults.UpdateMenus();
            comDefaults.UpdateMenus();
            offDefaults.UpdateMenus();
            indDefaults.UpdateMenus();
            schDefaults.UpdateMenus();
        }
    }
}