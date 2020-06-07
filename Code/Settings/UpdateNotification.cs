using System;
using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Base class of the update notification panel.
    /// </summary>
    public class UpdateNotification : UIPanel
    {
        // Constants.
        private const float panelWidth = 450;
        private const float panelHeight = 200;
        private const float spacing = 10;

        // Instance references.
        private static GameObject uiGameObject;
        private static UpdateNotification _instance;
        public static UpdateNotification instance { get { return _instance; } }


        /// <summary>
        /// Creates the panel object in-game.
        /// </summary>
        public void Create()
        {
            try
            {
                // Destroy existing (if any) instances.
                uiGameObject = GameObject.Find("RealPopUpgradeNotification");
                if (uiGameObject != null)
                {
                    UnityEngine.Debug.Log("Realistic Population Revisited: found existing upgrade notification instance.");
                    GameObject.Destroy(uiGameObject);
                }

                // Create new instance.
                // Give it a unique name for easy finding with ModTools.
                uiGameObject = new GameObject("RealPopUpgradeNotification");
                uiGameObject.transform.parent = UIView.GetAView().transform;
                _instance = uiGameObject.AddComponent<UpdateNotification>();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }


        /// <summary>
        /// Create the update notification panel; called by Unity just before any of the Update methods is called for the first time.
        /// </summary>
        public override void Start()
        {
            base.Start();

            try
            {
                // Basic setup.
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                width = panelWidth;
                height = panelHeight;
                relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));
                backgroundSprite = "UnlockingPanel2";

                // Title.
                UILabel title = this.AddUIComponent<UILabel>();
                title.relativePosition = new Vector3(0, spacing);
                title.textAlignment = UIHorizontalAlignment.Center;
                title.text = "Realistic Population Revisited 1.3 update";
                title.textScale = 1.0f;
                title.autoSize = false;
                title.width = this.width;

                // Note 1.
                UILabel note1 = this.AddUIComponent<UILabel>();
                note1.relativePosition = new Vector3(spacing, 40);
                note1.textAlignment = UIHorizontalAlignment.Left;
                note1.text = "Realistic Population Revisited has been updated to 1.3.  This update adds a mod options panel (accessible through the options menu) that enables direct editing of all key configuration options for population, jobs, and consumption calculations.";
                note1.textScale = 0.8f;
                note1.autoSize = false;
                note1.autoHeight = true;
                note1.width = this.width - (spacing * 2);
                note1.wordWrap = true;

                // Close button.
                UIButton closeButton = UIUtils.CreateButton(this, 200);
                closeButton.relativePosition = new Vector3(spacing, this.height - closeButton.height - spacing);
                closeButton.text = "Close";
                closeButton.Enable();
                
                // Event handler.
                closeButton.eventClick += (c, p) =>
                {
                    // Just hide this panel and destroy the game object - nothing more to do this load.
                    this.Hide();
                    GameObject.Destroy(uiGameObject);
                };

                // "Don't show again" button.
                UIButton noShowButton = UIUtils.CreateButton(this, 200);
                noShowButton.relativePosition = new Vector3(this.width - noShowButton.width - spacing, this.height - closeButton.height - spacing);
                noShowButton.text = "Don't show again";
                noShowButton.Enable();

                // Event handler.
                noShowButton.eventClick += (c, p) =>
                {
                    // Update and save settings file.
                    Loading.settingsFile.NotificationVersion = 2;
                    Configuration<SettingsFile>.Save();

                    // Just hide this panel and destroy the game object - nothing more to do.
                    this.Hide();
                    GameObject.Destroy(uiGameObject);
                };
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }


    }
}