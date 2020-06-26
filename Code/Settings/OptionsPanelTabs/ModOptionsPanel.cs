﻿using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class ModOptionsPanel
    {
        /// <summary>
        /// Adds mod options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal ModOptionsPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(tabStrip, Translations.Translate("RPR_OPT_MOD"), tabIndex);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = true;

            // Hotkey textfield
            UITextField hotkeyText = (UITextField)helper.AddTextfield(Translations.Translate("RPR_OPT_KEY"), OptionsPanel.settings.hotkey.ToString(), (value) => { }, (value) => { });

            // Event handler for getting the hotkey.
            hotkeyText.eventTextChanged += (control, value) =>
            {
                // Don't do anything if null or empty (probably transient).
                if (!string.IsNullOrEmpty(value))
                {
                    UITextField thisControl = control as UITextField;

                    // Whatever has been input, convert it to upper case and only use the first char.
                    thisControl.text = value.ToUpper()[0].ToString();

                    try
                    {
                        // Should throw an exception if this doesn't work.
                        UIThreading.hotKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), thisControl.text);

                        // Update configuration file.
                        OptionsPanel.settings.hotkey = UIThreading.hotKey.ToString();
                        Configuration<SettingsFile>.Save();
                    }
                    catch
                    {
                        // Something went wrong (probably non-translateable key); reset to current value.
                        thisControl.text = UIThreading.hotKey.ToString();
                    }
                }
            };


            UICheckBox controlBox = (UICheckBox)helper.AddCheckbox("Control", OptionsPanel.settings.ctrl, (isChecked) =>
            {
                // Update threading settings.
                UIThreading.hotCtrl = isChecked;

                // Update configuration file.
                OptionsPanel.settings.ctrl = isChecked;
                Configuration<SettingsFile>.Save();
            });
            UICheckBox altBox = (UICheckBox)helper.AddCheckbox("Alt", OptionsPanel.settings.alt, (isChecked) =>
            {
                // Update threading settings.
                UIThreading.hotAlt = isChecked;

                // Update configuration file.
                OptionsPanel.settings.alt = isChecked;
                Configuration<SettingsFile>.Save();
            });
            UICheckBox shiftBox = (UICheckBox)helper.AddCheckbox("Shift", OptionsPanel.settings.shift, (isChecked) =>
            {
                // Update threading settings.
                UIThreading.hotShift = isChecked;

                // Update configuration file.
                OptionsPanel.settings.shift = isChecked;
                Configuration<SettingsFile>.Save();
            });
        }
    }
}