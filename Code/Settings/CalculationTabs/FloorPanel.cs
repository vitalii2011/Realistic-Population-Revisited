using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for creating and editing calculation packs.
    /// </summary>
    internal class FloorPanel : CalculationPanelBase
    {
        // Constants.
        protected const float FloorHeightX = FirstItem;
        protected const float FirstMinX = FloorHeightX + ColumnWidth;
        protected const float FirstMaxX = FirstMinX + ColumnWidth;
        protected const float FirstEmptyX = FirstMaxX + ColumnWidth;
        protected const float MultiFloorX = FirstEmptyX + ColumnWidth;
        protected const float PackMenuY = 5f;
        protected const float DetailY = PackMenuY + 140f;

        // Textfield arrays.
        protected UITextField floorHeightField, firstMinField, firstExtraField;
        protected UICheckBox firstEmptyCheck;


        /// <summary>
        /// Adds editing options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal FloorPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Y position indicator.
            float currentY = DetailY;

            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(tabStrip, Translations.Translate("RPR_OPT_STO"), tabIndex);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = false;

            // Initialise arrays
            floorHeightField = new UITextField();
            firstMinField = new UITextField();
            firstExtraField = new UITextField();
            firstEmptyCheck = new UICheckBox();

            // Pack selection dropdown.
            packDropDown = UIControls.AddPlainDropDown(panel, Translations.Translate("RPR_OPT_CPK"), new string[0], -1);
            packDropDown.parent.relativePosition = new Vector3(20f, PackMenuY);

            // Headings.
            PanelUtils.ColumnLabel(panel, FloorHeightX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FLH"), Translations.Translate("RPR_CAL_VOL_FLH_TIP"), 1.0f);
            PanelUtils.ColumnLabel(panel, FirstMinX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FMN"), Translations.Translate("RPR_CAL_VOL_FMN_TIP"), 1.0f);
            PanelUtils.ColumnLabel(panel, FirstMaxX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FMX"), Translations.Translate("RPR_CAL_VOL_FMX_TIP"), 1.0f);
            PanelUtils.ColumnLabel(panel, FirstEmptyX, DetailY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_IGF"), Translations.Translate("RPR_CAL_VOL_IGF_TIP"), 1.0f);

            // Add level textfields.
            floorHeightField = UIControls.AddTextField(panel, FloorHeightX + Margin, currentY, width: TextFieldWidth);
            floorHeightField.eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);

            firstMinField = UIControls.AddTextField(panel, FirstMinX + Margin, currentY, width: TextFieldWidth);
            firstMinField.eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);

            firstExtraField = UIControls.AddTextField(panel, FirstMaxX + Margin, currentY, width: TextFieldWidth);
            firstExtraField.eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);

            firstEmptyCheck = AddCheckBox(panel, FirstEmptyX + (ColumnWidth / 2), currentY);

            // Move to next row.
            currentY += RowHeight;

            // Additional space before name textfield.
            currentY += RowHeight;

            // Pack name textfield.
            packNameField = UIControls.BigTextField(panel, 140f, currentY);
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

                // Starting with our default new pack name, check to see if we already have a pack with this name for the currently selected service.
                while (FloorData.instance.calcPacks.Find(pack => pack.name.Equals(newPackName)) != null)
                {
                    // We already have a match for this name; append the current integer suffix to the base name and try again, incementing the integer suffix for the next attempt (if required).
                    newPackName = "New pack " + packNum++;
                }

                // We now have a unique name; set the textfield.
                packNameField.text = newPackName;

                // New pack to add.
                FloorDataPack newPack = new FloorDataPack();

                // Update pack with information from the panel.
                UpdatePack(newPack);

                // Add our new pack to our list of packs and update defaults panel menus.
                FloorData.instance.AddCalculationPack(newPack);
                DefaultsPanel.instance.UpdateMenus();

                // Save configuration file. 
                ConfigUtils.SaveSettings();

                // Update pack menu.
                packDropDown.items = PackList();

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
                if (packNameField.text != null)
                {
                    // Update currently selected pack with information from the panel.
                    UpdatePack((FloorDataPack)packList[packDropDown.selectedIndex]);

                    // Save configuration file.
                    ConfigUtils.SaveSettings();

                    // Apply update.
                    FloorData.instance.CalcPackChanged(packList[packDropDown.selectedIndex]);
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

                    // Reset pack menu index.
                    packDropDown.selectedIndex = 0;
                }
            };

            // Pack menu event handler.
            packDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                // Populate text fields.
                PopulateTextFields(index);

                // Update button states.
                ButtonStates(index);
            };

            // Populate pack menu and set onitial pack selection.
            packDropDown.items = PackList();
            packDropDown.selectedIndex = 0;
        }


        /// <summary>
        /// Updates the given calculation pack with data from the panel.
        /// </summary>
        /// <param name="pack">Pack to update</param>
        private void UpdatePack(FloorDataPack pack)
        {
            // Basic pack attributes.
            pack.name = packNameField.text;
            pack.displayName = packNameField.text;
            pack.version = (int)DataVersion.customOne;

            // Textfields.
            PanelUtils.ParseFloat(ref pack.floorHeight, floorHeightField.text);
            PanelUtils.ParseFloat(ref pack.firstFloorMin, firstMinField.text);
            PanelUtils.ParseFloat(ref pack.firstFloorExtra, firstExtraField.text);

            // Checkboxes.
            pack.firstFloorEmpty = firstEmptyCheck.isChecked;

            // Update selected menu item in case the name has changed.
            packDropDown.items[packDropDown.selectedIndex] = pack.displayName ?? pack.name;

            // Update defaults panel menus.
            DefaultsPanel.instance.UpdateMenus();
        }


        /// <summary>
        /// Populates the textfields with data from the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number of calculation pack</param>
        private void PopulateTextFields(int index)
        {
            // Get local reference.
            FloorDataPack floorPack = (FloorDataPack)packList[index];

            // Set name field.
            packNameField.text = floorPack.displayName ?? floorPack.name;

            // Populate controls.
            floorHeightField.text = floorPack.floorHeight.ToString();
            firstMinField.text = floorPack.firstFloorMin.ToString();
            firstExtraField.text = floorPack.firstFloorExtra.ToString();
            firstEmptyCheck.isChecked = floorPack.firstFloorEmpty;
        }


        /// <summary>
        /// (Re)builds the list of available packs.
        /// </summary>
        /// <returns>String array of custom pack names, in order (suitable for use as dropdown menu items)</returns>
        private string[] PackList()
        {
            // Re-initialise pack list.
            packList = new List<DataPack>();
            List<string> packNames = new List<string>();

            // Iterate through all available packs.
            foreach (DataPack calcPack in FloorData.instance.calcPacks)
            {
                // Found one - add to our lists.
                packList.Add((FloorDataPack)calcPack);
                packNames.Add(calcPack.displayName ?? calcPack.name);
            }

            return packNames.ToArray();
        }
    }
}