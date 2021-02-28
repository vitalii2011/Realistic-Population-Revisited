using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default calculation packs.
    /// </summary>
    internal class DefaultsPanel
    {
        readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_RLO"),
            Translations.Translate("RPR_CAT_RHI"),
            Translations.Translate("RPR_CAT_ERL"),
            Translations.Translate("RPR_CAT_ERH"),
            Translations.Translate("RPR_CAT_CLO"),
            Translations.Translate("RPR_CAT_CHI"),
            Translations.Translate("RPR_CAT_ORG"),
            Translations.Translate("RPR_CAT_LEI"),
            Translations.Translate("RPR_CAT_TOU"),
            Translations.Translate("RPR_CAT_OFF"),
            Translations.Translate("RPR_CAT_ITC"),
            Translations.Translate("RPR_CAT_IND"),
            Translations.Translate("RPR_CAT_FAR"),
            Translations.Translate("RPR_CAT_FOR"),
            Translations.Translate("RPR_CAT_OIL"),
            Translations.Translate("RPR_CAT_ORE"),
            Translations.Translate("RPR_CAT_SCH")
        };

        readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Office,
            ItemClass.Service.Office,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Education
        };

        readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.ResidentialLow,
            ItemClass.SubService.ResidentialHigh,
            ItemClass.SubService.ResidentialLowEco,
            ItemClass.SubService.ResidentialHighEco,
            ItemClass.SubService.CommercialLow,
            ItemClass.SubService.CommercialHigh,
            ItemClass.SubService.CommercialEco,
            ItemClass.SubService.CommercialLeisure,
            ItemClass.SubService.CommercialTourist,
            ItemClass.SubService.OfficeGeneric,
            ItemClass.SubService.OfficeHightech,
            ItemClass.SubService.IndustrialGeneric,
            ItemClass.SubService.IndustrialFarming,
            ItemClass.SubService.IndustrialForestry,
            ItemClass.SubService.IndustrialOil,
            ItemClass.SubService.IndustrialOre,
            ItemClass.SubService.None
        };

        readonly string[] iconNames =
        {
            "ZoningResidentialLow",
            "ZoningResidentialHigh",
            "IconPolicySelfsufficient",
            "IconPolicySelfsufficient",
            "ZoningCommercialLow",
            "ZoningCommercialHigh",
            "IconPolicyOrganic",
            "IconPolicyLeisure",
            "IconPolicyTourist",
            "ZoningOffice",
            "IconPolicyHightech",
            "ZoningIndustrial",
            "IconPolicyFarming",
            "IconPolicyForest",
            "IconPolicyOil",
            "IconPolicyOre",
            "ToolbarIconEducation"
        };

        readonly string[] atlasNames =
        {
            "Thumbnails",
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Thumbnails",
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Ingame",
            "Thumbnails",
            "Ingame",
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Ingame",
            "Ingame",
            "Ingame"
        };


        // DropDown menus.
        readonly UIDropDown[] popMenus, floorMenus;

        // Available packs arrays.
        readonly PopDataPack[][] availablePopPacks;
        DataPack[] availableFloorPacks;

        // Instance reference.
        internal static DefaultsPanel instance;


        /// <summary>
        /// Adds calculation options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal DefaultsPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Layout constants.
            const float Margin = 5f;
            const float LeftColumn = 270f;
            const float RightColumn = 510f;


            // Y position indicator.
            float currentY = 5f;


            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(tabStrip, Translations.Translate("RPR_OPT_DEF"), tabIndex);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = false;

            // Initialise arrays.
            availablePopPacks = new PopDataPack[subServiceNames.Length][];
            availableFloorPacks = FloorData.instance.Packs;
            popMenus = new UIDropDown[subServiceNames.Length];
            floorMenus = new UIDropDown[subServiceNames.Length];

            // Add 'Use legacy by default' checkboxes.
            UILabel legacyLabel = UIControls.AddLabel(panel, Margin, currentY, Translations.Translate("RPR_DEF_LEG"), textScale: 0.9f);
            currentY += legacyLabel.height + 5f;

            UICheckBox legacyThisSaveCheck = UIControls.LabelledCheckBox(panel, Margin * 2, currentY, Translations.Translate("RPR_DEF_LTS"));
            legacyThisSaveCheck.label.wordWrap = true;
            legacyThisSaveCheck.label.autoSize = false;
            legacyThisSaveCheck.label.width = 710f;
            legacyThisSaveCheck.label.autoHeight = true;
            legacyThisSaveCheck.isChecked = ModSettings.ThisSaveLegacy;
            legacyThisSaveCheck.eventCheckChanged += (control, isChecked) =>
            {
                ModSettings.ThisSaveLegacy = isChecked;
                UpdateMenus();
            };
            currentY += 20f;

            UICheckBox legacyNewSaveCheck = UIControls.LabelledCheckBox(panel, Margin * 2, currentY, Translations.Translate("RPR_DEF_LAS"));
            legacyNewSaveCheck.label.wordWrap = true;
            legacyNewSaveCheck.label.autoSize = false;
            legacyNewSaveCheck.label.width = 710f;
            legacyNewSaveCheck.label.autoHeight = true;
            legacyNewSaveCheck.isChecked = ModSettings.newSaveLegacy;
            legacyNewSaveCheck.eventCheckChanged += (control, isChecked) =>
            {
                ModSettings.newSaveLegacy = isChecked;
                UpdateMenus();
            };

            // Reset current Y to fixed state.
            currentY = 75f;

            // Add titles.
            UILabel popLabel = UIControls.AddLabel(panel, LeftColumn, currentY, Translations.Translate("RPR_CAL_DEN"), 220f);
            UILabel floorLabel = UIControls.AddLabel(panel, RightColumn, currentY, Translations.Translate("RPR_CAL_BFL"), 220f);
            popLabel.textAlignment = UIHorizontalAlignment.Center;
            floorLabel.textAlignment = UIHorizontalAlignment.Center;
            currentY += 25f;

            for (int i = 0; i < subServiceNames.Length; ++i)
            {
                // Pop pack dropdown.
                popMenus[i] = UIControls.AddDropDown(panel, LeftColumn, currentY + 3f);

                // Save current index in object user data.
                popMenus[i].objectUserData = i;

                // Event handler.
                popMenus[i].eventSelectedIndexChanged += (control, index) =>
                {
                    // Retrieve stored index.
                    int serviceIndex = (int)control.objectUserData;

                    // Hide floor menu if we've selected legacy calcs, otherwise show it.
                    if (availablePopPacks[serviceIndex][index].version == (int)DataVersion.legacy)
                    {
                        floorMenus[serviceIndex].Hide();
                    }
                    else
                    {
                        floorMenus[serviceIndex].Show();
                    }
                };

                // Floor pack dropdown.
                floorMenus[i] = UIControls.AddDropDown(panel, RightColumn, currentY + 3f);

                // Row icon and label.
                PanelUtils.RowHeaderIcon(panel, ref currentY, subServiceNames[i], iconNames[i], atlasNames[i], 220f);

                // Extra space.
                currentY += 3f;
            }

            // Add buttons- add extra space.
            currentY += Margin;

            // Reset button.
            UIButton resetButton = UIControls.AddButton(panel, Margin, currentY, Translations.Translate("RPR_OPT_RTD"), 150f);
            resetButton.eventClicked += ResetDefaults;

            // Revert button.
            UIButton revertToSaveButton = UIControls.AddButton(panel, (Margin * 2) + 150f, currentY, Translations.Translate("RPR_OPT_RTS"), 150f);
            revertToSaveButton.eventClicked += (component, clickEvent) => UpdateMenus();

            // Save button.
            UIButton saveButton = UIControls.AddButton(panel, (Margin * 3) + 300f, currentY, Translations.Translate("RPR_OPT_SAA"), 150f);
            saveButton.eventClicked += Apply;

            // Populate menus.
            UpdateMenus();

            // Set instance.
            instance = this;
        }


        /// <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal void UpdateMenus()
        {
            for (int i = 0; i < subServiceNames.Length; ++i)
            {
                // Save current index in object user data.
                popMenus[i].objectUserData = i;
                floorMenus[i].objectUserData = i;

                // Get available packs for this service/subservice combination.
                availablePopPacks[i] = PopData.instance.GetPacks(services[i]);
                availableFloorPacks = FloorData.instance.Packs;

                // Get current and default packs for this item.
                DataPack currentPopPack = PopData.instance.CurrentDefaultPack(services[i], subServices[i]);
                DataPack defaultPopPack = PopData.instance.BaseDefaultPack(services[i], subServices[i]);
                DataPack currentFloorPack = FloorData.instance.CurrentDefaultPack(services[i], subServices[i]);
                DataPack defaultFloorPack = FloorData.instance.BaseDefaultPack(services[i], subServices[i]);

                // Build preset menus.
                popMenus[i].items = new string[availablePopPacks[i].Length];
                floorMenus[i].items = new string[availableFloorPacks.Length];

                // Iterate through each item in pop menu.
                for (int j = 0; j < popMenus[i].items.Length; ++j)
                {
                    // Set menu item text.
                    popMenus[i].items[j] = availablePopPacks[i][j].displayName;

                    // Check for deefault name match.
                    if (availablePopPacks[i][j].name.Equals(defaultPopPack.name))
                    {
                        // Match - add default postscript.
                        popMenus[i].items[j] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (availablePopPacks[i][j].name.Equals(currentPopPack.name))
                    {
                        popMenus[i].selectedIndex = j;
                    }
                }

                // Iterate through each item in floor menu.
                for (int j = 0; j < floorMenus[i].items.Length; ++j)
                {
                    // Set menu item text.
                    floorMenus[i].items[j] = availableFloorPacks[j].displayName;

                    // Check for deefault name match.
                    if (availableFloorPacks[j].name.Equals(defaultFloorPack.name))
                    {
                        // Match - add default postscript.
                        floorMenus[i].items[j] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (availableFloorPacks[j].name.Equals(currentFloorPack.name))
                    {
                        floorMenus[i].selectedIndex = j;
                    }
                }
            }
        }


        /// <summary>
        /// 'Revert to defaults' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        private void ResetDefaults(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Iterate through each sub-service menu.
            for (int i = 0; i < subServiceNames.Length; ++i)
            {
                // Get current and default packs for this item.
                DataPack defaultPopPack = PopData.instance.BaseDefaultPack(services[i], subServices[i]);
                DataPack defaultFloorPack = FloorData.instance.BaseDefaultPack(services[i], subServices[i]);

                // Iterate through each item in pop menu.
                for (int j = 0; j < popMenus[i].items.Length; ++j)
                {
                    // Check for deefault name match.
                    if (availablePopPacks[i][j].name.Equals(defaultPopPack.name))
                    {
                        // Match - set selection to this one.
                        popMenus[i].selectedIndex = j;
                    }
                }

                // Iterate through each item in floor menu.
                for (int j = 0; j < floorMenus[i].items.Length; ++j)
                {
                    // Check for deefault name match.
                    if (availableFloorPacks[j].name.Equals(defaultFloorPack.name))
                    {
                        // Match - set selection to this one.
                        floorMenus[i].selectedIndex = j;
                    }
                }
            }
        }


        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        private void Apply(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Iterate through each sub-service menu.
            for (int i = 0; i < subServiceNames.Length; ++i)
            {
                // Get population pack menu selected index.
                int popIndex = popMenus[i].selectedIndex;

                // Update default population dictionary for this subservice.
                PopData.instance.ChangeDefault(services[i], subServices[i], availablePopPacks[i][popIndex]);

                // Update floor data pack if we're not using legacy calculations.
                if (availablePopPacks[i][popIndex].version != (int)DataVersion.legacy)
                {
                    FloorData.instance.ChangeDefault(services[i], subServices[i], availableFloorPacks[floorMenus[i].selectedIndex]);
                }
            }

            // Clear population caches.
            PopData.instance.householdCache.Clear();
            PopData.instance.workplaceCache.Clear();

            // Clear RICO cache.
            if (ModUtils.ricoClearAllWorkplaces != null)
            {
                ModUtils.ricoClearAllWorkplaces.Invoke(null, null);
            }

            // Save settings.
            ConfigUtils.SaveSettings();
        }
    }
}