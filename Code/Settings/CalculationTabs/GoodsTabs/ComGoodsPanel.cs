using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting commercial goods calculations.
    /// </summary>
    internal class ComGoodsPanel : GoodsPanelBase
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
        private UIDropDown[] visitDefaultMenus;
        private UISlider[] visitMultSliders, goodsMultSliders;


        /// <summary>
        /// Legacy settings link.
        /// </summary>
        protected bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyCom; set => ModSettings.ThisSaveLegacyCom = value; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal ComGoodsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        // <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal override void UpdateControls()
        {
            base.UpdateControls();

            // Reset sliders and menus.
            for (int i = 0; i < visitMultSliders.Length; ++i)
            {
                // Reset visit multiplier slider values.
                visitMultSliders[i].value = RealisticVisitplaceCount.GetVisitMult(subServices[i]);

                // Reset visit mode menu selections.
                visitDefaultMenus[i].selectedIndex = RealisticVisitplaceCount.GetVisitMode(subServices[i]);
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
            if (visitDefaultMenus == null)
            {
                visitDefaultMenus = new UIDropDown[subServices.Length];
                visitMultSliders = new UISlider[subServices.Length];
                goodsMultSliders = new UISlider[subServices.Length];
            }

            // Sales multiplier slider.
            float currentY = yPos;
            goodsMultSliders[index] = AddSlider(panel, LeftColumn, currentY, ControlWidth, "RPR_DEF_CGM_TIP");
            goodsMultSliders[index].objectUserData = index;
            goodsMultSliders[index].value = (int)GoodsUtils.GetComMult(subServices[index]);
            MultSliderText(goodsMultSliders[index], goodsMultSliders[index].value);

            // Sales multiplier label.
            UILabel goodsLabel = UIControls.AddLabel(panel, 0f, 0f, Translations.Translate("RPR_DEF_CGM"), textScale: 0.8f);
            goodsLabel.relativePosition = new Vector2(LeftColumn - 10f - goodsLabel.width, currentY + (goodsMultSliders[index].parent.height - goodsLabel.height) / 2f);

            // Vist mode header label.
            UIControls.AddLabel(panel, RightColumn, currentY - 19f, Translations.Translate("RPR_DEF_VIS"), -1, 0.8f);

            // Visit mode menu.
            visitDefaultMenus[index] = UIControls.AddDropDown(panel, RightColumn, currentY, ControlWidth, height: 20f, itemVertPadding: 6, tooltip: Translations.Translate("RPR_DEF_VIS_TIP"));
            visitDefaultMenus[index].tooltipBox = TooltipUtils.TooltipBox;
            visitDefaultMenus[index].objectUserData = index;
            visitDefaultMenus[index].items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL")
            };

            // Visitor multiplication slider.
            currentY += RowHeight;
            visitMultSliders[index] = AddSlider(panel, RightColumn, currentY, ControlWidth, "RPR_DEF_VMU_TIP");
            visitMultSliders[index].objectUserData = index;
            visitMultSliders[index].value = RealisticVisitplaceCount.GetVisitMult(subServices[index]);
            MultSliderText(visitMultSliders[index], visitMultSliders[index].value);

            // Visit mode default event handler to show/hide multiplier slider.
            visitDefaultMenus[index].eventSelectedIndexChanged += VisitDefaultIndexChanged;

            // Set visit mode initial selection.
            visitDefaultMenus[index].selectedIndex = RealisticVisitplaceCount.GetVisitMode(subServices[index]);

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
                // Record vist calculation modes.
                RealisticVisitplaceCount.SetVisitMode(subServices[i], visitDefaultMenus[i].selectedIndex);

                // Record visitor multiplier.
                RealisticVisitplaceCount.SetVisitMult(subServices[i], (int)visitMultSliders[i].value);

                // Record goods multiplier.
                GoodsUtils.SetComMult(subServices[i], (int)goodsMultSliders[i].value);
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
            for (int i = 0; i < visitMultSliders.Length; ++i)
            {
                // Reset visit multiplier slider value.
                visitMultSliders[i].value = RealisticVisitplaceCount.DefaultVisitMult;

                // Reset visit mode menu selection.
                visitDefaultMenus[i].selectedIndex = ThisLegacyCategory ? (int)RealisticVisitplaceCount.ComVisitModes.legacy : (int)RealisticVisitplaceCount.ComVisitModes.popCalcs;

                // Reset goods multiplier slider value.
                visitMultSliders[i].value = GoodsUtils.DefaultSalesMult;
            }
        }


        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void ResetSaved(UIComponent control, UIMouseEventParameter mouseEvent) => UpdateControls();


        /// <summary>
        /// Visit default menu index changed event handler.
        /// <param name="control">Calling component</param>
        /// <param name="index">New selected index</param>
        /// </summary>
        private void VisitDefaultIndexChanged(UIComponent control, int index)
        {
            // Extract subservice index from this control's object user data.
            if (control.objectUserData is int subServiceIndex)
            {
                // Toggle multiplier slider visibility based on current state.
                visitMultSliders[subServiceIndex].parent.isVisible = index == (int)RealisticVisitplaceCount.ComVisitModes.popCalcs;
            }
        }
    }
}