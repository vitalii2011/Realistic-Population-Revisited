using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ICities;
using ColossalFramework.Math;
using ColossalFramework.UI;
using ColossalFramework.Plugins;


namespace RealisticPopulationRevisited
{
    public class Loading : LoadingExtensionBase
    {
        private static volatile bool isModEnabled = false;
        private static volatile bool isLevelLoaded = false;

        // Used to flag if a conflicting mod is running.
        private static bool conflictingMod = false;



        public static bool IsModEnabled(UInt64 id)
        {
            return PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == id && mod.isEnabled));
        }

        public override void OnCreated(ILoading loading)
        {
            // Don't do anything if not in game (e.g. if we're going into an editor).
            if (loading.currentMode != AppMode.Game)
            {
                isModEnabled = false;
                Debugging.Message("not loading into game, skipping activation");
                return;
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
                Debugging.Message("version v" + RealPopMod.Version + " loading");

                MergeDefaultBonus();

                // Remove bonus names from over rides
                foreach (string name in DataStore.bonusHouseholdCache.Keys)
                {
                    DataStore.householdCache.Remove(name);
                }

                foreach (string name in DataStore.bonusWorkerCache.Keys)
                {
                    DataStore.workerCache.Remove(name);
                }

                DataStore.seedToId.Clear();
                for (int i = 0; i <= ushort.MaxValue; ++i)  // Up to 1M buildings apparently is ok
                {
                    // This creates a unique number
                    try
                    {
                        Randomizer number = new Randomizer(i);
                        DataStore.seedToId.Add(number.seed, (ushort)i);
                    }
                    catch (Exception)
                    {
                        //Debugging.writeDebugToFile("Seed collision at number: " + i);
                    }
                }

                // Check for Ploppable RICO Revisited.
                ModUtils.RICOReflection();

                // Initialise volumetric datastores.
                EmploymentData.Setup();

                // Initialize data.
                DataUtils.Setup();
            }
        }

        private void MergeDefaultBonus()
        {
            if (DataStore.mergeResidentialNames)
            {
                foreach(KeyValuePair<string, int> entry in DataStore.defaultHousehold)
                {
                    try
                    {
                        DataStore.householdCache.Add(entry.Key, entry.Value);
                    }
                    catch (Exception)
                    {
                        // Don't care
                    }
                }
            }

            if (DataStore.mergeEmploymentNames)
            {
                foreach (KeyValuePair<string, int> entry in DataStore.defaultWorker)
                {
                    try
                    {
                        DataStore.workerCache.Add(entry.Key, entry.Value);
                    }
                    catch (Exception)
                    {
                        // Don't care
                    }
                }
            }
        }


        public override void OnLevelUnloading()
        {
            if (isLevelLoaded)
            {
                isLevelLoaded = false;
                DataStore.allowRemovalOfCitizens = false;
            }
        }


        public override void OnLevelLoaded(LoadMode mode)
        {
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

            else if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
                if (!isLevelLoaded)
                {
                    isLevelLoaded = true;
                    // Now we can remove people
                    DataStore.allowRemovalOfCitizens = true;
                    Debugging.releaseBuffer();
                    Debugging.Message("successfully applied");
                }
            }

            // Wait for loading to fully complete.
            while (!LoadingManager.instance.m_loadingComplete) { }

            // Record initial (default) school settings and apply ours over the top.
            SchoolData.instance.OnLoad();

            // Create new XML if one doesn't already exist.
            if (!File.Exists(DataStore.currentFileLocation))
            {
                XMLUtilsWG.WriteToXML();
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
