using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Panel showing the building preview.
    /// </summary>
    class UIPreviewPanel : UIPanel
    {
        // UI components.
        private UIPreview preview;
        private UICheckBox showFloorsCheck;
        private static bool lastFloorCheckState;

        // References.
        private BuildingInfo currentBuilding;


        /// <summary>
        /// Handles changes to selected floor data pack (for previewing).
        /// </summary>
        internal FloorDataPack FloorPack { set => preview.FloorPack = value; }


        /// <summary>
        /// Suppresses floor preview rendering (e.g. when legacy calculations have been selected).
        /// </summary>
        internal bool HideFloors { set => preview.HideFloors = value; }


        /// <summary>
        /// Handles changes to selected floor data override pack (for previewing).
        /// </summary>
        internal FloorDataPack OverrideFloors { set => preview.OverrideFloors = value; }


        /// <summary>
        /// Render and show a preview of a building.
        /// </summary>
        /// <param name="building">The building to render</param>
        public void Show(BuildingInfo building)
        {
            currentBuilding = building;
            preview.Show(building);
        }


        /// <summary>
        /// Performs initial setup for the panel.
        /// </summary>
        public void Setup()
        {
            // Basic setup.
            preview = AddUIComponent<UIPreview>();
            preview.width = width;
            preview.height = height - 40f;
            preview.relativePosition = Vector2.zero;
            preview.Setup();

            // 'Show floors' checkbox.
            showFloorsCheck = UIControls.AddCheckBox(this, Translations.Translate("RPR_PRV_SFL"), yPos: height - 30f);
            showFloorsCheck.eventCheckChanged += (control, isChecked) =>
            {
                preview.RenderFloors = isChecked;
                lastFloorCheckState = isChecked;
            };

            showFloorsCheck.isChecked = lastFloorCheckState;
        }
    }
}
