using System;
using System.Reflection;
using System.Collections.Generic;
using RealPop2.MessageBox;



namespace RealPop2
{
    /// <summary>
    /// "What's new" message box.  Based on macsergey's code in Intersection Marking Tool (Node Markup) mod.
    /// </summary>
    internal static class WhatsNew
    {
        // List of versions and associated update message lines (as translation keys).
        private static Dictionary<Version, string[]> Versions => new Dictionary<Version, string[]>
        {
            {
                // Beta message version is 99.
                new Version("99.0.9"),
                new string[]
                {
                    "2.0 BETA 9 updates",
                    "Separate residential and workplace default option panels, and allow 'use legacy by default' to be toggled separately for each",
                    "Trial use of icons instead of text for defaults panel tabs",
                    "Add buttons in residential defaults panel to apply changes in default packs to existing residential buildings",
                    "Update savegame serialization",
                }
            },
            {
                // Beta message version is 99.
                new Version("99.0.8"),
                new string[]
                {
                    "2.0 BETA 8 updates",
                    "Fix new-style population overrides not being recognised by legacy calculations",
                    "Add tooltips for population and floor calculation panel fields"
                }
            },
            {
                // Beta message version is 99.
                new Version("99.0.7"),
                new string[]
                {
                    "2.0 BETA 7 updates",
                    "Changes to default calculation packs now only take effect via a new 'Save and Apply' button",
                    "Redo residential population caching to better handle buildings with invalid levels"
                }
            },
            {
                new Version("99.0.6"),
                new string[]
                {
                    "2.0 BETA 6 updates",
                    "New population caching system that fully recognises multiple levels for the same building prefab (historical buildings)",
                    "Implement handling for reductions in residential homecounts when buildings upgrade",
                    "Fix building settings panel not opening when target building doesn't have a valid mesh material"
                }
            },
            {
                new Version("2.0"),
                new string[]
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
            ModSettings.whatsNewBeta = RealPopMod.Beta;
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
            if (whatsNewVersion >= modVersion && ModSettings.whatsNewBeta.Equals(RealPopMod.Beta))
            {
                return;
            }

            // Show messagebox.
            WhatsNewMessageBox messageBox = MessageBoxBase.ShowModal<WhatsNewMessageBox>();
            messageBox.Title = RealPopMod.ModName + " " + RealPopMod.Version;
            messageBox.DSAButton.eventClicked += (component, clickEvent) => DontShowAgain();
            messageBox.SetMessages(whatsNewVersion, Versions);
        }
    }
}