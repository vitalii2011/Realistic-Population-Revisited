using ColossalFramework.Math;
using ColossalFramework.UI;
using UnityEngine;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Different mod calculations shown (in text labels) by this panel.
    /// </summary>
    public enum Details
    {
        width,
        length,
        area,
        personArea,
        height,
        floorHeight,
        floors,
        extraFloors,
        numDetails
    }


    /// <summary>
    /// Panel to display the mod's calculations for jobs/workplaces.
    /// </summary>
    public class UIModCalcs : UIPanel
    {
        // Panel components. 
        private UIPanel labelPanel;
        private UILabel label;
        private UIPanel detailsPanel;
        private UILabel[] detailLabels;

        // Special-purpose labels used to display either jobs or households as appropriate.
        private UILabel homesJobsCalcLabel;
        private UILabel homesJobsActualLabel;


        /// <summary>
        /// Create the mod calcs panel; called by Unity just before any of the Update methods is called for the first time.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Generic setup.
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "UnlockingPanel";
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding.top = 5;
            autoLayoutPadding.right = 5;
            builtinKeyNavigation = true;
            clipChildren = true;

            // Panel title.
            labelPanel = this.AddUIComponent<UIPanel>();
            labelPanel.height = 20;

            label = labelPanel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(5, 0);
            label.width = 270;
            label.textAlignment = UIHorizontalAlignment.Center;
            label.text = "No building selected";

            // Panel to display calculations; hidden when no building is selected.
            detailsPanel = this.AddUIComponent<UIPanel>();
            detailsPanel.height = 0;
            detailsPanel.isVisible = false;
            detailsPanel.name = "DetailsPanel";

            // Set up detail fields.
            detailLabels = new UILabel[(int)Details.numDetails];
            for (int i = 0; i < (int)Details.numDetails; i++)
            {
                detailLabels[i] = detailsPanel.AddUIComponent<UILabel>();
                detailLabels[i].relativePosition = new Vector3(30, (i * 30) + 30);
                detailLabels[i].width = 270;
                detailLabels[i].textAlignment = UIHorizontalAlignment.Left;
            }

            // Homes/jobs labels.
            homesJobsCalcLabel = detailsPanel.AddUIComponent<UILabel>();
            homesJobsCalcLabel.relativePosition = new Vector3(30, ((int)Details.numDetails + 2) * 30);
            homesJobsCalcLabel.width = 270;
            homesJobsCalcLabel.textAlignment = UIHorizontalAlignment.Left;

            homesJobsActualLabel = detailsPanel.AddUIComponent<UILabel>();
            homesJobsActualLabel.relativePosition = new Vector3(30, ((int)Details.numDetails + 3) * 30);
            homesJobsActualLabel.width = 270;
            homesJobsActualLabel.textAlignment = UIHorizontalAlignment.Left;
        }


        /// <summary>
        /// Called whenever the currently selected building is changed to update the panel display.
        /// </summary>
        /// <param name="building"></param>
        public void SelectionChanged(BuildingInfo building)
        {
            if (building == null)
            {
                // If no building selected, then hide the details panel and update title accordingly.
                detailsPanel.height = 0;
                detailsPanel.isVisible = false;
                label.text = "No building selected";
            }
            else
            {
                // A building is selected - determine calculatons.
                int floorCount;
                // Building model size, not plot size.
                Vector3 buildingSize = building.m_size;
                // Array used for calculations depending on building service/subservice (via DataStore).
                int[] array;
                // Default minimum number of homes or jobs is one; different service types will override this.
                int minHomesJobs = 1;

                // Check for valid building AI.
                if (!(building.GetAI() is PrivateBuildingAI buildingAI))
                {
                    Debug.Log("Realistic Population Revisited: invalid building AI type in building details!");
                    return;
                }

                // Residential vs. workplace AI.
                if (buildingAI is ResidentialBuildingAI)
                {
                    // Get appropriate calculation array.
                    array = ResidentialBuildingAIMod.GetArray(building, (int)building.m_class.m_level);

                    // Set calculated homes label.
                    homesJobsCalcLabel.text = "Calculated homes: ";

                    // Applied homes is what's actually being returned by the CaclulateHomeCount call to this building AI.
                    // It differs from calculated homes if there's an override value for that building with this mod, or if another mod is overriding.
                    homesJobsActualLabel.text = "Applied homes: " + buildingAI.CalculateHomeCount(building.m_class.m_level, new Randomizer(0), building.GetWidth(), building.GetLength()); ;
                }
                else
                {
                    // Workplace AI.
                    // Default minimum number of jobs is 4.
                    minHomesJobs = 4;

                    // Find the correct array for the relevant building AI.
                    switch (building.m_class.m_service)
                    {
                        case ItemClass.Service.Commercial:
                            array = CommercialBuildingAIMod.GetArray(building, (int)building.m_class.m_level);
                            break;
                        case ItemClass.Service.Office:
                            array = OfficeBuildingAIMod.GetArray(building, (int)building.m_class.m_level);
                            break;
                        case ItemClass.Service.Industrial:
                            if (buildingAI is IndustrialExtractorAI)
                            {
                                array = IndustrialExtractorAIMod.GetArray(building, (int)building.m_class.m_level);
                            }
                            else
                            {
                                array = IndustrialBuildingAIMod.GetArray(building, (int)building.m_class.m_level);
                            }
                            break;
                        default:
                            Debug.Log("Realistic Population Revisited: invalid building service in building details!");
                            return;
                    }

                    // Set calculated jobs label.
                    homesJobsCalcLabel.text = "Calculated jobs (min. 4): ";

                    // Applied jobs is what's actually being returned by the CalculateWorkplaceCount call to this building AI.
                    // It differs from calculated jobs if there's an override value for that building with this mod, or if another mod is overriding.
                    int[] jobs = new int[4];
                    buildingAI.CalculateWorkplaceCount(building.m_class.m_level, new Randomizer(0), building.GetWidth(), building.GetLength(), out jobs[0], out jobs[1], out jobs[2], out jobs[3]);
                    homesJobsActualLabel.text = "Applied jobs: " + (jobs[0] + jobs[1] + jobs[2] + jobs[3]);
                }

                // Reproduce CalcBase calculations to get building area.
                int calcWidth = building.GetWidth();
                int calcLength = building.GetLength();
                floorCount = Mathf.Max(1, Mathf.FloorToInt(buildingSize.y / array[DataStore.LEVEL_HEIGHT]));

                // If CALC_METHOD is zero, then calculations are based on building model size, not plot size.
                if (array[DataStore.CALC_METHOD] == 0)
                {
                    // If asset has small x dimension, then use plot width in squares x 6m (75% of standard width) instead.
                    if (buildingSize.x <= 1)
                    {
                        calcWidth *= 6;
                    }
                    else
                    {
                        calcWidth = (int)buildingSize.x;
                    }

                    // If asset has small z dimension, then use plot length in squares x 6m (75% of standard length) instead.
                    if (buildingSize.z <= 1)
                    {
                        calcLength *= 6;
                    }
                    else
                    {
                        calcLength = (int)buildingSize.z;
                    }
                }
                else
                {
                    // If CALC_METHOD is nonzero, then caluclations are based on plot size, not building size.
                    // Plot size is 8 metres per square.
                    calcWidth *= 8;
                    calcLength *= 8;
                }

                // Display calculated (and retrieved) details.
                detailLabels[(int)Details.width].text = "Building width (m): " + calcWidth;
                detailLabels[(int)Details.length].text = "Building length (m): " + calcLength;
                detailLabels[(int)Details.height].text = "Scaffolding height (m): " + (int)buildingSize.y;
                detailLabels[(int)Details.personArea].text = "m2 per person: " + array[DataStore.PEOPLE];
                detailLabels[(int)Details.floorHeight].text = "Floor height (m): " + array[DataStore.LEVEL_HEIGHT];
                detailLabels[(int)Details.floors].text = "Calculated floors: " + floorCount;

                // Area calculation - will need this later.
                int calculatedArea = calcWidth * calcLength;
                detailLabels[(int)Details.area].text = "Calculated area: " + calculatedArea;

                // Show or hide extra floor modifier as appropriate (hide for zero or less, otherwise show).
                if (array[DataStore.DENSIFICATION] > 0)
                {
                    detailLabels[(int)Details.extraFloors].text = "Floor modifier: " + array[DataStore.DENSIFICATION];
                    detailLabels[(int)Details.extraFloors].isVisible = true;
                }
                else
                {
                    detailLabels[(int)Details.extraFloors].isVisible = false;
                }

                // Set minimum residences for high density.
                if ((building.m_class.m_subService == ItemClass.SubService.ResidentialHigh) || (building.m_class.m_subService == ItemClass.SubService.ResidentialHighEco))
                {
                    // Minimum of 2, or 90% number of floors, whichever is greater. This helps the 1x1 high density.
                    minHomesJobs = Mathf.Max(2, Mathf.CeilToInt(0.9f * floorCount));
                }

                // Perform actual household or workplace calculation.
                homesJobsCalcLabel.text += Mathf.Max(minHomesJobs, (calculatedArea * (floorCount + Mathf.Max(0, array[DataStore.DENSIFICATION]))) / array[DataStore.PEOPLE]);

                // We've got a valid building and results, so set panel title and ensure visiblity.
                label.text = "Mod calculations";
                detailsPanel.height = 270;
                detailsPanel.isVisible = true;
            }
        }
    }
}
