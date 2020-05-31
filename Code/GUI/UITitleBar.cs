using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Titlebar for the building details screen.
    /// </summary>
    public class UITitleBar : UIPanel
    {
        // Titlebar components.
        private UILabel titleLabel;
        private UIDragHandle dragHandle;
        private UISprite iconSprite;
        private UIButton closeButton;


        /// <summary>
        /// Create the titlebar; called by Unity just before any of the Update methods is called for the first time.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Basic setup.
            width = parent.width;
            height = UIBuildingDetails.titleHeight;
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            relativePosition = Vector3.zero;

            // Make it draggable.
            dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.width = width - 50;
            dragHandle.height = height;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.target = parent;

            // Decorative icon (top-left).
            iconSprite = AddUIComponent<UISprite>();
            iconSprite.relativePosition = new Vector3(10, 5);
            iconSprite.spriteName = "ToolbarIconZoomOutCity";
            UIUtils.ResizeIcon(iconSprite, new Vector2(30, 30));
            iconSprite.relativePosition = new Vector3(10, 5);

            // Titlebar label.
            titleLabel = AddUIComponent<UILabel>();
            titleLabel.relativePosition = new Vector3(50, 13);
            titleLabel.text = "Realistic Population v" + PopBalanceMod.Version;

            // Close button.
            closeButton = AddUIComponent<UIButton>();
            closeButton.relativePosition = new Vector3(width - 35, 2);
            closeButton.normalBgSprite = "buttonclose";
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.pressedBgSprite = "buttonclosepressed";
            closeButton.eventClick += (component, param) =>
            {
                parent.Hide();
            };
        }
    }
}