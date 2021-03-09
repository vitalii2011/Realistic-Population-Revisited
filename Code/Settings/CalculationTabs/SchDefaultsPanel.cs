using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal class SchDefaultsPanel : DefaultsPanel
    {
        // Service/subservice arrays.
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_SCH")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Education
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.None
        };

        private readonly string[] iconNames =
        {
            "ToolbarIconEducation"
        };

        private readonly string[] atlasNames =
        {
            "Ingame"
        };

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;

        // Tab width.
        protected override float TabWidth => 50f;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal SchDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }
    }
}