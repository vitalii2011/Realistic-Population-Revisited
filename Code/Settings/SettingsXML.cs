using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using UnityEngine;


namespace RealPop2
{
    /// <summary>
    /// Defines the XML settings file.
    /// </summary>
    [XmlRoot("SettingsFile")]
    public class XMLSettingsFile
    {
        [XmlElement("WhatsNewVersion")]
        public string WhatsNewVersion { get => ModSettings.whatsNewVersion; set => ModSettings.whatsNewVersion = value; }

        [XmlElement("WhatsNewBetaVersion")]
        [DefaultValue(0)]
        public int WhatsNewBetaVersion { get => ModSettings.whatsNewBetaVersion; set => ModSettings.whatsNewBetaVersion = value; }

        // Language.
        [XmlElement("Language")]
        public string Language
        {
            get
            {
                return Translations.Language;
            }
            set
            {
                Translations.Language = value;
            }
        }

        // Building details panel hotkey backwards-compatibility.
        [XmlElement("hotkey")]
        [DefaultValue("")]
        public string Hotkey { get => ""; set => UIThreading.hotKey = (KeyCode)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("ctrl")]
        [DefaultValue(false)]
        public bool Ctrl { get => false;  set => UIThreading.hotCtrl = value; }

        [XmlElement("alt")]
        [DefaultValue(false)]
        public bool Alt { get => false;  set => UIThreading.hotAlt = value; }

        [XmlElement("shift")]
        [DefaultValue(false)]
        public bool Shift { get => false;  set => UIThreading.hotShift = value; }

        // New building details panel hotkey element.
        [XmlElement("PanelKey")]
        public KeyBinding PanelKey
        {
            get
            {
                return new KeyBinding
                {
                    keyCode = (int)UIThreading.hotKey,
                    control = UIThreading.hotCtrl,
                    shift = UIThreading.hotShift,
                    alt = UIThreading.hotAlt
                };
            }
            set
            {
                // Backwads compatibility - this won't exist in older-format configuration files.
                if (value != null)
                {
                    UIThreading.hotKey = (KeyCode)value.keyCode;
                    UIThreading.hotCtrl = value.control;
                    UIThreading.hotShift = value.shift;
                    UIThreading.hotAlt = value.alt;
                }
            }
        }

        // Use legacy calculations by default (deprecated all-in-one setting).
        [XmlElement("NewSavesLegacy")]
        public bool DefaultLegacy
        {
            set
            {
                ModSettings.newSaveLegacyRes = value;
                ModSettings.newSaveLegacyCom = value;
                ModSettings.newSaveLegacyInd = value;
                ModSettings.newSaveLegacyOff = value;
            }
        }


        // Use legacy calculations by default (new granular setting).
        [XmlElement("NewSavesLegacyRes")]
        public bool DefaultLegacyRes { get => ModSettings.newSaveLegacyRes; set => ModSettings.newSaveLegacyRes = value; }

        [XmlElement("NewSavesLegacyCom")]
        public bool DefaultLegacyCom { get => ModSettings.newSaveLegacyCom; set => ModSettings.newSaveLegacyCom = value; }

        [XmlElement("NewSavesLegacyInd")]
        public bool DefaultLegacyInd { get => ModSettings.newSaveLegacyInd; set => ModSettings.newSaveLegacyInd = value; }

        [XmlElement("NewSavesLegacyOff")]
        public bool DefaultLegacyOff { get => ModSettings.newSaveLegacyOff; set => ModSettings.newSaveLegacyOff = value; }


        // Commercial visitor calculations - clamp to 0 or 1 at this stage.
        [XmlArray("CommercialVisitsModes")]
        [XmlArrayItem("Mode")]
        public List<SubServiceEntry<ItemClass.SubService, int>> comVisitModes;

        // Commercial visitor multiplier.
        [XmlArray("CommercialVisitsPercentages")]
        [XmlArrayItem("Percentage")]
        public List<SubServiceEntry<ItemClass.SubService, int>> comVisitMults;

        // Realistic education.
        [XmlElement("EnableSchoolPop")]
        public bool EnableSchools { get => ModSettings.enableSchoolPop; set => ModSettings.enableSchoolPop = value; }

