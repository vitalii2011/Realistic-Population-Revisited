using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Base class of the options panels.
    /// </summary>
    internal class ConsumptionPanelBase
    {
        // UI layout constants.
        protected const float Margin = 5f;
        protected const float LeftTitle = 50f;
        protected const float LeftItem = 75f;
        protected const float RowHeight = 23f;
        protected const float ColumnWidth = 50f;
        protected const float Column1Width = 100f;
        protected const float Column8Width = 60f;
        protected const float Column1 = 180f;
        protected const float Column2 = Column1 + Column1Width + Margin;
        protected const float Column3 = Column2 + ColumnWidth + Margin;
        protected const float Column4 = Column3 + ColumnWidth + Margin + Margin;
        protected const float Column5 = Column4 + ColumnWidth + Margin;
        protected const float Column6 = Column5 + ColumnWidth + Margin;
        protected const float Column7 = Column6 + ColumnWidth + Margin;
        protected const float Column8 = Column7 + ColumnWidth + Margin;
        protected const float TitleHeight = 60f;


        // Textfield array.
        protected UITextField[][] areaFields;
        protected UITextField[][] floorFields;
        protected UITextField[][] extraFloorFields;
        protected UITextField[][] powerFields;
        protected UITextField[][] waterFields;
        protected UITextField[][] sewageFields;
        protected UITextField[][] garbageFields;
        protected UITextField[][] incomeFields;

        // Column labels.
        protected string areaLabel;
        protected string floorLabel;
        protected string extraFloorLabel;
        protected string powerLabel;
        protected string waterLabel;
        protected string sewageLabel;
        protected string garbageLabel;
        protected string wealthLabel;

        // Reference variables.
        protected float currentY = TitleHeight;
        protected bool notResidential = true;


        // Event handlers for button events.
        protected virtual void PopulateFields() { }
        protected virtual void ApplyFields() { }
        protected virtual void ResetToDefaults() { }


        /// <summary>
        /// Initialises array structure.
        /// </summary>
        /// <param name="numSubServices">Number of sub-services to initialise for</param>
        protected void SetupArrays(int numSubServices)
        {
            // Initialise textfield array.
            areaFields = new UITextField[numSubServices][];
            floorFields = new UITextField[numSubServices][];
            extraFloorFields = new UITextField[numSubServices][];
            powerFields = new UITextField[numSubServices][];
            waterFields = new UITextField[numSubServices][];
            sewageFields = new UITextField[numSubServices][];
            garbageFields = new UITextField[numSubServices][];
            incomeFields = new UITextField[numSubServices][];
        }


        /// <summary>
        /// Adds column headings.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        protected void AddHeadings(UIPanel panel)
        {
            // Set string references (we'll reference these multiple times with the textfields, so this saves calling translate each time).
            areaLabel = Translations.Translate(notResidential ? "RPR_OPT_APW" : "RPR_OPT_APH");
            floorLabel = Translations.Translate("RPR_OPT_FLR");
            extraFloorLabel = Translations.Translate("RPR_CAL_FLR_M");
            powerLabel = Translations.Translate("RPR_OPT_POW");
            waterLabel = Translations.Translate("RPR_OPT_WAT");
            sewageLabel = Translations.Translate("RPR_OPT_SEW");
            garbageLabel = Translations.Translate("RPR_OPT_GAR");
            wealthLabel = Translations.Translate("RPR_OPT_WEA");

            // Headings.
            ColumnLabel(panel, Column1, Column1Width, areaLabel, 1.0f);
            ColumnLabel(panel, Column2, ColumnWidth, floorLabel, 1.0f);
            ColumnIcon(panel, Column4, ColumnWidth, powerLabel, "ToolbarIconElectricity");
            ColumnIcon(panel, Column5, ColumnWidth, waterLabel, "ToolbarIconWaterAndSewage");
            ColumnIcon(panel, Column6, ColumnWidth, sewageLabel, "ToolbarIconWaterAndSewageDisabled");
            ColumnIcon(panel, Column7, ColumnWidth, garbageLabel, "InfoIconGarbage");
            ColumnIcon(panel, Column8, Column8Width, wealthLabel, "ToolbarIconMoney");

            // Bonus floors.
            if (notResidential)
            {
                ColumnLabel(panel, Column3, ColumnWidth, extraFloorLabel, 0.8f);
            }

            // Consumption heading
            UILabel headingLabel = panel.AddUIComponent<UILabel>();
            headingLabel.autoSize = false;
            headingLabel.autoHeight = true;
            headingLabel.wordWrap = true;
            headingLabel.relativePosition = new Vector3(Column4, 0);
            headingLabel.verticalAlignment = UIVerticalAlignment.Middle;
            headingLabel.textAlignment = UIHorizontalAlignment.Center;
            headingLabel.width = Column8 + Column8Width - Column4;
            headingLabel.text = Translations.Translate(notResidential ? "RPR_OPT_PERW" : "RPR_OPT_PERH");
        }


        /// <summary>
        /// Adds control bouttons the the panel.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        protected void AddButtons(UIPanel panel)
        {
            // Add extra space.
            currentY += Margin;

            // Reset button.
            UIButton resetButton = UIUtils.CreateButton(panel, 150);
            resetButton.text = Translations.Translate("RPR_OPT_RTD");
            resetButton.relativePosition = new Vector3(Margin, currentY);
            resetButton.eventClicked += (component, clickEvent) => ResetToDefaults();

            UIButton revertToSaveButton = UIUtils.CreateButton(panel, 150);
            revertToSaveButton.text = Translations.Translate("RPR_OPT_RTS");
            revertToSaveButton.relativePosition = new Vector3((Margin * 2) + 150, currentY);

            revertToSaveButton.eventClicked += (component, clickEvent) => { XMLUtilsWG.ReadFromXML(); PopulateFields(); };

            UIButton saveButton = UIUtils.CreateButton(panel, 150);
            saveButton.text = Translations.Translate("RPR_OPT_SAA");
            saveButton.relativePosition = new Vector3((Margin * 3) + 300, currentY);
            saveButton.eventClicked += (component, clickEvent) => ApplyFields();
        }


        /// <summary>
        /// Adds a column header text label.
        /// </summary>
        /// <param name="panel">UI panel</param>
        /// <param name="xPos">Reference X position</param>
        /// <param name="width">Width of reference item (for centering)</param>
        /// <param name="text">Label text</param>
        /// <param name="scale">Label text size (default 0.8)</param>
        protected void ColumnLabel(UIPanel panel, float xPos, float width, string text, float scale = 0.8f)
        {
            // Basic setup.
            UILabel columnLabel = panel.AddUIComponent<UILabel>();
            columnLabel.textScale = scale;
            columnLabel.verticalAlignment = UIVerticalAlignment.Middle;
            columnLabel.textAlignment = UIHorizontalAlignment.Center;
            columnLabel.autoSize = false;
            columnLabel.autoHeight = true;
            columnLabel.wordWrap = true;
            columnLabel.width = width + Margin;

            columnLabel.text = text;

            // Set the relative position at the end so we can adjust for the final post-wrap autoheight.
            columnLabel.relativePosition = new Vector3(xPos + ((width - columnLabel.width) / 2), TitleHeight - columnLabel.height);
        }


        /// <summary>
        /// Adds a column header icon label.
        /// </summary>
        /// <param name="panel">UI panel</param>
        /// <param name="xPos">Reference X position</param>
        /// <param name="width">Width of reference item (for centering)</param>
        /// <param name="text">Tooltip text</param>
        /// <param name="icon">Icon name</param>
        protected void ColumnIcon(UIPanel panel, float xPos, float width, string text, string icon)
        {
            // Create mini-panel for the icon background.
            UIPanel thumbPanel = panel.AddUIComponent<UIPanel>();
            thumbPanel.width = 35f;
            thumbPanel.height = 35f;
            thumbPanel.relativePosition = new Vector3(xPos + ((width - 35f) / 2), TitleHeight - 40f);
            thumbPanel.clipChildren = true;
            thumbPanel.backgroundSprite = "IconPolicyBaseRect";
            thumbPanel.tooltip = text;

            // Actual icon.
            UISprite thumbSprite = thumbPanel.AddUIComponent<UISprite>();
            thumbSprite.relativePosition = Vector3.zero;
            thumbSprite.size = thumbPanel.size;
            thumbSprite.atlas = UIUtils.GetAtlas("Ingame");
            thumbSprite.spriteName = icon;
        }


        /// <summary>
        /// Adds a sub-service field group to the panel.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        /// <param name="subService">Subservice reference number</param>
        /// <param name="isExtract">Set this to true (and label to null) to add extractor/processor labels (default false, which is plain level labels)</param>
        /// <param name="label">Text label base for each row; null (default) to use level numbers or extractor/prcessor</param>
        protected void AddSubService(UIPanel panel, bool addLevels, int subService, bool isExtract = false, string label = null)
        {
            // Add a row for each level within this subservice.
            for (int i = 0; i < areaFields[subService].Length; ++i)
            {
                // Row label.
                RowLabel(panel, currentY, label ?? (isExtract ? Translations.Translate( i == 0 ? "RPR_CAT_EXT" : "RPR_CAT_PRO") : Translations.Translate("RPR_OPT_LVL") + " " + (i + 1).ToString()));

                // Textfields.
                areaFields[subService][i] = AddTextField(panel, Column1Width, Column1, currentY, areaLabel);
                floorFields[subService][i] = AddTextField(panel, ColumnWidth, Column2, currentY, floorLabel);
                powerFields[subService][i] = AddTextField(panel, ColumnWidth, Column4, currentY, powerLabel);
                waterFields[subService][i] = AddTextField(panel, ColumnWidth, Column5, currentY, waterLabel);
                sewageFields[subService][i] = AddTextField(panel, ColumnWidth, Column6, currentY, sewageLabel);
                garbageFields[subService][i] = AddTextField(panel, ColumnWidth, Column7, currentY, garbageLabel);
                incomeFields[subService][i] = AddTextField(panel, Column8Width, Column8, currentY, wealthLabel);

                // Bonus levels.
                if (notResidential)
                {
                    extraFloorFields[subService][i] = AddTextField(panel, ColumnWidth, Column3, currentY, extraFloorLabel);
                }

                // Increment Y position.
                currentY += RowHeight;
            }

            // Add an extra bit of space at the end.
            currentY += Margin;
        }


        /// <summary>
        /// Attempts to parse a string for an integer value; if the parse fails, simply does nothing (leaving the original value intact).
        /// </summary>
        /// <param name="intVar">Integer variable to store result (left unchanged if parse fails)</param>
        /// <param name="text">Text to parse</param>
        protected void ParseInt(ref int intVar, string text)
        {
            int result;

            if (int.TryParse(text, out result))
            {
                intVar = result;
            }
        }


        /// <summary>
        /// Populates the text fields for a given subservice with information from the DataStore.
        /// </summary>
        /// <param name="dataArray">DataStore data array for the SubService</param>
        /// <param name="subService">SubService reference number</param>
        protected void PopulateSubService(int[][] dataArray, int subService)
        {
            // Iterate though each level, populating each row as we go.
            for (int i = 0; i < areaFields[subService].Length; ++i)
            {
                areaFields[subService][i].text = dataArray[i][DataStore.PEOPLE].ToString();
                floorFields[subService][i].text = dataArray[i][DataStore.LEVEL_HEIGHT].ToString();
                powerFields[subService][i].text = dataArray[i][DataStore.POWER].ToString();
                waterFields[subService][i].text = dataArray[i][DataStore.WATER].ToString();
                sewageFields[subService][i].text = dataArray[i][DataStore.SEWAGE].ToString();
                garbageFields[subService][i].text = dataArray[i][DataStore.GARBAGE].ToString();
                incomeFields[subService][i].text = dataArray[i][DataStore.INCOME].ToString();

                // Extra floor field, if applicable.
                if (!(this is ResidentialPanel))
                {
                    extraFloorFields[subService][i].text = dataArray[i][DataStore.DENSIFICATION].ToString();
                }
            }
        }


        /// <summary>
        /// Updates the DataStore for a given SubService with information from text fields. 
        /// </summary>
        /// <param name="dataArray">DataStore data array for the SubService</param>
        /// <param name="subService">SubService reference number</param>
        protected void ApplySubService(int[][] dataArray, int subService)
        {
            // Iterate though each level, populating each row as we go.
            for (int i = 0; i < areaFields[subService].Length; ++i)
            {
                ParseInt(ref dataArray[i][DataStore.PEOPLE], areaFields[subService][i].text);
                ParseInt(ref dataArray[i][DataStore.LEVEL_HEIGHT], floorFields[subService][i].text);
                ParseInt(ref dataArray[i][DataStore.POWER], powerFields[subService][i].text);
                ParseInt(ref dataArray[i][DataStore.WATER], waterFields[subService][i].text);
                ParseInt(ref dataArray[i][DataStore.SEWAGE], sewageFields[subService][i].text);
                ParseInt(ref dataArray[i][DataStore.GARBAGE], garbageFields[subService][i].text);
                ParseInt(ref dataArray[i][DataStore.INCOME], incomeFields[subService][i].text);

                // Extra floor field, if applicable.
                if (!(this is ResidentialPanel))
                {
                    ParseInt(ref dataArray[i][DataStore.DENSIFICATION], extraFloorFields[subService][i].text);
                }
            }
        }


        /// <summary>
        /// Adds a row text label.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        /// <param name="yPos">Reference Y position</param>
        /// <param name="text">Label text</param>
        private void RowLabel(UIPanel panel, float yPos, string text)
        {
            // Text label.
            UILabel lineLabel = panel.AddUIComponent<UILabel>();
            lineLabel.textScale = 0.9f;
            lineLabel.verticalAlignment = UIVerticalAlignment.Middle;
            lineLabel.text = text;

            // X position: by default it's LeftItem, but we move it further left if the label is too long to fit (e.g. long translation strings).
            float xPos = Mathf.Min(LeftItem, (Column1 - Margin) - lineLabel.width);
            // But never further left than the edge of the screen.
            if (xPos < 0)
            {
                xPos = LeftItem;
                // Too long to fit in the given space, so we'll let this wrap across and just move the textfields down an extra line.
                currentY += RowHeight;
            }
            lineLabel.relativePosition = new Vector3(xPos, yPos + 2);
        }


        /// <summary>
        /// Adds an input text field at the specified coordinates.
        /// </summary>
        /// <param name="textField">Textfield object</param>
        /// <param name="panel">panel to add to</param>
        /// <param name="posX">Relative X postion</param>
        /// <param name="posY">Relative Y position</param>
        /// <param name="tooltip">Tooltip, if any</param>
        private UITextField AddTextField(UIPanel panel, float width, float posX, float posY, string tooltip = null)
        {
            UITextField textField = UIUtils.CreateTextField(panel, width, 18f, 0.9f);
            textField.relativePosition = new Vector3(posX, posY);
            textField.eventTextChanged += (control, value) => TextFilter((UITextField)control, value);

            // Add tooltip.
            if (tooltip != null)
            {
                textField.tooltip = tooltip;
            }

            return textField;
        }


        /// <summary>
        /// Event handler filter for text fields to ensure only integer values are entered.
        /// </summary>
        /// <param name="control">Relevant control</param>
        /// <param name="value">Text value</param>
        public void TextFilter(UITextField control, string value)
        {
            // If it's not blank and isn't an integer, remove the last character and set selection to end.
            if (!value.IsNullOrWhiteSpace() && !int.TryParse(value, out int result))
            {
                control.text = value.Substring(0, value.Length - 1);
                control.MoveSelectionPointRight();
            }
        }
    }
}