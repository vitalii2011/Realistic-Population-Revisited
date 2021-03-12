using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal class ComDefaultsPanel : EmpDefaultsPanel
    {
        // Service/subservice arrays.
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_CLO"),
            Translations.Translate("RPR_CAT_CHI"),
            Translations.Translate("RPR_CAT_ORG"),
            Translations.Translate("RPR_CAT_LEI"),
            Translations.Translate("RPR_CAT_TOU")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.CommercialLow,
            ItemClass.SubService.CommercialHigh,
            ItemClass.SubService.CommercialEco,
            ItemClass.SubService.CommercialLeisure,
            ItemClass.SubService.CommercialTourist
        };

        private readonly string[] iconNames =
        {
            "ZoningCommercialLow",
            "ZoningCommercialHigh",
            "IconPolicyOrganic",
            "IconPolicyLeisure",
            "IconPolicyTourist"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails",
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Ingame",
        };

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;


        // Panel components.
        UIDropDown visitDefaultMenu;

        // Legacy settings references.
        protected override bool NewLegacyCategory { get => ModSettings.newSaveLegacyCom; set => ModSettings.newSaveLegacyCom = value; }
        protected override bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyCom; set => ModSettings.ThisSaveLegacyCom = value; }
        protected override string LegacyCheckLabel => "RPR_DEF_LGC";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal ComDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Adds any additional controls below the menu arrays but above button footers.
        /// </summary>
        /// <param name="yPos">Relative Y position</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
        protected override float AddAdditional(float yPos)
        {
            // Set y position.
            float currentY = yPos + 30f;

            visitDefaultMenu = UIControls.AddLabelledDropDown(panel, Margin, currentY, Translations.Translate("RPR_DEF_VIS"), 300f, true, Translations.Translate("RPR_DEF_VIS_TIP"));
            visitDefaultMenu.tooltipBox = TooltipUtils.TooltipBox;
            visitDefaultMenu.items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL")
            };

            // Inital selection.
            visitDefaultMenu.selectedIndex = ModSettings.comVisitsMode;

            // Add vertical space after.
            return currentY + visitDefaultMenu.height + 30f;
        }


        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void Apply(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Record vistis mode calculations.
            ModSettings.comVisitsMode = visitDefaultMenu.selectedIndex;

            base.Apply(control, mouseEvent);
        }
    }
}