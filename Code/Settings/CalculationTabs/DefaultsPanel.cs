using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Options panel for setting default calculation packs.
    /// </summary>
    internal class DefaultsPanel
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
            Translations.Translate("RPR_CAT_ORE"),
            Translations.Translate("RPR_CAT_SCH")
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
            ItemClass.Service.Industrial,
            ItemClass.Service.Education
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
            ItemClass.SubService.IndustrialOre,
            ItemClass.SubService.None
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
            "IconPolicyOre",
            "ToolbarIconEducation"
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
            "Ingame",
            "Ingame"
        };


        // DropDown menus.
        UIDropDown[] popMenus, floorMenus;

        // Available packs arrays.
        PopDataPack[][] availablePopPacks;
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
            const float LeftColumn = 270f;
            const float RightColumn = 510f;


            // Y position indicator.
            float currentY = 30f;


            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(tabStrip, Translations.Translate("RPR_OPT_DEF"), tabIndex);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = false;

            // Initialise arrays.
            availablePopPacks = new PopDataPack[subServiceNames.Length][];
            availableFloorPacks = FloorData.instance.Packs;
            popMenus = new UIDropDown[subServiceNames.Length];
            floorMenus = new UIDropDown[subServiceNames.Length];


            // Add titles.
            UILabel popLabel = UIControls.AddLabel(panel, Translations.Translate("RPR_CAL_DEN"), LeftColumn, 5f, 220f);
            UILabel floorLabel = UIControls.AddLabel(panel, Translations.Translate("RPR_CAL_BFL"), RightColumn, 5f, 220f);
            popLabel.textAlignment = UIHorizontalAlignment.Center;
            floorLabel.textAlignment = UIHorizontalAlignment.Center;

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

                    // Update service dictionary.
                    PopData.instance.ChangeDefault(services[serviceIndex], subServices[serviceIndex], availablePopPacks[serviceIndex][index]);

                    // Save settings.
                    ConfigUtils.SaveSettings();
                };

                // Floor pack dropdown.
                floorMenus[i] = UIControls.AddDropDown(panel, RightColumn, currentY + 3f);

                // Save current index in object user data.
                floorMenus[i].objectUserData = i;

                // Event handler.
                floorMenus[i].eventSelectedIndexChanged += (control, index) =>
                {
                    // Retrieve stored index.
                    int serviceIndex = (int)control.objectUserData;

                    // Update service dictionary.
                    FloorData.instance.ChangeDefault(services[serviceIndex], subServices[serviceIndex], availableFloorPacks[index]);

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
                popMenus[i].objectUserData = i;
                floorMenus[i].objectUserData = i;

                // Get available packs for this service/subservice combination.
                availablePopPacks[i] = PopData.instance.GetPacks(services[i], subServices[i]);

                // Get current and default packs for this item
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

                // Iterate throiugh each item in floor menu.
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
    }
}