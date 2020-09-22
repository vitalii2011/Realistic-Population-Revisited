using System;
using System.IO;
using System.Collections.Generic;
using ColossalFramework;
using System.Xml.Serialization;


namespace RealisticPopulationRevisited
{
    /// <summary>
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
                                    levels = new LevelData[xmlPack.calculationLevels.Count]
                                };

                                // Iterate through each level in the xml and add to our volumetric pack.
                                foreach (CalculationLevel calculationLevel in xmlPack.calculationLevels)
                                {
                                    volPack.levels[calculationLevel.level] = new LevelData()
                                    {
                                        floorHeight = calculationLevel.floorHeight,
                                        areaPer = calculationLevel.areaPer,
                                        firstFloorMin = calculationLevel.firstMin,
                                        firstFloorMax = calculationLevel.firstMax,
                                        firstFloorEmpty = calculationLevel.firstEmpty,
                                        multiFloorUnits = calculationLevel.multiLevel
                                    };
                                }

                                // Add new pack to our dictionary.
                                PopData.AddCalculationPack(volPack);
                            }

                            // Deserialise consumption records.
                            DataMapping mapper = new DataMapping();
                            foreach (ConsumptionRecord consumptionRecord in configFile.consumption)
                            {
                                // Get relevant DataStore array for this record.
                                int[][] dataArray = mapper.GetArray(consumptionRecord.service, consumptionRecord.subService);

                                // Iterate through each consumption line and populate relevant DataStore fields.
                                foreach(ConsumptionLine consumptionLine in consumptionRecord.levels)
                                {
                                    int level = (int)consumptionLine.level;
                                    dataArray[level][DataStore.POWER] = consumptionLine.power;
                                    dataArray[level][DataStore.WATER] = consumptionLine.water;
                                    dataArray[level][DataStore.SEWAGE] = consumptionLine.sewage;
                                    dataArray[level][DataStore.GARBAGE] = consumptionLine.garbage;
                                    dataArray[level][DataStore.GROUND_POLLUTION] = consumptionLine.pollution;
                                    dataArray[level][DataStore.NOISE_POLLUTION] = consumptionLine.noise;
                                    dataArray[level][DataStore.MAIL] = consumptionLine.mail;
                                    dataArray[level][DataStore.INCOME] = consumptionLine.income;
                                    dataArray[level][DataStore.PRODUCTION] = consumptionLine.production;
                                }
                            }

                            // Deserialise default pack list into dictionary.
                            foreach (DefaultPack defaultPack in configFile.defaults)
                            {
                                // Find target preset.
                                CalcPack calcPack = PopData.calcPacks?.Find(pack => (pack?.name != null && pack.name.Equals(defaultPack.pack)));
                                if (calcPack?.name == null)
                                {
                                    Debugging.Message("Couldn't find calculation pack " + defaultPack.pack + " for sub-service " + defaultPack.subService);
                                    continue;
                                }

                                // Add service to our dictionary.
                                PopData.AddService(defaultPack.service, defaultPack.subService, calcPack);
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

                            // Deserialise building population overrides.
                            DeSerializePopOverrides(configFile.households, DataStore.householdCache);
                            DeSerializePopOverrides(configFile.workplaces, DataStore.workerCache);
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
                                    calculationLevels = new List<CalculationLevel>()
                                };

                                // Iterate through each level and add it to our serialisation.
                                for (int i = 0; i < volPack.levels.Length; ++i)
                                {
                                    CalculationLevel xmlLevel = new CalculationLevel()
                                    {
                                        level = i,
                                        floorHeight = volPack.levels[i].floorHeight,
                                        areaPer = volPack.levels[i].areaPer,
                                        firstMin = volPack.levels[i].firstFloorMin,
                                        firstMax = volPack.levels[i].firstFloorMax,
                                        firstEmpty = volPack.levels[i].firstFloorEmpty,
                                        multiLevel = volPack.levels[i].multiFloorUnits
                                    };

                                    xmlPack.calculationLevels.Add(xmlLevel);
                                }

                                // Add to file.
                                configFile.calcPacks.Add(xmlPack);
                            }
                        }
                    }

                    // Serialise consumption information.
                    configFile.consumption = GetConsumptionRecords();

                    // Serialise default packs dictionary.
                    configFile.defaults = new List<DefaultPack>();

                    // Iterate through each key (ItemClass.Service) in our dictionary.
                    foreach (ItemClass.Service service in PopData.serviceDict.Keys)
                    {
                        // Iterate through each key (ItemClass.SubService) in our sub-dictionary and serialise it into a DefaultPack.
                        foreach (ItemClass.SubService subService in PopData.serviceDict[service].Keys)
                        {
                            DefaultPack defaultPack = new DefaultPack();
                            defaultPack.service = service;
                            defaultPack.subService = subService;
                            defaultPack.pack = PopData.serviceDict[service][subService].name;

                            // Add new building record to our config file.
                            configFile.defaults.Add(defaultPack);
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

                    // Serialise building population overrides.
                    configFile.households = SerializePopOverrides(DataStore.householdCache);
                    configFile.workplaces = SerializePopOverrides(DataStore.workerCache);

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


        /// <summary>
        /// Serialise population overrides for individual building prefabs.
        /// </summary>
        /// <param name="overrideDict">Population dictionary to serialise</param>
        /// <returns>New list of seralised overrides</returns>
        private static List<PopCountOverride> SerializePopOverrides(Dictionary<string, int> overrideDict)
        {
            // Return list.
            List<PopCountOverride> overrideList = new List<PopCountOverride>();

            // Iterate through dictionary and serialise into new PopCountOverride element.
            foreach (String buildingName in overrideDict.Keys)
            {
                PopCountOverride newElement = new PopCountOverride();
                newElement.prefab = buildingName;
                newElement.population = overrideDict[buildingName];

                // Add new building record to return list.
                overrideList.Add(newElement);
            }

            return overrideList;
        }


        /// <summary>
        /// De-serialise population overrides for individual building prefabs.
        /// </summary>
        /// <param name="overrideList">Population dictionary to de-serialise</param>
        /// <param name="overrideDict">Population dictionary to de-serialise into</param>
        /// <returns>New list of seralised overrides</returns>
        private static void DeSerializePopOverrides(List<PopCountOverride> overrideList, Dictionary<string, int> overrideDict)
        {
            // Iterate through list and add each entry to dictionary.
            foreach(PopCountOverride overrideItem in overrideList)
            {
                // Check to see if we already have an override entry (probably from legacy file).
                if (overrideDict.ContainsKey(overrideItem.prefab))
                {
                    // Yes - update existing entry.
                    overrideDict[overrideItem.prefab] = overrideItem.population;
                }
                else
                {
                    // No - add new entry.
                    overrideDict.Add(overrideItem.prefab, overrideItem.population);
                }
            }
        }


        /// <summary>
        /// Returns a list of serialised consumption records from available DataStore arrays.
        /// </summary>
        /// <returns>New list of consumption records</returns>
        private static List<ConsumptionRecord> GetConsumptionRecords()
        {
            List<ConsumptionRecord> list = new List<ConsumptionRecord>(DataMapping.numData);
            DataMapping mapper = new DataMapping();


            // Iterate through each data structure.
            for (int i = 0; i < DataMapping.numData; ++i)
            {
                // Create new consumption record with relevant data and add it to our list.
                ConsumptionRecord newRecord = new ConsumptionRecord()
                {
                    service = mapper.services[i],
                    subService = mapper.subServices[i],
                    levels = SerializeConsumption(mapper.dataArrays[i])
                };
                list.Add(newRecord);
            }

            return list;
        }


        /// <summary>
        /// Serialise consumption levels for provided service/subservice data.
        /// </summary>
        /// <param name="data">DataStore integer array to serialise</param>
        /// <returns>New list of serialised consumption data level records</returns>
        private static List<ConsumptionLine> SerializeConsumption(int[][] dataArray)
        {
            List<ConsumptionLine> lines = new List<ConsumptionLine>();

            // Iterate through each row in the provided data array.
            for (int i = 0; i < dataArray.Length; ++i)
            {
                // Create new consumption line record from data array row.
                ConsumptionLine newLine = new ConsumptionLine()
                {
                    level = (ItemClass.Level)i,
                    power = dataArray[i][DataStore.POWER],
                    water = dataArray[i][DataStore.WATER],
                    sewage = dataArray[i][DataStore.SEWAGE],
                    garbage = dataArray[i][DataStore.GARBAGE],
                    pollution = dataArray[i][DataStore.GROUND_POLLUTION],
                    noise = dataArray[i][DataStore.NOISE_POLLUTION],
                    mail = dataArray[i][DataStore.MAIL],
                    income = dataArray[i][DataStore.INCOME],
                    production = dataArray[i][DataStore.PRODUCTION]
                };

                // Add new record to the return list.
                lines.Add(newLine);
            }

            return lines;
        }
    }
}