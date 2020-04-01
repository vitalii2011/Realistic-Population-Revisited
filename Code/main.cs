using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICities;
using System.Diagnostics;
using ColossalFramework.Math;
using ColossalFramework;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using System.Linq;
using Harmony;


namespace RealisticPopulationRevisited
{
    public class LoadingExtension : LoadingExtensionBase
    {
        const string HarmonyID = "com.github.algernon-A.csl.realisticpopulationrevisited";
        private HarmonyInstance _harmony = HarmonyInstance.Create(HarmonyID);

        private const int RES = 0;
        private const int COM = 0;
        private const int IND = 0;
        private const int INDEX = 0;
        private const int OFFICE = 0;
        public const String XML_FILE = "WG_RealisticCity.xml";

        private static volatile bool isModEnabled = false;
        private static volatile bool isLevelLoaded = false;
        private static Stopwatch sw;

        // Used to flag if a conflicting mod is running.
        private static bool conflictingMod = false;

        public static bool IsModEnabled(UInt64 id)
        {
            return PluginManager.instance.GetPluginsInfo().Any(mod => (mod.publishedFileID.AsUInt64 == id && mod.isEnabled));
        }

        public override void OnCreated(ILoading loading)
        {
            // Check for original WG Realistic Population and Consumption Mod; if it's enabled, flag and don't activate this mod.
            if (IsModEnabled(426163185ul))
            {
                conflictingMod = true;
                UnityEngine.Debug.Log("Realistic Population Revisited: Realistic Population and Consumption Mod detected, skipping activation.");
            }
            else if (!isModEnabled)
            {
                isModEnabled = true;
                sw = Stopwatch.StartNew();

                // Harmony patches.
                UnityEngine.Debug.Log("Realistic Population Revisited: version 1.1 loading.");
                _harmony.PatchAll(GetType().Assembly);
                UnityEngine.Debug.Log("Realistic Population Revisited: patching complete.");

                DataStore.ClearCache();

                ReadFromXML();
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

                sw.Stop();
                UnityEngine.Debug.Log("Realistic Population Revisited: Successfully loaded in " + sw.ElapsedMilliseconds + " ms.");
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
                UnityEngine.Debug.Log("Realistic Population Revisited: patches unapplied.");
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
            // Check to see if an conflicting mod has been detected - if so, alert the user and abort operation.
            if (conflictingMod)
            {
                ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                panel.SetMessage("Realistic Population Revisited", "Original Realistic Population and Consumption Mod mod detected - Realistic Population Revisited is shutting down to protect your game.  Only ONE of these mods can be enabled at the same time; please unsubscribe from the old Realistic Population and Consumption Mod, which is now deprecated!", false);
            }
            else if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
                if (!isLevelLoaded)
                {
                    isLevelLoaded = true;
                    // Now we can remove people
                    DataStore.allowRemovalOfCitizens = true;
                    Debugging.releaseBuffer();
                    UnityEngine.Debug.Log("Realistic Population Revisited successfully loaded in " + sw.ElapsedMilliseconds + " ms.");
                }
            }

            try
            {
                WG_XMLBaseVersion xml = new XML_VersionSix();
                xml.writeXML(DataStore.currentFileLocation);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Realistic Population Revisited - release exception:\r\n" + e.Message);
            }
        }


        /// <summary>
        ///
        /// </summary>
        private void ReadFromXML()
        {
            // Check the exe directory first
            DataStore.currentFileLocation = ColossalFramework.IO.DataLocation.executableDirectory + Path.DirectorySeparatorChar + XML_FILE;
            bool fileAvailable = File.Exists(DataStore.currentFileLocation);

            if (!fileAvailable)
            {
                // Switch to default which is the cities skylines in the application data area.
                DataStore.currentFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + XML_FILE;
                fileAvailable = File.Exists(DataStore.currentFileLocation);
            }

            if (fileAvailable)
            {
                // Load in from XML - Designed to be flat file for ease
                WG_XMLBaseVersion reader = new XML_VersionSix();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(DataStore.currentFileLocation);

                    int version = Convert.ToInt32(doc.DocumentElement.Attributes["version"].InnerText);
                    if (version > 3 && version <= 5)
                    {
                        // Use version 5
                        reader = new XML_VersionFive();

                        // Make a back up copy of the old system to be safe
                        File.Copy(DataStore.currentFileLocation, DataStore.currentFileLocation + ".ver5", true);
                        string error = "Detected an old version of the XML (v5). " + DataStore.currentFileLocation + ".ver5 has been created for future reference and will be upgraded to the new version.";
                        Debugging.bufferWarning(error);
                    }
                    else if (version <= 3) // Uh oh... version 4 was a while back..
                    {
                        string error = "Detected an unsupported version of the XML (v4 or less). Backing up for a new configuration as :" + DataStore.currentFileLocation + ".ver4";
                        Debugging.bufferWarning(error);
                        File.Copy(DataStore.currentFileLocation, DataStore.currentFileLocation + ".ver4", true);
                        return;
                    }
                    reader.readXML(doc);
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Debugging.bufferWarning("The following exception(s) were detected while loading the XML file. Some (or all) values may not be loaded.");
                    Debugging.bufferWarning(e.Message);
                }
            }
            else
            {
                UnityEngine.Debug.Log("Realistic Population Revisited: configuration file not found. Will output new file to : " + DataStore.currentFileLocation);
            }
        }
    }
}
