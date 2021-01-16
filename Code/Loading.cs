using System.IO;
using ICities;
using RealisticPopulationRevisited.MessageBox;


namespace RealisticPopulationRevisited
{
    public class Loading : LoadingExtensionBase
    {
        private static bool isModEnabled = false;
        private bool harmonyLoaded = false;

        // Used to flag if a conflicting mod is running.
        private static bool conflictingMod = false;


        /// <summary>
        /// Called by the game when the mod is initialised at the start of the loading process.
        /// </summary>
        /// <param name="loading">Loading mode (e.g. game, editor, scenario, etc.)</param>
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            // Don't do anything if not in game (e.g. if we're going into an editor).
            if (loading.currentMode != AppMode.Game)
            {
                isModEnabled = false;
                Debugging.Message("not loading into game, skipping activation");
                return;
            }

            // Ensure that Harmony patches have been applied.
            harmonyLoaded = Patcher.Patched;
            if (!harmonyLoaded)
            {
                isModEnabled = false;
                Debugging.Message("Harmony patches not applied; aborting");
                return;
            }

            // Check for mod conflicts.
            if (ModUtils.ConflictingMod())
            {
                conflictingMod = true;
                isModEnabled = false;

                // Unload Harmony patches.
                Patcher.UnpatchAll();
                return;
            }

            // Passed all checks - okay to load (if we haven't already fo some reason)>
            if (!isModEnabled)
            {
                isModEnabled = true;
                Debugging.Message("version v", RealPopMod.Version, " loading");

                // Perform legacy datastore setup.
                XMLUtilsWG.Setup();

                // Check for Ploppable RICO Revisited.
                ModUtils.RICOReflection();

                // Initialise volumetric datastores.
                EmploymentData.Setup();

                // Initialize data.
                DataUtils.Setup();
            }
        }


        /// <summary>
        /// Called by the game when level loading is complete.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.)</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            // Check to see that Harmony 2 was properly loaded.
            if (!harmonyLoaded)
            {
                // Harmony 2 wasn't loaded; display warning notification and exit.
                ListMessageBox harmonyBox = MessageBoxBase.ShowModal<ListMessageBox>();

                // Key text items.
                harmonyBox.AddParas(Translations.Translate("RPR_ERR_HAR0"), Translations.Translate("RPR_ERR_FAT"), Translations.Translate("RPR_ERR_HAR1"), Translations.Translate("RPR_ERR_HAR2"));

                // List of dot points.
                harmonyBox.AddList(Translations.Translate("RPR_ERR_HAR3"), Translations.Translate("RPR_ERR_HAR4"));

                // Closing para.
                harmonyBox.AddParas(Translations.Translate("RPR_ERR_HAR5"));
            }

            // Check to see if a conflicting mod has been detected.
            if (conflictingMod)
            {
                // Mod conflict detected - display warning notification and exit.
                ListMessageBox modConflictBox = MessageBoxBase.ShowModal<ListMessageBox>();

                // Key text items.
                modConflictBox.AddParas(Translations.Translate("RPR_ERR_CON0"), Translations.Translate("RPR_ERR_FAT"), Translations.Translate("RPR_ERR_CON1"), Translations.Translate("RPR_ERR_CON2"));

                // Add conflicting mod name(s).
                modConflictBox.AddList(ModUtils.conflictingModNames.ToArray());

                // Closing para.
                modConflictBox.AddParas(Translations.Translate("RPR_ERR_CON3"));
            }

            // Don't do anything further if mod hasn't activated for whatever reason (mod conflict, harmony error, something else).
            if (!isModEnabled)
            {
                // Disable keystrokes.
                UIThreading.operating = false;

                return;
            }

            // Show legacy choice message box if this save hasn't been flagged as being from Realistic Population 2.
            if (!ModSettings.saveFlag)
            {
                LegacyChoiceMessageBox legacyChoiceBox = MessageBoxBase.ShowModal<LegacyChoiceMessageBox>();
            }

            // Wait for loading to fully complete.
            while (!LoadingManager.instance.m_loadingComplete) { }

            // Record initial (default) school settings and apply ours over the top.
            SchoolData.instance.OnLoad();

            // IF a legacy file exists, flag it for writing.
            if (File.Exists(DataStore.currentFileLocation))
            {
                Debugging.Message("found legacy settings file");
                XMLUtilsWG.writeToLegacy = true;
            }

            // Add button to building info panels.
            BuildingDetailsPanel.AddInfoPanelButton();

            // Check if we need to display update notification.
            if (UpdateNotification.notificationVersion != 2)
            {
                // No update notification "Don't show again" flag found; show the notification.
                UpdateNotification notification = new UpdateNotification();
                notification.Create();
                notification.Show();
            }

            // Set up options panel event handler.
            OptionsPanel.OptionsEventHook();
        }
    }
}
