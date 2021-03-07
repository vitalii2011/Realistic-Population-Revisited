using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default calculation packs.
    /// </summary>
    internal class ResDefaultsPanel : DefaultsPanel
    {
        // Service/subservice arrays.
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_RLO"),
            Translations.Translate("RPR_CAT_RHI"),
            Translations.Translate("RPR_CAT_ERL"),
            Translations.Translate("RPR_CAT_ERH")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Residential
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.ResidentialLow,
            ItemClass.SubService.ResidentialHigh,
            ItemClass.SubService.ResidentialLowEco,
            ItemClass.SubService.ResidentialHighEco
        };

        private readonly string[] iconNames =
        {
            "ZoningResidentialLow",
            "ZoningResidentialHigh",
            "IconPolicySelfsufficient",
            "IconPolicySelfsufficient"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails",
            "Thumbnails",
            "Ingame",
            "Ingame"
        };

        private readonly int[] tabIconIndexes =
        {
            0, 1, 2, 3
        };

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;
        protected override int[] TabIcons => tabIconIndexes;

        // Legacy settings link.
        protected override bool LegacyCategory { get => ModSettings.ThisSaveLegacyRes; set => ModSettings.ThisSaveLegacyRes = value; }
        protected override string LegacyCheckLabel => "RPR_DEF_LGR";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal ResDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Sets up the defaults dropdown menus.
        /// </summary>
        /// <param name="panel">Parent panel</param>
        /// <param name="currentY">Current Y position</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
        protected override float SetUpMenus(UIPanel panel)
        {
            // Layout constants.
            const float LeftColumn = 200f;
            const float RowHeight = 30f;
            const float MenuWidth = 300f;
            const float ButtonX = LeftColumn + MenuWidth + (Margin * 2);
            const float ButtonHeight = 25f;

            // Starting y position.
            float currentY = 90f;

            for (int i = 0; i < SubServiceNames.Length; ++i)
            {

                // Row icon and label.
                PanelUtils.RowHeaderIcon(panel, ref currentY, SubServiceNames[i], IconNames[i], AtlasNames[i]);

                // Apply button label.
                UIControls.AddLabel(panel, ButtonX, currentY - 20f, Translations.Translate("RPR_CAL_SAT"));

                // Pop pack dropdown.
                popMenus[i] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAL_DEN"), MenuWidth, false);

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

                // Add 'apply to new buildings' button level with population pack dropdown.
                UIButton applyNewButton = UIControls.AddButton(panel, ButtonX, currentY, Translations.Translate("RPR_CAL_NBD"), 200f, ButtonHeight, 0.8f);
                applyNewButton.objectUserData = i;
                applyNewButton.eventClicked += ApplyToNew;

                // Floor pack on next row.
                currentY += RowHeight;

                // Floor pack dropdown.
                floorMenus[i] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAL_BFL"), MenuWidth, false);

                // Add 'apply to existing buildings' button level with floor pack dropdown.
                UIButton applyExistButton = UIControls.AddButton(panel, ButtonX, currentY, Translations.Translate("RPR_CAL_ABD"), 200f, ButtonHeight, 0.8f);
                applyExistButton.objectUserData = i;
                applyExistButton.eventClicked += ApplyToAll;


                // Next row.
                currentY += RowHeight + Margin;
            }

            // Return finishing Y position.
            return currentY;
        }


        /// <summary>
        /// 'Apply to new buildings only' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        private void ApplyToNew(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Extract subservice index from this control's object user data.
            if (control.objectUserData is int subServiceIndex)
            {
                // Status flag.
                bool isDirty = false;

                // Local references.
                ItemClass.Service service = services[subServiceIndex];
                ItemClass.SubService subService = subServices[subServiceIndex];

                // Get selected population pack.
                int popIndex = popMenus[subServiceIndex].selectedIndex;
                PopDataPack selectedPopPack = availablePopPacks[subServiceIndex][popIndex];

                // Check to see if this is a change from the current default.
                if (!PopData.instance.CurrentDefaultPack(service, subService).name.Equals(selectedPopPack.name))
                {
                    // A change has been confirmed - update default population dictionary for this subservice.
                    PopData.instance.ChangeDefault(service, subService, selectedPopPack);

                    // Set status (we've changed the pack).
                    isDirty = true;
                }

                // Check floor pack if we're not using legacy calcs.
                if (selectedPopPack.version != (int)DataVersion.legacy && availableFloorPacks[floorMenus[subServiceIndex].selectedIndex] is FloorDataPack selectedFloorPack)
                {
                    // Not legacy - check to see if this is a change from the current default.
                    if (!FloorData.instance.CurrentDefaultPack(service, subService).name.Equals(selectedFloorPack.name))
                    {
                        // A change has been confirmed - update default population dictionary for this subservice.
                        FloorData.instance.ChangeDefault(service, subService, selectedFloorPack);

                        // Set status (we've changed the pack).
                        isDirty = true;
                    }
                }

                // Did we make a change?
                if (isDirty)
                {
                    // Yes - clear population caches.
                    PopData.instance.householdCache.Clear();
                    PopData.instance.workplaceCache.Clear();

                    // Save settings.
                    ConfigUtils.SaveSettings();
                }
            }
            else
            {
                Logging.Error("ApplyToNew invalid objectUserData from control ", control.name);
            }
        }


        /// <summary>
        /// 'Apply to all buildings' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        private void ApplyToAll(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Extract subservice index from this control's object user data.
            if (control.objectUserData is int subServiceIndex)
            {
                // Apply any changed settings.
                ApplyToNew(control, mouseEvent);

                // Update existing households.
                ItemClass.SubService subService = subServices[subServiceIndex];
                Logging.Message("new residential defaults applied; updating populations of all existing residential buildings with subservice ", subService.ToString());
                PopData.instance.UpdateHouseholds(null, subService);
            }
        }
    }
}