using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting extractor goods calculations.
    /// </summary>
    internal class ExtGoodsPanel : GoodsPanelBase
    {
        // Service/subservice arrays.
        private readonly string[] subServiceNames =
        {
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
            ItemClass.Service.Industrial
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.IndustrialFarming,
            ItemClass.SubService.IndustrialForestry,
            ItemClass.SubService.IndustrialOil,
            ItemClass.SubService.IndustrialOre,
        };

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

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;


        // Title key.
        protected override string TitleKey => "RPR_TIT_EGO";


        // Panel components.
        private UISlider[] extProdMultSliders, procProdMultSliders;
        private UIDropDown[] extProdDefaultMenus, procProdDefaultMenus;


        /// <summary>
        /// Legacy settings link.
        /// </summary>
        protected bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyExt; set => ModSettings.ThisSaveLegacyExt = value; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal ExtGoodsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        // <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal override void UpdateControls()
        {
            base.UpdateControls();

            // Reset sliders and menus.
            for (int i = 0; i < extProdMultSliders.Length; ++i)
            {
                // Reset visit multiplier slider values.
                extProdMultSliders[i].value = RealisticExtractorProduction.GetProdMult(subServices[i]);

                // Reset visit mode menu selections.
                extProdDefaultMenus[i].selectedIndex = RealisticExtractorProduction.GetProdMode(subServices[i]);
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
            float currentY = yPos;

            // Header labels.
            UIControls.AddLabel(panel, LeftColumn, currentY - 19f, Translations.Translate("RPR_DEF_PMD"), -1, 0.8f);
            UIControls.AddLabel(panel, RightColumn, currentY - 19f, Translations.Translate("RPR_DEF_PRD"), -1, 0.8f);

            // SubServiceControls is called as part of parent constructor, so we need to initialise them here if they aren't already.
            if (extProdMultSliders == null)
            {
                extProdMultSliders = new UISlider[subServices.Length];
                procProdMultSliders = new UISlider[subServices.Length];
                extProdDefaultMenus = new UIDropDown[subServices.Length];
                procProdDefaultMenus = new UIDropDown[subServices.Length];
            }

            // Extractor production mode menus.
            extProdDefaultMenus[index] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAT_EXT"), ControlWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false, tooltip: Translations.Translate("RPR_DEF_PMD_TIP"));
            extProdDefaultMenus[index].tooltipBox = TooltipUtils.TooltipBox;
            extProdDefaultMenus[index].objectUserData = index;
            extProdDefaultMenus[index].items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL")
            };

            // Extractor production multiplication sliders.
            extProdMultSliders[index] = AddSlider(panel, RightColumn, currentY, ControlWidth, "RPR_DEF_PRD_TIP");
            extProdMultSliders[index].objectUserData = index;
            extProdMultSliders[index].maxValue = RealisticExtractorProduction.MaxProdMult;
            extProdMultSliders[index].value = RealisticExtractorProduction.GetProdMult(subServices[index]);
            PercentSliderText(extProdMultSliders[index], extProdMultSliders[index].value);

            // Processor production mode menus.
            currentY += RowHeight;
            procProdDefaultMenus[index] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAT_PRO"), ControlWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false, tooltip: Translations.Translate("RPR_DEF_PMD_TIP"));
            procProdDefaultMenus[index].tooltipBox = TooltipUtils.TooltipBox;
            procProdDefaultMenus[index].objectUserData = index;
            procProdDefaultMenus[index].items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL")
            };

            // Processor production multiplication sliders.
            procProdMultSliders[index] = AddSlider(panel, RightColumn, currentY, ControlWidth, "RPR_DEF_PRD_TIP");
            procProdMultSliders[index].objectUserData = index;
            procProdMultSliders[index].maxValue = RealisticIndustrialProduction.MaxProdMult;
            procProdMultSliders[index].value = RealisticIndustrialProduction.GetProdMult(subServices[index]);
            PercentSliderText(procProdMultSliders[index], procProdMultSliders[index].value);

            // Production calculation mode default event handlers to show/hide multiplier slider.
            extProdDefaultMenus[index].eventSelectedIndexChanged += ExtProdDefaultIndexChanged;
            procProdDefaultMenus[index].eventSelectedIndexChanged += ProcProdDefaultIndexChanged;

            // Set prodution calculation mode initial selection.
            extProdDefaultMenus[index].selectedIndex = RealisticExtractorProduction.GetProdMode(subServices[index]);
            procProdDefaultMenus[index].selectedIndex = RealisticIndustrialProduction.GetProdMode(subServices[index]);

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
                RealisticExtractorProduction.SetProdMode(subServices[i], extProdDefaultMenus[i].selectedIndex);
                RealisticIndustrialProduction.SetProdMode(subServices[i], procProdDefaultMenus[i].selectedIndex);

                // Record production multiplier.
                RealisticExtractorProduction.SetProdMult(subServices[i], (int)extProdMultSliders[i].value);
                RealisticIndustrialProduction.SetProdMult(subServices[i], (int)procProdMultSliders[i].value);
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
            for (int i = 0; i < extProdMultSliders.Length; ++i)
            {
                // Reset production multiplier slider value.
                extProdMultSliders[i].value = RealisticExtractorProduction.DefaultProdMult;
                procProdMultSliders[i].value = RealisticIndustrialProduction.DefaultProdMult;

                // Reset visit mode menu selection.
                extProdDefaultMenus[i].selectedIndex = ThisLegacyCategory ? (int)RealisticExtractorProduction.ProdModes.legacy : (int)RealisticExtractorProduction.ProdModes.popCalcs;
                procProdDefaultMenus[i].selectedIndex = ThisLegacyCategory ? (int)RealisticIndustrialProduction.ProdModes.legacy : (int)RealisticIndustrialProduction.ProdModes.popCalcs;
            }
        }


        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void ResetSaved(UIComponent control, UIMouseEventParameter mouseEvent) => UpdateControls();


        /// <summary>
        /// Extractor production mode menu index changed event handler.
        /// <param name="control">Calling component</param>
        /// <param name="index">New selected index</param>
        /// </summary>
        private void ExtProdDefaultIndexChanged(UIComponent control, int index)
        {
            // Extract subservice index from this control's object user data.
            if (control.objectUserData is int subServiceIndex)
            {
                // Toggle multiplier slider visibility based on current state.
                extProdMultSliders[subServiceIndex].parent.isVisible = index == (int)RealisticExtractorProduction.ProdModes.popCalcs;
            }
        }


        /// <summary>
        /// Processor production mode menu index changed event handler.
        /// <param name="control">Calling component</param>
        /// <param name="index">New selected index</param>
        /// </summary>
        private void ProcProdDefaultIndexChanged(UIComponent control, int index)
        {
            // Extract subservice index from this control's object user data.
            if (control.objectUserData is int subServiceIndex)
            {
                // Toggle multiplier slider visibility based on current state.
                procProdMultSliders[subServiceIndex].parent.isVisible = index == (int)RealisticIndustrialProduction.ProdModes.popCalcs;
            }
        }
    }
}