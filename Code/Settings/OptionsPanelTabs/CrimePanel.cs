using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Options panel for setting crime-related options.
    /// </summary>
    internal class CrimePanel
    {
        /// <summary>
        /// Adds crime options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CrimePanel(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(tabStrip, Translations.Translate("RPR_OPT_CRI"), tabIndex);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = true;

            // Crime multiplier.  Simple integer.
            UISlider lifeMult = PanelUtils.AddSliderWithValue(panel, Translations.Translate("RPR_OPT_CML"), 1f, 200f, 1f, ModSettings.crimeMultiplier, (value) => { ModSettings.crimeMultiplier = value; });
        }
    }
}