using System;
using System.IO;
using System.Xml;
using UnityEngine;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Class for XML configuration file utility methods.
    /// </summary>
    internal static class XMLUtils
    {
        // Configuration file name.
        internal const String XML_FILE = "WG_RealisticCity.xml";

        // Loaded flag.
        static bool configLoaded = false;

        /// <summary>
        /// Loads the configuration XML file and sets the datastore.
        /// </summary>
        internal static void ReadFromXML()
        {
            // Check to see if we've already loaded the file; if so, don't do anything.
            if(configLoaded)
            {
                Debug.Log("Realistic Population Revisited: ignoring reload of configuration file.");
                return;
            }

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
                Debug.Log("Realistic Population Revisited: loading configuration file " + DataStore.currentFileLocation);

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

                    // Successfully loaded.
                    configLoaded = true;
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
                Debug.Log("Realistic Population Revisited: configuration file not found. Will output new file to : " + DataStore.currentFileLocation);
            }
        }


        /// <summary>
        /// Updates (or creates a new) XML configuration file with current DataStore settings.
        /// </summary>
        internal static void WriteToXML()
        {
            try
            {
                WG_XMLBaseVersion xml = new XML_VersionSix();
                xml.writeXML(DataStore.currentFileLocation);
            }
            catch (Exception e)
            {
                Debug.Log("Realistic Population Revisited: XML writing exception:\r\n" + e.Message);
            }
        }
    }
}