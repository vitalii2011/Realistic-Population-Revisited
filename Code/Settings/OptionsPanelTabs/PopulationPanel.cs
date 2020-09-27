using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Options panel for creating and editing calculation packs.
    /// </summary>
    internal class PopulationPanel
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
        protected const float PackMenuY = 90f;
        protected const float DetailY = PackMenuY + 140f;
        protected const float LeftItem = 75f;
        protected const float RowHeight = 23f;

        string[] serviceNames = { Translations.Translate("RPR_CAT_RES"), Translations.Translate("RPR_CAT_IND"), Translations.Translate("RPR_CAT_COM"), Translations.Translate("RPR_CAT_OFF") };
        ItemClass.Service[] services = { ItemClass.Service.Residential, ItemClass.Service.Industrial, ItemClass.Service.Commercial, ItemClass.Service.Office };
        int[] maxLevels = { 5, 3, 3, 3 };

        // Textfield arrays.
        protected UITextField[] floorHeightFields, areaPerFields, firstMinFields, firstExtraFields;
        protected UICheckBox[] firstEmptyCheck, multiFloorCheck;
        protected UILabel[] rowLabels;

        // Panel components.
        protected UIDropDown packDropDown, serviceDropDown;
        protected UITextField packNameField;

        // List of packs.
        List<VolumetricPack> packList;


        /// <summary>
        /// Adds editing options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal PopulationPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Y position indicator.
            float currentY = DetailY;

            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(tabStrip, Translations.Translate("RPR_OPT_POP"), tabIndex);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = false;

            // Initialise arrays
            floorHeightFields = new UITextField[5];
            areaPerFields = new UITextField[5];
            firstMinFields = new UITextField[5];
            firstExtraFields = new UITextField[5];
            firstEmptyCheck = new UICheckBox[5];
            multiFloorCheck = new UICheckBox[5];
            rowLabels = new UILabel[5];

            // Service selection dropdown.
            serviceDropDown = PanelUtils.AddPlainDropDown(panel, Translations.Translate("RPR_OPT_SVC"), serviceNames, -1);
            serviceDropDown.parent.relativePosition = new Vector3(20f, Margin);

            // Pack selection dropdown.
            packDropDown = PanelUtils.AddPlainDropDown(panel, Translations.Translate("RPR_OPT_CPK"), new string[0], -1);
            packDropDown.parent.relativePosition = new Vector3(20f, PackMenuY);

            // Headings.
            PanelUtils.ColumnLabel(panel, FloorHeightX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FLH"), 1.0f);
            PanelUtils.ColumnLabel(panel, AreaPerX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_APU"), 1.0f);
            PanelUtils.ColumnLabel(panel, FirstMinX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FMN"), 1.0f);
            PanelUtils.ColumnLabel(panel, FirstMaxX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FMX"), 1.0f);
            PanelUtils.ColumnLabel(panel, FirstEmptyX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_IGF"), 1.0f);
            PanelUtils.ColumnLabel(panel, MultiFloorX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_MFU"), 1.0f);

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

                firstExtraFields[i] = AddTextField(panel, TextFieldWidth, FirstMaxX + Margin, currentY);
                firstExtraFields[i].eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);

                firstEmptyCheck[i] = AddCheckBox(panel, FirstEmptyX + (ColumnWidth / 2), currentY);
                multiFloorCheck[i] = AddCheckBox(panel, MultiFloorX + (ColumnWidth / 2), currentY);

                // Move to next row.
                currentY += RowHeight;
            }

            // Additional space before name textfield.
            currentY += RowHeight;

            // Pack name textfield.
            packNameField = UIUtils.CreateTextField(panel, 200f, 30f);
            UILabel packNameLabel = PanelUtils.AddLabel(packNameField, Translations.Translate("RPR_OPT_EDT_NAM"));
            packNameLabel.relativePosition = new Vector3(-100f, (packNameField.height - packNameLabel.height) / 2);
            packNameField.relativePosition = new Vector3(140f, currentY);
            packNameField.padding = new RectOffset(6, 6, 6, 6);
            packNameField.isEnabled = false;

            // Space for buttons.
            currentY += 50f;

            // 'Add new' button.
            UIButton addNewButton = UIUtils.CreateButton(panel, 200f);
            addNewButton.relativePosition = new Vector3(20f, currentY);
            addNewButton.text = Translations.Translate("RPR_OPT_NEW");
            addNewButton.eventClicked += (control, clickEvent) =>
            {
                // Default new pack name.
                string basePackName = Translations.Translate("RPR_OPT_NPK");
                string newPackName = basePackName;

                // Integer suffix for when the above name already exists (starts with 2).
                int packNum = 2;

                // Current service.
                ItemClass.Service currentService = services[serviceDropDown.selectedIndex];

                // Starting with our default new pack name, check to see if we already have a pack with this name for the currently selected service.
                while (PopData.calcPacks.Find(pack => pack.service == currentService && pack.name.Equals(newPackName)) != null)
                {
                    // We already have a match for this name; append the current integer suffix to the base name and try again, incementing the integer suffix for the next attempt (if required).
                    newPackName = "New pack " + packNum++;
                }

                // We now have a unique name; set the textfield.
                packNameField.text = newPackName;

                // New pack to add.
                VolumetricPack newPack = new VolumetricPack();

                // Update pack with information from the panel.
                UpdatePack(newPack);

                // Add our new pack to our list of packs and update defaults panel menus.
                PopData.AddCalculationPack(newPack);
                DefaultsPanel.instance.UpdateMenus();

                // Save configuration file. 
                ConfigUtils.SaveSettings();

                // Update pack menu.
                packDropDown.items = PackList(currentService);

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
            };

            // Save pack button.
            UIButton saveButton = UIUtils.CreateButton(panel, 200f);
            saveButton.relativePosition = new Vector3(250f, currentY);
            saveButton.text = Translations.Translate("RPR_OPT_SAA");

            // Event handler.
            saveButton.eventClicked += (control, clickEvent) =>
            {
                // Basic sanity checks - need a valid name and service to proceed.
                if (packNameField.text != null && serviceDropDown.selectedIndex >= 0)
                {
                    // Update currently selected pack with information from the panel.
                    UpdatePack(packList[packDropDown.selectedIndex]);

                    // Update selected menu item in case the name has changed.
                    packDropDown.items[packDropDown.selectedIndex] = packList[packDropDown.selectedIndex].displayName ?? packList[packDropDown.selectedIndex].name;

                    // Save configuration file.
                    ConfigUtils.SaveSettings();
                }
            };
            
            // Delete pack button.
            UIButton deleteButton = UIUtils.CreateButton(panel, 200f);
            deleteButton.relativePosition = new Vector3(480f, currentY);
            deleteButton.text = Translations.Translate("RPR_OPT_DEL");
            deleteButton.eventClicked += (control, clickEvent) =>
            {
                // Make sure it's not an inbuilt pack before proceeding.
                if (packList[packDropDown.selectedIndex].version == (int)DataVersion.customOne)
                {
                    // Remove from list of packs.
                    PopData.calcPacks.Remove(packList[packDropDown.selectedIndex]);

                    // Regenerate pack menu.
                    packDropDown.items = PackList(services[serviceDropDown.selectedIndex]);

                    // Reset pack menu index.
                    packDropDown.selectedIndex = 0;

                }
            };

            // Pack menu event handler.
            packDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                // Populate text fields.
                PopulateTextFields(index);

                // Set service menu by iterating through list of services looking for a match.
                for (int i = 0; i < services.Length; ++i)
                {
                    if (services[i] == packList[index].service)
                    {
                        // Found a service match; select it and stop looping.
                        serviceDropDown.selectedIndex = i;
                        break;
                    }
                }

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
            };

            // Service drop down event handler
            serviceDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                // Set textfield visibility depending on level.
                TextfieldVisibility(maxLevels[index]);

                // Reset pack menu items.
                packDropDown.items = PackList(services[index]);

                // Reset pack selection.  Pack drop down event handler will deal with button states.
                packDropDown.selectedIndex = 0;
            };

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
                    firstExtraFields[i].Show();
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
                    firstExtraFields[i].Hide();
                    firstEmptyCheck[i].Hide();
                    multiFloorCheck[i].Hide();
                }
            }
        }


        /// <summary>
        /// Updates the given calculation pack with data from the panel.
        /// </summary>
        /// <param name="pack">Pack to update</param>
        private void UpdatePack(VolumetricPack pack)
        {
            // Basic pack attributes.
            pack.name = packNameField.text;
            pack.displayName = packNameField.text;
            pack.version = (int)DataVersion.customOne;
            pack.service = services[serviceDropDown.selectedIndex];
            pack.levels = new LevelData[maxLevels[serviceDropDown.selectedIndex]];

            // Iterate through each level, parsing input fields.
            for (int i = 0; i < maxLevels[serviceDropDown.selectedIndex]; ++i)
            {
                // Textfields.
                PanelUtils.ParseFloat(ref pack.levels[i].floorHeight, floorHeightFields[i].text);
                PanelUtils.ParseInt(ref pack.levels[i].areaPer, areaPerFields[i].text);
                PanelUtils.ParseFloat(ref pack.levels[i].firstFloorMin, firstMinFields[i].text);
                PanelUtils.ParseFloat(ref pack.levels[i].firstFloorExtra, firstExtraFields[i].text);

                // Checkboxes.
                pack.levels[i].firstFloorEmpty = firstEmptyCheck[i].isChecked;
                pack.levels[i].multiFloorUnits = multiFloorCheck[i].isChecked;
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
            packNameField.text = volPack.displayName ?? volPack.name;

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
                firstExtraFields[i].text = level.firstFloorExtra.ToString();
                firstEmptyCheck[i].isChecked = level.firstFloorEmpty;
                multiFloorCheck[i].isChecked = level.multiFloorUnits;
            }
        }


        /// <summary>
        /// (Re)builds the list of available packs.
        /// </summary>
        /// <param name="service">Service index</param>
        /// <returns>String array of custom pack names, in order (suitable for use as dropdown menu items)</returns>
        private string[] PackList(ItemClass.Service service)
        {
            // Re-initialise pack list.
            packList = new List<VolumetricPack>();
            List<string> packNames = new List<string>();

            // Iterate through all available packs.
            foreach (CalcPack calcPack in PopData.calcPacks)
            {
                // Check for custom packs.
                if (calcPack is VolumetricPack volPack && volPack.service == service)
                {
                    // Found onw - add to our lists.
                    packList.Add(volPack);
                    packNames.Add(volPack.displayName ?? volPack.name);
                }
            }

            return packNames.ToArray();
        }
    }
}