using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class ConsumptionPanel
    {
        private readonly string[] iconNames =
        {
            "ToolbarIconElectricity",
            "ToolbarIconWaterAndSewage",
            "InfoIconGarbage",
            "InfoIconNoisePollution",
            "ToolbarIconMoney"
        };


        private readonly string[] atlasNames =
        {
            "ingame",
            "ingame",
            "ingame",
            "ingame",
            "ingame"
        };

        /// <summary>
        /// Adds education options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal ConsumptionPanel(UITabstrip parentTabStrip, int tabIndex)
        {
            // Add tab and helper.
            UIPanel panel = PanelUtils.AddIconTab(parentTabStrip, Translations.Translate("RPR_OPT_CON"), tabIndex, iconNames, atlasNames, 100f);

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
            new CommercialPanel(childTabStrip, 1);
            new IndustrialPanel(childTabStrip, 2);
            new OfficePanel(childTabStrip, 3);
        }
    }
}