using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting office goods calculations.
    /// </summary>
    internal class OffDefaultsPanel : EmpDefaultsPanel
    {
        // Service/subservice arrays.
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_OFF"),
            Translations.Translate("RPR_CAT_ITC")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Office,
            ItemClass.Service.Office
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.OfficeGeneric,
            ItemClass.SubService.OfficeHightech
        };

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

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;


        // Title key.
        protected override string TitleKey => "RPR_TIT_ODF";


        // Legacy settings references.
        protected override bool NewLegacyCategory { get => ModSettings.newSaveLegacyOff; set => ModSettings.newSaveLegacyOff = value; }
        protected override bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyOff; set => ModSettings.ThisSaveLegacyOff = value; }
        protected override string LegacyCheckLabel => "RPR_DEF_LGO";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal OffDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }
    }
}