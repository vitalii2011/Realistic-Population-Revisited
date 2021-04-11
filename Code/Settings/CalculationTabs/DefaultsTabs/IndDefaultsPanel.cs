using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal class IndDefaultsPanel : EmpDefaultsPanel
    {
        // Service/subservice arrays.
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_IND"),
            Translations.Translate("RPR_CAT_FAR"),
            Translations.Translate("RPR_CAT_FOR"),
            Translations.Translate("RPR_CAT_OIL"),
            Translations.Translate("RPR_CAT_ORE")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.IndustrialGeneric,
            ItemClass.SubService.IndustrialFarming,
            ItemClass.SubService.IndustrialForestry,
            ItemClass.SubService.IndustrialOil,
            ItemClass.SubService.IndustrialOre
        };

        private readonly string[] iconNames =
        {
            "ZoningIndustrial",
            "IconPolicyFarming",
            "IconPolicyForest",
            "IconPolicyOil",
            "IconPolicyOre"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Ingame",
            "Ingame"
        };


        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;


        // Title key.
        protected override string TitleKey => "RPR_TIT_IDF";


        // Legacy settings references.
        protected override bool NewLegacyCategory { get => ModSettings.newSaveLegacyInd; set => ModSettings.newSaveLegacyInd = value; }
        protected override bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyInd; set => ModSettings.ThisSaveLegacyInd = value; }
        protected override string LegacyCheckLabel => "RPR_DEF_LGI";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal IndDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }
    }
}