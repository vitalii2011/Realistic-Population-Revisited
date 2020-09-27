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
        /// Create the panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        public void Setup()
        {
            const int marginPadding = 10;

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
            title.text = Translations.Translate("RPR_CUS_TITLE");
            title.textScale = 1.2f;
            title.autoSize = false;
            title.width = this.width;
            title.height = 30;

            // Text field label.
            homeJobLabel = this.AddUIComponent<UILabel>();
            homeJobLabel.relativePosition = new Vector3(marginPadding, 40);
            homeJobLabel.textAlignment = UIHorizontalAlignment.Left;
            homeJobLabel.text = Translations.Translate("RPR_LBL_HOM");

            // Home or jobs count text field.
            homeJobsCount = UIUtils.CreateTextField(this, this.width - (marginPadding * 3) - homeJobLabel.width, 20);
            homeJobsCount.relativePosition = new Vector3(marginPadding + homeJobLabel.width + marginPadding, 40);

            // Save button.
            saveButton = UIUtils.CreateButton(this, 200);
            saveButton.relativePosition = new Vector3(marginPadding, 70);
            saveButton.text = Translations.Translate("RPR_CUS_ADD");
            saveButton.tooltip = Translations.Translate("RPR_CUS_ADD_TIP");
            saveButton.Disable();

            // Delete button.
            deleteButton = UIUtils.CreateButton(this, 200);
            deleteButton.relativePosition = new Vector3(marginPadding, 110);
            deleteButton.text = Translations.Translate("RPR_CUS_DEL");
            deleteButton.tooltip = Translations.Translate("RPR_CUS_DEL_TIP");
            deleteButton.Disable();

            // Save button event handler.
            saveButton.eventClick += (component, clickEvent) =>
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
                    // Minimum value of 1.
                    if (homesJobs < 1)
                    {
                        // Print warning message in red.
                        messageLabel.textColor = new Color32(255, 0, 0, 255);
                        messageLabel.text = Translations.Translate("RPR_ERR_ZERO");
                        messageLabel.isVisible = true;
                    }
                    else
                    {
                        // Homes or jobs?
                        if (currentSelection.GetService() == ItemClass.Service.Residential)
                        {
                            // Residential building.
                            ExternalCalls.SetResidential(currentSelection, homesJobs);

                            // Update household counts for existing instances of this building - only needed for residential buildings.
                            // Workplace counts will update automatically with next call to CalculateWorkplaceCount; households require more work (tied to CitizenUnits).
                            PopData.UpdateHouseholds(currentSelection.name);
                        }
                        else
                        {
                            // Employment building.
                            ExternalCalls.SetWorker(currentSelection, homesJobs);
                        }

                        // Refresh the display so that all panels reflect the updated settings.
                        BuildingDetailsPanel.Panel.UpdateSelectedBuilding(currentSelection);
                        BuildingDetailsPanel.Panel.Refresh();
                    }
                }
                else
                {
                    // TryParse couldn't parse the data; print warning message in red.
                    messageLabel.textColor = new Color32(255, 0, 0, 255);
                    messageLabel.text = Translations.Translate("RPR_ERR_INV");
                    messageLabel.isVisible = true;
                }
            };

            // Delete button event handler.
            deleteButton.eventClick += (component, clickEvent) =>
            {
                // Hide message.
                messageLabel.isVisible = false;

                // Don't do anything with invalid entries.
                if (currentSelection == null || currentSelection.name == null)
                {
                    return;
                }

                Debugging.Message("deleting custom entry for " + currentSelection.name);

                // Homes or jobs?  Remove custom entry as appropriate.
                if (currentSelection.GetService() == ItemClass.Service.Residential)
                {
                    // Residential building.
                    ExternalCalls.RemoveResidential(currentSelection);

                    // Update household counts for existing instances of this building - only needed for residential buildings.
                    // Workplace counts will update automatically with next call to CalculateWorkplaceCount; households require more work (tied to CitizenUnits).
                    PopData.UpdateHouseholds(currentSelection.name);
                }
                else
                {
                    // Employment building.
                    ExternalCalls.RemoveWorker(currentSelection);
                }

                // Refresh the display so that all panels reflect the updated settings.
                BuildingDetailsPanel.Panel.Refresh();
                homeJobsCount.text = string.Empty;
            };

            // Message label (initially hidden).
            messageLabel = this.AddUIComponent<UILabel>();
            messageLabel.relativePosition = new Vector3(marginPadding, 160);
            messageLabel.textAlignment = UIHorizontalAlignment.Left;
            messageLabel.autoSize = false;
            messageLabel.autoHeight = true;
            messageLabel.wordWrap = true;
            messageLabel.width = this.width - (marginPadding * 2);
            messageLabel.isVisible = false;
            messageLabel.text = "No message to display";
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
                homeJobLabel.text = Translations.Translate("RPR_LBL_HOM");
            }
            else
            {
                // Workplace building; see if a custom number of jobs applies to this building.
                homesJobs = ExternalCalls.GetWorker(building);
                homeJobLabel.text = Translations.Translate("RPR_LBL_JOB");
            }

            // If no custom settings have been found (return value was zero), then blank the text field, rename the save button, and disable the delete button.
            if (homesJobs == 0)
            {
                homeJobsCount.text = string.Empty;
                saveButton.text = Translations.Translate("RPR_CUS_ADD");
                deleteButton.Disable();
            }
            else
            {
                // Valid custom settings found; display the result, rename the save button, and enable the delete button.
                homeJobsCount.text = homesJobs.ToString();
                saveButton.text = Translations.Translate("RPR_CUS_UPD");
                deleteButton.Enable();
            }

            // We've got a valid building, so enable the save button.
            saveButton.Enable();
        }
    }
}