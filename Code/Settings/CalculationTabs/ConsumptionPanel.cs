using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class ConsumptionPanel
    {
        /// <summary>
        /// Adds education options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal ConsumptionPanel(UITabstrip parentTabStrip, int tabIndex)
        {
            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(parentTabStrip, Translations.Translate("RPR_OPT_CON"), tabIndex, out UIButton tabButton, 100f);
            panel.autoLayout = false;

            // Button size and text scale.
            tabButton.textScale = 0.7f;

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
            new ResidentialPanel(childTabStrip, 0);
            new IndustrialPanel(childTabStrip, 1);
            new CommercialPanel(childTabStrip, 2);
            new OfficePanel(childTabStrip, 3);

            // Change tab size and text scale (to differentiate from 'main' tabstrip).
            foreach (UIButton button in childTabStrip.components)
            {
                button.textScale = 0.8f;
                button.width = 100f;
            }
        }
    }
}