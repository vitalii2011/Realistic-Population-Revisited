using UnityEngine;
using ICities;
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
        public static void IntTextFilter(UITextField control, string value)
        {
            // If it's not blank and isn't an integer, remove the last character and set selection to end.
            if (!value.IsNullOrWhiteSpace() && !int.TryParse(value, out int result))
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
        public static void FloatTextFilter(UITextField control, string value)
        {
            // If it's not blank and isn't an integer, remove the last character and set selection to end.
            if (!value.IsNullOrWhiteSpace() && !float.TryParse(value, out float result))
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
            int result;

            if (int.TryParse(text, out result))
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
            float result;

            if (float.TryParse(text, out result))
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
        /// Adds a plain text label to the specified UI panel.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Label text</param>
        /// <returns></returns>
        internal static UILabel AddLabel(UIComponent parent, string text)
        {
            // Add label.
            UILabel label = (UILabel)parent.AddUIComponent<UILabel>();
            label.autoSize = false;
            label.autoHeight = true;
            label.wordWrap = true;
            label.width = 700;
            label.text = text;

            return label;
        }


        /// <summary>
        /// Creates a dropdown menu with an attached text label.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Text label</param>
        /// <param name="xPos">Relative x position (default 20)</param>
        /// <param name="yPos">Relative y position (default 0)</param>
        /// <returns></returns>
        internal static UIDropDown LabelledDropDown(UIComponent parent, string text, float xPos = 20f, float yPos = 0f)
        {
            // Create dropdown.
            UIDropDown dropDown = AddDropDown(parent, xPos, yPos);

            // Add label.
            UILabel label = dropDown.AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.text = text;

            // Get width and position.
            float labelWidth = label.width + 10f;

            label.relativePosition = new Vector3(-labelWidth, 6f);

            // Move dropdown to accomodate label.
            dropDown.relativePosition += new Vector3(labelWidth, 0f);

            return dropDown;
        }


        /// <summary>
        /// Creates a dropdown menu without text label or enclosing panel.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="xPos">Relative x position (default 20)</param>
        /// <param name="yPos">Relative y position (default 0)</param>
        /// <returns></returns>
        internal static UIDropDown AddDropDown(UIComponent parent, float xPos, float yPos)
        {
            // Constants.
            const float Width = 220f;
            const float Height = 25f;
            const int ItemHeight = 20;

            // Create dropdown menu.
            UIDropDown dropDown = parent.AddUIComponent<UIDropDown>();
            dropDown.listBackground = "GenericPanelLight";
            dropDown.itemHover = "ListItemHover";
            dropDown.itemHighlight = "ListItemHighlight";
            dropDown.normalBgSprite = "ButtonMenu";
            dropDown.disabledBgSprite = "ButtonMenuDisabled";
            dropDown.hoveredBgSprite = "ButtonMenuHovered";
            dropDown.focusedBgSprite = "ButtonMenu";
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            dropDown.popupColor = new Color32(45, 52, 61, 255);
            dropDown.popupTextColor = new Color32(170, 170, 170, 255);
            dropDown.zOrder = 1;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            dropDown.textFieldPadding = new RectOffset(8, 0, 8, 0);
            dropDown.itemPadding = new RectOffset(14, 0, 8, 0);

            dropDown.relativePosition = new Vector3(xPos, yPos);

            // Dropdown size parameters.
            dropDown.size = new Vector2(Width, Height);
            dropDown.listWidth = (int)Width;
            dropDown.listHeight = 500;
            dropDown.itemHeight = ItemHeight;
            dropDown.textScale = 0.7f;

            // Create dropdown button.
            UIButton button = dropDown.AddUIComponent<UIButton>();
            dropDown.triggerButton = button;
            button.size = dropDown.size;
            button.text = "";
            button.relativePosition = new Vector3(0f, 0f);
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.normalFgSprite = "IconDownArrow";
            button.hoveredFgSprite = "IconDownArrowHovered";
            button.pressedFgSprite = "IconDownArrowPressed";
            button.focusedFgSprite = "IconDownArrowFocused";
            button.disabledFgSprite = "IconDownArrowDisabled";
            button.spritePadding = new RectOffset(3, 3, 3, 3);
            button.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.zOrder = 0;

            return dropDown;
        }


        /// <summary>
        /// Creates a plain dropdown using the game's option panel dropdown template.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Descriptive label text</param>
        /// <param name="items">Dropdown menu item list</param>
        /// <param name="selectedIndex">Initially selected index (default 0)</param>
        /// <param name="width">Width of dropdown (default 60)</param>
        /// <returns></returns>
        public static UIDropDown AddPlainDropDown(UIComponent parent, string text, string[] items, int selectedIndex = 0, float width = 270f)
        {
            UIPanel panel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsDropdownTemplate")) as UIPanel;
            UIDropDown dropDown = panel.Find<UIDropDown>("Dropdown");

            // Set text.
            panel.Find<UILabel>("Label").text = text;

            // Slightly increase width.
            dropDown.autoSize = false;
            dropDown.width = width;

            // Add items.
            dropDown.items = items;
            dropDown.selectedIndex = selectedIndex;

            return dropDown;
        }


        /// <summary>
        /// Adds a slider with a descriptive text label above and an automatically updating value label immediately to the right.
        /// </summary>
        /// <param name="parent">Panel to add the control to</param>
        /// <param name="text">Descriptive label text</param>
        /// <param name="min">Slider minimum value</param>
        /// <param name="max">Slider maximum value</param>
        /// <param name="step">Slider minimum step</param>
        /// <param name="defaultValue">Slider initial value</param>
        /// <param name="eventCallback">Slider event handler</param>
        /// <param name="width">Slider width (excluding value label to right) (default 600)</param>
        /// <returns>New UI slider with attached labels</returns>
        public static UISlider AddSliderWithValue(UIComponent parent, string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback, float width = 600f)
        {
            // Add slider component.
            UIPanel sliderPanel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsSliderTemplate")) as UIPanel;
            sliderPanel.Find<UILabel>("Label").text = text;

            // Label.
            UILabel sliderLabel = sliderPanel.Find<UILabel>("Label");
            sliderLabel.autoHeight = true;
            sliderLabel.width = width;
            sliderLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            sliderLabel.relativePosition = Vector3.zero;
            sliderLabel.relativePosition = Vector3.zero;
            sliderLabel.text = text;

            // Slider configuration.
            UISlider newSlider = sliderPanel.Find<UISlider>("Slider");
            newSlider.minValue = min;
            newSlider.maxValue = max;
            newSlider.stepSize = step;
            newSlider.value = defaultValue;

            // Move default slider position to match resized label.
            sliderPanel.autoLayout = false;
            newSlider.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top;
            newSlider.relativePosition = PositionUnder(sliderLabel);
            newSlider.width = width;

            // Increase height of panel to accomodate it all plus some extra space for margin.
            sliderPanel.autoSize = false;
            sliderPanel.width = width + 50f;
            sliderPanel.height = newSlider.relativePosition.y + newSlider.height + 20f;

            // Value label.
            UILabel valueLabel = sliderPanel.AddUIComponent<UILabel>();
            valueLabel.name = "ValueLabel";
            valueLabel.text = newSlider.value.ToString();
            valueLabel.relativePosition = PositionRightOf(newSlider, 8f, 1f);

            // Event handler to update value label.
            newSlider.eventValueChanged += (component, value) =>
            {
                valueLabel.text = value.ToString();
                eventCallback(value);
            };

            return newSlider;
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


        /// <summary>
        /// Returns a relative position to the right of a specified UI component, suitable for placing an adjacent component.
        /// </summary>
        /// <param name="uIComponent">Original (anchor) UI component</param>
        /// <param name="margin">Margin between components (default 8)</param>
        /// <param name="verticalOffset">Vertical offset from first to second component (default 0)</param>
        /// <returns>Offset position (to right of original)</returns>
        internal static Vector3 PositionRightOf(UIComponent uIComponent, float margin = 8f, float verticalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + uIComponent.width + margin, uIComponent.relativePosition.y + verticalOffset);
        }


        /// <summary>
        /// Creates a plain checkbox using the game's option panel checkbox template.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Descriptive label text</param>
        /// <returns></returns>
        internal static UICheckBox AddPlainCheckBox(UIComponent parent, string text)
        {
            UICheckBox checkBox = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsCheckBoxTemplate")) as UICheckBox;

            // Set text.
            checkBox.text = text;

            return checkBox;
        }


        /// <summary>
        /// Adds a checkbox.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="xPos">Relative x position (default 0)</param>
        /// <param name="yPos">Relative y position (default 0)</param>
        /// <returns>New UI checkbox with attached labels</returns>
        internal static UICheckBox AddCheckBox(UIComponent parent, float xPos = 20f, float yPos = 0f)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();

            // Size and position.
            checkBox.height = 16f;
            checkBox.width = 16f;
            checkBox.clipChildren = true;
            checkBox.relativePosition = new Vector3(xPos, yPos);

            // Sprites.
            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            sprite.spriteName = "check-unchecked";
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = Vector3.zero;

            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).spriteName = "check-checked";
            checkBox.checkedBoxObject.size = new Vector2(16f, 16f);
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            return checkBox;
        }


        /// <summary>
        /// Returns a relative position below a specified UI component, suitable for placing an adjacent component.
        /// </summary>
        /// <param name="uIComponent">Original (anchor) UI component</param>
        /// <param name="margin">Margin between components (default 8)</param>
        /// <param name="horizontalOffset">Horizontal offset from first to second component (default 0)</param>
        /// <returns>Offset position (below original)</returns>
        private static Vector3 PositionUnder(UIComponent uIComponent, float margin = 8f, float horizontalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + horizontalOffset, uIComponent.relativePosition.y + uIComponent.height + margin);
        }


    }
}