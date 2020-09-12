using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Options panel for setting default calculation packs.
    /// </summary>
    internal class CalculationsPanel
    {
        string[] subServiceNames =
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
            Translations.Translate("RPR_CAT_ORE")
        };

        ItemClass.Service[] services =
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
            ItemClass.Service.Industrial
        };

        ItemClass.SubService[] subServices =
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
            ItemClass.SubService.IndustrialOre
        };

        string[] iconNames =
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
            "IconPolicyOre"
        };

        string[] atlasNames =
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
            "Ingame"
        };


        // DropDown menus.
        UIDropDown[] packMenus;

        // Available packs arrays.
        CalcPack[][] availablePacks;

        // Instance reference.
        internal static CalculationsPanel instance;


        /// <summary>
        /// Adds calculation options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CalculationsPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Y position indicator.
            float currentY = 30f;


            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(tabStrip, Translations.Translate("RPR_OPT_CAL"), tabIndex);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = false;

            // Don't initialise PopData if we've already done it, but make sure we do it if we haven't already.
            if (!PopData.ready)
            {
                PopData.Setup();

                // Load (volumetric) building settings file.
                ConfigUtils.LoadSettings();
            }

            // Initialise arrays.
            availablePacks = new CalcPack[subServiceNames.Length][];
            packMenus = new UIDropDown[subServiceNames.Length];

            for (int i = 0; i < subServiceNames.Length; ++i)
            {
                // Preset dropdown.
                packMenus[i] = PanelUtils.AddDropDown(panel, 270f, currentY + 3f);

                // Save current index in object user data.
                packMenus[i].objectUserData = i;

                // Event handler.
                packMenus[i].eventSelectedIndexChanged += (control, index) =>
                {
                    // Retrieve stored index.
                    int serviceIndex = (int)control.objectUserData;

                    // Update service dictionary.
                    PopData.AddService(services[serviceIndex], subServices[serviceIndex], availablePacks[serviceIndex][index]);

                    // Save settings.
                    ConfigUtils.SaveSettings();
                };

                // Header and icon.
                PanelUtils.RowHeaderIcon(panel, ref currentY, subServiceNames[i], iconNames[i], atlasNames[i]);

                // Extra space.
                currentY += 5f;
            }

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
                packMenus[i].objectUserData = i;

                // Get available packs for this service/subservice combination.
                availablePacks[i] = PopData.GetPacks(services[i], subServices[i]);

                // Get current and default packs for this item
                CalcPack currentPack = PopData.CurrentDefaultPack(services[i], subServices[i]);
                CalcPack defaultPack = PopData.BaseDefaultPack(services[i], subServices[i]);

                // Build preset menu.
                packMenus[i].items = new string[availablePacks[i].Length];

                // Iterate through each item.
                for (int j = 0; j < packMenus[i].items.Length; ++j)
                {
                    // Set menu item text.
                    packMenus[i].items[j] = availablePacks[i][j].displayName;

                    // Check for deefault name match.
                    if (availablePacks[i][j].name.Equals(defaultPack.name))
                    {
                        // Match - add default postscript.
                        packMenus[i].items[j] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (availablePacks[i][j].Equals(currentPack))
                    {
                        packMenus[i].selectedIndex = j;
                    }
                }
            }
        }
    }
}