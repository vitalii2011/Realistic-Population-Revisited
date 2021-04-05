using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class ExtractorTab : CalculationsTabBase
    {
        // Tab icons.
        private readonly string[] iconNames =
        {
            "IconPolicyFarming",
            "IconPolicyForest",
            "IconPolicyOil",
            "IconPolicyOre",
        };

        private readonly string[] atlasNames =
        {
            "Ingame",
            "Ingame",
            "Ingame",
            "Ingame",
        };

        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;
        protected override string Tooltip => Translations.Translate("RPR_CAT_SIN");


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal ExtractorTab(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Adds required sub-tabs.
        /// </summary>
        /// <param name="tabStrip">Tabstrip reference</param>
        protected override void AddTabs(UITabstrip tabStrip)
        {
            defaultsPanel = new ExtDefaultsPanel(tabStrip, 0);
            new ExtGoodsPanel(tabStrip, 1);
            //new ExtractorConsumptionPanel(tabStrip, 1);
        }
    }
}