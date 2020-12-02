using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Options panel for creating and editing calculation packs.
    /// </summary>
    internal class CalculationPanelBase
    {
        // Constants.
        protected const float Margin = 10f;
        protected const float TextFieldWidth = 90f;
        protected const float ColumnWidth = TextFieldWidth + (Margin * 2);
        protected const float LeftItem = 75f;
        protected const float FirstItem = 120f;
        protected const float RowHeight = 23f;

        // Panel components.
        protected UIDropDown packDropDown;
        protected UITextField packNameField;
        protected UIButton saveButton, deleteButton;

        // List of packs.
        protected List<DataPack> packList;


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


        /// <summary>
        /// Adds checkbox at the specified coordinates.
        /// </summary>
        /// <param name="textField">Textfield object</param>
        /// <param name="panel">panel to add to</param>
        /// <param name="posX">Relative X postion</param>
        /// <param name="posY">Relative Y position</param>
        /// <param name="tooltip">Tooltip, if any</param>
        protected UICheckBox AddCheckBox(UIPanel panel, float posX, float posY, string tooltip = null)
        {
            UICheckBox checkBox = PanelUtils.AddCheckBox(panel, posX, posY);

            // Add tooltip.
            if (tooltip != null)
            {
                checkBox.tooltip = tooltip;
            }

            return checkBox;
        }


        /// <summary>
        /// Adds an input text field at the specified coordinates.
        /// </summary>
        /// <param name="textField">Textfield object</param>
        /// <param name="panel">panel to add to</param>
        /// <param name="posX">Relative X postion</param>
        /// <param name="posY">Relative Y position</param>
        /// <param name="tooltip">Tooltip, if any</param>
        protected UITextField AddTextField(UIPanel panel, float width, float posX, float posY, string tooltip = null)
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
    }
}