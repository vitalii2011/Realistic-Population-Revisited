using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal abstract class EmpDefaultsPanel : RICODefaultsPanel
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal EmpDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Adds footer buttons to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons</param>
        protected override void FooterButtons(float yPos)
        {
            base.FooterButtons(yPos);

            // Save button.
            UIButton saveButton = UIControls.AddButton(panel, (Margin * 3) + 300f, yPos, Translations.Translate("RPR_OPT_SAA"), 150f);
            saveButton.eventClicked += Apply;
        }


        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected virtual void Apply(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Iterate through each sub-service menu.
            for (int i = 0; i < SubServiceNames.Length; ++i)
            {
                // Get population pack menu selected index.
                int popIndex = popMenus[i].selectedIndex;

                // Check to see if this is a change from the current default.
                if (!PopData.instance.CurrentDefaultPack(Services[i], SubServices[i]).name.Equals(availablePopPacks[i][popIndex].name))
                {
                    // Default has changed - update default population dictionary for this subservice.
                    PopData.instance.ChangeDefault(Services[i], SubServices[i], availablePopPacks[i][popIndex]);
                }

                // Update floor data pack if we're not using legacy calculations.
                if (availablePopPacks[i][popIndex].version != (int)DataVersion.legacy)
                {
                    // Check to see if this is a change from the current default.
                    if (!FloorData.instance.CurrentDefaultPack(Services[i], SubServices[i]).name.Equals(availableFloorPacks[floorMenus[i].selectedIndex]))
                    {
                        // Default has changed - update default floor dictionary for this subservice.
                        FloorData.instance.ChangeDefault(Services[i], SubServices[i], availableFloorPacks[floorMenus[i].selectedIndex]);
                    }
                }
            }

            // Clear population caches.
            PopData.instance.workplaceCache.Clear();

            // Clear RICO cache.
            if (ModUtils.ricoClearAllWorkplaces != null)
            {
                ModUtils.ricoClearAllWorkplaces.Invoke(null, null);
            }

            // Save settings.
            ConfigUtils.SaveSettings();
        }
    }
}