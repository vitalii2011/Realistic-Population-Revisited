using System;
using System.Reflection;
using RealPop2.MessageBox;


namespace RealPop2
{
    /// <summary>
    /// "What's new" message box.  Based on macsergey's code in Intersection Marking Tool (Node Markup) mod.
    /// </summary>
    internal static class WhatsNew
    {
        // List of versions and associated update message lines (as translation keys).
        private readonly static WhatsNewMessage[] WhatsNewMessages = new WhatsNewMessage[]
        {
            new WhatsNewMessage
            {
                version = new Version("2.0.0.0"),
                versionHeader = " BETA 12",
                betaVersion = 12,
                messageKeys = false,
                messages = new string[]
                {
                    "Commercial visit calculation modes and multipliers can now be set seperately for each commercial sub-service"
                }
            },
            new WhatsNewMessage
            {
                version = new Version("2.0.0.0"),
                versionHeader = " BETA 11",
                betaVersion = 11,
                messageKeys = false,
                messages = new string[]
                {
                    "Add configurable multiplier for new-style commercial customer (visit) calculations (in commercial defaults options panel), with initial default of 0.4",
                    "Fix floor calculation pack changes not being saved if the population pack also wasn't changed",
                    "Fix 'save and apply' for school calculations not showing",
                    "Fix custom tooltip remaining visible at edge of main screen under some conditions",
                    "Update building preview renderer and standardize background to plain sky blue"
                }
            },
            new WhatsNewMessage
            {
                version = new Version("2.0.0.0"),
                versionHeader = " BETA 10",
                betaVersion = 10,
                messageKeys = false,
                messages = new string[]
                {
                    "Overhaul commercial building visitor (customer) count code and add options for new (workforce-base) or old (building lot size-based) calculations",
                    "Display commercial building visitor counts in calculations display panels",
                    "Split workplace defaults panel into separate commercial, office, industrial and school panels, with separate 'legacy as default' selections fof each",
                    "Overhaul volumetric calculations display panel and add tooltips",
                    "Overhaul custom pack editing and saving code"
                }
            },
            new WhatsNewMessage
            {
                version = new Version("2.0.0.0"),
                versionHeader = " BETA 9",
                betaVersion = 9,
                messageKeys = false,
                messages = new string[]
                {
                    "Separate residential and workplace default option panels, and allow 'use legacy by default' to be toggled separately for each",
                    "Trial use of icons instead of text for calculations panel tabs",
                    "Add buttons in residential defaults panel to apply changes in default packs to existing residential buildings",
                    "Update savegame serialization",
                }
            },
            new WhatsNewMessage
            {
                version = new Version("2.0.0.0"),
                versionHeader = " BETA 8",
                betaVersion = 8,
                messageKeys = false,
                messages = new string[]
                {
                    "Fix new-style population overrides not being recognised by legacy calculations",
                    "Add tooltips for population and floor calculation panel fields"
                }
            },
            new WhatsNewMessage
            {
                version = new Version("2.0.0.0"),
                versionHeader = " BETA 7",
                betaVersion = 7,
                messageKeys = false,
                messages = new string[]
                {
                    "Changes to default calculation packs now only take effect via a new 'Save and Apply' button",
                    "Redo residential population caching to better handle buildings with invalid levels"
                }
            },
            new WhatsNewMessage
            {
                version = new Version("2.0.0.0"),
                versionHeader = " BETA 6",
                betaVersion = 6,
                messageKeys = false,
                messages = new string[]
                {
                    "New population caching system that fully recognises multiple levels for the same building prefab (historical buildings)",
                    "Implement handling for reductions in residential homecounts when buildings upgrade",
                    "Fix building settings panel not opening when target building doesn't have a valid mesh material"
                }
            },
            new WhatsNewMessage
            {
                version = new Version("2.0.0.0"),
                versionHeader = "",
                messageKeys = true,
                messages = new string[]
                {
                    "RPR_200_0",
                    "RPR_200_1",
                    "RPR_200_2",
                    "RPR_200_3",
                    "RPR_200_4",
                    "RPR_200_5",
                    "RPR_200_6",
                    "RPR_200_7",
                    "RPR_200_8",
                    "RPR_200_9"
                }
            }
        };


        /// <summary>
        /// Close button action.
        /// </summary>
        /// <returns>True (always)</returns>
        internal static bool Confirm() => true;

        /// <summary>
        /// 'Don't show again' button action.
        /// </summary>
        /// <returns>True (always)</returns>
        internal static bool DontShowAgain()
        {
            // Save current version to settings file.
            ModSettings.whatsNewVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Save current version header as beta.
            ModSettings.whatsNewBetaVersion = WhatsNewMessages[0].betaVersion;
            SettingsUtils.SaveSettings();

            return true;
        }


        /// <summary>
        /// Check if there's been an update since the last notification, and if so, show the update.
        /// </summary>
        internal static void ShowWhatsNew()
        {
            // Get last notified version and current mod version.
            Version whatsNewVersion = new Version(ModSettings.whatsNewVersion);
            Version modVersion = Assembly.GetExecutingAssembly().GetName().Version;

            // Don't show notification if we're already up to (or ahead of) this version AND there hasn't been a beta update.
            if (whatsNewVersion >= modVersion && ModSettings.whatsNewBetaVersion == RealPopMod.BetaVersion)
            {
                return;
            }

            // Show messagebox.
            WhatsNewMessageBox messageBox = MessageBoxBase.ShowModal<WhatsNewMessageBox>();
            messageBox.Title = RealPopMod.ModName + " " + RealPopMod.Version;
            messageBox.DSAButton.eventClicked += (component, clickEvent) => DontShowAgain();
            messageBox.SetMessages(whatsNewVersion, WhatsNewMessages);
        }
    }


    /// <summary>
    /// Version message struct.
    /// </summary>
    public struct WhatsNewMessage
    {
        public Version version;
        public string versionHeader;
        public int betaVersion;
        public bool messageKeys;
        public string[] messages;
    }
}