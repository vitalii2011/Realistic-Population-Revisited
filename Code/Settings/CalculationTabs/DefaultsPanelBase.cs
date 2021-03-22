using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal abstract class DefaultsPanel
    {
        // Layout constants.
        protected const float LeftColumn = 200f;
        private const float MenuWidth = 300f;
        protected const float Margin = 5f;
        protected const float RowAdditionX = LeftColumn + MenuWidth + (Margin * 2);
        protected float RowHeight = 25f;


        // Instance references.
        internal static DefaultsPanel instance;


        // Dropdown menus.
        protected UIDropDown[] PopMenus { get; private set; }
        protected UIDropDown[] FloorMenus { get; private set; }

        // Available packs arrays.
        protected PopDataPack[][] AvailablePopPacks { get; private set; }
        protected DataPack[] AvailableFloorPacks { get; private set; }


        // Service/subservice arrays.
        protected abstract string[] SubServiceNames { get; }
        protected abstract ItemClass.Service[] Services { get; }
        protected abstract ItemClass.SubService[] SubServices { get; }
        protected abstract string[] IconNames { get; }
        protected abstract string[] AtlasNames { get; }

        // Tab width.
        protected virtual float TabWidth => 100f;


        /// <summary>
        /// Constructor - adds default options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal DefaultsPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Layout constants.
            const float TabIconSize = 23f;


            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTab(tabStrip, "", tabIndex, out UIButton tabButton, TabWidth);
            panel.autoLayout = false;

            // Add tab sprites.
            float spriteBase = (TabWidth - 2f) / IconNames.Length;
            float spriteOffset = (spriteBase - TabIconSize) / 2f;
            for (int i = 0; i < IconNames.Length; ++i)
            {
                UISprite thumbSprite = tabButton.AddUIComponent<UISprite>();
                thumbSprite.relativePosition = new Vector2(1f + (spriteBase * i) + spriteOffset, 1f);
                thumbSprite.width = TabIconSize;
                thumbSprite.height = TabIconSize;
                thumbSprite.atlas = TextureUtils.GetTextureAtlas(AtlasNames[i]);
                thumbSprite.spriteName = IconNames[i];

                // Put later sprites behind earlier sprites, for clarity.
                thumbSprite.SendToBack();
            }

            // Set tooltip.
            tabButton.tooltip = Translations.Translate("RPR_OPT_DEF");

            // Initialise arrays.
            AvailablePopPacks = new PopDataPack[SubServiceNames.Length][];
            AvailableFloorPacks = FloorData.instance.Packs;
            PopMenus = new UIDropDown[SubServiceNames.Length];
            FloorMenus = new UIDropDown[SubServiceNames.Length];

            // Add header controls.
            float currentY = PanelHeader(panel, Margin);

            // Add menus.
            currentY = SetUpMenus(panel, currentY);

            // Add any additional components.
            currentY = AddAdditional(panel, currentY);

            // Add buttons- add extra space.
            FooterButtons(panel, currentY + Margin);

            // Populate menus.
            UpdateMenus();

            // Set instance.
            instance = this;
        }


        /// <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal virtual void UpdateMenus()
        {
            for (int i = 0; i < SubServiceNames.Length; ++i)
            {
                // Save current index in object user data.
                PopMenus[i].objectUserData = i;
                FloorMenus[i].objectUserData = i;

                // Get available packs for this service/subservice combination.
                AvailablePopPacks[i] = PopData.instance.GetPacks(Services[i]);
                AvailableFloorPacks = FloorData.instance.Packs;

                // Get current and default packs for this item.
                DataPack currentPopPack = PopData.instance.CurrentDefaultPack(Services[i], SubServices[i]);
                DataPack defaultPopPack = PopData.instance.BaseDefaultPack(Services[i], SubServices[i]);
                DataPack currentFloorPack = FloorData.instance.CurrentDefaultPack(Services[i], SubServices[i]);
                DataPack defaultFloorPack = FloorData.instance.BaseDefaultPack(Services[i], SubServices[i]);

                // Build preset menus.
                PopMenus[i].items = new string[AvailablePopPacks[i].Length];
                FloorMenus[i].items = new string[AvailableFloorPacks.Length];

                // Iterate through each item in pop menu.
                for (int j = 0; j < PopMenus[i].items.Length; ++j)
                {
                    // Set menu item text.
                    PopMenus[i].items[j] = AvailablePopPacks[i][j].displayName;

                    // Check for default name match.
                    if (AvailablePopPacks[i][j].name.Equals(defaultPopPack.name))
                    {
                        // Match - add default postscript.
                        PopMenus[i].items[j] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (AvailablePopPacks[i][j].name.Equals(currentPopPack.name))
                    {
                        PopMenus[i].selectedIndex = j;
                    }
                }

                // Iterate through each item in floor menu.
                for (int j = 0; j < FloorMenus[i].items.Length; ++j)
                {
                    // Set menu item text.
                    FloorMenus[i].items[j] = AvailableFloorPacks[j].displayName;

                    // Check for deefault name match.
                    if (AvailableFloorPacks[j].name.Equals(defaultFloorPack.name))
                    {
                        // Match - add default postscript.
                        FloorMenus[i].items[j] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (AvailableFloorPacks[j].name.Equals(currentFloorPack.name))
                    {
                        FloorMenus[i].selectedIndex = j;
                    }
                }
            }
        }


        /// <summary>
        /// Sets up the defaults dropdown menus.
        /// </summary>
        /// <param name="panel">Panel reference</param>
        /// <param name="yPos">Relative Y position for buttons</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
        private float SetUpMenus(UIPanel panel, float yPos)
        {
            // Layout constants.
            const float LeftColumn = 200f;
            const float MenuWidth = 300f;

            // Starting y position.
            float currentY = yPos + Margin;

            for (int i = 0; i < SubServiceNames.Length; ++i)
            {

                // Row icon and label.
                PanelUtils.RowHeaderIcon(panel, ref currentY, SubServiceNames[i], IconNames[i], AtlasNames[i]);

                // Pop pack dropdown.
                PopMenus[i] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAL_DEN"), MenuWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false);

                // Save current index in object user data.
                PopMenus[i].objectUserData = i;

                // Event handler.
                PopMenus[i].eventSelectedIndexChanged += (control, index) =>
                {
                    // Retrieve stored index.
                    int serviceIndex = (int)control.objectUserData;

                    // Hide floor menu if we've selected legacy calcs, otherwise show it.
                    if (AvailablePopPacks[serviceIndex][index].version == (int)DataVersion.legacy)
                    {
                        FloorMenus[serviceIndex].Hide();
                    }
                    else
                    {
                        FloorMenus[serviceIndex].Show();
                    }
                };

                // Floor pack on next row.
                currentY += RowHeight;

                // Floor pack dropdown.
                FloorMenus[i] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAL_BFL"), MenuWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false);

                // Add any additional controls.
                currentY = RowAdditions(panel, currentY, i);


                // Next row.
                currentY += RowHeight + Margin;
            }

            // Return finishing Y position.
            return currentY;
        }


        /// <summary>
        /// Adds header controls to the panel.
        /// </summary>
        /// <param name="panel">Panel reference</param>
        /// <param name="yPos">Relative Y position for buttons</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
        protected virtual float PanelHeader (UIPanel panel, float yPos)
        {
            return yPos;
        }


        /// <summary>
        /// Adds footer buttons to the panel.
        /// </summary>
        /// <param name="panel">Panel reference</param>
        /// <param name="yPos">Relative Y position for buttons</param>
        protected virtual void FooterButtons(UIPanel panel, float yPos)
        {
            // Reset button.
            UIButton resetButton = UIControls.AddButton(panel, Margin, yPos, Translations.Translate("RPR_OPT_RTD"), 150f);
            resetButton.eventClicked += ResetDefaults;

            // Revert button.
            UIButton revertToSaveButton = UIControls.AddButton(panel, (Margin * 2) + 150f, yPos, Translations.Translate("RPR_OPT_RTS"), 150f);
            revertToSaveButton.eventClicked += ResetSaved;
        }


        /// <summary>
        /// Adds any additional controls for each row.
        /// </summary>
        /// <param name="panel">Panel reference</param>
        /// <param name="yPos">Relative Y position at top of row items</param>
        /// <param name="index">Index number of this row</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
        protected virtual float RowAdditions(UIPanel panel, float yPos, int index)
        {
            return yPos;
        }


        /// <summary>
        /// Adds any additional controls below the menu arrays but above button footers.
        /// </summary>
        /// <param name="panel">Panel reference</param>
        /// <param name="yPos">Relative Y position</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
        protected virtual float AddAdditional(UIPanel panel, float yPos)
        {
            return yPos;
        }


        /// <summary>
        /// 'Revert to defaults' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected virtual void ResetDefaults(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Iterate through each sub-service menu.
            for (int i = 0; i < SubServiceNames.Length; ++i)
            {
                // Get current and default packs for this item.
                DataPack defaultPopPack = PopData.instance.BaseDefaultPack(Services[i], SubServices[i]);
                DataPack defaultFloorPack = FloorData.instance.BaseDefaultPack(Services[i], SubServices[i]);

                // Iterate through each item in pop menu.
                for (int j = 0; j < PopMenus[i].items.Length; ++j)
                {
                    // Check for deefault name match.
                    if (AvailablePopPacks[i][j].name.Equals(defaultPopPack.name))
                    {
                        // Match - set selection to this one.
                        PopMenus[i].selectedIndex = j;
                    }
                }

                // Iterate through each item in floor menu.
                for (int j = 0; j < FloorMenus[i].items.Length; ++j)
                {
                    // Check for deefault name match.
                    if (AvailableFloorPacks[j].name.Equals(defaultFloorPack.name))
                    {
                        // Match - set selection to this one.
                        FloorMenus[i].selectedIndex = j;
                    }
                }
            }
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
                int popIndex = PopMenus[i].selectedIndex;

                // Check to see if this is a change from the current default.
                if (!PopData.instance.CurrentDefaultPack(Services[i], SubServices[i]).name.Equals(AvailablePopPacks[i][popIndex].name))
                {
                    // Default has changed - update default population dictionary for this subservice.
                    PopData.instance.ChangeDefault(Services[i], SubServices[i], AvailablePopPacks[i][popIndex]);
                }

                // Update floor data pack if we're not using legacy calculations.
                if (AvailablePopPacks[i][popIndex].version != (int)DataVersion.legacy)
                {
                    // Check to see if this is a change from the current default.
                    if (!FloorData.instance.CurrentDefaultPack(Services[i], SubServices[i]).name.Equals(AvailableFloorPacks[FloorMenus[i].selectedIndex]))
                    {
                        // Default has changed - update default floor dictionary for this subservice.
                        FloorData.instance.ChangeDefault(Services[i], SubServices[i], AvailableFloorPacks[FloorMenus[i].selectedIndex]);
                    }
                }
            }

            // Save settings.
            ConfigUtils.SaveSettings();
        }


        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected virtual void ResetSaved(UIComponent control, UIMouseEventParameter mouseEvent) => UpdateMenus();
    }
}