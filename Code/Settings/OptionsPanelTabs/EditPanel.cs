using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Options panel for creating and editing calculation packs.
    /// </summary>
    internal class EditPanel
    {
        // Constants.
        protected const float Margin = 10f;
        protected const float TextFieldWidth = 60f;
        protected const float ColumnWidth = TextFieldWidth + (Margin * 2);
        protected const float FloorHeightX = 200f;
        protected const float AreaPerX = FloorHeightX + ColumnWidth;
        protected const float FirstMinX = AreaPerX + ColumnWidth;
        protected const float FirstMaxX = FirstMinX + ColumnWidth;
        protected const float FirstEmptyX = FirstMaxX + ColumnWidth;
        protected const float MultiFloorX = FirstEmptyX + ColumnWidth;
        protected const float DetailHeader = 130f;
        protected const float TitleHeight = DetailHeader + 140f;
        protected const float LeftItem = 75f;
        protected const float RowHeight = 23f;

        string[] serviceNames = { Translations.Translate("RPR_CAT_RES"), Translations.Translate("RPR_CAT_IND"), Translations.Translate("RPR_CAT_COM"), Translations.Translate("RPR_CAT_OFF") };
        ItemClass.Service[] services = { ItemClass.Service.Residential, ItemClass.Service.Industrial, ItemClass.Service.Commercial, ItemClass.Service.Office };
        int[] maxLevels = { 5, 3, 3, 3 };

        // Textfield arrays.
        protected UITextField[] floorHeightFields, areaPerFields, firstMinFields, firstMaxFields;
        protected UICheckBox[] firstEmptyCheck, multiFloorCheck;
        protected UILabel[] rowLabels;

        // Panel components.
        protected UIDropDown packDropDown, serviceDropDown;
        protected UITextField packNameField;

        // List of custom packs.
        List<VolumetricPack> packList;


        /// <summary>
        /// Adds editing options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal EditPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Y position indicator.
            float currentY = TitleHeight + Margin;

            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(tabStrip, Translations.Translate("RPR_OPT_EDT"), tabIndex);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = false;

            // Initialise arrays
            floorHeightFields = new UITextField[5];
            areaPerFields = new UITextField[5];
            firstMinFields = new UITextField[5];
            firstMaxFields = new UITextField[5];
            firstEmptyCheck = new UICheckBox[5];
            multiFloorCheck = new UICheckBox[5];
            rowLabels = new UILabel[5];

            // Pack selection dropdown.
            packDropDown = PanelUtils.AddPlainDropDown(panel, Translations.Translate("RPR_OPT_CPK"), CustomPacks(), -1);
            packDropDown.parent.relativePosition = new Vector3(20f, Margin);
            packDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                PopulateTextFields(index);
            };

            // Service selection dropdown.
            serviceDropDown = PanelUtils.AddPlainDropDown(panel, Translations.Translate("RPR_OPT_SVC"), serviceNames, -1);
            serviceDropDown.parent.relativePosition = new Vector3(20f, DetailHeader);
            serviceDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                TextfieldVisibility(maxLevels[index]);
            };

            // Pack name textfield.
            packNameField = UIUtils.CreateTextField(panel, 200f, 20f);
            UILabel packNameLabel = PanelUtils.AddLabel(packNameField, Translations.Translate("RPR_OPT_EDT_NAM"));
            packNameLabel.relativePosition = new Vector3(-100f, 0f);
            packNameField.relativePosition = new Vector3(400f, Margin);
            packNameField.isEnabled = false;

            // Headings.
            PanelUtils.ColumnLabel(panel, FloorHeightX, TitleHeight, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FLH"), 1.0f);
            PanelUtils.ColumnLabel(panel, AreaPerX, TitleHeight, ColumnWidth, Translations.Translate("RPR_CAL_VOL_APU"), 1.0f);
            PanelUtils.ColumnLabel(panel, FirstMinX, TitleHeight, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FMN"), 1.0f);
            PanelUtils.ColumnLabel(panel, FirstMaxX, TitleHeight, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FMX"), 1.0f);
            PanelUtils.ColumnLabel(panel, FirstEmptyX, TitleHeight, ColumnWidth, Translations.Translate("RPR_CAL_VOL_IGF"), 1.0f);
            PanelUtils.ColumnLabel(panel, MultiFloorX, TitleHeight, ColumnWidth, Translations.Translate("RPR_CAL_VOL_MFU"), 1.0f);

            // Add level textfields.
            for (int i = 0; i < 5; ++i)
            {
                // Row label.
                rowLabels[i] = RowLabel(panel, currentY, Translations.Translate("RPR_OPT_LVL") + " " + (i + 1).ToString());

                floorHeightFields[i] = AddTextField(panel, TextFieldWidth, FloorHeightX + Margin, currentY);
                floorHeightFields[i].eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);

                areaPerFields[i] = AddTextField(panel, TextFieldWidth, AreaPerX + Margin, currentY);
                areaPerFields[i].eventTextChanged += (control, value) => PanelUtils.IntTextFilter((UITextField)control, value);

                firstMinFields[i] = AddTextField(panel, TextFieldWidth, FirstMinX + Margin, currentY);
                firstMinFields[i].eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);

                firstMaxFields[i] = AddTextField(panel, TextFieldWidth, FirstMaxX + Margin, currentY);
                firstMaxFields[i].eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);

                firstEmptyCheck[i] = AddCheckBox(panel, FirstEmptyX + (ColumnWidth / 2), currentY);
                multiFloorCheck[i] = AddCheckBox(panel, MultiFloorX + (ColumnWidth / 2), currentY);

                // Move to next row.
                currentY += RowHeight;
            }

            // Additional space before buttons.
            currentY += RowHeight;

            // 'Add new' button.
            UIButton addNewButton = UIUtils.CreateButton(panel, 200f);
            addNewButton.relativePosition = new Vector3(400f, 50f);
            addNewButton.text = Translations.Translate("RPR_OPT_NEW");
            addNewButton.eventClicked += (control, clickEvent) =>
            {
                // Set pack name field to a basic new name.
                packNameField.isEnabled = true;
                packNameField.text = "NewPack";
            };

            // Save pack button.
            UIButton saveButton = UIUtils.CreateButton(panel, 200f);
            saveButton.relativePosition = new Vector3(20f, currentY);
            saveButton.text = Translations.Translate("RPR_OPT_SAA");

            // Event handler.
            saveButton.eventClicked += (control, clickEvent) =>
            {
                // Basic sanity checks - need a valid name and service to proceed.
                if (packNameField.text != null && serviceDropDown.selectedIndex >= 0)
                {
                    // New pack to add.
                    VolumetricPack newPack = new VolumetricPack();

                    // Basic pack attributes.
                    newPack.name = packNameField.text;
                    newPack.displayName = packNameField.text;
                    newPack.version = (int)DataVersion.customOne;
                    newPack.service = services[serviceDropDown.selectedIndex];
                    newPack.levels = new LevelData[maxLevels[serviceDropDown.selectedIndex]];

                    // Iterate through each level, parsing input fields.
                    for (int i = 0; i < maxLevels[serviceDropDown.selectedIndex]; ++i)
                    {
                        // Textfields.
                        PanelUtils.ParseFloat(ref newPack.levels[i].floorHeight, floorHeightFields[i].text);
                        PanelUtils.ParseInt(ref newPack.levels[i].areaPer, areaPerFields[i].text);
                        PanelUtils.ParseFloat(ref newPack.levels[i].firstFloorMin, firstMinFields[i].text);
                        PanelUtils.ParseFloat(ref newPack.levels[i].firstFloorMax, firstMaxFields[i].text);

                        // Checkboxes.
                        newPack.levels[i].firstFloorEmpty = firstEmptyCheck[i].isChecked;
                        newPack.levels[i].multiFloorUnits = multiFloorCheck[i].isChecked;
                    }

                    // Add our new pack to our list of packs and update calculations panel menus.
                    PopData.AddCalculationPack(newPack);
                    CalculationsPanel.instance.UpdateMenus();

                    // Save configuration file.
                    ConfigUtils.SaveSettings();

                    // Update pack menu.
                    packDropDown.items = CustomPacks();

                    // Set pack selection by iterating through each pack in the menu and looking for a match.
                    for (int i = 0; i < packDropDown.items.Length; ++i)
                    {
                        if (packDropDown.items[i].Equals(packNameField.text))
                        {
                            // Got a match; apply selected index and stop looping.
                            packDropDown.selectedIndex = i;
                            break;
                        }
                    }

                    // Now that we've saved, disble name field.
                    packNameField.isEnabled = false;
                }
            };

            /*
            // Delete pack button.
            UIButton deleteButton = UIUtils.CreateButton(panel, 200f);
            deleteButton.relativePosition = new Vector3(250f, currentY);
            deleteButton.text = Translations.Translate("RPR_OPT_DEL");
            deleteButton.eventClicked += (control, clickEvent) =>
            {
                
            };*/

            // Set service menu to initial state (residential), which will also update textfield visibility via event handler.
            serviceDropDown.selectedIndex = 0;
        }



        /// <summary>
        /// Shows/hides textfields according to the provided maximum level to show.
        /// </summary>
        /// <param name="maxLevel">Maximum number of levels to show</param>
        public void TextfieldVisibility(int maxLevel)
        {
            // Iterate through all fields.
            for (int i = 0; i < 5; ++i)
            {
                // If this level is less than the maximum given, show.
                if (i < maxLevel)
                {
                    rowLabels[i].Show();
                    floorHeightFields[i].Show();
                    areaPerFields[i].Show();
                    firstMinFields[i].Show();
                    firstMaxFields[i].Show();
                    firstEmptyCheck[i].Show();
                    multiFloorCheck[i].Show();
                }
                else
                {
                    // Otherwise, hide.
                    rowLabels[i].Hide();
                    floorHeightFields[i].Hide();
                    areaPerFields[i].Hide();
                    firstMinFields[i].Hide();
                    firstMaxFields[i].Hide();
                    firstEmptyCheck[i].Hide();
                    multiFloorCheck[i].Hide();
                }
            }
        }


        /// <summary>
        /// Adds a row text label.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        /// <param name="yPos">Reference Y position</param>
        /// <param name="text">Label text</param>
        private UILabel RowLabel(UIPanel panel, float yPos, string text)
        {
            // Text label.
            UILabel lineLabel = panel.AddUIComponent<UILabel>();
            lineLabel.textScale = 0.9f;
            lineLabel.verticalAlignment = UIVerticalAlignment.Middle;
            lineLabel.text = text;

            // X position: by default it's LeftItem, but we move it further left if the label is too long to fit (e.g. long translation strings).
            float xPos = Mathf.Min(LeftItem, (FloorHeightX - Margin) - lineLabel.width);
            // But never further left than the edge of the screen.
            if (xPos < 0)
            {
                xPos = LeftItem;
                // Too long to fit in the given space, so we'll let this wrap across and just move the textfields down an extra line.
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
        private UICheckBox AddCheckBox(UIPanel panel, float posX, float posY, string tooltip = null)
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
        private UITextField AddTextField(UIPanel panel, float width, float posX, float posY, string tooltip = null)
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
        /// Populates the textfields with data from the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number of calculation pack</param>
        private void PopulateTextFields(int index)
        {
            // Get local reference.
            VolumetricPack volPack = packList[index];

            // Set name field.
            packNameField.text = volPack.name;

            // Set service selection menu by iterating through each service and looking for a match.
            for (int i = 0; i < services.Length; ++i)
            {
                if (services[i] == volPack.service)
                {
                    // Got a match; apply selected index and stop looping.
                    // This also applies text field visibility via the service menue event handler.
                    serviceDropDown.selectedIndex = i;
                    break;
                }    
            }

            // Iterate through each level in the pack and populate the relevant row.
            for (int i = 0; i < volPack.levels.Length; ++i)
            {
                // Local reference.
                LevelData level = volPack.levels[i];

                // Populate controls.
                floorHeightFields[i].text = level.floorHeight.ToString();
                areaPerFields[i].text = level.areaPer.ToString();
                firstMinFields[i].text = level.firstFloorMin.ToString();
                firstMaxFields[i].text = level.firstFloorMax.ToString();
                firstEmptyCheck[i].isChecked = level.firstFloorEmpty;
                multiFloorCheck[i].isChecked = level.multiFloorUnits;
            }
        }


        /// <summary>
        /// (Re)builds the list of custom packs.
        /// </summary>
        /// <returns>String array of custom pack names, in order (suitable for use as dropdown menu items)</returns>
        private string[] CustomPacks()
        {
            // Re-initialise pack list.
            packList = new List<VolumetricPack>();
            List<string> packNames = new List<string>();

            // Iterate through all available packs.
            foreach (CalcPack calcPack in PopData.calcPacks)
            {
                // Check for custom packs.
                if (calcPack is VolumetricPack volPack && volPack.version == (int)DataVersion.customOne)
                {
                    // Found onw - add to our lists.
                    packList.Add(volPack);
                    packNames.Add(volPack.name);
                }
            }

            return packNames.ToArray();
        }
    }
}