using ColossalFramework.Math;
using ColossalFramework.UI;
using UnityEngine;


namespace RealPop2
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
    public class UILegacyCalcs : UIPanel
    {
        // Margin at left of standard selection
        private const float LeftPadding = 10;
        private const float LineHeight = 25f;

        // Panel components.
        private UILabel[] detailLabels;
        private UILabel messageLabel;

        // Special-purpose labels used to display either jobs or households as appropriate.
        private UILabel homesJobsCalcLabel;
        private UILabel homesJobsCustomLabel;
        private UILabel homesJobsActualLabel;


        /// <summary>
        /// Create the mod calcs panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        public void Setup()
        {
            // Generic setup.
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "UnlockingPanel";
            autoLayout = false;
            autoLayoutPadding.top = 5;
            autoLayoutPadding.right = 5;
            builtinKeyNavigation = true;
            clipChildren = true;

            // Set up detail fields.
            detailLabels = new UILabel[(int)Details.numDetails];
            for (int i = 0; i < (int)Details.numDetails; i++)
            {
                detailLabels[i] = this.AddUIComponent<UILabel>();
                detailLabels[i].relativePosition = new Vector3(LeftPadding, (i * LineHeight) + LineHeight);
                detailLabels[i].width = 270;
                detailLabels[i].textAlignment = UIHorizontalAlignment.Left;
            }

            // Homes/jobs labels.
            homesJobsCalcLabel = this.AddUIComponent<UILabel>();
            homesJobsCalcLabel.relativePosition = new Vector3(LeftPadding, ((int)Details.numDetails + 1) * LineHeight);
            homesJobsCalcLabel.width = 270;
            homesJobsCalcLabel.textAlignment = UIHorizontalAlignment.Left;

            homesJobsCustomLabel = this.AddUIComponent<UILabel>();
            homesJobsCustomLabel.relativePosition = new Vector3(LeftPadding, ((int)Details.numDetails + 2) * LineHeight);
            homesJobsCustomLabel.width = 270;
            homesJobsCustomLabel.textAlignment = UIHorizontalAlignment.Left;

            homesJobsActualLabel = this.AddUIComponent<UILabel>();
            homesJobsActualLabel.relativePosition = new Vector3(LeftPadding, ((int)Details.numDetails + 4) * LineHeight);
            homesJobsActualLabel.width = 270;
            homesJobsActualLabel.textAlignment = UIHorizontalAlignment.Left;

            // Message label (initially hidden).
            messageLabel = this.AddUIComponent<UILabel>();
            messageLabel.relativePosition = new Vector3(LeftPadding, ((int)Details.numDetails + 6) * LineHeight);
            messageLabel.textAlignment = UIHorizontalAlignment.Left;
            messageLabel.autoSize = false;
            messageLabel.autoHeight = true;
            messageLabel.wordWrap = true;
            messageLabel.width = this.width - (LeftPadding * 2);
            messageLabel.text = "No message to display";
            messageLabel.isVisible = false;
        }


        /// <summary>
        /// Called whenever the currently selected building is changed to update the panel display.
        /// </summary>
        /// <param name="building">Newly selected building</param>
        public void SelectionChanged(BuildingInfo building)
        {
            // Make sure we have a valid selection before proceeding.
            if (building?.name == null)
            {
                return;
            }

            // Variables to compare actual counts vs. mod count, to see if there's another mod overriding counts.
            int appliedCount;
            int modCount;

            // Building model size, not plot size.
            Vector3 buildingSize = building.m_size;
            int floorCount;
            // Array used for calculations depending on building service/subservice (via DataStore).
            int[] array;
            // Default minimum number of homes or jobs is one; different service types will override this.
            int minHomesJobs = 1;
            int customHomeJobs;

            // Check for valid building AI.
            if (!(building.GetAI() is PrivateBuildingAI buildingAI))
            {
                Logging.Error("invalid building AI type in building details for building ", building.name);
                return;
            }

            // Residential vs. workplace AI.
            if (buildingAI is ResidentialBuildingAI)
            {
                // Get appropriate calculation array.
                array = ResidentialBuildingAIMod.GetArray(building, (int)building.GetClassLevel());

                // Set calculated homes label.
                homesJobsCalcLabel.text = Translations.Translate("RPR_CAL_HOM_CALC");

                // Set customised homes label and get value (if any).
                homesJobsCustomLabel.text = Translations.Translate("RPR_CAL_HOM_CUST");
                customHomeJobs = ExternalCalls.GetResidential(building);

                // Applied homes is what's actually being returned by the CaclulateHomeCount call to this building AI.
                // It differs from calculated homes if there's an override value for that building with this mod, or if another mod is overriding.
                appliedCount = buildingAI.CalculateHomeCount(building.GetClassLevel(), new Randomizer(0), building.GetWidth(), building.GetLength());
                homesJobsActualLabel.text = Translations.Translate("RPR_CAL_HOM_APPL") + appliedCount;
            }
            else
            {
                // Workplace AI.
                // Default minimum number of jobs is 4.
                minHomesJobs = 4;

                // Find the correct array for the relevant building AI.
                switch (building.GetService())
                {
                    case ItemClass.Service.Commercial:
                        array = CommercialBuildingAIMod.GetArray(building, (int)building.GetClassLevel());
                        break;
                    case ItemClass.Service.Office:
                        array = OfficeBuildingAIMod.GetArray(building, (int)building.GetClassLevel());
                        break;
                    case ItemClass.Service.Industrial:
                        if (buildingAI is IndustrialExtractorAI)
                        {
                            array = IndustrialExtractorAIMod.GetArray(building, (int)building.GetClassLevel());
                        }
                        else
                        {
                            array = IndustrialBuildingAIMod.GetArray(building, (int)building.GetClassLevel());
                        }
                        break;
                    default:
                        Logging.Error("invalid building service in building details for building ", building.name);
                        return;
                }

                // Set calculated jobs label.
                homesJobsCalcLabel.text = Translations.Translate("RPR_CAL_JOB_CALC") + " ";

                // Set customised jobs label and get value (if any).
                homesJobsCustomLabel.text = Translations.Translate("RPR_CAL_JOB_CUST") + " ";
                customHomeJobs = ExternalCalls.GetWorker(building);

                // Applied jobs is what's actually being returned by the CalculateWorkplaceCount call to this building AI.
                // It differs from calculated jobs if there's an override value for that building with this mod, or if another mod is overriding.
                int[] jobs = new int[4];
                buildingAI.CalculateWorkplaceCount(building.GetClassLevel(), new Randomizer(0), building.GetWidth(), building.GetLength(), out jobs[0], out jobs[1], out jobs[2], out jobs[3]);
                appliedCount = jobs[0] + jobs[1] + jobs[2] + jobs[3];
                homesJobsActualLabel.text = Translations.Translate("RPR_CAL_JOB_APPL") + " " + appliedCount;
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
            detailLabels[(int)Details.width].text = Translations.Translate("RPR_CAL_BLD_X") + " " + calcWidth;
            detailLabels[(int)Details.length].text = Translations.Translate("RPR_CAL_BLD_Z") + " " + calcLength;
            detailLabels[(int)Details.height].text = Translations.Translate("RPR_CAL_BLD_Y") + " " + (int)buildingSize.y;
            detailLabels[(int)Details.personArea].text = Translations.Translate("RPR_CAL_BLD_M2") + " " + array[DataStore.PEOPLE];
            detailLabels[(int)Details.floorHeight].text = Translations.Translate("RPR_CAL_FLR_Y") + " " + array[DataStore.LEVEL_HEIGHT];
            detailLabels[(int)Details.floors].text = Translations.Translate("RPR_CAL_FLR") + " " + floorCount;

            // Area calculation - will need this later.
            int calculatedArea = calcWidth * calcLength;
            detailLabels[(int)Details.area].text = Translations.Translate("RPR_CAL_M2") + " " + calculatedArea;

            // Show or hide extra floor modifier as appropriate (hide for zero or less, otherwise show).
            if (array[DataStore.DENSIFICATION] > 0)
            {
                detailLabels[(int)Details.extraFloors].text = Translations.Translate("RPR_CAL_FLR_M") + " " + array[DataStore.DENSIFICATION];
                detailLabels[(int)Details.extraFloors].isVisible = true;
            }
            else
            {
                detailLabels[(int)Details.extraFloors].isVisible = false;
            }

            // Set minimum residences for high density.
            if ((building.GetSubService() == ItemClass.SubService.ResidentialHigh) || (building.GetSubService() == ItemClass.SubService.ResidentialHighEco))
            {
                // Minimum of 2, or 90% number of floors, whichever is greater. This helps the 1x1 high density.
                minHomesJobs = Mathf.Max(2, Mathf.CeilToInt(0.9f * floorCount));
            }

            // Perform actual household or workplace calculation.
            modCount = Mathf.Max(minHomesJobs, (calculatedArea * (floorCount + Mathf.Max(0, array[DataStore.DENSIFICATION]))) / array[DataStore.PEOPLE]);
            homesJobsCalcLabel.text += modCount;

            // Set customised homes/jobs label (leave blank if no custom setting retrieved).
            if (customHomeJobs > 0)
            {
                homesJobsCustomLabel.text += customHomeJobs.ToString();
            }

            // Check to see if Ploppable RICO Revisited is controlling this building's population.
            if (ModUtils.CheckRICOPopControl(building))
            {
                messageLabel.text = Translations.Translate("RPR_CAL_RICO");
                messageLabel.Show();
            }
            else
            {
                // Hide message text by default.
                messageLabel.Hide();
            }
        }
    }
}
