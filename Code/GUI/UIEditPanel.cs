using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Panel for editing and creating building settings.
    /// </summary>
    public class UIEditPanel : UIPanel
    {
        // Layout constants.
        const float MarginPadding = 10f;
        const float LabelWidth = 150f;


        // Panel components
        private UITextField homeJobsCount, floorCount, perFloorCount;
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

            // Text field labels.
            homeJobLabel = AddLabel(40f, "RPR_LBL_HOM");
            //AddLabel(80f, "RPR_LBL_FLR");
            //AddLabel(120f, "RPR_LBL_PFL");

            // Text fields.
            homeJobsCount = AddEditField(40f);
            //floorCount = AddEditField(80f);
            //perFloorCount = AddEditField(120f);

            // Save button.
            saveButton = UIUtils.CreateButton(this, 200);
            saveButton.relativePosition = new Vector3(MarginPadding, 150);
            saveButton.text = Translations.Translate("RPR_CUS_ADD");
            saveButton.tooltip = Translations.Translate("RPR_CUS_ADD_TIP");
            saveButton.Disable();

            // Delete button.
            deleteButton = UIUtils.CreateButton(this, 200);
            deleteButton.relativePosition = new Vector3(MarginPadding, 190);
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
                            PopData.instance.UpdateHouseholds(currentSelection.name);
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
                    PopData.instance.UpdateHouseholds(currentSelection.name);
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
            messageLabel.relativePosition = new Vector3(MarginPadding, 160);
            messageLabel.textAlignment = UIHorizontalAlignment.Left;
            messageLabel.autoSize = false;
            messageLabel.autoHeight = true;
            messageLabel.wordWrap = true;
            messageLabel.width = this.width - (MarginPadding * 2);
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

        
        /// <summary>
        /// Adds a textfield.
        /// </summary>
        /// <param name="yPos">Relative y-position of textfield</param>
        /// <returns>New textfield</returns>
        private UITextField AddEditField(float yPos)
        {
            UITextField newField = UIUtils.CreateTextField(this, this.width - (MarginPadding * 3) - LabelWidth, 20);
            newField.relativePosition = new Vector3(MarginPadding + LabelWidth + MarginPadding, yPos);

            return newField;
        }


        /// <summary>
        /// Adds a label to left of a textfield.
        /// </summary>
        /// <param name="yPos">Relative y-position of textfield</param>
        /// <param name="key">Translation key for label</param>
        /// <returns></returns>
        private UILabel AddLabel(float yPos, string key)
        {
            UILabel newLabel = this.AddUIComponent<UILabel>();
            newLabel.relativePosition = new Vector3(MarginPadding, yPos);
            newLabel.textAlignment = UIHorizontalAlignment.Left;
            newLabel.text = Translations.Translate(key);

            return newLabel;
        }
    }
}