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
        numCategories
    }


    /// <summary>
    /// Panel containing filtering mechanisms (category buttons, name search) for the building list.
    /// </summary>
    public class UIBuildingFilter : UIPanel
    {
        // Panel components.
        public UICheckBox[] categoryToggles;
        public UITextField nameFilter;


        /// <summary>
        /// Building filter category buttons.
        /// </summary>
        public class CategoryIcons
        {
            // Atlas that each icon sprite comes from.
            public static readonly string[] atlases = {"Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails" };

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
                "DistrictSpecializationSelfsufficient"
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
                "IconPolicySelfsufficient"
            };

            // Icon sprite tooltips.
            public static readonly string[] tooltips =
            {
                "Residential low",
                "Residential high",
                "Commercial low",
                "Commercial high",
                "Office" ,
                "Industrial",
                "Tourism",
                "Leisure",
                "Organic commercial",
                "Self-sufficient homes"
            };
        }

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
                categoryToggles[i] = UIUtils.CreateIconToggle(this, CategoryIcons.atlases[i], CategoryIcons.spriteNames[i], CategoryIcons.spriteNames[i] + "Disabled");
                categoryToggles[i].tooltip = CategoryIcons.tooltips[i];
                categoryToggles[i].relativePosition = new Vector3(40 * i, 0);
                categoryToggles[i].isChecked = true;
                categoryToggles[i].readOnly = true;
                categoryToggles[i].checkedBoxObject.isInteractive = false; // Don't eat my double click event please

                // Single click event handler - toggle state of this button.
                categoryToggles[i].eventClick += (c, p) =>
                {
                    ((UICheckBox)c).isChecked = !((UICheckBox)c).isChecked;
                    eventFilteringChanged(this, 0);
                };

                // Double click event handler - enable this button and deactivate all others.
                categoryToggles[i].eventDoubleClick += (c, p) =>
                {
                    for (int j = 0; j < (int)BuildingCategories.numCategories; j++)
                        categoryToggles[j].isChecked = false;
                    ((UICheckBox)c).isChecked = true;

                    eventFilteringChanged(this, 0);
                };
            }

            // Name filter.
            UILabel nameLabel = AddUIComponent<UILabel>();
            nameLabel.textScale = 0.8f;
            nameLabel.padding = new RectOffset(0, 0, 8, 0);
            nameLabel.relativePosition = new Vector3(width - 250, 0);
            nameLabel.text = "Name: ";

            nameFilter = UIUtils.CreateTextField(this, 200f, 30f);
            nameFilter.relativePosition = new Vector3(width - nameFilter.width, 0);

            // Name filter event handling - update on any change.
            nameFilter.eventTextChanged += (c, s) => eventFilteringChanged(this, 5);
            nameFilter.eventTextSubmitted += (c, s) => eventFilteringChanged(this, 5);
        }
    }
}