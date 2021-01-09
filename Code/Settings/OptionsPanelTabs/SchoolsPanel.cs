using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Options panel for setting school options.
    /// </summary>
    internal class EducationPanel
    {
        /// <summary>
        /// Adds school options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal EducationPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(tabStrip, Translations.Translate("RPR_OPT_SCH"), tabIndex);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = true;

            // Enable realistic schools checkbox.
            UICheckBox schoolCapacityCheck = UIControls.AddPlainCheckBox(panel, Translations.Translate("RPR_OPT_SEN"));
            schoolCapacityCheck.isChecked = ModSettings.enableSchoolPop;
            schoolCapacityCheck.eventCheckChanged += (control, isChecked) => ModSettings.enableSchoolPop = isChecked;

            // Enable realistic schools checkbox.
            UICheckBox schoolPropertyCheck = UIControls.AddPlainCheckBox(panel, Translations.Translate("RPR_OPT_SEJ"));
            schoolPropertyCheck.isChecked = ModSettings.enableSchoolProperties;
            schoolPropertyCheck.eventCheckChanged += (control, isChecked) => ModSettings.enableSchoolProperties = isChecked;

            // School default multiplier.  Simple integer.
            UISlider schoolMult = UIControls.AddSliderWithValue(panel, Translations.Translate("RPR_OPT_SDM"), 1f, 5f, 0.5f, ModSettings.DefaultSchoolMult, (value) => { ModSettings.DefaultSchoolMult = value; });
        }
    }
}