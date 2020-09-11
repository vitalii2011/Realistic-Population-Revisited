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

        [XmlArray("services")]
        [XmlArrayItem("service")]
        public List<ServiceRecord> services { get; set; }

        [XmlArray("buildings")]
        [XmlArrayItem("building")]
        public List<BuildingRecord> buildings { get; set; }
    }


    /// <summary>
    /// Service setting dictionary record.
    /// </summary>
    public class ServiceRecord
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
        public List<ServiceLevelXML> serviceLevels { get; set; }
    }


    /// <summary>
    /// Service level record.
    /// </summary>
    public class ServiceLevelXML
    {
        // Level ID.
        [XmlAttribute("level")]
        public int level;

        // Floor height.
        [XmlAttribute("floorheight")]
        public float floorHeight;

        // Area per unit.
        [XmlAttribute("areaper")]
        public int areaPer;

        // Floor height.
        [XmlAttribute("firstmin")]
        public float firstMin;

        // Floor height.
        [XmlAttribute("firstmax")]
        public float firstMax;

        // First floor empty.
        [XmlAttribute("firstempty")]
        public bool firstEmpty;

        // Multi-level units.
        [XmlAttribute("multilevel")]
        public bool multiLevel;
    }
}