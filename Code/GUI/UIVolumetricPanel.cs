using System.Text;
using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Panel to display volumetric calculations.
    /// </summary>
    public class UIVolumetricPanel : UIPanel
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float ColumnWidth = 300f;
        private const float LabelOffset = 190f;
        private const float LeftColumn = LabelOffset;
        private const float RightColumn = ColumnWidth + LabelOffset;
        private const float RowTopMargin = Margin;
        private const float RowHeight = 25f;
        private const float Row1 = RowTopMargin;
        private const float Row2 = Row1 + RowHeight;
        private const float Row3 = Row2 + RowHeight;
        private const float Row4 = Row3 + RowHeight;
        private const float Row5 = Row4 + RowHeight;
        private const float Row6 = Row5 + RowHeight;
        private const float Row7 = Row6 + RowHeight;
        private const float Row8 = Row7 + RowHeight;


        // Panel components.
        private UIScrollablePanel floorsPanel;
        private UIFastList floorsList;
        private UILabel numFloorsLabel;
        private UILabel floorAreaLabel;
        private UILabel totalLabel;
        private UILabel firstMinLabel;
        private UILabel firstExtraLabel;
        private UILabel floorHeightLabel;
        private UILabel emptyAreaLabel;
        private UILabel emptyPercentLabel;
        private UILabel perLabel;
        private UICheckBox ignoreFirstCheckBox;


        /// <summary>
        /// Create the panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        public void Setup()
        {
            // Generic setup.
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "UnlockingPanel";
            autoLayout = false;
            autoSize = false;
            width = parent.width;
            height = 395f;
            builtinKeyNavigation = true;
            clipChildren = true;

            // Labels.
            numFloorsLabel = AddLabel(this, Translations.Translate("RPR_CAL_VOL_FLR"), RightColumn, Row1);
            floorAreaLabel = AddLabel(this, Translations.Translate("RPR_CAL_VOL_TFA"), RightColumn, Row2);
            totalLabel = AddLabel(this, Translations.Translate("RPR_CAL_VOL_UTS"), RightColumn, Row3);
            floorHeightLabel = AddLabel(this, Translations.Translate("RPR_CAL_VOL_FLH"), RightColumn, Row4);
            firstMinLabel = AddLabel(this, Translations.Translate("RPR_CAL_VOL_FMN"), RightColumn, Row5);
            firstExtraLabel = AddLabel(this, Translations.Translate("RPR_CAL_VOL_FMX"), RightColumn, Row6);
            emptyAreaLabel = AddLabel(this, Translations.Translate("RPR_CAL_VOL_EMP"), LeftColumn, Row1);
            emptyPercentLabel = AddLabel(this, Translations.Translate("RPR_CAL_VOL_EPC"), LeftColumn, Row2);
            perLabel = AddLabel(this, Translations.Translate("RPR_CAL_VOL_APU"), LeftColumn, Row3);

            // Checkbox.
            ignoreFirstCheckBox = AddCheckBox(this, Translations.Translate("RPR_CAL_VOL_IGF"), RightColumn, Row7);
            ignoreFirstCheckBox.isInteractive = false;
            ignoreFirstCheckBox.Disable();

            // Floor panel.
            floorsPanel = this.AddUIComponent<UIScrollablePanel>();
            floorsPanel.relativePosition = new Vector3(0, Row8);
            floorsPanel.autoSize = false;
            floorsPanel.width = this.width;
            floorsPanel.height = this.height - floorsPanel.relativePosition.y;
            floorsPanel.autoLayout = false;
            floorsPanel.scrollWheelDirection = UIOrientation.Vertical;

            // Building selection list.
            floorsList = UIFastList.Create<UIFloorRow>(floorsPanel);
            floorsList.backgroundSprite = "UnlockingPanel";
            floorsList.relativePosition = Vector3.zero;
            floorsList.width = floorsPanel.width;
            floorsList.height = floorsPanel.height;
            floorsList.isInteractive = true;
            floorsList.canSelect = true;
            floorsList.rowHeight = 20;
            floorsList.autoHideScrollbar = true;
            floorsList.rowsData = new FastList<object>();
            floorsList.selectedIndex = -1;
        }


        /// <summary>
        /// Updates the population summary text labels with data from the current level.
        /// </summary>
        /// <param name="levelData">LevelData record to summarise</param>
        internal void UpdatePopText(LevelData levelData)
        {
            // Set textfield values.
            emptyAreaLabel.text = levelData.emptyArea.ToString();
            emptyPercentLabel.text = levelData.emptyPercent.ToString();
            perLabel.text = levelData.areaPer.ToString();
        }


        /// <summary>
        /// Updates the floor summary text labels with data from the current floor.
        /// </summary>
        /// <param name="floorData">FloorData record to summarise</param>
        internal void UpdateFloorText(FloorDataPack floorData)
        {
            // Set textfield values.
            firstMinLabel.text = floorData.firstFloorMin.ToString();
            firstExtraLabel.text = floorData.firstFloorExtra.ToString();
            floorHeightLabel.text = floorData.floorHeight.ToString();

            // Set checkbox.
            ignoreFirstCheckBox.isChecked = floorData.firstFloorEmpty;
        }


        /// <summary>
        /// Perform and display volumetric calculations for the currently selected building.
        /// </summary>
        /// <param name="building">Selected building prefab</param>
        /// <param name="levelData">Level calculation data to apply to calculations</param>
        /// <param name="floorData">Floor calculation data to apply to calculations</param>
        internal void CalculateVolumetric(BuildingInfo building, LevelData levelData, FloorDataPack floorData)
        {
            // Safety first!
            if (building == null)
            {
                return;
            }

            // Perform calculations.
            // Get floors and allocate area an number of floor labels.
            SortedList<int, float> floors = PopData.instance.VolumetricFloors(building.m_generatedInfo, levelData, floorData, out float totalArea);
            floorAreaLabel.text = totalArea.ToString();
            numFloorsLabel.text = floors.Count.ToString();

            // Get total units.
            int totalUnits = PopData.instance.VolumetricPopulation(building.m_generatedInfo, levelData, floorData, floors, totalArea);

            // Floor labels list.
            List<string> floorLabels = new List<string>();

            // What we call our units for this building.
            string unitName;
            switch (building.GetService())
            {
                case ItemClass.Service.Residential:
                    // Residential - households.
                    unitName = Translations.Translate("RPR_CAL_VOL_HOU");
                    break;
                case ItemClass.Service.Education:
                    // Education - students.
                    unitName = Translations.Translate("RPR_CAL_VOL_STU");
                    break;
                default:
                    // Default - workplaces.
                    unitName = Translations.Translate("RPR_CAL_VOL_WOR");
                    break;
            }

            // See if we're using area calculations for numbers of units, i.e. areaPer is at least one.
            if (levelData.areaPer > 0)
            {
                // Determine area percentage to use for calculations (inverse of empty area percentage).
                float areaPercent = 1 - (levelData.emptyPercent / 100f);

                // Create new floor area labels by iterating through each floor.
                for (int i = 0; i < floors.Count; ++i)
                {
                    // StringBuilder, because we're doing a fair bit of manipulation here.
                    StringBuilder floorString = new StringBuilder("Floor ");

                    // Floor number
                    floorString.Append(i + 1);
                    floorString.Append(" " + Translations.Translate("RPR_CAL_VOL_ARA") + " ");
                    floorString.Append(floors[i]);

                    // See if we're calculating units per individual floor.
                    if (!levelData.multiFloorUnits)
                    {
                        // Number of units on this floor - always rounded down.
                        int floorUnits = (int)((floors[i] * areaPercent) / levelData.areaPer);

                        // Add extra info to label.
                        floorString.Append(" (");
                        floorString.Append(floorUnits);
                        floorString.Append(" ");
                        floorString.Append(unitName);
                        floorString.Append(")");
                    }

                    // Add new floor label item with results for this calculation.
                    floorLabels.Add(floorString.ToString());
                }
            }

            // Allocate our new list of labels to the floors list (via an interim fastlist to avoid race conditions if we 'build' manually directly into floorsList).
            FastList<object> fastList = new FastList<object>();
            fastList.m_buffer = floorLabels.ToArray();
            fastList.m_size = floorLabels.Count;
            floorsList.rowsData = fastList;

            // Display total unit calculation result.
            totalLabel.text = totalUnits.ToString();
        }


        /// <summary>
        /// Adds a text label to the specified UIComponent.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="yPos">Relative X position</param>
        /// <param name="yPos">Relative Y position</param>
        /// <param name="text">Label text</param>
        /// <returns>New UILabel</returns>
        private UILabel AddLabel(UIComponent parent, string text, float xPos, float yPos)
        {
            // Create new label.
            UILabel newLabel = parent.AddUIComponent<UILabel>();
            newLabel.relativePosition = new Vector3(xPos, yPos);
            newLabel.textAlignment = UIHorizontalAlignment.Left;
            newLabel.textScale = 0.9f;
            newLabel.text = "Blank";

            // Add label title to the left.
            AddLabelToComponent(newLabel, text);

            return newLabel;
        }


        /// <summary>
        /// Adds a checkbox with a text label to the left.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Label text</param>s
        /// <param name="yPos">Relative X position</param>
        /// <param name="yPos">Relative Y position</param>
        /// <returns>New checkbox with attached label to left</returns>
        private UICheckBox AddCheckBox(UIComponent parent, string text, float xPos, float yPos)
        {
            // Create checkbox.
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();

            // Size and position.
            checkBox.width = parent.width - xPos;
            checkBox.height = 20f;
            checkBox.relativePosition = new Vector3(xPos, yPos);

            // Unselected sprite.
            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            sprite.spriteName = "ToggleBase";
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = Vector3.zero;

            // Selected sprite.
            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).spriteName = "ToggleBaseFocused";
            checkBox.checkedBoxObject.size = new Vector2(16f, 16f);
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            // Text label.
            AddLabelToComponent(checkBox, text);

            return checkBox;
        }


        /// <summary>
        /// Adds a text label to the left of the specified component.
        /// </summary>
        /// <param name="parent">Component to add label to</param>
        /// <param name="text">Label text</param>
        private void AddLabelToComponent(UIComponent parent, string text)
        {
            UILabel label = parent.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(-(LabelOffset - Margin), 3);
            label.autoSize = false;
            label.width = LabelOffset - (Margin * 2);
            label.textScale = 0.8f;
            label.autoHeight = true;
            label.wordWrap = true;
            label.text = text;
        }
    }
}