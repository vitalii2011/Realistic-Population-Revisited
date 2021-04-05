using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting residential calculation options.
    /// </summary>
    internal class OfficeTab : CalculationsTabBase
    {
        // Tab icons.
        private readonly string[] iconNames =
        {
            "ZoningOffice",
            "IconPolicyHightech"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails",
            "Ingame"
        };

        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;
        protected override string Tooltip => Translations.Translate("RPR_CAT_OFF");

        // Tab width.
        protected override float TabWidth => 50f;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal OfficeTab(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Adds required sub-tabs.
        /// </summary>
        /// <param name="tabStrip">Tabstrip reference</param>
        protected override void AddTabs(UITabstrip tabStrip)
        {
            defaultsPanel = new OffDefaultsPanel(tabStrip, 0);
            new OffGoodsPanel(tabStrip, 1);
            new OffConsumptionPanel(tabStrip, 2);
        }
    }
}