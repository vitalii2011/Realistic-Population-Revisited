using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    public class UIModCalcs : UIPanel
    {
        // Panel components.
        private UILabel title;
        private UILegacyCalcs legacyPanel;
        private UIVolumetricPanel volumetricPanel;
        private UIDropDown presetMenu;

        // Data.
        private CalcPack[] availablePacks;

        // Current selections.
        private BuildingInfo currentBuilding;
        private CalcPack currentPack;


        /// <summary>
        /// Create the mod calcs panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        public void Setup()
        {
            // Basic setup.
            clipChildren = true;

            // Title.
            title = this.AddUIComponent<UILabel>();
            title.relativePosition = new Vector3(0, 0);
            title.textAlignment = UIHorizontalAlignment.Center;
            title.text = "Mod calculations";
            title.textScale = 1.2f;
            title.autoSize = false;
            title.width = this.width;

            // Volumetric calculations panel.
            volumetricPanel = this.AddUIComponent<UIVolumetricPanel>();
            volumetricPanel.relativePosition = new Vector3(0, title.height + 30f);
            volumetricPanel.height = this.height - title.height + 30f;
            volumetricPanel.width = this.width;
            volumetricPanel.Setup();

            // Legacy calculations panel - copy volumetric calculations panel.
            legacyPanel = this.AddUIComponent<UILegacyCalcs>();
            legacyPanel.relativePosition = volumetricPanel.relativePosition;
            legacyPanel.height = volumetricPanel.height;
            legacyPanel.width = volumetricPanel.width;
            legacyPanel.Setup();
            legacyPanel.Hide();

            // Preset dropdown.
            presetMenu = CreateDropDown(this, "Preset", yPos: title.height);
            presetMenu.eventSelectedIndexChanged += (component, index) =>
            {

                // Update selected pack.
                currentPack = availablePacks[index];

                // Update building setting and save.
                PopData.UpdateBuildingPack(currentBuilding, currentPack);
                ConfigUtils.SaveSettings();

                // Check if we're using legacy or volumetric data.
                if (currentPack is VolumetricPack)
                {
                    // Volumetric pack.
                    // Update panel with new calculations.
                    LevelData thisLevel = GetFloorData();
                    volumetricPanel.UpdateText(thisLevel);
                    volumetricPanel.CalculateVolumetric(currentBuilding, thisLevel);

                    // Set visibility.
                    legacyPanel.Hide();
                    volumetricPanel.Show();
                }
                else
                {
                    // Set visibility.
                    volumetricPanel.Hide();
                    legacyPanel.Show();
                }
            };
        }


        /// <summary>
        /// Returns the level data record from the current floor pack that's relevant to the selected building's level.
        /// </summary>
        /// <returns>Floordata instance for building</returns>
        private LevelData GetFloorData() => ((VolumetricPack)currentPack).levels[(int)currentBuilding.GetClassLevel()];


        /// <summary>
        /// Called whenever the currently selected building is changed to update the panel display.
        /// </summary>
        /// <param name="building">Newly selected building</param>
        public void SelectionChanged(BuildingInfo building)
        {
            // Set current building.
            currentBuilding = building;

            // Safety first!
            if (currentBuilding != null)
            {
                // Get available packs for this building.
                availablePacks = PopData.GetPacks(building);

                // Get current and default packs for this item
                CalcPack currentPack = PopData.ActivePack(building);
                CalcPack defaultPack = PopData.DefaultPack(building);

                // Build preset menu.
                presetMenu.items = new string[availablePacks.Length];
                for (int i = 0; i < presetMenu.items.Length; ++i)
                {
                    presetMenu.items[i] = availablePacks[i].displayName;

                    // Check for deefault name match,
                    if (availablePacks[i].name.Equals(defaultPack.name))
                    {
                        presetMenu.items[i] += " (default)";
                    }

                    // Set menu selection to current pack if it matches.
                    if (availablePacks[i].Equals(currentPack))
                    {
                        presetMenu.selectedIndex = i;
                    }
                }

                // Update legacy panel (volumetric panel is updated by menu selection change above).
                legacyPanel.SelectionChanged(building);
            }
        }


        /// <summary>
        /// Creates a dropdown menu.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Text label</param>
        /// <param name="xPos">Relative x position (default 20)</param>
        /// <param name="yPos">Relative y position (default 0)</param>
        /// <returns></returns>
        private UIDropDown CreateDropDown(UIComponent parent, string text, float xPos = 20f, float yPos = 0f)
        {
            // Constants.
            const float Width = 200f;
            const float Height = 25f;
            const int ItemHeight = 20;

            // Add container at specified position.
            UIPanel container = parent.AddUIComponent<UIPanel>();
            container.height = 25;
            container.relativePosition = new Vector3(xPos, yPos);

            // Add label.
            UILabel label = container.AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.text = text;
            label.relativePosition = new Vector3(0f, 6f);

            // Create dropdown menu.
            UIDropDown dropDown = container.AddUIComponent<UIDropDown>();
            dropDown.listBackground = "GenericPanelLight";
            dropDown.itemHover = "ListItemHover";
            dropDown.itemHighlight = "ListItemHighlight";
            dropDown.normalBgSprite = "ButtonMenu";
            dropDown.disabledBgSprite = "ButtonMenuDisabled";
            dropDown.hoveredBgSprite = "ButtonMenuHovered";
            dropDown.focusedBgSprite = "ButtonMenu";
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            dropDown.popupColor = new Color32(45, 52, 61, 255);
            dropDown.popupTextColor = new Color32(170, 170, 170, 255);
            dropDown.zOrder = 1;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            dropDown.textFieldPadding = new RectOffset(8, 0, 8, 0);
            dropDown.itemPadding = new RectOffset(14, 0, 8, 0);

            // Dropdown relative position - right of label.
            dropDown.relativePosition = new Vector3(label.width + 10f, 0f);

            // Dropdown size parameters.
            dropDown.size = new Vector2(Width, Height);
            dropDown.listWidth = (int)Width;
            dropDown.listHeight = 500;
            dropDown.itemHeight = ItemHeight;
            dropDown.textScale = 0.7f;

            // Create dropdown button.
            UIButton button = dropDown.AddUIComponent<UIButton>();
            dropDown.triggerButton = button;
            button.size = dropDown.size;
            button.text = "";
            button.relativePosition = new Vector3(0f, 0f);
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.normalFgSprite = "IconDownArrow";
            button.hoveredFgSprite = "IconDownArrowHovered";
            button.pressedFgSprite = "IconDownArrowPressed";
            button.focusedFgSprite = "IconDownArrowFocused";
            button.disabledFgSprite = "IconDownArrowDisabled";
            button.spritePadding = new RectOffset(3, 3, 3, 3);
            button.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.zOrder = 0;

            return dropDown;
        }
    }
}
