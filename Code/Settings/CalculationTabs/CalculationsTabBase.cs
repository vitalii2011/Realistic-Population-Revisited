using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel tab for calculations for a particular service group.
    /// </summary>
    internal abstract class CalculationsTabBase
    {
        // Status flag.
        protected bool isSetup = false;


        // Defaults panel reference.
        protected DefaultsPanelBase defaultsPanel;

        // Tab icons.
        protected abstract string[] IconNames { get; }
        protected abstract string[] AtlasNames { get; }
        protected abstract string Tooltip { get; }

        // Tab width.
        protected virtual float TabWidth => 100f;


        // Tab strip and button.
        protected UITabstrip childTabStrip;
        protected UIButton tabButton;


        /// <summary>
        /// Adds options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CalculationsTabBase(UITabstrip parentTabStrip, int tabIndex)
        {
            // Add tab and helper.
            UIPanel panel = PanelUtils.AddIconTab(parentTabStrip, Tooltip, tabIndex, IconNames, AtlasNames, out tabButton, TabWidth);

            // Add tabstrip.
            childTabStrip = panel.AddUIComponent<UITabstrip>();
            childTabStrip.relativePosition = new Vector3(0, 0);
            childTabStrip.size = new Vector2(744f, 725f);

            // Tab container (the panels underneath each tab).
            UITabContainer tabContainer = panel.AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0, 30f);
            tabContainer.size = new Vector3(744f, 720);
            childTabStrip.tabPages = tabContainer;

            // Event handler to set up child tabs when tab is first clicked.
            tabButton.eventClicked += (control, clickEvent) => Setup();
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


        /// <summary>
        /// Performs initial setup; called when panel first becomes visible.
        /// </summary>
        internal void Setup()
        {
            // Don't do anything if already set up.
            if (!isSetup)
            {
                // Perform initial setup.
                isSetup = true;
                Logging.Message("setting up ", this.GetType().ToString());

                // Set up child tabs and make sure first one is selected.
                AddTabs(childTabStrip);
                childTabStrip.selectedIndex = 0;
            }
        }
    }
}