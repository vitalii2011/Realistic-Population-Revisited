using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Globalization;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Base class of the building details screen.  Based (via AJ3D's Ploppable RICO) ultimately on SamsamTS's Building Themes panel; many thanks to him for his work.
    /// </summary>
    public class UIBuildingDetails : UIPanel
    {
        // Constants.
        private const float leftWidth = 400;
        private const float middleWidth = 250;
        private const float rightWidth = 280;
        private const float filterHeight = 40;
        private const float panelHeight = 550;
        private const float spacing = 5;

        public const float titleHeight = 40;

        // Panel components.
        private UITitleBar titleBar;
        private UIBuildingFilter filterBar;
        private UIFastList buildingSelection;
        private UIPreviewPanel previewPanel;
        private UIModCalcs modCalcs;

        // General vars.
        public BuildingInfo currentSelection;

        // Instance references.
        private static GameObject uiGameObject;
        private static UIBuildingDetails _instance;
        public static UIBuildingDetails instance { get { return _instance; } }


        /// <summary>
        /// Creates the panel object in-game.
        /// </summary>
        public static void Create()
        {
            try
            {
                // Destroy existing (if any) instances.
                uiGameObject = GameObject.Find("RealPopBuildingEditor");
                if (uiGameObject != null)
                {
                    Debug.Log("Realistic Population Revisited: destroying existing building details panel instance.");
                    GameObject.Destroy(uiGameObject);
                }

                // Create new instance.
                // Give it a unique name for easy finding with ModTools.
                uiGameObject = new GameObject("RealPopBuildingDetails");
                uiGameObject.transform.parent = UIView.GetAView().transform;
                _instance = uiGameObject.AddComponent<UIBuildingDetails>();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        /// <summary>
        /// Create the building editor panel; called by Unity just before any of the Update methods is called for the first time.
        /// </summary>
        public override void Start()
        {
            base.Start();

            try
            {
                // Basic setup.
                isVisible = false;
                canFocus = true;
                isInteractive = true;
                width = leftWidth + middleWidth + rightWidth + (spacing * 4);
                height = panelHeight + titleHeight + filterHeight + (spacing * 2);
                relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));
                backgroundSprite = "UnlockingPanel2";

                // Titlebar.
                titleBar = AddUIComponent<UITitleBar>();

                // Filter.
                filterBar = AddUIComponent<UIBuildingFilter>();
                filterBar.width = width - (spacing * 2);
                filterBar.height = filterHeight;
                filterBar.relativePosition = new Vector3(spacing, titleHeight);

                filterBar.eventFilteringChanged += (c, i) =>
                {
                    if (i == -1) return;

                    int listCount = buildingSelection.rowsData.m_size;
                    float position = buildingSelection.listPosition;

                    buildingSelection.selectedIndex = -1;

                    buildingSelection.rowsData = GenerateFastList();
                };

                // Set up panels.
                // Left panel - list of buildings.
                UIPanel leftPanel = AddUIComponent<UIPanel>();
                leftPanel.width = leftWidth;
                leftPanel.height = panelHeight;
                leftPanel.relativePosition = new Vector3(spacing, titleHeight + filterHeight + spacing);

                // Middle panel - building preview and save panels.
                UIPanel middlePanel = AddUIComponent<UIPanel>();
                middlePanel.width = middleWidth;
                middlePanel.height = panelHeight;
                middlePanel.relativePosition = new Vector3(leftWidth + (spacing * 2), titleHeight + filterHeight + spacing);

                previewPanel = middlePanel.AddUIComponent<UIPreviewPanel>();
                previewPanel.width = middlePanel.width;
                previewPanel.height = (panelHeight - spacing) / 2;
                previewPanel.relativePosition = Vector3.zero;

                // Right panel - mod calculations.
                UIPanel rightPanel = AddUIComponent<UIPanel>();
                rightPanel.width = rightWidth;
                rightPanel.height = panelHeight;
                rightPanel.relativePosition = new Vector3(leftWidth + middleWidth + (spacing * 3), titleHeight + filterHeight + spacing);

                modCalcs = rightPanel.AddUIComponent<UIModCalcs>();
                modCalcs.width = rightWidth;
                modCalcs.height = panelHeight;
                modCalcs.relativePosition = Vector3.zero;

                // Building selection list.
                buildingSelection = UIFastList.Create<UIBuildingRow>(leftPanel);
                buildingSelection.backgroundSprite = "UnlockingPanel";
                buildingSelection.width = leftPanel.width;
                buildingSelection.height = leftPanel.height;
                buildingSelection.canSelect = true;
                buildingSelection.rowHeight = 40;
                buildingSelection.autoHideScrollbar = true;
                buildingSelection.relativePosition = Vector3.zero;
                buildingSelection.rowsData = new FastList<object>();
                buildingSelection.selectedIndex = -1;

                // Set up filterBar to make sure selection filters are properly initialised before calling GenerateFastList.
                filterBar.Setup();

                // Populate the list.
                buildingSelection.rowsData = GenerateFastList();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        /// <summary>
        /// Shows/hides the building details screen.
        /// </summary>
        public void Toggle()
        {
            if (isVisible)
                Hide();
            else
                Show(true);
        }


        /// <summary>
        /// Called when the building selection changes to update building preview and mod calculations panel.
        /// </summary>
        /// <param name="building"></param>
        public void UpdateSelectedBuilding(BuildingInfo building)
        {
            if (building != null)
            {
                // Update building preview.
                currentSelection = building;
                previewPanel.Show(currentSelection);
            }

            // Update mod calculations panel.
            modCalcs.SelectionChanged(building);
        }


        /// <summary>
        /// Generates the list of buildings depending on current filter settings.
        /// </summary>
        /// <returns></returns>
        private FastList<object> GenerateFastList()
        {
            // List to store all building prefabs that pass the filter.
            List<BuildingInfo> filteredList = new List<BuildingInfo>();

            // Iterate through all loaded building prefabs and add them to the list if they meet the filter conditions.
            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                // Skip any null prefabs.
                if (PrefabCollection<BuildingInfo>.GetLoaded(i) == null)
                {
                    continue;
                }

                BuildingInfo item = PrefabCollection<BuildingInfo>.GetLoaded(i);

                // Apply zone type filters.
                // Laid out this way for clear visibility.
                if (item.m_class.m_subService == ItemClass.SubService.ResidentialLow && filterBar.categoryToggles[(int)BuildingCategories.ResidentialLow].isChecked)
                {
                }
                else if (item.m_class.m_subService == ItemClass.SubService.ResidentialHigh && filterBar.categoryToggles[(int)BuildingCategories.ResidentialHigh].isChecked)
                {
                }
                else if (item.m_class.m_subService == ItemClass.SubService.CommercialLow && filterBar.categoryToggles[(int)BuildingCategories.CommercialLow].isChecked)
                {
                }
                else if (item.m_class.m_subService == ItemClass.SubService.CommercialHigh && filterBar.categoryToggles[(int)BuildingCategories.CommercialHigh].isChecked)
                {
                }
                else if (item.m_class.m_service == ItemClass.Service.Office && filterBar.categoryToggles[(int)BuildingCategories.Office].isChecked)
                {
                }
                else if (item.m_class.m_service == ItemClass.Service.Industrial && filterBar.categoryToggles[(int)BuildingCategories.Industrial].isChecked)
                {
                }
                else if (item.m_class.m_subService == ItemClass.SubService.CommercialTourist && filterBar.categoryToggles[(int)BuildingCategories.Tourism].isChecked)
                {
                }
                else if (item.m_class.m_subService == ItemClass.SubService.CommercialLeisure && filterBar.categoryToggles[(int)BuildingCategories.Leisure].isChecked)
                {
                }
                else if (item.m_class.m_subService == ItemClass.SubService.CommercialEco && filterBar.categoryToggles[(int)BuildingCategories.Organic].isChecked)
                {
                }
                else if ((item.m_class.m_subService == ItemClass.SubService.ResidentialLowEco || item.m_class.m_subService == ItemClass.SubService.ResidentialHighEco)  && filterBar.categoryToggles[(int)BuildingCategories.Selfsufficient].isChecked)
                {
                }
                else
                {
                    // If we've gotten here, then we've matched no categories; move on to next item.
                    continue;
                }

                // Filter by name.
                if (!filterBar.nameFilter.text.Trim().IsNullOrWhiteSpace() && !GetDisplayName(item.name).ToLower().Contains(filterBar.nameFilter.text.Trim().ToLower()))
                {
                    continue;
                }

                // Finally!  We've got an item that's passed all filters; add it to the list.
                filteredList.Add(item);
            }

            // Create return list with our filtered list, sorted alphabetically.
            FastList<object> fastList = new FastList<object>();
            fastList.m_buffer = filteredList.OrderBy(x => UIBuildingDetails.GetDisplayName(x.name)).ToArray();
            fastList.m_size = filteredList.Count;
            return fastList;
        }


        /// <summary>
        /// Returns the name of the building prefab cleaned up for display.
        /// </summary>
        /// <param name="fullName">Raw prefab name</param>
        /// <returns></returns>
        public static string GetDisplayName(string fullName)
        {
            // Filter out leading package number and trailing '_Data'.
            return fullName.Substring(fullName.IndexOf('.') + 1).Replace("_Data", "");
        }
    }
}