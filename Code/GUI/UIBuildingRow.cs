﻿using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// An individual row in the list of buildings.
    /// </summary>
    public class UIBuildingRow : UIPanel, UIFastListRow
    {
        // Height of each row.
        private const int rowHeight = 40;

        // Panel components.
        private UIPanel panelBackground;
        private UILabel buildingName;
        private BuildingInfo thisBuilding;
        private UISprite hasCustom, hasNonDefault;


        // Background for each list item.
        public UIPanel Background
        {
            get
            {
                if (panelBackground == null)
                {
                    panelBackground = AddUIComponent<UIPanel>();
                    panelBackground.width = width;
                    panelBackground.height = rowHeight;
                    panelBackground.relativePosition = Vector2.zero;

                    panelBackground.zOrder = 0;
                }

                return panelBackground;
            }
        }


        /// <summary>
        /// Called when dimensions are changed, including as part of initial setup (required to set correct relative position of label).
        /// </summary>
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (buildingName != null)
            {
                Background.width = width;
                buildingName.relativePosition = new Vector3(10f, 5f);
            }
        }


        /// <summary>
        /// Mouse click event handler - updates the selected building to what was clicked.
        /// </summary>
        /// <param name="p">Mouse event parameter</param>
        protected override void OnClick(UIMouseEventParameter p)
        {
            base.OnClick(p);
            BuildingDetailsPanel.Panel.UpdateSelectedBuilding(thisBuilding);
        }


        /// <summary>
        /// Generates and displays a building row.
        /// </summary>
        /// <param name="data">Object to list</param>
        /// <param name="isRowOdd">If the row is an odd-numbered row (for background banding)</param>
        public void Display(object data, bool isRowOdd)
        {
            // Perform initial setup for new rows.
            if (buildingName == null)
            {
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                width = parent.width;
                height = 40;

                buildingName = AddUIComponent<UILabel>();
                buildingName.width = 200;

                // Checkbox to indicate which items have custom settings.
                hasCustom = AddUIComponent<UISprite>();
                hasCustom.size = new Vector2(20, 20);
                hasCustom.relativePosition = new Vector3(310f, 10f);
                hasCustom.tooltip = Translations.Translate("RPR_CUS_HAS");

                // Checkbox to indicate which items have default overrides.
                hasNonDefault = AddUIComponent<UISprite>();
                hasNonDefault.size = new Vector2(20, 20);
                hasNonDefault.relativePosition = new Vector3(340f, 10f);
                hasNonDefault.tooltip = Translations.Translate("RPR_CUS_NDF");
            }

            // Set selected building.
            thisBuilding = data as BuildingInfo;
            buildingName.text = UIBuildingDetails.GetDisplayName(thisBuilding.name);

            // Update custom settings checkbox to correct state.
            if (ExternalCalls.GetResidential(thisBuilding) > 0 || ExternalCalls.GetWorker(thisBuilding) > 0)
            {
                // Custom value found.
                hasCustom.spriteName = "AchievementCheckedTrue";
            }
            else
            {
                // No custom value.
                hasCustom.spriteName = "AchievementCheckedFalse";
            }

            // Update default overrider checkbox to correct state.
            if (PopData.HasPackOverride(thisBuilding) != null)
            {
                // Custom value found.
                hasNonDefault.spriteName = "AchievementCheckedTrue";
            }
            else
            {
                // No custom value.
                hasNonDefault.spriteName = "AchievementCheckedFalse";
            }

            // Set initial background as deselected state.
            Deselect(isRowOdd);
        }


        /// <summary>
        /// Highlights the selected row.
        /// </summary>
        /// <param name="isRowOdd">If the row is an odd-numbered row (for background banding)</param>
        public void Select(bool isRowOdd)
        {
            Background.backgroundSprite = "ListItemHighlight";
            Background.color = new Color32(255, 255, 255, 255);
        }


        /// <summary>
        /// Unhighlights the (un)selected row.
        /// </summary>
        /// <param name="isRowOdd">If the row is an odd-numbered row (for background banding)</param>
        public void Deselect(bool isRowOdd)
        {
            if (isRowOdd)
            {
                // Lighter background for odd rows.
                Background.backgroundSprite = "UnlockingItemBackground";
                Background.color = new Color32(0, 0, 0, 128);
            }
            else
            {
                // Darker background for even rows.
                Background.backgroundSprite = null;
            }
        }
    }
}