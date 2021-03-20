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
        private UISlider[] visitMultSliders;

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
        protected override void RowAdditions(UIPanel panel, float yPos, int index)
        {
            // Layout constants.
            const float SliderPanelHeight = 20f;
            const float SliderHeight = 6f;
            const float OffsetX = (SliderPanelHeight - SliderHeight) / 2f;
            float controlWidth = panel.width - RowAdditionX;

            // Header label.
            UIControls.AddLabel(panel, RowAdditionX, yPos - 19f, Translations.Translate("RPR_DEF_VIS"), -1, 0.8f);

            // RowAdditions is called as part of parent constructor, so we need to initialise them here if they aren't already.
            if (visitDefaultMenus == null)
            {
                visitDefaultMenus = new UIDropDown[subServices.Length];
                visitMultSliders = new UISlider[subServices.Length];
            }

            visitDefaultMenus[index] = UIControls.AddDropDown(panel, RowAdditionX, yPos, controlWidth, Translations.Translate("RPR_DEF_VIS_TIP"));
            visitDefaultMenus[index].tooltipBox = TooltipUtils.TooltipBox;
            visitDefaultMenus[index].objectUserData = index;
            visitDefaultMenus[index].items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL")
            };

            // Mutiplier slider panel.
            UIPanel sliderPanel = panel.AddUIComponent<UIPanel>();
            sliderPanel.autoSize = false;
            sliderPanel.autoLayout = false;
            sliderPanel.size = new Vector2(controlWidth, SliderPanelHeight);
            sliderPanel.relativePosition = new Vector2(RowAdditionX, yPos + RowHeight);

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
            visitMultSliders[index] = sliderPanel.AddUIComponent<UISlider>();
            visitMultSliders[index].size = new Vector2(sliderPanel.width - visitMult.width - (Margin * 3), SliderHeight);
            visitMultSliders[index].relativePosition = new Vector2(0f, OffsetX);
            visitMultSliders[index].objectUserData = index;

            // Mutiplier slider track.
            UISlicedSprite sliderSprite = visitMultSliders[index].AddUIComponent<UISlicedSprite>();
            sliderSprite.autoSize = false;
            sliderSprite.size = new Vector2(visitMultSliders[index].width, visitMultSliders[index].height);
            sliderSprite.relativePosition = new Vector2(0f, 0f);
            sliderSprite.atlas = TextureUtils.InGameAtlas;
            sliderSprite.spriteName = "ScrollbarTrack";

            // Mutiplier slider thumb.
            UISlicedSprite sliderThumb = visitMultSliders[index].AddUIComponent<UISlicedSprite>();
            sliderThumb.atlas = TextureUtils.InGameAtlas;
            sliderThumb.spriteName = "ScrollbarThumb";
            sliderThumb.height = 20f;
            sliderThumb.width = 10f;
            sliderThumb.relativePosition = new Vector2(0f, -OffsetX);
            visitMultSliders[index].thumbObject = sliderThumb;

            // Mutiplier slider value range.
            visitMultSliders[index].stepSize = 0.01f;
            visitMultSliders[index].minValue = 0.01f;
            visitMultSliders[index].maxValue = 1f;

            // Set initial value and force intitial display.
            visitMultSliders[index].eventValueChanged += MultSliderText;
            visitMultSliders[index].value = RealisticVisitplaceCount.comVisitMults[subServices[index]];
            MultSliderText(visitMultSliders[index], visitMultSliders[index].value);

            // Visit mode default event handler to show/hide multiplier slider.
            visitDefaultMenus[index].eventSelectedIndexChanged += VisitDefaultIndexChanged;

            // Set visit mode initial selection.
            visitDefaultMenus[index].selectedIndex = RealisticVisitplaceCount.comVisitModes[subServices[index]];
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

                // Record mutltiplier.
                RealisticVisitplaceCount.comVisitMults[subServices[i]] = visitMultSliders[i].value;
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
                valueLabel.text = Mathf.RoundToInt(value * 100f).ToString() + "%";
            }
        }
    }
}