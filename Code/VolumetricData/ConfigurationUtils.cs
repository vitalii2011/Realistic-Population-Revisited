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
                            // Deserialise calculation packs.
                            foreach (CalcPackXML xmlPack in configFile.calcPacks)
                            {
                                // Convert to volumetric pack.
                                VolumetricPack volPack = new VolumetricPack()
                                {
                                    name = xmlPack.name,
                                    displayName = xmlPack.name,
                                    service = xmlPack.service,
                                    version = (int)DataVersion.customOne,
                                    levels = new LevelData[xmlPack.serviceLevels.Count]
                                };

                                // Iterate through each level in the xml and add to our volumetric pack.
                                foreach (ServiceLevelXML serviceLevel in xmlPack.serviceLevels)
                                {
                                    volPack.levels[serviceLevel.level] = new LevelData()
                                    {
                                        floorHeight = serviceLevel.floorHeight,
                                        areaPer = serviceLevel.areaPer,
                                        firstFloorMin = serviceLevel.firstMin,
                                        firstFloorMax = serviceLevel.firstMax,
                                        firstFloorEmpty = serviceLevel.firstEmpty,
                                        multiFloorUnits = serviceLevel.multiLevel
                                    };
                                }

                                // Add new pack to our dictionary.
                                PopData.AddCalculationPack(volPack);
                            }

                            // Deserialise service list into dictionary.
                            foreach (ServiceRecord serviceElement in configFile.services)
                            {
                                // Find target preset.
                                CalcPack calcPack = PopData.calcPacks?.Find(pack => (pack?.name != null && pack.name.Equals(serviceElement.pack)));
                                if (calcPack?.name == null)
                                {
                                    Debugging.Message("Couldn't find calculation pack " + serviceElement.pack + " for sub-service " + serviceElement.subService);
                                    continue;
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
                                    continue;
                                }

                                // Find target preset.
                                CalcPack calcPack = PopData.calcPacks?.Find(pack => (pack?.name != null && pack.name.Equals(buildingElement.pack)));
                                if (calcPack?.name == null)
                                {
                                    Debugging.Message("Couldn't find calculation pack " + buildingElement.pack + " for " + buildingElement.prefab);
                                    continue;
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

                    // Serialise custom packs.
                    configFile.calcPacks = new List<CalcPackXML>();

                    // Iterate through all calculation packs in our dictionary.
                    foreach (CalcPack calcPack in PopData.calcPacks)
                    {
                        // Look for volumetric packs.
                        if (calcPack is VolumetricPack volPack)
                        {
                            // Look for packs marked as custom.
                            if (volPack.version == (int)DataVersion.customOne)
                            {
                                // Found one - serialise it.
                                CalcPackXML xmlPack = new CalcPackXML()
                                {
                                    name = volPack.name,
                                    service = volPack.service,
                                    serviceLevels = new List<ServiceLevelXML>()
                                };

                                // Iterate through each level and add it to our serialisation.
                                for (int i = 0; i < volPack.levels.Length; ++i)
                                {
                                    ServiceLevelXML xmlLevel = new ServiceLevelXML()
                                    {
                                        level = i,
                                        floorHeight = volPack.levels[i].floorHeight,
                                        areaPer = volPack.levels[i].areaPer,
                                        firstMin = volPack.levels[i].firstFloorMin,
                                        firstMax = volPack.levels[i].firstFloorMax,
                                        firstEmpty = volPack.levels[i].firstFloorEmpty,
                                        multiLevel = volPack.levels[i].multiFloorUnits
                                    };

                                    xmlPack.serviceLevels.Add(xmlLevel);
                                }

                                // Add to file.
                                configFile.calcPacks.Add(xmlPack);
                            }
                        }
                    }

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