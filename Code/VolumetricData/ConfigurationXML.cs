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
}