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
        UIDropDown visitDefaultMenu;
        UISlider visitMultSlider;

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

            // Reset visit multiplier slider value, if control exists.
            if (visitMultSlider != null)
            {
                visitMultSlider.value = ModSettings.comVisitMult;
            }

            // Reset visit multiplier menu selection, if control exists.
            if (visitDefaultMenu != null)
            {
                visitDefaultMenu.selectedIndex = ModSettings.comVisitMode;
            }
        }


        /// <summary>
        /// Adds any additional controls below the menu arrays but above button footers.
        /// </summary>
        /// <param name="yPos">Relative Y position</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
        protected override float AddAdditional(float yPos)
        {
            // Layout constants.
            const float ControlX = 300f, ControlWidth = 300f;
            const float SliderPanelHeight = 20f;
            const float SliderHeight = 6f;
            const float OffsetX = (SliderPanelHeight - SliderHeight) / 2f;

            // Set y position.
            float currentY = yPos + 10f;

            visitDefaultMenu = UIControls.AddLabelledDropDown(panel, ControlX, currentY, Translations.Translate("RPR_DEF_VIS"), ControlWidth, false, Translations.Translate("RPR_DEF_VIS_TIP"));
            visitDefaultMenu.tooltipBox = TooltipUtils.TooltipBox;
            visitDefaultMenu.items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL")
            };

            currentY += visitDefaultMenu.height + (Margin * 2f);

            // Mutiplier slider panel.
            UIPanel sliderPanel = panel.AddUIComponent<UIPanel>();
            sliderPanel.autoSize = false;
            sliderPanel.autoLayout = false;
            sliderPanel.size = new Vector2(ControlWidth, SliderPanelHeight);
            sliderPanel.relativePosition = new Vector2(ControlX, currentY);

            // Mutiplier slider value label.
            UILabel visitMult = sliderPanel.AddUIComponent<UILabel>();
            visitMult.verticalAlignment = UIVerticalAlignment.Middle;
            visitMult.textAlignment = UIHorizontalAlignment.Center;
            visitMult.textScale = 0.7f;
            visitMult.text = ModSettings.comVisitMult.ToString();
            visitMult.autoSize = false;
            visitMult.color = new Color32(91, 97, 106, 255);
            visitMult.size = new Vector2(38, 15);
            visitMult.relativePosition = new Vector2(sliderPanel.width - visitMult.width - Margin, (SliderPanelHeight - visitMult.height) / 2f);

            // Mutiplier slider control.
            visitMultSlider = sliderPanel.AddUIComponent<UISlider>();
            visitMultSlider.size = new Vector2(sliderPanel.width - visitMult.width - (Margin * 3), SliderHeight);
            visitMultSlider.relativePosition = new Vector2(0f, OffsetX);

            // Mutiplier slider track.
            UISlicedSprite sliderSprite = visitMultSlider.AddUIComponent<UISlicedSprite>();
            sliderSprite.autoSize = false;
            sliderSprite.size = new Vector2(visitMultSlider.width, visitMultSlider.height);
            sliderSprite.relativePosition = new Vector2(0f, 0f);
            sliderSprite.atlas = TextureUtils.InGameAtlas;
            sliderSprite.spriteName = "ScrollbarTrack";

            // Mutiplier slider thumb.
            UISlicedSprite sliderThumb = visitMultSlider.AddUIComponent<UISlicedSprite>();
            sliderThumb.atlas = TextureUtils.InGameAtlas;
            sliderThumb.spriteName = "ScrollbarThumb";
            sliderThumb.height = 20f;
            sliderThumb.width = 10f;
            sliderThumb.relativePosition = new Vector2(0f, -OffsetX);
            visitMultSlider.thumbObject = sliderThumb;

            // Mutiplier slider values.
            visitMultSlider.stepSize = 0.05f;
            visitMultSlider.minValue = 0.1f;
            visitMultSlider.maxValue = 1f;
            visitMultSlider.value = ModSettings.comVisitMult;

            visitMultSlider.eventValueChanged += (control, value) =>
            {
                visitMult.text = value.ToString();
            };

            // Mutiplier slider label.
            UILabel sliderLabel = UIControls.AddLabel(sliderPanel, Margin, currentY, Translations.Translate("RPR_DEF_VMU"), -1, 0.8f);
            sliderLabel.relativePosition = new Vector2(-sliderLabel.width - Margin, (SliderPanelHeight - sliderLabel.height) / 2f);

            // Visit mode default event handler to show/hide multiplier slider.
            visitDefaultMenu.eventSelectedIndexChanged += VisitDefaultIndexChanged;

            // Set visit mode initial selection.
            visitDefaultMenu.selectedIndex = ModSettings.comVisitMode;

            // Add vertical space after.
            return currentY + visitMultSlider.parent.height + (Margin * 2f);
        }


        /// <summary>
        /// Visit default menu index changed event handler.
        /// <param name="control">Calling component (unused)</param>
        /// <param name="index">New selected index</param>
        /// </summary>
        private void VisitDefaultIndexChanged(UIComponent control, int index)
        {
            // Toggle multiplier slider visibility based on current state.
            visitMultSlider.parent.isVisible = index == (int)ModSettings.ComVisitModes.popCalcs;
        }


        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void Apply(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Record vistis mode calculations.
            ModSettings.comVisitMode = visitDefaultMenu.selectedIndex;

            // Record mutltiplier.
            ModSettings.comVisitMult = visitMultSlider.value;

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

            // Reset visit multiplier slider value.
            visitMultSlider.value = ModSettings.DefaultVisitMult;

            // Reset visit multiplier menu selection.
            visitDefaultMenu.selectedIndex = ThisLegacyCategory ? (int)ModSettings.ComVisitModes.legacy : (int)ModSettings.ComVisitModes.popCalcs;
        }

        /*
        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void ResetSaved(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            base.ResetSaved(control, mouseEvent);

            // Reset visit multiplier slider value.
            visitMultSlider.value = ModSettings.comVisitMult;

            // Reset visit multiplier menu selection.
            visitDefaultMenu.selectedIndex = ModSettings.comVisitMode;
        }*/
    }
}