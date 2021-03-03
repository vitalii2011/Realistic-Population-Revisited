using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal class EmpDefaultsPanel : DefaultsPanel
    {
        // Service/subservice arrays.
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_CLO"),
            Translations.Translate("RPR_CAT_CHI"),
            Translations.Translate("RPR_CAT_ORG"),
            Translations.Translate("RPR_CAT_LEI"),
            Translations.Translate("RPR_CAT_TOU"),
            Translations.Translate("RPR_CAT_OFF"),
            Translations.Translate("RPR_CAT_ITC"),
            Translations.Translate("RPR_CAT_IND"),
            Translations.Translate("RPR_CAT_FAR"),
            Translations.Translate("RPR_CAT_FOR"),
            Translations.Translate("RPR_CAT_OIL"),
            Translations.Translate("RPR_CAT_ORE"),
            Translations.Translate("RPR_CAT_SCH")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Office,
            ItemClass.Service.Office,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Education
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.CommercialLow,
            ItemClass.SubService.CommercialHigh,
            ItemClass.SubService.CommercialEco,
            ItemClass.SubService.CommercialLeisure,
            ItemClass.SubService.CommercialTourist,
            ItemClass.SubService.OfficeGeneric,
            ItemClass.SubService.OfficeHightech,
            ItemClass.SubService.IndustrialGeneric,
            ItemClass.SubService.IndustrialFarming,
            ItemClass.SubService.IndustrialForestry,
            ItemClass.SubService.IndustrialOil,
            ItemClass.SubService.IndustrialOre,
            ItemClass.SubService.None
        };

        private readonly string[] iconNames =
        {
            "ZoningCommercialLow",
            "ZoningCommercialHigh",
            "IconPolicyOrganic",
            "IconPolicyLeisure",
            "IconPolicyTourist",
            "ZoningOffice",
            "IconPolicyHightech",
            "ZoningIndustrial",
            "IconPolicyFarming",
            "IconPolicyForest",
            "IconPolicyOil",
            "IconPolicyOre",
            "ToolbarIconEducation"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails",
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Ingame",
            "Thumbnails",
            "Ingame",
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Ingame",
            "Ingame",
            "Ingame"
        };

        private readonly int[] tabIconIndexes =
        {
            0, 1, 5, 7
        };

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;
        protected override int[] TabIcons => tabIconIndexes;


        // Legacy settings link.
        protected override bool LegacyCategory { get => ModSettings.ThisSaveLegacyWrk; set => ModSettings.ThisSaveLegacyWrk = value; }

        // Translation key for legacy settings label.
        protected override string LegacyCheckLabel => "RPR_DEF_LGW";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal EmpDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Adds footer buttons to the panel.
        /// </summary>
        /// <param name="panel">Panel reference</param>
        /// <param name="yPos">Relative Y position for buttons</param>
        protected override void FooterButtons(UIPanel panel, float yPos)
        {
            base.FooterButtons(panel, yPos);

            // Save button.
            UIButton saveButton = UIControls.AddButton(panel, (Margin * 3) + 300f, yPos, Translations.Translate("RPR_OPT_SAA"), 150f);
            saveButton.eventClicked += Apply;
        }


        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        private void Apply(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Iterate through each sub-service menu.
            for (int i = 0; i < SubServiceNames.Length; ++i)
            {
                // Get population pack menu selected index.
                int popIndex = popMenus[i].selectedIndex;

                // Check to see if this is a change from the current default.
                if (PopData.instance.CurrentDefaultPack(Services[i], SubServices[i]).name.Equals(availablePopPacks[i][popIndex].name))
                {
                    // No change - continue.
                    continue;
                }

                // Update default population dictionary for this subservice.
                PopData.instance.ChangeDefault(Services[i], SubServices[i], availablePopPacks[i][popIndex]);

                // Update floor data pack if we're not using legacy calculations.
                if (availablePopPacks[i][popIndex].version != (int)DataVersion.legacy)
                {
                    FloorData.instance.ChangeDefault(Services[i], SubServices[i], availableFloorPacks[floorMenus[i].selectedIndex]);
                }
            }

            // Clear population caches.
            PopData.instance.householdCache.Clear();
            PopData.instance.workplaceCache.Clear();

            // Clear RICO cache.
            if (ModUtils.ricoClearAllWorkplaces != null)
            {
                ModUtils.ricoClearAllWorkplaces.Invoke(null, null);
            }

            // Save settings.
            ConfigUtils.SaveSettings();
        }
    }
}