using ColossalFramework.UI;


namespace RealisticPopulationRevisited.MessageBox
{
    /// <summary>
    /// Message box prompting the user to chose between legacy and new calculations for this save file.
    /// </summary>
    internal class LegacyChoiceMessageBox : ListMessageBox
    {
        // Button instances.
        UIButton legacyButton, newButton;


        /// <summary>
        /// Constructor - sets text.
        /// </summary>
        public LegacyChoiceMessageBox()
        {
            AddParas(Translations.Translate("RPR_OLD_0"), Translations.Translate("RPR_OLD_1"), Translations.Translate("RPR_OLD_2"), Translations.Translate("RPR_OLD_3"));
        }


        /// <summary>
        /// Adds buttons to the message box.
        /// </summary>
        public override void AddButtons()
        {
            // Add close button.
            legacyButton = AddButton(1, 2, ChoseLegacy);
            legacyButton.text = Translations.Translate("RPR_OLD_LEG");
            newButton = AddButton(2, 2, ChoseNew);
            newButton.text = Translations.Translate("RPR_OLD_NEW");
        }

        /// <summary>
        /// Button action for choosing legacy calculations for this save.
        /// </summary>
        private void ChoseLegacy()
        {
            ModSettings.ThisSaveLegacy = true;
            Close();
        }


        /// <summary>
        /// Button action for choosing new calculations for this save.
        /// </summary>
        private void ChoseNew()
        {
            ModSettings.ThisSaveLegacy = false;
            Close();
        }
    }
}