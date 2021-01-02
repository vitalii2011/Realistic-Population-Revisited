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
        private const float LabelHeight = 20f;
        private const float MenuHeight = 30f;
        private const float DescriptionHeight = 40f;
        private const float ColumnLabelY = 30f;
        private const float MenuY = ColumnLabelY + LabelHeight;
        private const float DescriptionY = MenuY + MenuHeight;
        private const float BaseSaveY = DescriptionY + DescriptionHeight;
        private const float BaseCalcY = BaseSaveY + 35f;
        private const float SchoolSaveY = BaseSaveY + LabelHeight + MenuHeight + DescriptionHeight;
        private const float SchoolCalcY = SchoolSaveY + 35f;
        private const float ButtonWidth = 200f;
        private const float ApplyX = ColumnWidth - (ButtonWidth / 2);
        private const float Row2LabelY = DescriptionY + DescriptionHeight;

        // Panel components.
        private UILabel title;
        private UIPanel floorPanel, schoolPanel;
        private UILegacyCalcs legacyPanel;
        private UIVolumetricPanel volumetricPanel;
        private UIDropDown popMenu, floorMenu, schoolMenu;
        private UILabel popDescription, floorDescription, schoolDescription, floorOverrideLabel;
        private UIButton applyButton;

        // Data arrays.
        private PopDataPack[] popPacks;
        private DataPack[] floorPacks;
        private SchoolDataPack[] schoolPacks;

        // Current selections.
        private BuildingInfo currentBuilding;
        private PopDataPack currentPopPack;
        private FloorDataPack currentFloorPack, currentFloorOverride;
        private SchoolDataPack currentSchoolPack;

        // Flags.
        private bool usingLegacy;


        /// <summary>
        /// Sets the a floor data manual override for previewing.
        /// </summary>
        internal FloorDataPack OverrideFloors
        {
            set
            {
                // Store override.
                currentFloorOverride = value;

                // Don't do anything else if we're using legacy calculations.
                if (usingLegacy)
                {
                    return;
                }

                // Floor data pack to display.
                FloorDataPack displayPack;

                // If value is null (no override), show floor panel and display current floor pack data; otherwise, hide the floor panel and show the provided override data.
                if (value == null)
                {
                    displayPack = currentFloorPack;
                    floorOverrideLabel.Hide();
                    floorPanel.Show();
                }
                else
                {
                    // Valid override - hide floor panel.
                    floorPanel.Hide();

                    // Set override text label and show it.
                    floorOverrideLabel.text = Translations.Translate("RPR_CAL_FOV");
                    floorOverrideLabel.Show();

                    // Display figures for override, not current floor pack.
                    displayPack = value;
                }

                // Update panel with new calculations.
                volumetricPanel.UpdateFloorText(displayPack);
                volumetricPanel.CalculateVolumetric(currentBuilding, CurrentLevelData, displayPack, currentSchoolPack);
            }
        }
        

        /// <summary>
        /// /// Returns the level data record from the current floor pack that's relevant to the selected building's level.
        /// /// </summary>
        private LevelData CurrentLevelData => ((VolumetricPopPack)currentPopPack).levels[(int)currentBuilding.GetClassLevel()];


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
            UILabel densityTitle = ColumnLabel(this, Translations.Translate("RPR_CAL_DEN"), Margin, ColumnLabelY);
            UILabel floorTitle = ColumnLabel(this, Translations.Translate("RPR_CAL_BFL"), RightColumnX, ColumnLabelY);

            // Volumetric calculations panel.
            volumetricPanel = this.AddUIComponent<UIVolumetricPanel>();
            volumetricPanel.relativePosition = new Vector2(0f, BaseCalcY);
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

            // Floor override label (for when floor dropdown menu is hidden).
            floorOverrideLabel = UIControls.AddLabel(this, Translations.Translate("RPR_CAL_FOV"), RightColumnX, MenuY, this.width - RightColumnX, 0.7f);
            floorOverrideLabel.Hide();

            // Pack dropdowns.
            popMenu = UIControls.AddDropDown(this, Margin, MenuY, ComponentWidth);
            floorMenu = UIControls.AddDropDown(floorPanel, 0f, 0f, ComponentWidth);

            // School dropdown panel.
            schoolPanel = this.AddUIComponent<UIPanel>();
            schoolPanel.relativePosition = new Vector2(Margin, Row2LabelY);
            schoolPanel.autoSize = true;
            schoolPanel.autoLayout = false;
            schoolPanel.clipChildren = false;

            // School panel title and dropdown menu.
            UILabel schoolTitle = ColumnLabel(schoolPanel, Translations.Translate("RPR_CAL_SCH"), 0, 0);
            schoolMenu = UIControls.AddDropDown(schoolPanel, 0f, LabelHeight, ComponentWidth);
            schoolPanel.Hide();

            // Pack descriptions.
            popDescription = Description(this, Margin, DescriptionY);
            floorDescription = Description(floorPanel, 0f, DescriptionY - MenuY);
            schoolDescription = Description(schoolPanel, 0f, LabelHeight + DescriptionY - MenuY);

            // Apply button.
            applyButton = UIUtils.CreateButton(this, ButtonWidth);
            applyButton.relativePosition = new Vector2(ApplyX, BaseSaveY);
            applyButton.text = Translations.Translate("RPR_OPT_SAA");
            applyButton.eventClicked += (control, clickEvent) =>
            {
                // Update building setting and save.
                PopData.instance.UpdateBuildingPack(currentBuilding, currentPopPack);
                FloorData.instance.UpdateBuildingPack(currentBuilding, currentFloorPack);
                // Make sure SchoolData is called AFTER student count is settled via Pop and Floor packs, so it can work from updated data.
                SchoolData.instance.UpdateBuildingPack(currentBuilding, currentSchoolPack);
                ConfigUtils.SaveSettings();

                // Refresh the selection list (to make sure settings checkboxes reflect new state).
                BuildingDetailsPanel.Panel.RefreshList();
            };

            // Dropdown event handlers.
            popMenu.eventSelectedIndexChanged += (component, index) => UpdatePopSelection(index);
            floorMenu.eventSelectedIndexChanged += (component, index) => UpdateFloorSelection(index);
            schoolMenu.eventSelectedIndexChanged += (component, index) => UpdateSchoolSelection(index);
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
                // Volumetric pack.  Are we coming from a legacy setting?
                if (usingLegacy)
                {
                    // Reset flag.
                    usingLegacy = false;

                    // Restore floor rendering.
                    BuildingDetailsPanel.Panel.HideFloors = false;

                    // Update override label text.
                    floorOverrideLabel.text = Translations.Translate("RPR_CAL_FOV");

                    // Set visibility.
                    legacyPanel.Hide();
                    volumetricPanel.Show();
                }

                // Is there an override in place?
                if (currentFloorOverride == null)
                {
                    // No override - update panel with new calculations.
                    LevelData thisLevel = CurrentLevelData;
                    volumetricPanel.UpdatePopText(thisLevel);
                    volumetricPanel.CalculateVolumetric(currentBuilding, thisLevel, currentFloorPack, currentSchoolPack);

                    // Set visibility.
                    floorOverrideLabel.Hide();
                    floorPanel.Show();
                }
            }
            else
            {
                // Using legacy calcs = set flag.
                usingLegacy = true;

                // Set visibility.
                volumetricPanel.Hide();
                floorPanel.Hide();
                legacyPanel.Show();

                // Set override label and show.
                floorOverrideLabel.text = Translations.Translate("RPR_CAL_FLG");
                floorOverrideLabel.Show();

                // Cancel any floor rendering.
                BuildingDetailsPanel.Panel.HideFloors = true;
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
            volumetricPanel.CalculateVolumetric(currentBuilding, CurrentLevelData, currentFloorPack, currentSchoolPack);

            // Communicate change with to rest of panel.
            BuildingDetailsPanel.Panel.FloorDataPack = currentFloorPack;
        }


        /// <summary>
        /// Updates the school calculation pack selection to the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number (from menu) of selection pack</param>
        internal void UpdateSchoolSelection(int index)
        {
            // Update selected pack.
            currentSchoolPack = schoolPacks[index];

            // Update description.
            schoolDescription.text = currentSchoolPack.description;

            // Update panel with new calculations.
            volumetricPanel.CalculateVolumetric(currentBuilding, CurrentLevelData, currentFloorPack, currentSchoolPack);

            // School selections aren't used anywhere else, so no need to communicate change to rest of panel.
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

                // Get current and default packs for this item.
                currentPopPack = (PopDataPack)PopData.instance.ActivePack(building);
                currentFloorPack = (FloorDataPack)FloorData.instance.ActivePack(building);
                PopDataPack defaultPopPack = (PopDataPack)PopData.instance.CurrentDefaultPack(building);
                FloorDataPack defaultFloorPack = (FloorDataPack)FloorData.instance.CurrentDefaultPack(building);

                // Build pop pack menu.
                popMenu.items = new string[popPacks.Length];
                for (int i = 0; i < popMenu.items.Length; ++i)
                {
                    popMenu.items[i] = popPacks[i].displayName;

                    // Check for default name match,
                    if (popPacks[i].name.Equals(defaultPopPack.name))
                    {
                        popMenu.items[i] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (popPacks[i].name.Equals(currentPopPack.name))
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

                    // Check for default name match,
                    if (floorPacks[i].name.Equals(defaultFloorPack.name))
                    {
                        floorMenu.items[i] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (floorPacks[i].name.Equals(currentFloorPack.name))
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

                // Is this a school building (need to do school building after pop and floor packs are updated)?
                if (building.GetAI() is SchoolAI)
                {
                    // Yes - school building.  Set current pack.
                    currentSchoolPack = (SchoolDataPack)SchoolData.instance.ActivePack(building);

                    // Are we using custom school settings?
                    if (ModSettings.enableSchoolProperties)
                    {
                        // Yes -extend panel height and show school panel.
                        volumetricPanel.relativePosition = new Vector2(0f, SchoolCalcY);
                        applyButton.relativePosition = new Vector2(ApplyX, SchoolSaveY);

                        // Get available school packs for this building.
                        schoolPacks = SchoolData.instance.GetPacks(building);

                        // Get current and default packs for this item.
                        currentSchoolPack = (SchoolDataPack)SchoolData.instance.ActivePack(building);
                        SchoolDataPack defaultSchoolPack = (SchoolDataPack)SchoolData.instance.CurrentDefaultPack(building);

                        // Build school pack menu.
                        schoolMenu.items = new string[schoolPacks.Length];
                        for (int i = 0; i < schoolMenu.items.Length; ++i)
                        {
                            schoolMenu.items[i] = schoolPacks[i].displayName;

                            // Check for default name match,
                            if (schoolPacks[i].name.Equals(defaultSchoolPack.name))
                            {
                                schoolMenu.items[i] += Translations.Translate("RPR_PCK_DEF");
                            }

                            // Set menu selection to current pack if it matches.
                            if (schoolPacks[i].name.Equals(currentSchoolPack.name))
                            {
                                schoolMenu.selectedIndex = i;

                                // Force pack selection update.
                                UpdateSchoolSelection(i);
                            }
                        }

                        schoolPanel.Show();
                    }
                    else
                    {
                        // We're not using custom school settings, so use the non-school layout.
                        volumetricPanel.relativePosition = new Vector2(0f, BaseCalcY);
                        applyButton.relativePosition = new Vector2(ApplyX, BaseSaveY);
                        schoolPanel.Hide();
                    }
                }
                else
                {
                    // Not a school building - use non-school layout.
                    currentSchoolPack = null;
                    volumetricPanel.relativePosition = new Vector2(0f, BaseCalcY);
                    applyButton.relativePosition = new Vector2(ApplyX, BaseSaveY);
                    schoolPanel.Hide();
                }
            }
        }


        /// <summary>
        /// Adds a column header label.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Label text</param>
        /// <param name="xPos">Label x-position</param>
        /// <param name="xPos">Label y-position</param>
        /// <returns>New column label</returns>
        private UILabel ColumnLabel(UIComponent parent, string text, float xPos, float yPos)
        {
            UILabel newLabel = parent.AddUIComponent<UILabel>();
            newLabel.relativePosition = new Vector3(xPos, yPos);
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
