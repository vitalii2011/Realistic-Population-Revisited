using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Base class of the options panels.
    /// </summary>
    internal class PanelBase
    {
        // UI layout constants.
        protected const float Margin = 10f;
        protected const float RowHeight = 25f;
        protected const float ColumnWidth = 50f;
        protected const float Column1Width = 100f;
        protected const float Column8Width = 60f;
        protected const float Column1 = 175f;
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
            // Headings.
            ColumnLabel(panel, Column1, Column1Width, Translations.Translate(notResidential ? "RPR_OPT_APW" : "RPR_OPT_APH"), 1.0f);
            ColumnLabel(panel, Column2, ColumnWidth, Translations.Translate("RPR_OPT_FLR"), 1.0f);
            ColumnLabel(panel, Column4, ColumnWidth, Translations.Translate("RPR_OPT_POW"));
            ColumnLabel(panel, Column5, ColumnWidth, Translations.Translate("RPR_OPT_WAT"));
            ColumnLabel(panel, Column6, ColumnWidth, Translations.Translate("RPR_OPT_SEW"));
            ColumnLabel(panel, Column7, ColumnWidth, Translations.Translate("RPR_OPT_GAR"));
            ColumnLabel(panel, Column8, Column8Width, Translations.Translate("RPR_OPT_WEA"));

            // Bonus floors.
            if(notResidential)
            {
                ColumnLabel(panel, Column3, ColumnWidth, "Floor mod", 0.8f);
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
            headingLabel.text = Translations.Translate("RPR_OPT_UTIL") + "\r\n" + Translations.Translate(notResidential ? "RPR_OPT_PERW" : "RPR_OPT_PERH");
        }


        /// <summary>
        /// Adds control bouttons the the panel.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        protected void AddButtons(UIPanel panel)
        {
            // Reset button.
            UIButton resetButton = UIUtils.CreateButton(panel, 150);
            resetButton.text = Translations.Translate("RPR_OPT_RTD");
            resetButton.relativePosition = new Vector3(Margin, currentY);
            resetButton.eventClicked += (component, clickEvent) => ResetToDefaults();

            UIButton revertToSaveButton = UIUtils.CreateButton(panel, 150);
            revertToSaveButton.text = Translations.Translate("RPR_OPT_RTS");
            revertToSaveButton.relativePosition = new Vector3((Margin * 2) + 150, currentY);

            revertToSaveButton.eventClicked += (component, clickEvent) => { XMLUtils.ReadFromXML(); PopulateFields(); };

            UIButton saveButton = UIUtils.CreateButton(panel, 150);
            saveButton.text = Translations.Translate("SRPR_OPT_SAA");
            saveButton.relativePosition = new Vector3((Margin * 3) + 300, currentY);
            saveButton.eventClicked += (component, clickEvent) => ApplyFields();
        }


        /// <summary>
        /// Adds a column header text label.
        /// </summary>
        /// <param name="panel">UI panel</param>
        /// <param name="xPos">Reference X position</param>
        /// <param name="text">Label text</param>
        protected void ColumnLabel(UIPanel panel, float xPos, float width, string text, float scale = 0.8f)
        {

            UILabel lineLabel = panel.AddUIComponent<UILabel>();
            lineLabel.textScale = scale;
            lineLabel.verticalAlignment = UIVerticalAlignment.Middle;
            lineLabel.textAlignment = UIHorizontalAlignment.Center;
            lineLabel.autoSize = false;
            lineLabel.autoHeight = true;
            lineLabel.wordWrap = true;

            lineLabel.text = text;
            lineLabel.width = width + Margin;
            lineLabel.relativePosition = new Vector3(xPos + ((width - lineLabel.width) / 2), TitleHeight - lineLabel.height);
        }


        /// <summary>
        /// Adds a sub-service field group to the panel.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        /// <param name="label">Text label base for each row</param>
        /// <param name="addLevels">True to add building levels to the end of the label, false to leave label as given</param>
        /// <param name="subService">Subservice reference number</param>
        /// <param name="isExtract">Set this to true (and addLevels to false) to add extractor/processor labels</param>
        protected void AddSubService(UIPanel panel, string label, bool addLevels, int subService, bool isExtract = false)
        {
            // Add a row for each level within this subservice.
            for (int i = 0; i < areaFields[subService].Length; i++)
            {
                // Row label.
                RowLabel(panel, currentY, label + " " +  (addLevels ? (i + 1).ToString() : (isExtract ? Translations.Translate( i == 0 ? "RPR_CAT_EXT" : "RPR_CAT_PRO") : string.Empty)));

                // Textfields.
                areaFields[subService][i] = AddTextField(panel, Column1Width, Column1, currentY);
                floorFields[subService][i] = AddTextField(panel, ColumnWidth, Column2, currentY);
                powerFields[subService][i] = AddTextField(panel, ColumnWidth, Column4, currentY);
                waterFields[subService][i] = AddTextField(panel, ColumnWidth, Column5, currentY);
                sewageFields[subService][i] = AddTextField(panel, ColumnWidth, Column6, currentY);
                garbageFields[subService][i] = AddTextField(panel, ColumnWidth, Column7, currentY);
                incomeFields[subService][i] = AddTextField(panel, Column8Width, Column8, currentY);

                // Bonus levels.
                if (notResidential)
                {
                    extraFloorFields[subService][i] = AddTextField(panel, ColumnWidth, Column3, currentY);
                }

                // Increment Y position.
                currentY += RowHeight;
            }

            // Add an extra blank line at the end.
            currentY += RowHeight;
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
            for (int i = 0; i < areaFields[subService].Length; i++)
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
            for (int i = 0; i < areaFields[subService].Length; i++)
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
            UILabel lineLabel = panel.AddUIComponent<UILabel>();
            lineLabel.text = text;
            lineLabel.relativePosition = new Vector3(Margin, yPos + 2);
            lineLabel.verticalAlignment = UIVerticalAlignment.Middle;
        }


        /// <summary>
        /// Adds an input text field at the specified coordinates.
        /// </summary>
        /// <param name="textField">Textfield object</param>
        /// <param name="panel">panel to add to</param>
        /// <param name="posX">Relative X postion</param>
        /// <param name="posY">Relative Y position</param>
        private UITextField AddTextField(UIPanel panel, float width, float posX, float posY)
        {
            UITextField textField = UIUtils.CreateTextField(panel, width, 20);
            textField.relativePosition = new Vector3(posX, posY);
            textField.eventTextChanged += (control, value) => TextFilter((UITextField)control, value);

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