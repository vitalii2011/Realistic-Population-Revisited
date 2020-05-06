using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using ColossalFramework.Math;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Panel for editing and creating building settings.
    /// </summary>
    public class UIEditPanel : UIPanel
    {
        // Panel components
        private UITextField homeJobsCount;
        private UILabel homeJobLabel;
        private UIButton saveButton;
        private UIButton deleteButton;
        private UILabel messageLabel;

        // Currently selected building.
        private BuildingInfo currentSelection;
        

        /// <summary>
        /// Create the panel; called by Unity just before any of the Update methods is called for the first time.
        /// </summary>
        public override void Start()
        {
            const int marginPadding = 10;

            base.Start();

            // Generic setup.
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "UnlockingPanel";
            autoLayout = false;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding.top = 5;
            autoLayoutPadding.right = 5;
            builtinKeyNavigation = true;
            clipChildren = true;

            // Panel title.
            UILabel title = this.AddUIComponent<UILabel>();
            title.relativePosition = new Vector3(0, 5);
            title.textAlignment = UIHorizontalAlignment.Center;
            title.text = "Custom settings";
            title.textScale = 1.2f;
            title.autoSize = false;
            title.width = this.width;
            title.height = 30;

            // Text field label.
            homeJobLabel = this.AddUIComponent<UILabel>();
            homeJobLabel.relativePosition = new Vector3(marginPadding, 40);
            homeJobLabel.textAlignment = UIHorizontalAlignment.Left;
            homeJobLabel.text = "Homes:";

            // Home or jobs count text field.
            homeJobsCount = UIUtils.CreateTextField(this, this.width - (marginPadding * 3) - homeJobLabel.width, 20);
            homeJobsCount.relativePosition = new Vector3(marginPadding + homeJobLabel.width + marginPadding, 40);

            // Save button.
            saveButton = UIUtils.CreateButton(this, 200);
            saveButton.relativePosition = new Vector3(marginPadding, 70);
            saveButton.text = "Add custom setting";
            saveButton.tooltip = "Adds (or updates) a custom setting for the selected building";
            saveButton.Disable();

            // Delete button.
            deleteButton = UIUtils.CreateButton(this, 200);
            deleteButton.relativePosition = new Vector3(marginPadding, 110);
            deleteButton.text = "Delete custom setting";
            deleteButton.tooltip = "Removes the custom setting from the selected building";
            deleteButton.Disable();

            // Save button event handler.
            saveButton.eventClick += (c, p) =>
            {
                // Hide message.
                messageLabel.isVisible = false;

                // Don't do anything with invalid entries.
                if (currentSelection == null || currentSelection.name == null)
                {
                    return;
                }

                // Read textfield if possible.
                if (int.TryParse(homeJobsCount.text, out int homesJobs))
                {
                    // Homes or jobs?
                    if (currentSelection.GetService() == ItemClass.Service.Residential)
                    {
                        Debug.Log("Realistic Population Revisited: adding custom household count of " + homesJobs + " for '" + currentSelection.name + "'.");

                        // Residential building.
                        ExternalCalls.SetResidential(currentSelection, homesJobs);

                        // Update household counts for existing instances of this building - only needed for residential buildings.
                        // Workplace counts will update automatically with next call to CalculateWorkplaceCount; households require more work (tied to CitizenUnits).
                        UpdateHouseholds(currentSelection.name);
                    }
                    else
                    {
                        Debug.Log("Realistic Population Revisited: adding custom workplace count of " + homesJobs + " for '" + currentSelection.name + "'.");

                        // Employment building.
                        ExternalCalls.SetWorker(currentSelection, homesJobs);
                    }

                    // Refresh the display so that all panels reflect the updated settings.
                    UIBuildingDetails.instance.Refresh();
                }
                else
                {
                    // TryParse couldn't parse the data; print warning message in red.
                    messageLabel.textColor = new Color32(255, 0, 0, 255);
                    messageLabel.text = "ERROR: invalid value";
                    messageLabel.isVisible = true;
                }
            };

            // Delete button event handler.
            deleteButton.eventClick += (c, p) =>
            {
                // Hide message.
                messageLabel.isVisible = false;

                // Don't do anything with invalid entries.
                if (currentSelection == null || currentSelection.name == null)
                {
                    return;
                }

                Debug.Log("Realistic Population Revisited: deleting custom entry for '" + currentSelection.name + "'.");

                // Homes or jobs?  Remove custom entry as appropriate.
                if (currentSelection.GetService() == ItemClass.Service.Residential)
                {
                    // Residential building.
                    ExternalCalls.RemoveResidential(currentSelection);

                    // Update household counts for existing instances of this building - only needed for residential buildings.
                    // Workplace counts will update automatically with next call to CalculateWorkplaceCount; households require more work (tied to CitizenUnits).
                    UpdateHouseholds(currentSelection.name);
                }
                else
                {
                    // Employment building.
                    ExternalCalls.RemoveWorker(currentSelection);
                }

                // Refresh the display so that all panels reflect the updated settings.
                UIBuildingDetails.instance.Refresh();
                homeJobsCount.text = string.Empty;
            };

            // Message label (initially hidden).
            messageLabel = this.AddUIComponent<UILabel>();
            messageLabel.relativePosition = new Vector3(marginPadding, 160);
            messageLabel.textAlignment = UIHorizontalAlignment.Left;
            messageLabel.text = "No message to display";
            messageLabel.isVisible = false;
        }


        /// <summary>
        /// Called whenever the currently selected building is changed to update the panel display.
        /// </summary>
        /// <param name="building"></param>
        public void SelectionChanged(BuildingInfo building)
        {
            // Hide message.
            messageLabel.isVisible = false;

            // Set current selecion.
            currentSelection = building;

            // Set text field to blank and disable buttons if no valid building is selected.
            if (building == null || building.name == null)
            {
                homeJobsCount.text = string.Empty;
                saveButton.Disable();
                deleteButton.Disable();
                return;
            }

            int homesJobs;

            if (building.GetService() == ItemClass.Service.Residential)
            {
                // See if a custom number of households applies to this building.
                homesJobs = ExternalCalls.GetResidential(building);
                homeJobLabel.text = "Homes:";
            }
            else
            {
                // Workplace building; see if a custom number of jobs applies to this building.
                homesJobs = ExternalCalls.GetWorker(building);
                homeJobLabel.text = "Jobs:";
            }

            // If no custom settings have been found (return value was zero), then blank the text field, rename the save button, and disable the delete button.
            if (homesJobs == 0)
            {
                homeJobsCount.text = string.Empty;
                saveButton.text = "Add custom setting";
                deleteButton.Disable();
            }
            else
            {
                // Valid custom settings found; display the result, rename the save button, and enable the delete button.
                homeJobsCount.text = homesJobs.ToString();
                saveButton.text = "Update custom setting";
                deleteButton.Enable();
            }

            // We've got a valid building, so enable the save button.
            saveButton.Enable();
        }


        /// <summary>
        /// Updates the household numbers of already existing (placed/grown) residential building instances to the current prefab value.
        /// Called after updating a residential prefab's household count in order to apply changes to existing buildings.
        /// </summary>
        /// <param name="prefabName">The (raw BuildingInfo) name of the prefab</param>
        private void UpdateHouseholds(string prefabName)
        {
            // Get building manager instance.
            var instance = Singleton<BuildingManager>.instance;

            // Iterate through each building in the scene.
            for (ushort i = 0; i < instance.m_buildings.m_buffer.Length; i ++)
            {
                // Get current building instance.
                Building thisBuilding = instance.m_buildings.m_buffer[i];

                // Only interested in residential buildings.
                BuildingAI thisAI = thisBuilding.Info.GetAI() as ResidentialBuildingAI;
                if (thisAI != null)
                {
                    // Residential building; check for name match.
                    if (thisBuilding.Info.name.Equals(prefabName))
                    {
                        // Got one!  Recalculate home and visit counts.
                        int homeCount = ((ResidentialBuildingAI)thisAI).CalculateHomeCount((ItemClass.Level)thisBuilding.m_level, new Randomizer(i), thisBuilding.Width, thisBuilding.Length);
                        int visitCount = ((ResidentialBuildingAI)thisAI).CalculateVisitplaceCount((ItemClass.Level)thisBuilding.m_level, new Randomizer(i), thisBuilding.Width, thisBuilding.Length);

                        // Apply changes via direct call to EnsureCitizenUnits prefix patch from this mod.
                        RealisticCitizenUnits.Prefix(ref thisAI, i, ref thisBuilding, homeCount, 0, visitCount, 0);
                    }
                }
            }
        }
    }
}