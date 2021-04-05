using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting residential calculation options.
    /// </summary>
    internal class IndustrialTab : CalculationsTabBase
    {
        // Tab icons.
        private readonly string[] iconNames =
        {
            "ZoningIndustrial"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails"
        };

        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;
        protected override string Tooltip => Translations.Translate("RPR_CAT_IND");

        // Tab width.
        protected override float TabWidth => 40f;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal IndustrialTab(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Adds required sub-tabs.
        /// </summary>
        /// <param name="tabStrip">Tabstrip reference</param>
        protected override void AddTabs(UITabstrip tabStrip)
        {
            defaultsPanel = new IndDefaultsPanel(tabStrip, 0);
            new IndGoodsPanel(tabStrip, 1);
            new IndConsumptionPanel(tabStrip, 2);
        }
    }
}