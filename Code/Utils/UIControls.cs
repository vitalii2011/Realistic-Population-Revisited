using UnityEngine;
using ICities;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// UI textfield with an attached label.
    /// </summary>
    public class UILabelledTextfield
    {
        public UITextField textField;
        public UILabel label;
    }


    /// <summary>
    /// Static utilities class for creating UI controls.
    /// </summary>
    public static class UIControls
    {
        /// <summary>
        /// Adds an input text field at the specified coordinates.
        /// </summary>
        /// <param name="textField">Textfield object</param>
        /// <param name="panel">panel to add to</param>
        /// <param name="posX">Relative X postion</param>
        /// <param name="posY">Relative Y position</param>
        /// <param name="tooltip">Tooltip, if any</param>
        public static UITextField AddTextField(UIPanel panel, float width, float posX, float posY, string tooltip = null)
        {
            UITextField textField = UIUtils.CreateTextField(panel, width, 18f, 0.9f);
            textField.relativePosition = new Vector3(posX, posY);

            // Add tooltip.
            if (tooltip != null)
            {
                textField.tooltip = tooltip;
            }

            return textField;
        }


        /// <summary>
        /// Adds a checkbox with a descriptive text label immediately to the right.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Descriptive label text</param>
        /// <param name="xPos">Relative x position (default 0)</param>
        /// <param name="yPos">Relative y position (default 0)</param>
        /// <param name="textScale">Text scale of label (default 0.8)</param>
        /// <returns>New UI checkbox with attached labels</returns>
        public static UICheckBox AddCheckBox(UIComponent parent, string text, float xPos = 20f, float yPos = 0f, float textScale = 0.8f)
        {
            // Create base checkbox.
            UICheckBox checkBox = AddCheckBox(parent, xPos, yPos);

            // Label.
            checkBox.label = checkBox.AddUIComponent<UILabel>();
            checkBox.label.relativePosition = new Vector3(21f, checkBox.height / 2f);
            checkBox.label.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            checkBox.label.textScale = textScale;
            checkBox.label.autoSize = true;
            checkBox.label.text = text;

            // Dynamic width to accomodate label.
            checkBox.width = checkBox.label.width + 21f;

            return checkBox;
        }


        /// <summary>
        /// Adds a checkbox without a label.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="xPos">Relative x position (default 0)</param>
        /// <param name="yPos">Relative y position (default 0)</param>
        /// <returns>New UI checkbox *without* attached labels</returns>
        public static UICheckBox AddCheckBox(UIComponent parent, float xPos = 20f, float yPos = 0f)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();

            // Size and position.
            checkBox.height = 16f;
            checkBox.width = 16f;
            checkBox.clipChildren = false;
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
        /// Adds a plain text label to the specified UI panel.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Label text</param>
        /// <param name="xPos">Relative x position)</param>
        /// <param name="yPos">Relative y position</param>
        /// <param name="width">Label width (default 700)</param>
        /// <param name="width">Text scale (default 1.0)</param>
        /// <returns></returns>
        public static UILabel AddLabel(UIComponent parent, string text, float xPos, float yPos, float width = 700f, float textScale = 1.0f)
        {
            // Add label.
            UILabel label = (UILabel)parent.AddUIComponent<UILabel>();
            label.autoSize = false;
            label.autoHeight = true;
            label.wordWrap = true;
            label.width = width;
            label.textScale = textScale;
            label.text = text;
            label.relativePosition = new Vector2(xPos, yPos);

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
        public static UIDropDown LabelledDropDown(UIComponent parent, string text, float xPos = 20f, float yPos = 0f)
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
        public static UIDropDown AddDropDown(UIComponent parent, float xPos, float yPos, float width = 220f)
        {
            // Constants.
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
            dropDown.size = new Vector2(width, Height);
            dropDown.listWidth = (int)width;
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
        public static UISlider AddSlider(UIComponent parent, string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback, float width = 600f)
        {
            // Add slider component.
            UIPanel sliderPanel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsSliderTemplate")) as UIPanel;

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

            return newSlider;
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
            UISlider newSlider = AddSlider(parent, text, min, max, step, defaultValue, eventCallback, width);
            UIPanel sliderPanel = (UIPanel)newSlider.parent;

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
        public static UISlider AddSliderWithMultipler(UIComponent parent, string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback, float width = 600f)
        {
            // Add slider component.
            UISlider newSlider = AddSlider(parent, text, min, max, step, defaultValue, eventCallback, width);
            UIPanel sliderPanel = (UIPanel)newSlider.parent;

            // Value label.
            UILabel valueLabel = sliderPanel.AddUIComponent<UILabel>();
            valueLabel.name = "ValueLabel";
            valueLabel.text = "x" + newSlider.value.ToString();
            valueLabel.relativePosition = PositionUnder(newSlider, 0, 0f);

            // Event handler to update value label.
            newSlider.eventValueChanged += (component, value) =>
            {
                valueLabel.text = "x" + value.ToString();
                eventCallback(value);
            };

            return newSlider;
        }


        /// <summary>
        /// Creates a plain checkbox using the game's option panel checkbox template.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Descriptive label text</param>
        /// <returns></returns>
        public static UICheckBox AddPlainCheckBox(UIComponent parent, string text)
        {
            UICheckBox checkBox = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsCheckBoxTemplate")) as UICheckBox;

            // Set text.
            checkBox.text = text;

            return checkBox;
        }


        /// <summary>
        /// Returns a relative position to the right of a specified UI component, suitable for placing an adjacent component.
        /// </summary>
        /// <param name="uIComponent">Original (anchor) UI component</param>
        /// <param name="margin">Margin between components (default 8)</param>
        /// <param name="verticalOffset">Vertical offset from first to second component (default 0)</param>
        /// <returns>Offset position (to right of original)</returns>
        public static Vector3 PositionRightOf(UIComponent uIComponent, float margin = 8f, float verticalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + uIComponent.width + margin, uIComponent.relativePosition.y + verticalOffset);
        }


        /// <summary>
        /// Returns a relative position below a specified UI component, suitable for placing an adjacent component.
        /// </summary>
        /// <param name="uIComponent">Original (anchor) UI component</param>
        /// <param name="margin">Margin between components (default 8)</param>
        /// <param name="horizontalOffset">Horizontal offset from first to second component (default 0)</param>
        /// <returns>Offset position (below original)</returns>
        public static Vector3 PositionUnder(UIComponent uIComponent, float margin = 8f, float horizontalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + horizontalOffset, uIComponent.relativePosition.y + uIComponent.height + margin);
        }
    }
}