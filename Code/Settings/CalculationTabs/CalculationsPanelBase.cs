using System.Linq;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Globalization;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal abstract class CalculationsPanelBase
    {
        // Layout constants.
        protected const float Margin = 5f;
        protected float RowHeight = 25f;
        protected const float LeftColumn = 200f;


        // Instance references.
        internal static CalculationsPanelBase instance;
        protected UIPanel panel;


        // Service/subservice arrays.
        protected abstract string[] SubServiceNames { get; }
        protected abstract ItemClass.Service[] Services { get; }
        protected abstract ItemClass.SubService[] SubServices { get; }
        protected abstract string[] IconNames { get; }
        protected abstract string[] AtlasNames { get; }


        // Tab settings.
        protected virtual float TabWidth => 100f;
        protected abstract string TabName { get; }
        protected abstract string[] TabIconNames { get; }
        protected abstract string[] TabAtlasNames { get; }


        /// <summary>
        /// Constructor - adds default options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CalculationsPanelBase(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab and helper.
            panel = PanelUtils.AddIconTab(tabStrip, TabName, tabIndex, TabIconNames, TabAtlasNames, TabWidth);

            // Set instance.
            instance = this;
        }


        /// <summary>
        /// Adds footer buttons to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons</param>
        protected virtual void FooterButtons(float yPos)
        {
            // Reset button.
            UIButton resetButton = UIControls.AddButton(panel, Margin, yPos, Translations.Translate("RPR_OPT_RTD"), 150f);
            resetButton.eventClicked += ResetDefaults;

            // Revert button.
            UIButton revertToSaveButton = UIControls.AddButton(panel, (Margin * 2) + 150f, yPos, Translations.Translate("RPR_OPT_RTS"), 150f);
            revertToSaveButton.eventClicked += ResetSaved;
        }


        /// <summary>
        /// 'Revert to defaults' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected abstract void ResetDefaults(UIComponent control, UIMouseEventParameter mouseEvent);


        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected abstract void ResetSaved(UIComponent control, UIMouseEventParameter mouseEvent);


        /// <summary>
        /// Adds a slider.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="xPos">Relative X position</param>
        /// <param name="yPos">Relative Y position</param>
        /// <param name="width">Slider width</param>
        /// <param name="tooltipKey">Tooltip translation key</param>
        /// <param name="isPercent">True if this slider should display percentage values, false to display absolute values</param>
        /// <returns>New slider</returns>
        protected UISlider AddSlider(UIComponent parent, float xPos, float yPos, float width, string tooltipKey, bool isPercent = true)
        {
            // Layout constants.
            const float SliderPanelHeight = 20f;
            const float SliderHeight = 6f;
            const float ValueLabelWidth = 45f;
            const float OffsetX = (SliderPanelHeight - SliderHeight) / 2f;

            // Mutiplier slider panel.
            UIPanel sliderPanel = parent.AddUIComponent<UIPanel>();
            sliderPanel.autoSize = false;
            sliderPanel.autoLayout = false;
            sliderPanel.size = new Vector2(width, SliderPanelHeight);
            sliderPanel.relativePosition = new Vector2(xPos, yPos);

            // Mutiplier slider value label.
            UILabel valueLabel = sliderPanel.AddUIComponent<UILabel>();
            valueLabel.name = "ValueLabel";
            valueLabel.verticalAlignment = UIVerticalAlignment.Middle;
            valueLabel.textAlignment = UIHorizontalAlignment.Center;
            valueLabel.textScale = 0.7f;
            valueLabel.autoSize = false;
            valueLabel.color = new Color32(91, 97, 106, 255);
            valueLabel.size = new Vector2(ValueLabelWidth, 15f);
            valueLabel.relativePosition = new Vector2(sliderPanel.width - ValueLabelWidth - Margin, (SliderPanelHeight - valueLabel.height) / 2f);

            // Mutiplier slider control.
            UISlider newSlider = sliderPanel.AddUIComponent<UISlider>();
            newSlider.size = new Vector2(sliderPanel.width - ValueLabelWidth - (Margin * 3), SliderHeight);
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

            // Tooltip.
            newSlider.tooltipBox = TooltipUtils.TooltipBox;
            newSlider.tooltip = Translations.Translate(tooltipKey);

            // Mutiplier slider value range.
            newSlider.stepSize = 1f;
            newSlider.minValue = 1f;
            newSlider.maxValue = 100f;

            // Event handler to update text.
            if (isPercent)
            {
                newSlider.eventValueChanged += PercentSliderText;
            }
            else
            {
                newSlider.eventValueChanged += AbsSliderText;
            }

            return newSlider;
        }


        /// <summary>
        /// Updates the displayed percentage value on a multiplier slider.
        /// </summary>
        /// <param name="control">Calling component</param>
        /// <param name="value">New valie</param>
        protected void PercentSliderText(UIComponent control, float value)
        {
            if (control?.parent?.Find<UILabel>("ValueLabel") is UILabel valueLabel)
            {
                valueLabel.text = Mathf.RoundToInt(value).ToString() + "%";
            }
        }


        /// <summary>
        /// Updates the displayed absolute value on a multiplier slider.
        /// </summary>
        /// <param name="control">Calling component</param>
        /// <param name="value">New valie</param>
        protected void AbsSliderText(UIComponent control, float value)
        {
            if (control?.parent?.Find<UILabel>("ValueLabel") is UILabel valueLabel)
            {
                valueLabel.text = Mathf.RoundToInt(value).ToString("N0", LocaleManager.cultureInfo);
            }
        }


        /// <summary>
        /// Adds a title label at the top of the panel.
        /// </summary>
        /// <param name="titleKey">Title translation key</param>
        /// <returns>Y position below title</returns>
        protected float TitleLabel(string titleKey)
        {
            // Add title.
            UILabel titleLabel = UIControls.AddLabel(panel, 0f, Margin, Translations.Translate(titleKey), panel.width, 1.5f);
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.font = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault((UIFont f) => f.name == "OpenSans-Semibold");

            return Margin + titleLabel.height + Margin;
        }
    }
}