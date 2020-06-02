using ICities;


namespace RealisticPopulationRevisited
{
    public class PopBalanceMod : IUserMod
    {
        public static string Version => "1.2.2";

        public string Name => "Realistic Population Revisited " + Version;
        
        public string Description => "More realistic building populations (based on building size) and utility needs.";


        /// <summary>
        /// Called by the game when the mod options panel is setup.
        /// </summary>
        public void OnSettingsUI(UIHelperBase helper)
        {
            // Create options panel.
            OptionsPanel optionsPanel = new OptionsPanel(helper);
        }
    }
}
