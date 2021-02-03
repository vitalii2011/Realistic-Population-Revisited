using System;
using System.IO;
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

        [XmlElement("WhatsNewBeta")]
        [DefaultValue("")]
        public string WhatsNewBeta { get => ModSettings.whatsNewBeta; set => ModSettings.whatsNewBeta = value; }

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

        // Use legacy calculations by default.
        [XmlElement("NewSavesLegacy")]
        public bool DefaultLegacy { get => ModSettings.newSaveLegacy; set => ModSettings.newSaveLegacy = value; }

        // Realistic education.
        [XmlElement("EnableSchoolPop")]
        public bool EnableSchools { get => ModSettings.enableSchoolPop; set => ModSettings.enableSchoolPop = value; }

        // School properties.
        [XmlElement("EnableSchoolProperties")]
        public bool EnableSchoolProperties { get => ModSettings.enableSchoolProperties; set => ModSettings.enableSchoolProperties = value; }

        // Default school capacity multiplier.
        [XmlElement("DefaultSchoolMultiplier")]
        public float DefaultSchoolModifier { get => ModSettings.DefaultSchoolMult; set => ModSettings.DefaultSchoolMult = value; }
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
                    xmlSerializer.Serialize(writer, new XMLSettingsFile());
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving XML settings file");
            }
        }
    }
}