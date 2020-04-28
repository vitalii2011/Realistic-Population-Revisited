using UnityEngine;
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
        private BuildingInfo selectedBuilding;
        private UISprite hasCustom;


        // Background for each list item.
        public UIPanel background
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
                background.width = width;
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
            UIBuildingDetails.instance.UpdateSelectedBuilding(selectedBuilding);
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
                hasCustom.relativePosition = new Vector3(340, 10);
                hasCustom.tooltip = "Has custom settings";
            }

            // Set selected building.
            selectedBuilding = data as BuildingInfo;
            buildingName.text = UIBuildingDetails.GetDisplayName(selectedBuilding.name);

            // Update custom settings checkbox to correct state.
            if (ExternalCalls.GetResidential(selectedBuilding) > 0 || ExternalCalls.GetWorker(selectedBuilding) > 0)
            {
                // Custom value found.
                hasCustom.spriteName = "AchievementCheckedTrue";
            }
            else
            {
                // No custom value.
                hasCustom.spriteName = "AchievementCheckedFalse";
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
            background.backgroundSprite = "ListItemHighlight";
            background.color = new Color32(255, 255, 255, 255);
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
                background.backgroundSprite = "UnlockingItemBackground";
                background.color = new Color32(0, 0, 0, 128);
            }
            else
            {
                // Darker background for even rows.
                background.backgroundSprite = null;
            }
        }
    }
}