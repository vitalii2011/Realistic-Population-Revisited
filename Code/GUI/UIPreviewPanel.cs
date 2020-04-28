using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Panel that contains the building preview image.
    /// </summary>
    public class UIPreviewPanel : UIPanel
    {
        // Panel components.
        private UITextureSprite previewSprite;
        private UISprite noPreviewSprite;
        private UIPreviewRenderer previewRender;
        private UILabel buildingName;

        // Currently selected building and its pre-rendered (by game) equivalent for rendering.
        private BuildingInfo currentSelection;


        /// <summary>
        /// Create the panel; called by Unity just before any of the Update methods is called for the first time.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Set background and sprites.
            backgroundSprite = "GenericPanel";

            previewSprite = AddUIComponent<UITextureSprite>();
            previewSprite.size = size;
            previewSprite.relativePosition = Vector3.zero;

            noPreviewSprite = AddUIComponent<UISprite>();
            noPreviewSprite.size = size;
            noPreviewSprite.relativePosition = Vector3.zero;

            // Initialise renderer; use double size for anti-aliasing.
            previewRender = gameObject.AddComponent<UIPreviewRenderer>();
            previewRender.Size = previewSprite.size * 2;

            // Click-and-drag rotation.
            eventMouseDown += (c, p) =>
            {
                eventMouseMove += RotateCamera;
            };

            eventMouseUp += (c, p) =>
            {
                eventMouseMove -= RotateCamera;
            };

            // Zoom with mouse wheel.
            eventMouseWheel += (c, p) =>
            {
                previewRender.Zoom -= Mathf.Sign(p.wheelDelta) * 0.25f;
                RenderPreview();
            };

            // Display building name.
            buildingName = AddUIComponent<UILabel>();
            buildingName.textScale = 0.9f;
            buildingName.useDropShadow = true;
            buildingName.dropShadowColor = new Color32(80, 80, 80, 255);
            buildingName.dropShadowOffset = new Vector2(2, -2);
            buildingName.text = "Name";
            buildingName.isVisible = false;
            buildingName.relativePosition = new Vector3(5, 10);
        }


        /// <summary>
        /// Render and show a preview of a building.
        /// </summary>
        /// <param name="building">The building to render</param>
        public void Show(BuildingInfo building)
        {
            // If we're already showing this building, nothing further needs to be done.
            if (building == currentSelection)
            {
                return;
            }

            // Update current selection to the new building.
            currentSelection = building;

            // Generate render if there's a selection with a mesh.
            if (currentSelection != null && currentSelection.m_mesh != null)
            {
                // Set default values.
                previewRender.CameraRotation = 210f;
                previewRender.Zoom = 4f;
                previewRender.Mesh = currentSelection.m_mesh;
                previewRender.material = currentSelection.m_material;

                RenderPreview();

                // Set background.
                previewSprite.texture = previewRender.Texture;
                noPreviewSprite.isVisible = false;
            }
            else
            {
                // No valid current selection with a mesh; reset background.
                previewSprite.texture = null;
                noPreviewSprite.isVisible = true;
            }

            // Hide any empty building names.
            if (building == null)
            {
                buildingName.isVisible = false;
            }
            else
            {
                // Set and show building name.
                buildingName.isVisible = true;
                buildingName.text = UIBuildingDetails.GetDisplayName(currentSelection.name);
                UIUtils.TruncateLabel(buildingName, width - 45);
                buildingName.autoHeight = true;
            }
        }


        /// <summary>
        /// Render the preview image.
        /// </summary>
        private void RenderPreview()
        {
            if (currentSelection == null)
            {
                return;
            }

            // If the selected building has colour variations, temporarily set the colour to the default for rendering.
            if (currentSelection.m_useColorVariations)
            {
                Color originalColor = currentSelection.m_material.color;
                currentSelection.m_material.color = currentSelection.m_color0;
                previewRender.Render();
                currentSelection.m_material.color = originalColor;
            }
            else
            {
                // No temporary colour change needed.
                previewRender.Render();
            }
        }


        /// <summary>
        /// Rotates the preview camera (model rotation) in accordance with mouse movement.
        /// </summary>
        /// <param name="c">Not used</param>
        /// <param name="p">Mouse event</param>
        private void RotateCamera(UIComponent c, UIMouseEventParameter p)
        {
            previewRender.CameraRotation -= p.moveDelta.x / previewSprite.width * 360f;
            RenderPreview();
        }
    }
}