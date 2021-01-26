﻿using System;
using System.Reflection;
using System.Collections.Generic;
using RealisticPopulationRevisited.MessageBox;



namespace RealisticPopulationRevisited
{
    /// <summary>
    /// "What's new" message box.  Based on macsergey's code in Intersection Marking Tool (Node Markup) mod.
    /// </summary>
    internal static class WhatsNew
    {
        // List of versions and associated update message lines (as translation keys).
        private static Dictionary<Version, List<string>> Versions => new Dictionary<Version, List<String>>
        {
            {
                new Version("2.0"),
                new List<string>
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
        public static bool Confirm() => true;

        /// <summary>
        /// 'Don't show again' button action.
        /// </summary>
        /// <returns>True (always)</returns>
        public static bool DontShowAgain()
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