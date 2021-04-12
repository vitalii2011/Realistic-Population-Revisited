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
        protected override string TitleKey => "RPR_TIT_IGO";


        // Panel components.
        private UISlider[] procProdMultSliders, extProdMultSliders;
        private UIDropDown[] procProdModeMenus, extProdModeMenus;


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
            for (int i = 0; i < procProdMultSliders.Length; ++i)
            {
                // Reset visit multiplier slider values.
                procProdMultSliders[i].value = RealisticIndustrialProduction.GetProdMult(subServices[i]);
                extProdMultSliders[i].value = RealisticExtractorProduction.GetProdMult(subServices[i]);

                // Reset visit mode menu selections.
                procProdModeMenus[i].selectedIndex = RealisticIndustrialProduction.GetProdMode(subServices[i]);
                extProdModeMenus[i].selectedIndex = RealisticExtractorProduction.GetProdMode(subServices[i]);
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
            if (procProdMultSliders == null)
            {
                procProdModeMenus = new UIDropDown[subServices.Length];
                extProdModeMenus = new UIDropDown[subServices.Length];
                procProdMultSliders = new UISlider[subServices.Length];
                extProdMultSliders = new UISlider[subServices.Length];
            }

            // Processor production mode menus.
            procProdModeMenus[index] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAT_PRO"), ControlWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false, tooltip: Translations.Translate("RPR_DEF_PMD_TIP"));
            procProdModeMenus[index].tooltipBox = TooltipUtils.TooltipBox;
            procProdModeMenus[index].objectUserData = index;
            procProdModeMenus[index].items = new string[]
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

            // Extractor production mode menus.
            currentY += RowHeight;
            extProdModeMenus[index] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAT_EXT"), ControlWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false, tooltip: Translations.Translate("RPR_DEF_PMD_TIP"));
            extProdModeMenus[index].tooltipBox = TooltipUtils.TooltipBox;
            extProdModeMenus[index].objectUserData = index;
            extProdModeMenus[index].items = new string[]
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

            // Always hide generic industrial extractor (index 0) controls.
            if (index == 0)
            {
                extProdModeMenus[0].Hide();
                extProdMultSliders[0].Hide();
            }

            // Production calculation mode default event handlers to show/hide multiplier slider.
            procProdModeMenus[index].eventSelectedIndexChanged += ProcProdDefaultIndexChanged;
            extProdModeMenus[index].eventSelectedIndexChanged += ExtProdDefaultIndexChanged;

            // Set prodution calculation mode initial selection.
            procProdModeMenus[index].selectedIndex = RealisticIndustrialProduction.GetProdMode(subServices[index]);
            extProdModeMenus[index].selectedIndex = RealisticExtractorProduction.GetProdMode(subServices[index]);

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
                RealisticIndustrialProduction.SetProdMode(subServices[i], procProdModeMenus[i].selectedIndex);
                RealisticExtractorProduction.SetProdMode(subServices[i], extProdModeMenus[i].selectedIndex);

                // Record production multipliers.
                RealisticIndustrialProduction.SetProdMult(subServices[i], (int)procProdMultSliders[i].value);
                RealisticExtractorProduction.SetProdMult(subServices[i], (int)extProdMultSliders[i].value);
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
            for (int i = 0; i < procProdMultSliders.Length; ++i)
            {
                // Reset production multiplier slider value.
                extProdMultSliders[i].value = RealisticExtractorProduction.DefaultProdMult;
                procProdMultSliders[i].value = RealisticIndustrialProduction.DefaultProdMult;

                // Reset visit mode menu selection.
                procProdModeMenus[i].selectedIndex = ThisLegacyCategory ? (int)RealisticIndustrialProduction.ProdModes.legacy : (int)RealisticIndustrialProduction.ProdModes.popCalcs;
                extProdModeMenus[i].selectedIndex = ThisLegacyCategory ? (int)RealisticExtractorProduction.ProdModes.legacy : (int)RealisticExtractorProduction.ProdModes.popCalcs;
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
            // Extract subservice index from this control's object user data - 0 is generic industrial, for which the extractor controls are always hidden.
            if (control.objectUserData is int subServiceIndex && subServiceIndex != 0)
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