﻿using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel (sub)-tab for legacy office building consumption configuration.
    /// </summary>
    internal class LegacyOfficePanel : LegacyPanelBase
    {
        // Array reference constants.
        private const int Office = 0;
        private const int HighTech = 1;
        private const int NumSubServices = 2;
        private const int NumLevels = 3;


        // Label constants.
        private readonly string[] subServiceLables =
        {
            "RPR_CAT_OFF",
            "RPR_CAT_ITC"
        };


        // Tab title.
        protected override string TabNameKey => "RPR_CAT_OFF";


        /// <summary>
        /// Adds commercial options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal LegacyOfficePanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal override void Setup()
        {
            // Don't do anything if already set up.
            if (!isSetup)
            {
                // Perform initial setup.
                isSetup = true;
                Logging.Message("setting up ", this.GetType().ToString());

                // Initialise textfield array.
                SetupArrays(NumSubServices);

                for (int i = 0; i < NumSubServices; i++)
                {
                    int levels = i == 0 ? NumLevels : 1;

                    areaFields[i] = new UITextField[levels];
                    floorFields[i] = new UITextField[levels];
                    extraFloorFields[i] = new UITextField[levels];
                    powerFields[i] = new UITextField[levels];
                    waterFields[i] = new UITextField[levels];
                    sewageFields[i] = new UITextField[levels];
                    garbageFields[i] = new UITextField[levels];
                    incomeFields[i] = new UITextField[levels];
                    productionFields[i] = new UITextField[NumLevels];
                }

                // Headings.
                AddHeadings(panel);

                // Create residential per-person area textfields and labels.
                PanelUtils.RowHeaderIcon(panel, ref currentY, Translations.Translate(subServiceLables[Office]), "ZoningOffice", "Thumbnails");
                AddSubService(panel, Office);
                PanelUtils.RowHeaderIcon(panel, ref currentY, Translations.Translate(subServiceLables[HighTech]), "IconPolicyHightech", "Ingame");
                AddSubService(panel, HighTech, label: Translations.Translate(subServiceLables[HighTech]));

                // Populate initial values.
                PopulateFields();

                // Add command buttons.
                AddButtons(panel);
            }
        }


        /// <summary>
        /// Populates the text fields with information from the DataStore.
        /// </summary>
        protected override void PopulateFields()
        {
            // Populate each subservice.
            PopulateSubService(DataStore.office, Office);
            PopulateSubService(DataStore.officeHighTech, HighTech);
        }


        /// <summary>
        /// Updates the DataStore with the information from the text fields.
        /// </summary>
        protected override void ApplyFields()
        {
            // Apply each subservice.
            ApplySubService(DataStore.office, Office);
            ApplySubService(DataStore.officeHighTech, HighTech);

            // Clear cached values.
            PopData.instance.workplaceCache.Clear();

            // Save new settings.
            SaveLegacy();

            // Refresh settings.
            PopulateFields();
        }


        /// <summary>
        /// Resets all textfields to mod default values.
        /// </summary>
        protected override void ResetToDefaults()
        {
            // Defaults copied from Datastore.
            int[][] office = { new int [] {34, 5, 0, 0, -1,   2,  8, 20, 70,   12, 4, 4, 3, 1000,   0, 1,   10, 25},
                                         new int [] {36, 5, 0, 0, -1,   1,  5, 14, 80,   13, 5, 5, 3, 1125,   0, 1,   10, 37},
                                         new int [] {38, 5, 0, 0, -1,   1,  3,  6, 90,   14, 5, 5, 2, 1250,   0, 1,   10, 50} };

            int[][] officeHighTech = { new int[] { 74, 5, 0, 0, -1, 1, 2, 3, 94, 22, 5, 5, 3, 4000, 0, 1, 10, 10 } };

            // Populate text fields with these.
            PopulateSubService(office, Office);
            PopulateSubService(officeHighTech, HighTech);
        }
    }
}