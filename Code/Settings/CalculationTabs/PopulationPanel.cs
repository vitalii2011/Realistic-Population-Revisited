using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Options panel for creating and editing calculation packs.
    /// </summary>
    internal class PopulationPanel : CalculationPanelBase
    {
        // Constants.
        protected const float EmptyAreaX = FirstItem;
        protected const float EmptyPercentX = EmptyAreaX + ColumnWidth;
        protected const float AreaPerX = EmptyPercentX + ColumnWidth;
        protected const float MultiFloorX = AreaPerX + ColumnWidth;
        protected const float PackMenuY = 90f;
        protected const float DetailY = PackMenuY + 140f;

        string[] serviceNames = { Translations.Translate("RPR_CAT_RES"), Translations.Translate("RPR_CAT_IND"), Translations.Translate("RPR_CAT_COM"), Translations.Translate("RPR_CAT_OFF"), Translations.Translate("RPR_CAT_SCH") };
        ItemClass.Service[] services = { ItemClass.Service.Residential, ItemClass.Service.Industrial, ItemClass.Service.Commercial, ItemClass.Service.Office, ItemClass.Service.Education };
        int[] maxLevels = { 5, 3, 3, 3, 2 };

        // Textfield arrays.
        protected UITextField[] emptyAreaFields, emptyPercentFields, areaPerFields;
        protected UICheckBox[] multiFloorCheck;
        protected UILabel[] rowLabels;

        // Panel components.
        protected UIDropDown serviceDropDown;


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
            emptyAreaFields = new UITextField[5];
            emptyPercentFields = new UITextField[5];
            areaPerFields = new UITextField[5];
            multiFloorCheck = new UICheckBox[5];
            rowLabels = new UILabel[5];

            // Service selection dropdown.
            serviceDropDown = UIControls.AddPlainDropDown(panel, Translations.Translate("RPR_OPT_SVC"), serviceNames, -1);
            serviceDropDown.parent.relativePosition = new Vector3(20f, Margin);

            // Pack selection dropdown.
            packDropDown = UIControls.AddPlainDropDown(panel, Translations.Translate("RPR_OPT_CPK"), new string[0], -1);
            packDropDown.parent.relativePosition = new Vector3(20f, PackMenuY);

            // Headings.
            PanelUtils.ColumnLabel(panel, EmptyAreaX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_EMP"), 1.0f);
            PanelUtils.ColumnLabel(panel, EmptyPercentX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_EPC"), 1.0f);
            PanelUtils.ColumnLabel(panel, AreaPerX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_APU"), 1.0f);
            PanelUtils.ColumnLabel(panel, MultiFloorX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_MFU"), 1.0f);

            // Add level textfields.
            for (int i = 0; i < 5; ++i)
            {
                // Row label.
                rowLabels[i] = RowLabel(panel, currentY, Translations.Translate("RPR_OPT_LVL") + " " + (i + 1).ToString());

                emptyAreaFields[i] = UIControls.AddTextField(panel, EmptyAreaX + Margin, currentY, width: TextFieldWidth, height: 20f);
                emptyAreaFields[i].eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);

                emptyPercentFields[i] = UIControls.AddTextField(panel, EmptyPercentX + Margin, currentY, width: TextFieldWidth, height: 20f);
                emptyPercentFields[i].eventTextChanged += (control, value) => PanelUtils.IntTextFilter((UITextField)control, value);

                areaPerFields[i] = UIControls.AddTextField(panel, AreaPerX + Margin, currentY, width: TextFieldWidth, height: 20f);
                areaPerFields[i].eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);

                multiFloorCheck[i] = AddCheckBox(panel, MultiFloorX + (ColumnWidth / 2), currentY);

                // Move to next row.
                currentY += RowHeight;
            }

            // Additional space before name textfield.
            currentY += RowHeight;

            // Pack name textfield.
            packNameField = UIControls.AddTextField(panel, 140f, currentY, scale:1.0f);
            packNameField.padding = new RectOffset(6, 6, 6, 6);
            packNameField.isEnabled = false;
            UILabel packNameLabel = UIControls.AddLabel(packNameField, -100f, (packNameField.height - 18f) / 2, Translations.Translate("RPR_OPT_EDT_NAM"));

            // Space for buttons.
            currentY += 50f;

            // 'Add new' button.
            UIButton addNewButton = UIControls.AddButton(panel, 20f, currentY, Translations.Translate("RPR_OPT_NEW"));
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
                while (PopData.instance.calcPacks.Find(pack => ((PopDataPack)pack).service == currentService && pack.name.Equals(newPackName)) != null)
                {
                    // We already have a match for this name; append the current integer suffix to the base name and try again, incementing the integer suffix for the next attempt (if required).
                    newPackName = "New pack " + packNum++;
                }

                // We now have a unique name; set the textfield.
                packNameField.text = newPackName;

                // New pack to add.
                VolumetricPopPack newPack = new VolumetricPopPack();

                // Update pack with information from the panel.
                UpdatePack(newPack);

                // Add our new pack to our list of packs and update defaults panel menus.
                PopData.instance.AddCalculationPack(newPack);
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
            saveButton = UIControls.AddButton(panel, 250f, currentY, Translations.Translate("RPR_OPT_SAA"));

            // Event handler.
            saveButton.eventClicked += (control, clickEvent) =>
            {
                // Basic sanity checks - need a valid name and service to proceed.
                if (packNameField.text != null && serviceDropDown.selectedIndex >= 0)
                {
                    // Update currently selected pack with information from the panel.
                    UpdatePack((VolumetricPopPack)packList[packDropDown.selectedIndex]);

                    // Update selected menu item in case the name has changed.
                    packDropDown.items[packDropDown.selectedIndex] = packList[packDropDown.selectedIndex].displayName ?? packList[packDropDown.selectedIndex].name;

                    // Save configuration file.
                    ConfigUtils.SaveSettings();

                    // Apply update.
                    PopData.instance.CalcPackChanged(packList[packDropDown.selectedIndex]);
                }
            };

            // Delete pack button.
            deleteButton = UIControls.AddButton(panel, 480f, currentY, Translations.Translate("RPR_OPT_DEL"));
            deleteButton.eventClicked += (control, clickEvent) =>
            {
                // Make sure it's not an inbuilt pack before proceeding.
                if (packList[packDropDown.selectedIndex].version == (int)DataVersion.customOne)
                {
                    // Remove from list of packs.
                    PopData.instance.calcPacks.Remove(packList[packDropDown.selectedIndex]);

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
                    if (services[i] == ((VolumetricPopPack)packList[index]).service)
                    {
                        // Found a service match; select it and stop looping.
                        serviceDropDown.selectedIndex = i;
                        break;
                    }
                }

                // Update button states.
                ButtonStates(index);
            };

            // Service drop down event handler
            serviceDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                // Set textfield visibility depending on level.
                TextfieldVisibility(maxLevels[index]);

                // Reset pack menu items.
                packDropDown.items = PackList(services[index]);

                // Reset pack selection and force update of fields and button states.
                packDropDown.selectedIndex = 0;
                PopulateTextFields(0);
                ButtonStates(0);
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
                    emptyAreaFields[i].Show();
                    emptyPercentFields[i].Show();
                    areaPerFields[i].Show();
                    multiFloorCheck[i].Show();
                }
                else
                {
                    // Otherwise, hide.
                    rowLabels[i].Hide();
                    emptyAreaFields[i].Hide();
                    emptyPercentFields[i].Hide();
                    areaPerFields[i].Hide();
                    multiFloorCheck[i].Hide();
                }
            }
        }



        /// <summary>
        /// Updates the given calculation pack with data from the panel.
        /// </summary>
        /// <param name="pack">Pack to update</param>
        private void UpdatePack(VolumetricPopPack pack)
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
                PanelUtils.ParseFloat(ref pack.levels[i].emptyArea, emptyAreaFields[i].text);
                PanelUtils.ParseInt(ref pack.levels[i].emptyPercent, emptyPercentFields[i].text);
                PanelUtils.ParseFloat(ref pack.levels[i].areaPer, areaPerFields[i].text);

                // Checkboxes.
                pack.levels[i].multiFloorUnits = multiFloorCheck[i].isChecked;
            }
        }


        /// <summary>
        /// Populates the textfields with data from the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number of calculation pack</param>
        private void PopulateTextFields(int index)
        {
            // Get local reference.
            VolumetricPopPack volPack = (VolumetricPopPack)packList[index];

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
                emptyAreaFields[i].text = level.emptyArea.ToString();
                emptyPercentFields[i].text = level.emptyPercent.ToString();
                areaPerFields[i].text = level.areaPer.ToString();
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
            packList = new List<DataPack>();
            List<string> packNames = new List<string>();

            // Iterate through all available packs.
            foreach (PopDataPack calcPack in PopData.instance.calcPacks)
            {
                // Check for custom packs.
                if (calcPack is VolumetricPopPack volPack && volPack.service == service)
                {
                    // Found one - add to our lists.
                    packList.Add(volPack);
                    packNames.Add(volPack.displayName ?? volPack.name);
                }
            }

            return packNames.ToArray();
        }
    }
}