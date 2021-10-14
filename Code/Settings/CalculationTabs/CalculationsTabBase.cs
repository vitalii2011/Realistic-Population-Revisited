using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel tab for calculations for a particular service group.
    /// </summary>
    internal abstract class CalculationsTabBase
    {
        // Defaults panel reference.
        protected DefaultsPanelBase defaultsPanel;

        // Tab icons.
        protected abstract string[] IconNames { get; }
        protected abstract string[] AtlasNames { get; }
        protected abstract string Tooltip { get; }

        // Tab width.
        protected virtual float TabWidth => 100f;



        /// <summary>
        /// Adds options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CalculationsTabBase(UITabstrip parentTabStrip, int tabIndex)
        {
            // Add tab and helper.
            UIPanel panel = PanelUtils.AddIconTab(parentTabStrip, Tooltip, tabIndex, IconNames, AtlasNames, out UIButton _, TabWidth);

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
            AddTabs(childTabStrip);
        }


        /// <summary>
        /// Updates control values for relevant defaults panel.
        /// </summary>
        internal void UpdateControls() => defaultsPanel?.UpdateControls();


        /// <summary>
        /// Adds required sub-tabs.
        /// </summary>
        /// <param name="tabStrip">Tabstrip reference</param>
        protected abstract void AddTabs(UITabstrip tabStrip);
    }
}