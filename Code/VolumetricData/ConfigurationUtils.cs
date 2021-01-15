using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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


        // Read flag.
        internal static bool configRead = false;


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
                            // Deserialise population calculation packs.
                            foreach (PopPackXML xmlPack in configFile.popPacks)
                            {
                                // Convert to volumetric pack.
                                VolumetricPopPack volPack = new VolumetricPopPack()
                                {
                                    name = xmlPack.name,
                                    displayName = xmlPack.name,
                                    service = xmlPack.service,
                                    version = (int)DataVersion.customOne,
                                    levels = new LevelData[xmlPack.calculationLevels.Count]
                                };

                                // Iterate through each level in the xml and add to our volumetric pack.
                                foreach (PopLevel calculationLevel in xmlPack.calculationLevels)
                                {
                                    volPack.levels[calculationLevel.level] = new LevelData()
                                    {
                                        //floorHeight = calculationLevel.floorHeight,
                                        emptyArea = calculationLevel.emptyArea,
                                        emptyPercent = calculationLevel.emptyPercent,
                                        areaPer = calculationLevel.areaPer,
                                        multiFloorUnits = calculationLevel.multiLevel
                                    };
                                }

                                // Add new pack to our dictionary.
                                PopData.instance.AddCalculationPack(volPack);
                            }

                            // Deserialise floor calculation packs.
                            foreach (FloorPackXML xmlPack in configFile.floorPacks)
                            {
                                // Convert to floor pack.
                                FloorDataPack floorPack = new FloorDataPack()
                                {
                                    name = xmlPack.name,
                                    displayName = xmlPack.name,
                                    version = (int)DataVersion.customOne,
                                    floorHeight = xmlPack.floorHeight,
                                    firstFloorMin = xmlPack.firstMin,
                                    firstFloorExtra = xmlPack.firstExtra,
                                    firstFloorEmpty = xmlPack.firstEmpty
                                };

                                // Add new pack to our dictionary.
                                FloorData.instance.AddCalculationPack(floorPack);
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

                            // Deserialise default pack lists.
                            PopData.instance.DeserializeDefaults(configFile.popDefaults);
                            FloorData.instance.DeserializeDefaults(configFile.floorDefaults);

                            // Deserialise building pack lists.
                            PopData.instance.DeserializeBuildings(configFile.buildings);
                            FloorData.instance.DeserializeBuildings(configFile.buildings);
                            SchoolData.instance.DeserializeBuildings(configFile.buildings);
                            Multipliers.instance.DeserializeBuildings(configFile.buildings);

                            // Deserialise building population overrides.
                            PopData.instance.DeserializePopOverrides(configFile.popOverrides);

                            // Deserialize floor overrides.
                            foreach (FloorCalcOverride floorOverride in configFile.floors)
                            {
                                FloorData.instance.AddOverride(floorOverride.prefab, new FloorDataPack
                                {
                                    firstFloorMin = floorOverride.firstHeight,
                                    floorHeight = floorOverride.floorHeight
                                });
                            }
                        }
                    }
                }
                else
                {
                    Debugging.Message("no configuration file found");
                }

                // Set status flag.
                configRead = true;
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
                    configFile.popPacks = new List<PopPackXML>();

                    // Iterate through all calculation packs in our dictionary.
                    foreach (PopDataPack calcPack in PopData.instance.calcPacks)
                    {
                        // Look for volumetric packs.
                        if (calcPack is VolumetricPopPack volPack)
                        {
                            // Look for packs marked as custom.
                            if (volPack.version == (int)DataVersion.customOne)
                            {
                                // Found one - serialise it.
                                PopPackXML xmlPack = new PopPackXML()
                                {
                                    name = volPack.name,
                                    service = volPack.service,
                                    calculationLevels = new List<PopLevel>()
                                };

                                // Iterate through each level and add it to our serialisation.
                                for (int i = 0; i < volPack.levels.Length; ++i)
                                {
                                    PopLevel xmlLevel = new PopLevel()
                                    {
                                        level = i,
                                        emptyArea = volPack.levels[i].emptyArea,
                                        emptyPercent = volPack.levels[i].emptyPercent,
                                        areaPer = volPack.levels[i].areaPer,
                                        multiLevel = volPack.levels[i].multiFloorUnits
                                    };

                                    xmlPack.calculationLevels.Add(xmlLevel);
                                }

                                // Add to file.
                                configFile.popPacks.Add(xmlPack);
                            }
                        }
                    }

                    // Serialise custom packs.
                    // TODO method with above
                    configFile.floorPacks = new List<FloorPackXML>();

                    // Iterate through all calculation packs in our dictionary.
                    foreach (DataPack calcPack in FloorData.instance.calcPacks)
                    {
                        // Look for volumetric packs.
                        if (calcPack is FloorDataPack floorPack)
                        {
                            // Look for packs marked as custom.
                            if (floorPack.version == (int)DataVersion.customOne)
                            {
                                // Found one - serialise it.
                                FloorPackXML xmlPack = new FloorPackXML()
                                {
                                    name = floorPack.name,
                                    floorHeight = floorPack.floorHeight,
                                    firstMin = floorPack.firstFloorMin,
                                    firstExtra = floorPack.firstFloorExtra,
                                    firstEmpty = floorPack.firstFloorEmpty
                                };

                                // Add to file.
                                configFile.floorPacks.Add(xmlPack);
                            }
                        }
                    }

                    // Serialise consumption information.
                    configFile.consumption = GetConsumptionRecords();

                    // Serialise default dictionaries.
                    configFile.popDefaults = PopData.instance.SerializeDefaults();
                    configFile.floorDefaults = FloorData.instance.SerializeDefaults();

                    // Serialise building pack dictionaries, in order.
                    SortedList<string, BuildingRecord> buildingList = PopData.instance.SerializeBuildings();
                    buildingList = FloorData.instance.SerializeBuildings(buildingList);
                    buildingList = SchoolData.instance.SerializeBuildings(buildingList);
                    buildingList = Multipliers.instance.SerializeBuildings(buildingList);
                    configFile.buildings = buildingList.Values.ToList();

                    // Serialise building population overrides.
                    configFile.popOverrides = PopData.instance.SerializePopOverrides();

                    // Serialise floor overrides.
                    configFile.floors = new List<FloorCalcOverride>();
                    foreach (KeyValuePair<string, FloorDataPack> floorOverride in FloorData.instance.overrides)
                    {
                        configFile.floors.Add(new FloorCalcOverride
                        {
                            prefab = floorOverride.Key,
                            firstHeight = floorOverride.Value.firstFloorMin,
                            floorHeight = floorOverride.Value.floorHeight
                        });
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