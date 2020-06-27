using UnityEngine;
using ICities;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Class to handle the mod settings options panel.
    /// </summary>
    internal static class OptionsPanel
    {
        // Parent UI panel reference.
        internal static UIScrollablePanel optionsPanel;

        // Instance reference.
        private static GameObject optionsGameObject;


        /// <summary>
        /// Attaches an event hook to options panel visibility, to create/destroy our options panel as appropriate.
        /// Destroying when not visible saves UI overhead and performance impacts, especially with so many UITextFields.
        /// </summary>
        public static void OptionsEventHook()
        {
            // Get options panel instance.
            UIPanel gameOptionsPanel = UIView.library.Get<UIPanel>("OptionsPanel");

            if (gameOptionsPanel == null)
            {
                Debugging.Message("couldn't find OptionsPanel");
            }
            else
            {
                // Simple event hook to create/destroy GameObject based on appropriate visibility.
                gameOptionsPanel.eventVisibilityChanged += (control, isVisible) =>
                {
                    // Create/destroy based on visible.
                    if(isVisible)
                    {
                        // We're now visible - create our gameobject, and give it a unique name for easy finding with ModTools.
                        optionsGameObject = new GameObject("RealPopOptionsPanel");

                        // Attach to game options panel.
                        optionsGameObject.transform.parent = optionsPanel.transform;

                        // Create a base panel attached to our game object, perfectly overlaying the game options panel.
                        UIPanel basePanel = optionsGameObject.AddComponent<UIPanel>();
                        basePanel.absolutePosition = optionsPanel.absolutePosition;
                        basePanel.size = optionsPanel.size;

                        // Add tabstrip.
                        UITabstrip tabStrip = basePanel.AddUIComponent<UITabstrip>();
                        tabStrip.relativePosition = new Vector3(0, 0);
                        tabStrip.size = new Vector2(744, 713);

                        // Tab container (the panels underneath each tab).
                        UITabContainer tabContainer = basePanel.AddUIComponent<UITabContainer>();
                        tabContainer.relativePosition = new Vector3(0, 40);
                        tabContainer.size = new Vector3(744, 713);
                        tabStrip.tabPages = tabContainer;

                        // Add tabs and panels.
                        new ModOptionsPanel(tabStrip, 0);
                        new ResidentialPanel(tabStrip, 1);
                        new IndustrialPanel(tabStrip, 2);
                        new CommercialPanel(tabStrip, 3);
                        new OfficePanel(tabStrip, 4);
                    }
                    else
                    {
                        // We're no longer visible - destroy out game object.
                        GameObject.Destroy(optionsGameObject);
                    }
                };
            }
        }
    }
}