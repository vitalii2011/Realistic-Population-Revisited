using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class CalculationsPanel
    {
        /// <summary>
        /// Adds education options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CalculationsPanel(UITabstrip parentTabStrip, int tabIndex)
        {
            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(parentTabStrip, Translations.Translate("RPR_PCK_NAM"), tabIndex);
            UIHelper helper = new UIHelper(panel);
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
            new DefaultsPanel(childTabStrip, 0);
            new PopulationPanel(childTabStrip, 1);
            new FloorPanel(childTabStrip, 2);
            new ConsumptionPanel(childTabStrip, 3);
            new LegacyPanel(childTabStrip, 4);

            // Change tab size and text scale (to differentiate from 'main' tabstrip).
            foreach (UIButton button in childTabStrip.components)
            {
                button.textScale = 0.8f;
                button.width = 100f;
            }
        }
    }
}