        // School properties.
        [XmlElement("EnableSchoolProperties")]
        public bool EnableSchoolProperties { get => ModSettings.enableSchoolProperties; set => ModSettings.enableSchoolProperties = value; }

        // Default school capacity multiplier.
        [XmlElement("DefaultSchoolMultiplier")]
        public float DefaultSchoolModifier { get => ModSettings.DefaultSchoolMult; set => ModSettings.DefaultSchoolMult = value; }

        // Crime rate multiplier.
        [XmlElement("CrimeRateMultiplier")]
        public float CrimeRateMultiplier { get => ModSettings.crimeMultiplier; set => ModSettings.crimeMultiplier = value; }
    }


    /// <summary>
    /// Basic keybinding class - code and modifiers.
    /// </summary>
    public class KeyBinding
    {
        [XmlAttribute("KeyCode")]
        public int keyCode;

        [XmlAttribute("Control")]
        public bool control;

        [XmlAttribute("Shift")]
        public bool shift;

        [XmlAttribute("Alt")]
        public bool alt;
    }



    /// <summary>
    /// Basic serializable KeyValuePair for SubService dictionaries. 
    /// </summary>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="V">Value</typeparam>
    [Serializable]
    [XmlType(TypeName = "SubServiceEntry")]
    public struct SubServiceEntry<K, V>
    {
        [XmlAttribute("SubService")]
        public K Key
        { get; set; }

        [XmlAttribute("Value")]
        public V Value
        { get; set; }
    }


    /// XML serialization/deserialization utilities class.
    /// </summary>
    internal static class SettingsUtils
    {
        internal static readonly string SettingsFileName = "RealisticPopulation.xml";


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
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLSettingsFile));
                        if (!(xmlSerializer.Deserialize(reader) is XMLSettingsFile xmlSettingsFile))
                        {
                            Logging.Error("couldn't deserialize settings file");
                        }
                        else
                        {
                            // Iterate through each KeyValuePair parsed and add update entry in commercial visit modes dictionary.
                            foreach (SubServiceEntry<ItemClass.SubService, int> entry in xmlSettingsFile.comVisitModes)
                            {
                                RealisticVisitplaceCount.comVisitModes[entry.Key] = entry.Value;
                            }

                            // Iterate through each KeyValuePair parsed and add update entry in commercial visit multipliers dictionary.
                            foreach (SubServiceEntry<ItemClass.SubService, int> entry in xmlSettingsFile.comVisitMults)
                            {
                                RealisticVisitplaceCount.comVisitMults[entry.Key] = entry.Value;
                            }
                        }
                    }
                }
                else
                {
                    Logging.Message("no settings file found");
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception reading XML settings file");
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
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLSettingsFile));
                    XMLSettingsFile settingsFile = new XMLSettingsFile();

                    // Iterate though each entry in the commercial visit modes dictionary and serialize into XML KeyValuePair.
                    List<SubServiceEntry<ItemClass.SubService, int>> vistModes = new List<SubServiceEntry<ItemClass.SubService, int>>();
                    foreach (KeyValuePair<ItemClass.SubService, int> entry in RealisticVisitplaceCount.comVisitModes)
                    {
                        vistModes.Add(new SubServiceEntry<ItemClass.SubService, int>
                        {
                            Key = entry.Key,
                            Value = entry.Value
                        });
                    }
                    settingsFile.comVisitModes = vistModes;

                    // Iterate though each entry in the commercial visit multipliers dictionary and serialize into XML KeyValuePair.
                    List<SubServiceEntry<ItemClass.SubService, int>> visitMults = new List<SubServiceEntry<ItemClass.SubService, int>>();
                    foreach (KeyValuePair<ItemClass.SubService, int> entry in RealisticVisitplaceCount.comVisitMults)
                    {
                        visitMults.Add(new SubServiceEntry<ItemClass.SubService, int>
                        {
                            Key = entry.Key,
                            Value = entry.Value
                        });
                    }
                    settingsFile.comVisitMults = visitMults;

                    // Save to file.
                    xmlSerializer.Serialize(writer, settingsFile);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving XML settings file");
            }
        }
    }
}