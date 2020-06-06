using UnityEngine;
using ICities;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Class to handle the mod settings options panel.
    /// </summary>
    internal class OptionsPanel
    {
        // Settings file.
        internal static SettingsFile settings;


        /// <summary>
        /// Options panel constructor.
        /// </summary>
        /// <param name="helper">UIHelperBase parent</param>
        internal OptionsPanel(UIHelperBase helper)
        {
            // Load settings.
            settings = Configuration<SettingsFile>.Load();

            // Set up tab strip and containers.
            UIScrollablePanel optionsPanel = ((UIHelper)helper).self as UIScrollablePanel;
            optionsPanel.autoLayout = false;

            UITabstrip tabStrip = optionsPanel.AddUIComponent<UITabstrip>();
            tabStrip.relativePosition = new Vector3(0, 0);
            tabStrip.size = new Vector2(744, 713);

            UITabContainer tabContainer = optionsPanel.AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0, 40);
            tabContainer.size = new Vector3(744, 713);
            tabStrip.tabPages = tabContainer;

            // Populate Datastore from configuration file.
            XMLUtils.ReadFromXML();

            // Add tabs and panels.
            new ModOptionsPanel(tabStrip, 0);
            ResidentialPanel residentialPanel = new ResidentialPanel(tabStrip, 1);
            IndustrialPanel industrialPanel = new IndustrialPanel(tabStrip, 2);
            CommercialPanel commercialPanel = new CommercialPanel(tabStrip, 3);
            OfficePanel officePanel = new OfficePanel(tabStrip, 4);
        }
    }
}