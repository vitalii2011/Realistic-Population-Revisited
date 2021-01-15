using System;
using System.IO;
using System.Linq;
using ICities;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using RealisticPopulationRevisited.MessageBox;


namespace RealisticPopulationRevisited
{
    public class Loading : LoadingExtensionBase
    {
        private static bool isModEnabled = false;
        private bool harmonyLoaded = false;

        // Used to flag if a conflicting mod is running.
        private static bool conflictingMod = false;



        public static bool IsModEnabled(UInt64 id)
        {
            return PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == id && mod.isEnabled));
        }

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
            }

            // Check for original WG Realistic Population and Consumption Mod; if it's enabled, flag and don't activate this mod.
            if (IsModEnabled(426163185ul))
            {
                conflictingMod = true;
                Debugging.Message("Realistic Population and Consumption Mod detected, skipping activation");
            }
            else if (!isModEnabled)
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


        public override void OnLevelLoaded(LoadMode mode)
        {
            // Check to see that Harmony was properly loaded.
            if (!harmonyLoaded)
            {
                // Patch wasn't operating; display warning notification and exit.
                ListMessageBox harmonyBox = MessageBoxBase.ShowModal<ListMessageBox>();

                // Key text items.
                harmonyBox.Title = RealPopMod.ModName;
                harmonyBox.ButtonText = Translations.Translate("RPR_MES_CLS");
                harmonyBox.AddParas(Translations.Translate("RPR_ERR_HAR0"), Translations.Translate("RPR_ERR_HAR1"), Translations.Translate("RPR_ERR_HAR2"));

                // List of dot points.
                harmonyBox.AddList(Translations.Translate("RPR_ERR_HAR3"), Translations.Translate("RPR_ERR_HAR4"));

                // Closing para.
                harmonyBox.AddParas(Translations.Translate("RPR_ERR_HAR5"));

                return;
            }


            // Check to see if a conflicting mod has been detected - if so, alert the user.
            if (conflictingMod)
            {
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Realistic Population Revisited", "Original Realistic Population and Consumption Mod mod detected - Realistic Population Revisited is shutting down to protect your game.  Only ONE of these mods can be enabled at the same time; please unsubscribe from the old Realistic Population and Consumption Mod, which is now deprecated!", true);
            }

            // Don't do anything further if mod hasn't activated (conflicting mod detected, or loading into editor instead of game).
            if (!isModEnabled)
            {
                return;
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
