using System.ComponentModel;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Defines the XML configuration file.
    /// </summary>
    [XmlRoot("Configuration")]
    public class XMLConfigurationFile
    {
        // Stores the version of the most recent update notification that the user has decided to "Don't show again".
        [XmlElement("version")]
        public int NotificationVersion { get; set; }

        [XmlArray("calcpacks")]
        [XmlArrayItem("calcpack")]
        public List<CalcPackXML> calcPacks { get; set; }

        [XmlArray("consumption")]
        [XmlArrayItem("consumption")]
        public List<ConsumptionRecord> consumption { get; set; }

        [XmlArray("defaults")]
        [XmlArrayItem("default")]
        public List<DefaultPack> defaults { get; set; }

        [XmlArray("buildings")]
        [XmlArrayItem("building")]
        public List<BuildingRecord> buildings { get; set; }

        [XmlArray("override_households")]
        [XmlArrayItem("households")]
        public List<PopCountOverride> households { get; set; }

        [XmlArray("override_workplaces")]
        [XmlArrayItem("workplaces")]
        public List<PopCountOverride> workplaces { get; set; }
    }


    /// <summary>
    /// Default calculation pack dictionary record.
    /// </summary>
    public class DefaultPack
    {
        // Service ID.
        [XmlAttribute("service")]
        public ItemClass.Service service;

        // Sub-service ID.
        [XmlAttribute("subservice")]
        public ItemClass.SubService subService;

        // Calculation pack.
        [XmlAttribute("pack")]
        [DefaultValue("")]
        public string pack;
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


    /// <summary>
    /// Building population count override record.
    /// </summary>
    public class PopCountOverride
    {
        // Building prefab name.
        [XmlAttribute("prefab")]
        public string prefab;

        //Household count.
        [XmlAttribute("population")]
        [DefaultValue(0)]
        public int population;
    }


    /// <summary>
    /// Custom calculation pack record.
    /// </summary>
    public class CalcPackXML
    {
        // Pack name.
        [XmlAttribute("name")]
        public string name;

        // Service type.
        [XmlAttribute("service")]
        public ItemClass.Service service;

        // Level records.
        [XmlArray("levels")]
        [XmlArrayItem("level")]
        public List<CalculationLevel> calculationLevels { get; set; }
    }


    /// <summary>
    /// Custom calculation pack individual level line.
    /// </summary>
    public class CalculationLevel
    {
        // Level ID.
        [XmlAttribute("level")]
        public int level;

        // Floor height.
        [XmlAttribute("floorheight")]
        public float floorHeight;

        // Empty area.
        [XmlAttribute("empty")]
        public int emptyArea;

        // Area per unit.
        [XmlAttribute("areaper")]
        public int areaPer;

        // Floor height.
        [XmlAttribute("firstmin")]
        public float firstMin;

        // Floor height.
        [XmlAttribute("firstextra")]
        public float firstExtra;

        // First floor empty.
        [XmlAttribute("firstempty")]
        public bool firstEmpty;

        // Multi-level units.
        [XmlAttribute("multilevel")]
        public bool multiLevel;
    }


    /// <summary>
    /// Service consumption record.
    /// </summary>
    public class ConsumptionRecord
    {
        // Service ID.
        [XmlAttribute("service")]
        public ItemClass.Service service;

        // Sub-service ID.
        [XmlAttribute("subservice")]
        public ItemClass.SubService subService;

        // Level records.
        [XmlArray("levels")]
        [XmlArrayItem("level")]
        public List<ConsumptionLine> levels { get; set; }
    }


    /// <summary>
    /// Consumption record individual level line.
    /// </summary>
    public class ConsumptionLine
    {
        // Level.
        [XmlAttribute("level")]
        public ItemClass.Level level;

        // Power consumption.
        [XmlAttribute("power")]
        public int power;

        // Water consumption.
        [XmlAttribute("water")]
        public int water;

        // Sewage generation.
        [XmlAttribute("sewage")]
        public int sewage;

        // Garbage generation.
        [XmlAttribute("garbage")]
        public int garbage;

        // Pollution generation.
        [XmlAttribute("pollution")]
        public int pollution;

        // Noise generation.
        [XmlAttribute("noise")]
        public int noise;

        // Mail generation.
        [XmlAttribute("mail")]
        public int mail;

        // Income generation.
        [XmlAttribute("income")]
        public int income;

        // Production.
        [XmlAttribute("production")]
        public int production;
    }
}