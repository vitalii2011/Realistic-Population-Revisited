using System;
using System.IO;
using System.Xml;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Class for XML configuration file utility methods.
    /// </summary>
    internal static class XMLUtilsWG
    {
        // Configuration file name.
        internal const String XML_FILE = "WG_RealisticCity.xml";

        // Flag to determine if we're writing to this legacy file.
        internal static bool writeToLegacy = false;


        /// <summary>
        /// Loads the configuration XML file and sets the datastore.
        /// </summary>
        internal static void ReadFromXML()
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
                Debugging.Message("loading legacy configuration file ", DataStore.currentFileLocation);

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
                Debugging.Message("legacy configuration file not found. Will output new file to: " + DataStore.currentFileLocation);
            }
        }


        /// <summary>
        /// Updates (or creates a new) XML configuration file with current DataStore settings.
        /// </summary>
        internal static void WriteToXML()
        {
            // Only write to files if the relevant setting is set (either through a legacy configuration file already existing, or through the user specifically creating one via the options panel).
            if (writeToLegacy)
            {
                try
                {
                    WG_XMLBaseVersion xml = new XML_VersionSix();
                    xml.writeXML(DataStore.currentFileLocation);
                }
                catch (Exception e)
                {
                    Debugging.Message("XML writing exception:\r\n", e.Message);
                }
            }
        }
    }
}