using System;
using System.IO;
using System.Collections.Generic;
using ColossalFramework;
using System.Xml.Serialization;


namespace RealisticPopulationRevisited
{
    /// XML serialization/deserialization utilities class.
    /// </summary>
    internal static class ConfigUtils
    {
        // Configuration file name.
        internal static readonly string ConfigFileName = "RealisticPopulationConfig.xml";


        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void LoadSettings()
        {
            try
            {
                // Check to see if configuration file exists.
                if (File.Exists(ConfigFileName))
                {
                    // Read it.
                    using (StreamReader reader = new StreamReader(ConfigFileName))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLConfigurationFile));

                        if (!(xmlSerializer.Deserialize(reader) is XMLConfigurationFile configFile))
                        {
                            Debugging.Message("couldn't deserialize configuration file");
                        }
                        else
                        {
                            // Deserialise service list into dictionary.
                            foreach (ServiceRecord serviceElement in configFile.services)
                            {
                                // Find target preset.
                                CalcPack calcPack = PopData.calcPacks?.Find(pack => (pack?.name != null && pack.name.Equals(serviceElement.pack)));
                                if (calcPack?.name == null)
                                {
                                    Debugging.Message("Couldn't find calculation pack " + serviceElement.pack + " for sub-service " + serviceElement.subService);
                                    return;
                                }

                                // Add service to our dictionary.
                                PopData.AddService(serviceElement.service, serviceElement.subService, calcPack);
                            }

                            // Deserialise building list into dictionary.
                            foreach (BuildingRecord buildingElement in configFile.buildings)
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
                    Debugging.Message("no configuration file found");
                }
            }
            catch (Exception e)
            {
                Debugging.Message("exception reading configuration file");
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
                using (StreamWriter writer = new StreamWriter(ConfigFileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLConfigurationFile));
                    XMLConfigurationFile configFile = new XMLConfigurationFile();

                    // Serialise service dictionary.
                    configFile.services = new List<ServiceRecord>();

                    // Iterate through each key (ItemClass.Service) in our dictionary.
                    foreach (ItemClass.Service service in PopData.serviceDict.Keys)
                    {
                        // Iterate through each key (ItemClass.SubService) in our sub-dictionary and serialise it into a ServiceRecord.
                        foreach (ItemClass.SubService subService in PopData.serviceDict[service].Keys)
                        {
                            ServiceRecord newElement = new ServiceRecord();
                            newElement.service = service;
                            newElement.subService = subService;
                            newElement.pack = PopData.serviceDict[service][subService].name;

                            // Add new building record to our config file.
                            configFile.services.Add(newElement);
                        }
                    }

                    // Serialise building dictionary.
                    configFile.buildings = new List<BuildingRecord>();

                    // Iterate through each key (BuildingInfo) in our dictionary and serialise it into a BuildingRecord.
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
                Debugging.Message("exception saving configuration file");
                Debugging.LogException(e);
            }
        }
    }
}