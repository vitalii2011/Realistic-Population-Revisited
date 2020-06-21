using System;
using System.Collections.Generic;
using UnityEngine;
using ICities;
using ColossalFramework.Math;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using System.Linq;
using Harmony;


namespace RealisticPopulationRevisited
{
    public class Loading : LoadingExtensionBase
    {
        const string HarmonyID = "com.github.algernon-A.csl.realisticpopulationrevisited";
        private HarmonyInstance _harmony = HarmonyInstance.Create(HarmonyID);

        private static volatile bool isModEnabled = false;
        private static volatile bool isLevelLoaded = false;

        // Used to flag if a conflicting mod is running.
        private static bool conflictingMod = false;

        // XML settings file.
        public static SettingsFile settingsFile;


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

                // Harmony patches.
                Debugging.Message("version v" + RealPopMod.Version + " loading.");
                _harmony.PatchAll(GetType().Assembly);
                Debugging.Message("patching complete.");

                DataStore.ClearCache();

                XMLUtils.ReadFromXML();
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
                        DataStore.seedToId.Add(number.seed, (ushort) i);
                    }
                    catch (Exception)
                    {
                        //Debugging.writeDebugToFile("Seed collision at number: " + i);
                    }
                }

                // Check for Ploppable RICO Revisited.
                ModUtils.isRICOEnabled = ModUtils.IsModInstalled("ploppablerico");

                Debugging.Message("successfully loaded");
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

        public override void OnReleased()
        {
            if (isModEnabled)
            {
                isModEnabled = false;

                // Unapply Harmony patches.
                _harmony.UnpatchAll(HarmonyID);
                Debugging.Message("patches unapplied");
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

            // Save updated XML (or create new).
            XMLUtils.WriteToXML();

            // Add button to building info panels.
            BuildingDetailsPanel.AddInfoPanelButton();

            // Load settings file and check if we need to display update notification.
            settingsFile = Configuration<SettingsFile>.Load();
            if (settingsFile.NotificationVersion != 2)
            {
                // No update notification "Don't show again" flag found; show the notification.
                UpdateNotification notification = new UpdateNotification();
                notification.Create();
                notification.Show();
            }

            // Load settings.
            SettingsFile settings = Configuration<SettingsFile>.Load();

            // Hotkey.
            try
            {
                // Should throw an exception if this doesn't work.
                UIThreading.hotKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), settings.hotkey);
            }
            catch
            {
                // Don't care - just don't do anything.
            }

            // Hotkey modifiers.
            UIThreading.hotCtrl = settings.ctrl;
            UIThreading.hotAlt = settings.alt;
            UIThreading.hotShift = settings.shift;

            // Set up options panel event handler.
            OptionsPanel.OptionsEventHook();
        }
    }
}
