using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    public class UIModCalcs : UIPanel
    {
        // Panel components.
        private UILabel title;
        private UILegacyCalcs legacyPanel;
        private UIVolumetricPanel volumetricPanel;
        private UIDropDown packMenu;
        private UILabel packDescription;

        // Data.
        private CalcPack[] availablePacks;

        // Current selections.
        private BuildingInfo currentBuilding;
        private CalcPack currentPack;


        /// <summary>
        /// Create the mod calcs panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        public void Setup()
        {
            // Basic setup.
            clipChildren = true;

            // Title.
            title = this.AddUIComponent<UILabel>();
            title.relativePosition = new Vector3(0, 0);
            title.textAlignment = UIHorizontalAlignment.Center;
            title.text = "Mod calculations";
            title.textScale = 1.2f;
            title.autoSize = false;
            title.width = this.width;

            // Volumetric calculations panel.
            volumetricPanel = this.AddUIComponent<UIVolumetricPanel>();
            volumetricPanel.relativePosition = new Vector3(0, title.height + 80f);
            volumetricPanel.height = this.height - title.height + 80f;
            volumetricPanel.width = this.width;
            volumetricPanel.Setup();

            // Legacy calculations panel - copy volumetric calculations panel.
            legacyPanel = this.AddUIComponent<UILegacyCalcs>();
            legacyPanel.relativePosition = volumetricPanel.relativePosition;
            legacyPanel.height = volumetricPanel.height;
            legacyPanel.width = volumetricPanel.width;
            legacyPanel.Setup();
            legacyPanel.Hide();

            // Preset dropdown.
            packMenu = PanelUtils.LabelledDropDown(this, Translations.Translate("RPR_PCK_NAM"), yPos: title.height + 5f);

            // Preset description.
            packDescription = this.AddUIComponent<UILabel>();
            packDescription.relativePosition = new Vector3(10f, packMenu.relativePosition.y + packMenu.height + 15f);
            packDescription.autoSize = false;
            packDescription.autoHeight = true;
            packDescription.wordWrap = true;
            packDescription.textScale = 0.7f;
            packDescription.width = legacyPanel.width - 20f;

            // Dropdown event handler.
            packMenu.eventSelectedIndexChanged += (component, index) => UpdatePackSelection(index);
        }


        /// <summary>
        /// Updates the selection to the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number (from menu) of selection pack</param>
        public void UpdatePackSelection(int index)
        {
            // Update selected pack.
            currentPack = availablePacks[index];

            // Update description.
            packDescription.text = currentPack.description;

            // Update building setting and save.
            PopData.UpdateBuildingPack(currentBuilding, currentPack);
            ConfigUtils.SaveSettings();

            // Check if we're using legacy or volumetric data.
            if (currentPack is VolumetricPack)
            {
                // Volumetric pack.
                // Update panel with new calculations.
                LevelData thisLevel = GetFloorData();
                volumetricPanel.UpdateText(thisLevel);
                volumetricPanel.CalculateVolumetric(currentBuilding, thisLevel);

                // Set visibility.
                legacyPanel.Hide();
                volumetricPanel.Show();
            }
            else
            {
                // Set visibility.
                volumetricPanel.Hide();
                legacyPanel.Show();
            }
        }


        /// <summary>
        /// Returns the level data record from the current floor pack that's relevant to the selected building's level.
        /// </summary>
        /// <returns>Floordata instance for building</returns>
        private LevelData GetFloorData() => ((VolumetricPack)currentPack).levels[(int)currentBuilding.GetClassLevel()];


        /// <summary>
        /// Called whenever the currently selected building is changed to update the panel display.
        /// </summary>
        /// <param name="building">Newly selected building</param>
        public void SelectionChanged(BuildingInfo building)
        {
            // Set current building.
            currentBuilding = building;

            // Safety first!
            if (currentBuilding != null)
            {
                // Get available packs for this building.
                availablePacks = PopData.GetPacks(building);

                // Get current and default packs for this item
                CalcPack currentPack = PopData.ActivePack(building);
                CalcPack defaultPack = PopData.CurrentDefaultPack(building);

                // Build preset menu.
                packMenu.items = new string[availablePacks.Length];
                for (int i = 0; i < packMenu.items.Length; ++i)
                {
                    packMenu.items[i] = availablePacks[i].displayName;

                    // Check for deefault name match,
                    if (availablePacks[i].name.Equals(defaultPack.name))
                    {
                        packMenu.items[i] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (availablePacks[i].Equals(currentPack))
                    {
                        packMenu.selectedIndex = i;

                        // Force pack selection update.
                        UpdatePackSelection(i);
                    }
                }

                // Update legacy panel for private building AIs (volumetric panel is updated by menu selection change above).
                if (building.GetAI() is PrivateBuildingAI)
                {
                    legacyPanel.SelectionChanged(building);
                }
            }
        }
    }
}
