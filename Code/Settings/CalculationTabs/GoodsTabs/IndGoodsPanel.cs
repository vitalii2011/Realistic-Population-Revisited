using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting industry goods calculations.
    /// </summary>
    internal class IndGoodsPanel : GoodsPanelBase
    {
        // Service/subservice arrays.
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_IND")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Industrial
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.IndustrialGeneric
        };

        private readonly string[] iconNames =
        {
            "ZoningIndustrial"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails"
        };

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;


        // Panel components.
        private UISlider[] prodMultSliders;
        private UIDropDown[] prodDefaultMenus;


        /// <summary>
        /// Legacy settings link.
        /// </summary>
        protected bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyInd; set => ModSettings.ThisSaveLegacyInd = value; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal IndGoodsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        // <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal override void UpdateControls()
        {
            base.UpdateControls();

            // Reset sliders and menus.
            for (int i = 0; i < prodMultSliders.Length; ++i)
            {
                // Reset visit multiplier slider values.
                prodMultSliders[i].value = RealisticIndustrialProduction.GetProdMult(ItemClass.SubService.IndustrialGeneric);

                // Reset visit mode menu selections.
                prodDefaultMenus[i].selectedIndex = RealisticIndustrialProduction.GetProdMode(ItemClass.SubService.IndustrialGeneric);
            }
        }


        /// <summary>
        /// Adds controls for each sub-service.
        /// </summary>
        /// <param name="yPos">Relative Y position at top of row items</param>
        /// <param name="index">Index number of this row</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
        protected override float SubServiceControls(float yPos, int index)
        {
            // SubServiceControls is called as part of parent constructor, so we need to initialise them here if they aren't already.
            if (prodMultSliders == null)
            {
                prodMultSliders = new UISlider[subServices.Length];
                prodDefaultMenus = new UIDropDown[subServices.Length];
            }

            // Production mode menus.
            float currentY = yPos;
            prodDefaultMenus[index] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_DEF_PMD"), ControlWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false, tooltip: Translations.Translate("RPR_DEF_PMD_TIP"));
            prodDefaultMenus[index].tooltipBox = TooltipUtils.TooltipBox;
            prodDefaultMenus[index].objectUserData = index;
            prodDefaultMenus[index].items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL")
            };

            // Production multiplication sliders.
            currentY += RowHeight;
            prodMultSliders[index] = AddSlider(panel, LeftColumn, currentY, ControlWidth, "RPR_DEF_PRD_TIP");
            prodMultSliders[index].objectUserData = index;
            prodMultSliders[index].maxValue = RealisticIndustrialProduction.MaxProdMult;
            prodMultSliders[index].value = RealisticIndustrialProduction.GetProdMult(ItemClass.SubService.IndustrialGeneric);
            PercentSliderText(prodMultSliders[index], prodMultSliders[index].value);

            // Production multiplier label.
            UILabel multiplierLabel = UIControls.AddLabel(panel, 0f, 0f, Translations.Translate("RPR_DEF_PRD"), textScale: 0.8f);
            multiplierLabel.relativePosition = new UnityEngine.Vector2(LeftColumn - 10f - multiplierLabel.width, currentY + (prodMultSliders[index].parent.height - multiplierLabel.height) / 2f);

            // Production calculation mode default event handler to show/hide multiplier slider.
            prodDefaultMenus[index].eventSelectedIndexChanged += ProdDefaultIndexChanged;

            // Set prodution calculation mode initial selection.
            prodDefaultMenus[index].selectedIndex = RealisticIndustrialProduction.GetProdMode(ItemClass.SubService.IndustrialGeneric);

            return currentY;
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
                // Record production calculation modes.
                RealisticIndustrialProduction.SetProdMode(ItemClass.SubService.IndustrialGeneric, prodDefaultMenus[i].selectedIndex);

                // Record production multiplier.
                RealisticIndustrialProduction.SetProdMult(ItemClass.SubService.IndustrialGeneric, (int)prodMultSliders[i].value);
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
            // Reset sliders and menus.
            for (int i = 0; i < prodMultSliders.Length; ++i)
            {
                // Reset production multiplier slider value.
                prodMultSliders[i].value = RealisticIndustrialProduction.DefaultProdMult;

                // Reset visit mode menu selection.
                prodDefaultMenus[i].selectedIndex = ThisLegacyCategory ? (int)RealisticIndustrialProduction.ProdModes.legacy : (int)RealisticIndustrialProduction.ProdModes.popCalcs;
            }
        }


        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void ResetSaved(UIComponent control, UIMouseEventParameter mouseEvent) => UpdateControls();


        /// <summary>
        /// Production mode menu index changed event handler.
        /// <param name="control">Calling component</param>
        /// <param name="index">New selected index</param>
        /// </summary>
        private void ProdDefaultIndexChanged(UIComponent control, int index)
        {
            // Extract subservice index from this control's object user data.
            if (control.objectUserData is int subServiceIndex)
            {
                // Toggle multiplier slider visibility based on current state.
                prodMultSliders[subServiceIndex].parent.isVisible = index == (int)RealisticIndustrialProduction.ProdModes.popCalcs;
            }
        }
    }
}