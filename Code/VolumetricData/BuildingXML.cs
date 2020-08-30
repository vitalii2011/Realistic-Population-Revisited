using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using ColossalFramework;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Defines the XML settings file.
    /// </summary>
    [XmlRoot("SettingsFile")]
    public class XMLBuildingFile
    {
        // Stores the version of the most recent update notification that the user has decided to "Don't show again".
        [XmlElement("version")]
        public int NotificationVersion { get; set; }

        [XmlArray("buildings")]
        [XmlArrayItem("building")]
        public List<BuildingRecord> buildings { get; set; }
    }


    /// <summary>
    /// Building setting dictionary record.
    /// </summary>
    public class BuildingRecord
    {
        // Building prefab name.
        [XmlAttribute("prefab")]
        public string prefab;

        // Calculation pack.
        [XmlAttribute("pack")]
        [DefaultValue("")]
        public string pack;
    }


    /// XML serialization/deserialization utilities class.
    /// </summary>
    internal static class ConfigUtils
    {
        internal static readonly string SettingsFileName = "RealisticPopulationConfig.xml";


        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void LoadSettings()
        {
            try
            {
                // Check to see if configuration file exists.
                if (File.Exists(SettingsFileName))
                {
                    // Read it.
                    using (StreamReader reader = new StreamReader(SettingsFileName))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLBuildingFile));

                        if (!(xmlSerializer.Deserialize(reader) is XMLBuildingFile xmlBuildingFile))
                        {
                            Debugging.Message("couldn't deserialize settings file");
                        }
                        else
                        {
                            // Deserialize building list into dictionary.
                            foreach (BuildingRecord buildingElement in xmlBuildingFile.buildings)
                            {
                                // Safety first!
                                if (buildingElement.prefab.IsNullOrWhiteSpace() || buildingElement.pack.IsNullOrWhiteSpace())
                                {
                                    Debugging.Message("Null element in configuration file");
                                    return;
                                }
                                
                                // Find target preset.
                                CalcPack calcPack = PopData.calcPacks?.Find(pack => (pack?.name != null && pack.name.Equals(buildingElement.pack)));
                                if (calcPack?.name == null)
                                {
                                    Debugging.Message("Couldn't find calculation pack " + buildingElement.pack + " for " + buildingElement.prefab);
                                    return;
                                }

                                // Add building to our dictionary.
                                PopData.buildingDict.Add(buildingElement.prefab, calcPack);
                            }
                        }
                    }
                }
                else
                {
                    Debugging.Message("no settings file found");
                }
            }
            catch (Exception e)
            {
                Debugging.Message("exception reading XML settings file");
                Debugging.LogException(e);
            }
        }


        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        internal static void SaveSettings()
        {
            try
            {
                // Pretty straightforward.  Serialisation is within settings file class.
                using (StreamWriter writer = new StreamWriter(SettingsFileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLBuildingFile));
                    XMLBuildingFile configFile = new XMLBuildingFile();

                    // Serialize our dictionary.
                    configFile.buildings = new List<BuildingRecord>();

                    // Iterate through each key (BuildingInfo) in our dictionary and serialize it into a BuildingRecord.
                    foreach (string prefabName in PopData.buildingDict.Keys)
                    {
                        BuildingRecord newElement = new BuildingRecord();
                        newElement.prefab = prefabName;
                        newElement.pack = PopData.buildingDict[prefabName].name;

                        // Add new building record to our config file.
                        configFile.buildings.Add(newElement);
                    }

                    // Write to file.
                    xmlSerializer.Serialize(writer, configFile);
                }
            }
            catch (Exception e)
            {
                Debugging.Message("exception saving XML settings file");
                Debugging.LogException(e);
            }
        }
    }
}