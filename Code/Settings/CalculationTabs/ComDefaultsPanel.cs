using UnityEngine;
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
        private UIDropDown[] visitDefaultMenus;
        private UISlider[] visitMultSliders, goodsMultSliders;

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

        // <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal override void UpdateMenus()
        {
            base.UpdateMenus();

            // Reset sliders and menus.
            for (int i = 0; i < visitMultSliders.Length; ++i)
            {
                // Reset visit multiplier slider value
                visitMultSliders[i].value = RealisticVisitplaceCount.comVisitMults[subServices[i]];

                // Reset visit multiplier menu selection/
                visitDefaultMenus[i].selectedIndex = RealisticVisitplaceCount.comVisitModes[subServices[i]];
            }
        }


        /// <summary>
        /// Adds any additional controls to the right of each row.
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
            UIControls.AddLabel(panel, RowAdditionX, currentY - 19f, Translations.Translate("RPR_DEF_VIS"), -1, 0.8f);

            // RowAdditions is called as part of parent constructor, so we need to initialise them here if they aren't already.
            if (visitDefaultMenus == null)
            {
                visitDefaultMenus = new UIDropDown[subServices.Length];
                visitMultSliders = new UISlider[subServices.Length];
                goodsMultSliders = new UISlider[subServices.Length];
            }

            visitDefaultMenus[index] = UIControls.AddDropDown(panel, RowAdditionX, currentY, controlWidth, height: 20f, itemVertPadding: 6, tooltip: Translations.Translate("RPR_DEF_VIS_TIP"));
            visitDefaultMenus[index].tooltipBox = TooltipUtils.TooltipBox;
            visitDefaultMenus[index].objectUserData = index;
            visitDefaultMenus[index].items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL")
            };

            // Visitor multiplication slider.
            currentY = yPos;
            visitMultSliders[index] = AddSlider(panel, RowAdditionX, currentY, controlWidth);
            visitMultSliders[index].objectUserData = index;
            visitMultSliders[index].value = RealisticVisitplaceCount.comVisitMults[subServices[index]];
            visitMultSliders[index].tooltipBox = TooltipUtils.TooltipBox;
            visitMultSliders[index].tooltip = Translations.Translate("RPR_DEF_VMU_TIP");
            MultSliderText(visitMultSliders[index], visitMultSliders[index].value);

            // Visit mode default event handler to show/hide multiplier slider.
            visitDefaultMenus[index].eventSelectedIndexChanged += VisitDefaultIndexChanged;

            // Set visit mode initial selection.
            visitDefaultMenus[index].selectedIndex = RealisticVisitplaceCount.comVisitModes[subServices[index]];

            // Customer multiplier slider.
            currentY += RowHeight;
            goodsMultSliders[index] = AddSlider(panel, LeftColumn, currentY, controlWidth);
            goodsMultSliders[index].objectUserData = index;
            goodsMultSliders[index].value = (int)GoodsUtils.GetComMult(subServices[index]);
            goodsMultSliders[index].tooltipBox = TooltipUtils.TooltipBox;
            goodsMultSliders[index].tooltip = Translations.Translate("RPR_DEF_CGM_TIP");
            MultSliderText(goodsMultSliders[index], goodsMultSliders[index].value);

            // Customer multiplier label.
            UILabel goodsLabel = UIControls.AddLabel(panel, 0f, 0f, Translations.Translate("RPR_DEF_CGM"), textScale: 0.8f);
            goodsLabel.relativePosition = new Vector3(LeftColumn - 10f - goodsLabel.width, currentY + (goodsMultSliders[index].parent.height - goodsLabel.height) / 2f);

            return currentY;
        }


        /// <summary>
        /// Adds a slider.
        /// </summary>
        /// <param name="panel">Panel reference</param>
        /// <param name="xPos">Relative X position</param>
        /// <param name="yPos">Relative Y position</param>
        /// <param name="width">Slider width</param>
        /// <returns>New slider</returns>
        private UISlider AddSlider(UIPanel panel, float xPos, float yPos, float width)
        {
            // Layout constants.
            const float SliderPanelHeight = 20f;
            const float SliderHeight = 6f;
            const float OffsetX = (SliderPanelHeight - SliderHeight) / 2f;

            // Mutiplier slider panel.
            UIPanel sliderPanel = panel.AddUIComponent<UIPanel>();
            sliderPanel.autoSize = false;
            sliderPanel.autoLayout = false;
            sliderPanel.size = new Vector2(width, SliderPanelHeight);
            sliderPanel.relativePosition = new Vector2(xPos, yPos);

            // Mutiplier slider value label.
            UILabel visitMult = sliderPanel.AddUIComponent<UILabel>();
            visitMult.name = "ValueLabel";
            visitMult.verticalAlignment = UIVerticalAlignment.Middle;
            visitMult.textAlignment = UIHorizontalAlignment.Center;
            visitMult.textScale = 0.7f;
            visitMult.autoSize = false;
            visitMult.color = new Color32(91, 97, 106, 255);
            visitMult.size = new Vector2(38, 15);
            visitMult.relativePosition = new Vector2(sliderPanel.width - visitMult.width - Margin, (SliderPanelHeight - visitMult.height) / 2f);

            // Mutiplier slider control.
            UISlider newSlider = sliderPanel.AddUIComponent<UISlider>();
            newSlider.size = new Vector2(sliderPanel.width - visitMult.width - (Margin * 3), SliderHeight);
            newSlider.relativePosition = new Vector2(0f, OffsetX);

            // Mutiplier slider track.
            UISlicedSprite sliderSprite = newSlider.AddUIComponent<UISlicedSprite>();
            sliderSprite.autoSize = false;
            sliderSprite.size = new Vector2(newSlider.width, newSlider.height);
            sliderSprite.relativePosition = new Vector2(0f, 0f);
            sliderSprite.atlas = TextureUtils.InGameAtlas;
            sliderSprite.spriteName = "ScrollbarTrack";

            // Mutiplier slider thumb.
            UISlicedSprite sliderThumb = newSlider.AddUIComponent<UISlicedSprite>();
            sliderThumb.atlas = TextureUtils.InGameAtlas;
            sliderThumb.spriteName = "ScrollbarThumb";
            sliderThumb.height = 20f;
            sliderThumb.width = 10f;
            sliderThumb.relativePosition = new Vector2(0f, -OffsetX);
            newSlider.thumbObject = sliderThumb;

            // Mutiplier slider value range.
            newSlider.stepSize = 1f;
            newSlider.minValue = 1f;
            newSlider.maxValue = 100f;

            // Event handler to update text.
            newSlider.eventValueChanged += MultSliderText;

            return newSlider;
        }


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


        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void Apply(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Extract subservice index from this control's object user data.
            for (int i = 0; i < subServices.Length; ++i)
            {
                // Record vist mode calculations.
               RealisticVisitplaceCount.comVisitModes[subServices[i]] = visitDefaultMenus[i].selectedIndex;

                // Record visitor mutltiplier.
                RealisticVisitplaceCount.comVisitMults[subServices[i]] = (int)visitMultSliders[i].value;

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
            base.ResetDefaults(control, mouseEvent);

            // Reset sliders and menus.
            for (int i = 0; i < visitMultSliders.Length; ++i)
            {
                // Reset visit multiplier slider value.
                visitMultSliders[i].value = RealisticVisitplaceCount.DefaultVisitMult;

                // Reset visit multiplier menu selection.
                visitDefaultMenus[i].selectedIndex = ThisLegacyCategory ? (int)RealisticVisitplaceCount.ComVisitModes.legacy : (int)RealisticVisitplaceCount.ComVisitModes.popCalcs;

                // Reset goods multiplier slider value.
                visitMultSliders[i].value = GoodsUtils.DefaultSalesMult;
            }
        }

        /// <summary>
        /// Updates the displayed value on a multiplier slider.
        /// </summary>
        /// <param name="control">Calling component</param>
        /// <param name="value">New valie</param>
        private  void MultSliderText(UIComponent control, float value)
        {
            if (control?.parent?.Find<UILabel>("ValueLabel") is UILabel valueLabel)
            {
                valueLabel.text = Mathf.RoundToInt(value).ToString() + "%";
            }
        }
    }
}