using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for creating and editing calculation packs.
    /// </summary>
    internal abstract class CalculationPanelBase
    {
        // Constants.
        protected const float Margin = 5f;
        protected const float TextFieldWidth = 85f;
        protected const float ColumnWidth = TextFieldWidth + (Margin * 2);
        protected const float LeftItem = 75f;
        protected const float FirstItem = 110f;
        protected const float RowHeight = 27f;

        // Panel components.
        protected UIDropDown packDropDown;
        protected UITextField packNameField;
        protected UIButton saveButton, deleteButton;

        // List of packs.
        protected List<DataPack> packList;

        // Panel reference.
        protected readonly UIPanel panel;

        // Tab sprite name and tooltip key.
        protected abstract string TabSprite { get; }
        protected abstract string TabTooltipKey { get; }


        /// Constructor - dds editing options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CalculationPanelBase(UITabstrip tabStrip, int tabIndex)
        {
            // Layout constants.
            const float TabIconSize = 23f;
            const float TabWidth = 50f;

            // Add tab and helper.
            panel = PanelUtils.AddTab(tabStrip, "", tabIndex, out UIButton tabButton, TabWidth);
            panel.autoLayout = false;

            // Add tab sprite.
            UISprite thumbSprite = tabButton.AddUIComponent<UISprite>();
            thumbSprite.relativePosition = new Vector2((TabWidth - TabIconSize) / 2f, 1f);
            thumbSprite.width = TabIconSize;
            thumbSprite.height = TabIconSize;
            thumbSprite.atlas = TextureUtils.InGameAtlas;
            thumbSprite.spriteName = TabSprite;

            // Set tooltip.
            tabButton.tooltip = Translations.Translate(TabTooltipKey);
        }


        /// <summary>
        /// Sets button and textfield enabled/disabled states.
        /// </summary>
        /// <param name="index">Selected pack list index</param>
        protected void ButtonStates(int index)
        {
            // Enable save and delete buttons and name textfield if this is a custom pack, otherwise disable.
            if (packList[index].version == (int)DataVersion.customOne)
            {
                packNameField.Enable();
                saveButton.Enable();
                deleteButton.Enable();
            }
            else
            {
                packNameField.Disable();
                saveButton.Disable();
                deleteButton.Disable();
            }
        }


        /// <summary>
        /// Adds a row text label.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        /// <param name="yPos">Reference Y position</param>
        /// <param name="text">Label text</param>
        protected UILabel RowLabel(UIPanel panel, float yPos, string text)
        {
            // Text label.
            UILabel lineLabel = panel.AddUIComponent<UILabel>();
            lineLabel.textScale = 0.9f;
            lineLabel.verticalAlignment = UIVerticalAlignment.Middle;
            lineLabel.text = text;

            // X position: by default it's LeftItem, but we move it further left if the label is too long to fit (e.g. long translation strings).
            float xPos = Mathf.Min(LeftItem, (FirstItem - Margin) - lineLabel.width);
            // But never further left than the edge of the screen.
            if (xPos < 0)
            {
                xPos = LeftItem;
            }
            lineLabel.relativePosition = new Vector3(xPos, yPos + 2);

            return lineLabel;
        }
    }
}