using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
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

        // Tab width.
        protected override float TabWidth => 50f;


        // Panel components.
        private UISlider[] prodMultSliders;


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

        // <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal override void UpdateMenus()
        {
            base.UpdateMenus();

            // Reset sliders and menus.
            for (int i = 0; i < prodMultSliders.Length; ++i)
            {
                // Reset production multiplier slider values.
                prodMultSliders[i].value = RealisticOfficeProduction.GetProdMult(subServices[i]);
            }
        }


        /// <summary>
        /// Adds any additional controls to each row.
        /// </summary>
        /// <param name="panel">Panel reference</param>
        /// <param name="yPos">Relative Y position at top of row items</param>
        /// <param name="index">Index number of this row</param>
        /// <returns>Relative Y coordinate adjusted for any finished setup</returns>
        protected override float RowAdditions(UIPanel panel, float yPos, int index)
        {
            // Layout constants.
            float controlWidth = panel.width - RowAdditionX;


            float currentY = yPos - RowHeight;

            // Header label.
            UIControls.AddLabel(panel, RowAdditionX, currentY - 19f, Translations.Translate("RPR_DEF_PRD"), -1, 0.8f);

            // RowAdditions is called as part of parent constructor, so we need to initialise them here if they aren't already.
            if (prodMultSliders == null)
            {
                prodMultSliders = new UISlider[subServices.Length];
            }

            // Production multiplication slider.
            prodMultSliders[index] = AddSlider(panel, RowAdditionX, currentY, controlWidth);
            prodMultSliders[index].objectUserData = index;
            prodMultSliders[index].maxValue = RealisticOfficeProduction.MaxProdMult;
            prodMultSliders[index].value = RealisticOfficeProduction.GetProdMult(subServices[index]);
            prodMultSliders[index].tooltipBox = TooltipUtils.TooltipBox;
            prodMultSliders[index].tooltip = Translations.Translate("RPR_DEF_PRD_TIP");
            MultSliderText(prodMultSliders[index], prodMultSliders[index].value);

            return yPos;
        }


        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void Apply(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Iterate through all subservices.
            for (int i = 0; i < subServices.Length; ++i)
            {
                // Record production mutltiplier.
                RealisticOfficeProduction.SetProdMult(subServices[i], (int)prodMultSliders[i].value);
            }

            base.Apply(control, mouseEvent);
        }


        /// <summary>
        /// 'Revert to defaults' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void ResetDefaults(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            base.ResetDefaults(control, mouseEvent);

            // Reset sliders.
            for (int i = 0; i < prodMultSliders.Length; ++i)
            {
                // Reset production multiplier slider value.
                prodMultSliders[i].value = RealisticOfficeProduction.DefaultOfficeMult;
            }
        }
    }
}