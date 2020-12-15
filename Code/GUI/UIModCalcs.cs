using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    public class UIModCalcs : UIPanel
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float ColumnWidth = 300f;
        private const float ComponentWidth = ColumnWidth - (Margin * 2f);
        private const float RightColumnX = ColumnWidth + Margin;
        private const float ColumnLabelY = 30f;
        private const float MenuY = ColumnLabelY + 20f;
        private const float DescriptionY = MenuY + 30f;
        private const float SaveY = DescriptionY + 40f;
        private const float CalcY = SaveY + 35f;

        // Panel components.
        private UILabel title;
        private UIPanel floorPanel;
        private UILegacyCalcs legacyPanel;
        private UIVolumetricPanel volumetricPanel;
        private UIDropDown popMenu, floorMenu;
        private UILabel popDescription, floorDescription;

        // Data.
        private PopDataPack[] popPacks;
        private DataPack[] floorPacks;

        // Current selections.
        private BuildingInfo currentBuilding;
        private PopDataPack currentPopPack;
        private FloorDataPack currentFloorPack;


        /// <summary>
        /// Returns the level data record from the current floor pack that's relevant to the selected building's level.
        /// </summary>
        /// <returns>Floordata instance for building</returns>
        private LevelData CurrentFloorData => ((VolumetricPopPack)currentPopPack).levels[(int)currentBuilding.GetClassLevel()];


        /// <summary>
        /// Create the mod calcs panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        internal void Setup()
        {
            // Basic setup.
            clipChildren = true;

            // Title.
            title = this.AddUIComponent<UILabel>();
            title.relativePosition = new Vector3(0, 0);
            title.textAlignment = UIHorizontalAlignment.Center;
            title.text = Translations.Translate("RPR_CAL_MOD");
            title.textScale = 1.2f;
            title.autoSize = false;
            title.width = this.width;

            // Column titles.
            UILabel densityTitle = ColumnLabel(Translations.Translate("RPR_CAL_DEN"), Margin);
            UILabel floorTitle = ColumnLabel(Translations.Translate("RPR_CAL_BFL"), RightColumnX);

            // Volumetric calculations panel.
            volumetricPanel = this.AddUIComponent<UIVolumetricPanel>();
            volumetricPanel.relativePosition = new Vector2(0f, CalcY);
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

            // Floor dropdown panel.
            floorPanel = this.AddUIComponent<UIPanel>();
            floorPanel.relativePosition = new Vector2(RightColumnX, MenuY);
            floorPanel.autoSize = true;
            floorPanel.autoLayout = false;
            floorPanel.clipChildren = false;
            floorPanel.Show();

            // Pack dropdowns.
            popMenu = UIControls.AddDropDown(this, Margin, MenuY, ComponentWidth);
            floorMenu = UIControls.AddDropDown(floorPanel, 0f, 0f, ComponentWidth);

            // Pack descriptions.
            popDescription = Description(this, Margin, DescriptionY);
            floorDescription = Description(floorPanel, 0f, DescriptionY - MenuY);

            // Apply button.
            UIButton applyButton = UIUtils.CreateButton(this, 200f);
            applyButton.relativePosition = new Vector2(ColumnWidth - (200f / 2), SaveY);
            applyButton.text = Translations.Translate("RPR_OPT_SAA");
            applyButton.eventClicked += (control, clickEvent) =>
            {
                // Update building setting and save.
                PopData.instance.UpdateBuildingPack(currentBuilding, currentPopPack);
                FloorData.instance.UpdateBuildingPack(currentBuilding, currentFloorPack);
                ConfigUtils.SaveSettings();
            };

            // Dropdown event handlers.
            popMenu.eventSelectedIndexChanged += (component, index) => UpdatePopSelection(index);
            floorMenu.eventSelectedIndexChanged += (component, index) => UpdateFloorSelection(index);
        }


        /// <summary>
        /// Updates the population calculation pack selection to the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number (from menu) of selection pack</param>
        internal void UpdatePopSelection(int index)
        {
            // Update selected pack.
            currentPopPack = popPacks[index];

            // Update description.
            popDescription.text = currentPopPack.description;

            // Check if we're using legacy or volumetric data.
            if (currentPopPack is VolumetricPopPack)
            {
                // Volumetric pack.
                // Update panel with new calculations.
                LevelData thisLevel = CurrentFloorData;
                volumetricPanel.UpdatePopText(thisLevel);
                volumetricPanel.CalculateVolumetric(currentBuilding, thisLevel, currentFloorPack);

                // Set visibility.
                legacyPanel.Hide();
                volumetricPanel.Show();
                floorMenu.Show();
                floorDescription.Show();
            }
            else
            {
                // Set visibility.
                volumetricPanel.Hide();
                floorMenu.Hide();
                floorDescription.Hide();
                legacyPanel.Show();
            }
        }


        /// <summary>
        /// Updates the floor calculation pack selection to the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number (from menu) of selection pack</param>
        internal void UpdateFloorSelection(int index)
        {
            // Update selected pack.
            currentFloorPack = (FloorDataPack)floorPacks[index];

            // Update description.
            floorDescription.text = currentFloorPack.description;

            // Update panel with new calculations.
            volumetricPanel.UpdateFloorText(currentFloorPack);
            volumetricPanel.CalculateVolumetric(currentBuilding, CurrentFloorData, currentFloorPack);

            // Communicate change with to rest of panel.
            BuildingDetailsPanel.Panel.FloorDataPack = currentFloorPack;
        }


        /// <summary>
        /// Called whenever the currently selected building is changed to update the panel display.
        /// </summary>
        /// <param name="building">Newly selected building</param>
        internal void SelectionChanged(BuildingInfo building)
        {
            // Set current building.
            currentBuilding = building;

            // Safety first!
            if (currentBuilding != null)
            {
                // Get available calculation packs for this building.
                popPacks = PopData.instance.GetPacks(building);
                floorPacks = FloorData.instance.Packs;

                // Get current and default packs for this item
                currentPopPack = (PopDataPack)PopData.instance.ActivePack(building);
                currentFloorPack = (FloorDataPack)FloorData.instance.ActivePack(building);
                PopDataPack defaultPopPack = (PopDataPack)PopData.instance.CurrentDefaultPack(building);
                FloorDataPack defaultFloorPack = (FloorDataPack)FloorData.instance.CurrentDefaultPack(building);

                // Build pop pack menu.
                popMenu.items = new string[popPacks.Length];
                for (int i = 0; i < popMenu.items.Length; ++i)
                {
                    popMenu.items[i] = popPacks[i].displayName;

                    // Check for deefault name match,
                    if (popPacks[i].name.Equals(defaultPopPack.name))
                    {
                        popMenu.items[i] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (popPacks[i].Equals(currentPopPack))
                    {
                        popMenu.selectedIndex = i;

                        // Force pack selection update.
                        UpdatePopSelection(i);
                    }
                }

                // Build floor pack menu.
                floorMenu.items = new string[floorPacks.Length];
                for (int i = 0; i < floorPacks.Length; ++i)
                {
                    floorMenu.items[i] = floorPacks[i].displayName;

                    // Check for deefault name match,
                    if (floorPacks[i].name.Equals(defaultFloorPack.name))
                    {
                        floorMenu.items[i] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (floorPacks[i].Equals(currentFloorPack))
                    {
                        floorMenu.selectedIndex = i;

                        // Force pack selection update.
                        UpdateFloorSelection(i);
                    }
                }

                // Update legacy panel for private building AIs (volumetric panel is updated by menu selection change above).
                if (building.GetAI() is PrivateBuildingAI)
                {
                    legacyPanel.SelectionChanged(building);
                }
            }
        }


        /// <summary>
        /// Adds a column header label.
        /// </summary>
        /// <param name="text">Label text</param>
        /// <param name="xPos">Label x-position</param>
        /// <returns>New column label</returns>
        private UILabel ColumnLabel(string text, float xPos)
        {
            UILabel newLabel = this.AddUIComponent<UILabel>();
            newLabel.relativePosition = new Vector3(xPos, ColumnLabelY);
            newLabel.textAlignment = UIHorizontalAlignment.Center;
            newLabel.text = text;
            newLabel.textScale = 1f;
            newLabel.autoSize = false;
            newLabel.width = ComponentWidth;

            return newLabel;
        }


        /// <summary>
        /// Adds a pack description text label.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="xPos">Label x-position</param>
        /// <param name="xPos">Label y-position</param>
        /// <returns></returns>
        private UILabel Description(UIComponent parent, float xPos, float yPos)
        {
            UILabel newLabel = parent.AddUIComponent<UILabel>();
            newLabel.relativePosition = new Vector2(xPos, yPos);
            newLabel.autoSize = false;
            newLabel.autoHeight = true;
            newLabel.wordWrap = true;
            newLabel.textScale = 0.7f;
            newLabel.width = ComponentWidth;

            return newLabel;
        }
    }
}
