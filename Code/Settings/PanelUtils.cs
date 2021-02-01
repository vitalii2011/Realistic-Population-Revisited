using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Utilities for Options Panel UI.
    /// </summary>
    internal static class PanelUtils
    {
        /// <summary>
        /// Event handler filter for text fields to ensure only integer values are entered.
        /// </summary>
        /// <param name="control">Relevant control</param>
        /// <param name="value">Text value</param>
        internal static void IntTextFilter(UITextField control, string value)
        {
            // If it's not blank and isn't an integer, remove the last character and set selection to end.
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            if (!value.IsNullOrWhiteSpace() && !int.TryParse(value, out int result))
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            {
                control.text = value.Substring(0, value.Length - 1);
                control.MoveSelectionPointRight();
            }
        }


        /// <summary>
        /// Event handler filter for text fields to ensure only floating-point values are entered.
        /// </summary>
        /// <param name="control">Relevant control</param>
        /// <param name="value">Text value</param>
        internal static void FloatTextFilter(UITextField control, string value)
        {
            // If it's not blank and isn't an integer, remove the last character and set selection to end.
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            if (!value.IsNullOrWhiteSpace() && !float.TryParse(value, out float result))
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            {
                control.text = value.Substring(0, value.Length - 1);
                control.MoveSelectionPointRight();
            }
        }


        /// <summary>
        /// Attempts to parse a string for an integer value; if the parse fails, simply does nothing (leaving the original value intact).
        /// </summary>
        /// <param name="intVar">Integer variable to store result (left unchanged if parse fails)</param>
        /// <param name="text">Text to parse</param>
        internal static void ParseInt(ref int intVar, string text)
        {
            if (int.TryParse(text, out int result))
            {
                intVar = result;
            }
        }


        /// <summary>
        /// Attempts to parse a string for an floating-point value; if the parse fails, simply does nothing (leaving the original value intact).
        /// </summary>
        /// <param name="floatVer">Float variable to store result (left unchanged if parse fails)</param>
        /// <param name="text">Text to parse</param>
        internal static void ParseFloat(ref float floatVar, string text)
        {
            if (float.TryParse(text, out float result))
            {
                floatVar = result;
            }
        }


        /// <summary>
        /// Adds a tab to a UI tabstrip.
        /// </summary>
        /// <param name="tabStrip">UIT tabstrip to add to</param>
        /// <param name="tabName">Name of this tab</param>
        /// <param name="tabIndex">Index number of this tab</param>
        /// <returns>UIHelper instance for the new tab panel</returns>
        internal static UIPanel AddTab(UITabstrip tabStrip, string tabName, int tabIndex, bool autoLayout = false)
        {
            // Create tab.
            UIButton tabButton = tabStrip.AddTab(tabName);

            // Sprites.
            tabButton.normalBgSprite = "SubBarButtonBase";
            tabButton.disabledBgSprite = "SubBarButtonBaseDisabled";
            tabButton.focusedBgSprite = "SubBarButtonBaseFocused";
            tabButton.hoveredBgSprite = "SubBarButtonBaseHovered";
            tabButton.pressedBgSprite = "SubBarButtonBasePressed";

            // Tooltip.
            tabButton.tooltip = tabName;

            tabStrip.selectedIndex = tabIndex;

            // Force width.
            tabButton.width = 120;

            // Get tab root panel.
            UIPanel rootPanel = tabStrip.tabContainer.components[tabIndex] as UIPanel;

            // Panel setup.
            rootPanel.autoLayout = autoLayout;
            rootPanel.autoLayoutDirection = LayoutDirection.Vertical;
            rootPanel.autoLayoutPadding.top = 5;
            rootPanel.autoLayoutPadding.left = 10;

            return rootPanel;
        }


        /// <summary>
        /// Adds a row header icon label at the current Y position.
        /// </summary>
        /// <param name="panel">UI panel</param>
        /// <param name="yPos">Reference Y positions</param>
        /// <param name="text">Tooltip text</param>
        /// <param name="icon">Icon name</param>
        internal static void RowHeaderIcon(UIPanel panel, ref float yPos, string text, string icon, string atlas)
        {
            // UI layout constants.
            const float Margin = 5f;
            const float LeftTitle = 50f;


            // Actual icon.
            UISprite thumbSprite = panel.AddUIComponent<UISprite>();
            thumbSprite.relativePosition = new Vector3(Margin, yPos - 2.5f);
            thumbSprite.width = 35f;
            thumbSprite.height = 35f;
            thumbSprite.atlas = UIUtils.GetAtlas(atlas);
            thumbSprite.spriteName = icon;

            // Text label.
            UILabel lineLabel = panel.AddUIComponent<UILabel>();
            lineLabel.textScale = 1.0f;
            lineLabel.text = text;
            lineLabel.relativePosition = new Vector3(LeftTitle, yPos + 7);
            lineLabel.verticalAlignment = UIVerticalAlignment.Middle;

            // Increment our current height.
            yPos += 30f;
        }


        /// <summary>
        /// Adds a column header text label.
        /// </summary>
        /// <param name="panel">UI panel</param>
        /// <param name="xPos">Reference X position</param>
        /// <param name="baseY">Y position of base of label/param>
        /// <param name="width">Width of reference item (for centering)</param>
        /// <param name="text">Label text</param>
        /// <param name="scale">Label text size (default 0.8)</param>
        internal static void ColumnLabel(UIPanel panel, float xPos, float baseY, float width, string text, float scale = 0.8f)
        {
            // Basic setup.
            UILabel columnLabel = panel.AddUIComponent<UILabel>();
            columnLabel.textScale = scale;
            columnLabel.verticalAlignment = UIVerticalAlignment.Middle;
            columnLabel.textAlignment = UIHorizontalAlignment.Center;
            columnLabel.autoSize = false;
            columnLabel.autoHeight = true;
            columnLabel.wordWrap = true;
            columnLabel.width = width;

            columnLabel.text = text;

            // Set the relative position at the end so we can adjust for the final post-wrap autoheight.
            columnLabel.relativePosition = new Vector3(xPos + ((width - columnLabel.width) / 2), baseY - columnLabel.height);
        }
    }
}