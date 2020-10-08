using UnityEngine;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Index numbers for building category filter buttons.
    /// </summary>
    public enum BuildingCategories
    {
        None = -1,
        ResidentialLow,
        ResidentialHigh,
        CommercialLow,
        CommercialHigh,
        Office,
        Industrial,
        Tourism,
        Leisure,
        Organic,
        Selfsufficient,
        Education,
        numCategories
    }


    /// <summary>
    /// Building filter category buttons.
    /// </summary>
    public class CategoryIcons
    {
        // ItemClass ServiceClass services for each toggle.
        public static readonly ItemClass.Service[] serviceMapping =
        {
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Office,
            ItemClass.Service.Industrial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Residential,
            ItemClass.Service.Education
        };

        // ItemClass ServiceClass services for each toggle.
        public static readonly ItemClass.SubService[] subServiceMapping =
        {
            ItemClass.SubService.ResidentialLow,
            ItemClass.SubService.ResidentialHigh,
            ItemClass.SubService.CommercialLow,
            ItemClass.SubService.CommercialHigh,
            ItemClass.SubService.None,
            ItemClass.SubService.None,
            ItemClass.SubService.CommercialTourist,
            ItemClass.SubService.CommercialLeisure,
            ItemClass.SubService.CommercialEco,
            ItemClass.SubService.ResidentialLowEco,
            ItemClass.SubService.None
        };


        // Atlas that each icon sprite comes from.
        public static readonly string[] atlases = { "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Ingame" };

        // Icon sprite enabled names.
        public static readonly string[] spriteNames =
        {
            "ZoningResidentialLow",
            "ZoningResidentialHigh",
            "ZoningCommercialLow",
            "ZoningCommercialHigh",
            "ZoningOffice",
            "ZoningIndustrial",
            "DistrictSpecializationTourist",
            "DistrictSpecializationLeisure",
            "DistrictSpecializationOrganic",
            "DistrictSpecializationSelfsufficient",
            "ToolbarIconEducation"
        };

        // Icon sprite disnabled names.
        public static readonly string[] disabledSpriteNames =
        {
            "ZoningResidentialLowDisabled",
            "ZoningResidentialHighDisabled",
            "ZoningCommercialLowDisabled",
            "ZoningCommercialHighDisabled",
            "ZoningOfficeDisabled",
            "ZoningIndustrialDisabled",
            "IconPolicyTourist",
            "IconPolicyLeisure",
            "IconPolicyOrganic",
            "IconPolicySelfsufficient",
            "ToolbarIconEducationDisabled"
        };

        // Icon sprite tooltips.
        public static readonly string[] tooltips =
        {
            "RPR_CAT_RLO",
            "RPR_CAT_RHI",
            "RPR_CAT_CLO",
            "RPR_CAT_CHI",
            "RPR_CAT_OFF",
            "RPR_CAT_IND",
            "RPR_CAT_TOU",
            "RPR_CAT_LEI",
            "RPR_CAT_ORG",
            "RPR_CAT_SSH",
            "RPR_CAT_SCH"
        };
    }


    /// <summary>
    /// Panel containing filtering mechanisms (category buttons, name search) for the building list.
    /// </summary>
    public class UIBuildingFilter : UIPanel
    {
        // Panel components.
        public UICheckBox[] categoryToggles;
        public UIButton allCategories;
        public UITextField nameFilter;

        // Basic event handler for filtering changes.
        public event PropertyChangedEventHandler<int> eventFilteringChanged;


        /// <summary>
        /// Set up filter bar.
        /// We don't use Start() here as we need to access the category toggle states to set up the initial filtering list before Start() is called by UnityEngine.
        /// </summary>
        public void Setup()
        {
            // Catgegory buttons.
            categoryToggles = new UICheckBox[(int)BuildingCategories.numCategories];

            for (int i = 0; i < (int)BuildingCategories.numCategories; i++)
            {
                // Basic setup.
                categoryToggles[i] = UIUtils.CreateIconToggle(this, CategoryIcons.atlases[i], CategoryIcons.spriteNames[i], CategoryIcons.spriteNames[i] + "Disabled");
                categoryToggles[i].tooltip = Translations.Translate(CategoryIcons.tooltips[i]);
                categoryToggles[i].relativePosition = new Vector3(40 * i, 0);
                categoryToggles[i].isChecked = true;
                categoryToggles[i].readOnly = true;

                // Single click event handler - toggle state of this button.
                categoryToggles[i].eventClick += (c, p) =>
                {
                    // If either shift or control is NOT held down, deselect all other toggles.
                    if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                    {
                        for (int j = 0; j < (int)BuildingCategories.numCategories; j++)
                        {
                            categoryToggles[j].isChecked = false;
                        }
                    }

                    // Select this toggle.
                    ((UICheckBox)c).isChecked = true;

                    // Trigger an update.
                    eventFilteringChanged(this, 0);
                };
            }

            // 'All categories' button.
            allCategories = UIUtils.CreateButton(this, 120);
            allCategories.text = Translations.Translate("RPR_CAT_ALL");
            allCategories.relativePosition = new Vector3(445, 5);

            // All categories event handler.
            allCategories.eventClick += (c, p) =>
            {
                // Select all category toggles.
                for (int i = 0; i < (int)BuildingCategories.numCategories; i++)
                {
                    categoryToggles[i].isChecked = true;
                }

                // Trigger an update.
                eventFilteringChanged(this, 0);
            };

            // Name filter.
            UILabel nameLabel = AddUIComponent<UILabel>();
            nameLabel.textScale = 0.8f;
            nameLabel.padding = new RectOffset(0, 0, 8, 0);
            nameLabel.relativePosition = new Vector3(width - 250, 0);
            nameLabel.text = Translations.Translate("RPR_FIL_NAME");

            nameFilter = UIUtils.CreateTextField(this, 200f, 30f);
            nameFilter.relativePosition = new Vector3(width - nameFilter.width, 0);

            // Name filter event handling - update on any change.
            nameFilter.eventTextChanged += (c, s) => eventFilteringChanged(this, 5);
            nameFilter.eventTextSubmitted += (c, s) => eventFilteringChanged(this, 5);
        }


        /// <summary>
        /// Sets the category toggles so that the one that includes this building is on, and the rest are off
        /// </summary>
        /// <param name="buildingClass">ItemClass of the building (to match toggle categories)</param>
        public void SelectBuildingCategory(ItemClass buildingClass)
        {
            for (int i = 0; i < (int)BuildingCategories.numCategories; i ++)
            {
                if (CategoryIcons.subServiceMapping[i] == ItemClass.SubService.None && buildingClass.m_service == CategoryIcons.serviceMapping[i])
                {
                    categoryToggles[i].isChecked = true;
                }
                else if (buildingClass.m_subService == CategoryIcons.subServiceMapping[i])
                {
                    categoryToggles[i].isChecked = true;
                }
                else if (buildingClass.m_subService == ItemClass.SubService.ResidentialHighEco && CategoryIcons.subServiceMapping[i] == ItemClass.SubService.ResidentialLowEco)
                {
                    categoryToggles[i].isChecked = true;
                }
                else
                {
                    categoryToggles[i].isChecked = false;
                }
            }
        }
    }
}