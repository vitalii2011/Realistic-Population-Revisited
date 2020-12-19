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
        // Layout constants.
        internal const float popOverrideX = 290f;
        internal const float floorOverrideX = popOverrideX + 30f;
        internal const float defaultPopOverrideX = floorOverrideX + 30f;
        internal const float defaultFloorOverrideX = defaultPopOverrideX + 30f;
        internal const float anyX = defaultFloorOverrideX + 30f;

        // Panel components.
        internal UICheckBox[] categoryToggles;
        private UICheckBox popOverrideFilter, floorOverrideFilter, defaultPopFilter, defaultFloorFilter, anyFilter;
        internal UIButton allCategories;
        internal UITextField nameFilter;

        // Basic event handler for filtering changes.
        public event PropertyChangedEventHandler<int> eventFilteringChanged;


        internal UICheckBox PopOverrideFilter => popOverrideFilter;
        internal UICheckBox FloorOverrideFilter => floorOverrideFilter;
        internal UICheckBox DefaultPopFilter => defaultPopFilter;
        internal UICheckBox DefaultFloorFilter => defaultFloorFilter;
        internal UICheckBox AnyFilter => anyFilter;


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

            // Create settings filters.
            UILabel filterLabel = SettingsFilterLabel(34f, Translations.Translate("RPR_FIL_SET"));
            UILabel subLabel = SettingsFilterLabel(48f, Translations.Translate("RPR_FIL_SES"));

            // Settings filter checkboxes.
            popOverrideFilter = AddFilterCheckbox(popOverrideX, Translations.Translate("RPR_CUS_POP"));
            floorOverrideFilter = AddFilterCheckbox(floorOverrideX, Translations.Translate("RPR_CUS_FLR"));
            defaultPopFilter = AddFilterCheckbox(defaultPopOverrideX, Translations.Translate("RPR_CUS_NDP"));
            defaultFloorFilter = AddFilterCheckbox(defaultFloorOverrideX, Translations.Translate("RPR_CUS_NDF"));
            anyFilter = AddFilterCheckbox(anyX, Translations.Translate("RPR_CUS_ANY"));

            // Settings filter checkbox handlers - 'any' checkbox clears others, other checkboxes clear 'any'.

            popOverrideFilter.eventCheckChanged += (control, isChecked) => { if (isChecked) anyFilter.isChecked = false; };
            floorOverrideFilter.eventCheckChanged += (control, isChecked) => { if (isChecked) anyFilter.isChecked = false; };
            defaultPopFilter.eventCheckChanged += (control, isChecked) => { if (isChecked) anyFilter.isChecked = false; };
            defaultFloorFilter.eventCheckChanged += (control, isChecked) => { if (isChecked) anyFilter.isChecked = false; };
            anyFilter.eventCheckChanged += (control, isChecked) =>
            {
                if (isChecked)
                {
                    popOverrideFilter.isChecked = false;
                    floorOverrideFilter.isChecked = false;
                    defaultPopFilter.isChecked = false;
                    defaultFloorFilter.isChecked = false;
                }
            };
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


        /// <summary>
        /// Adds a filter label.
        /// </summary>
        /// <param name="yPos">Relative Y position of label</param>
        /// <param name="text">Label text</param>
        /// <returns>New label</returns>
        private UILabel SettingsFilterLabel(float yPos, string text)
        {
            // Basic setup.
            UILabel newLabel = this.AddUIComponent<UILabel>();
            newLabel.textScale = 0.8f;
            newLabel.relativePosition = new Vector3(10f, yPos, 0);
            newLabel.autoSize = false;
            newLabel.height = 30f;
            newLabel.width = 280f;
            newLabel.wordWrap = false;
            newLabel.verticalAlignment = UIVerticalAlignment.Middle;

            // Assign text.
            newLabel.text = text;

            return newLabel;
        }


        /// <summary>
        /// Adds a filter checkbox.
        /// </summary>
        /// <param name="xPos">Relative X position of checkbox</param>
        /// <param name="tooltip">Checkbox tooltip</param>
        /// <returns>New filter checkbox</returns>
        private UICheckBox AddFilterCheckbox(float xPos, string tooltip)
        {
            // Basic setup.
            UICheckBox newCheckBox = this.AddUIComponent<UICheckBox>();
            newCheckBox.width = 20f;
            newCheckBox.height = 20f;
            newCheckBox.clipChildren = true;
            newCheckBox.relativePosition = new Vector3(xPos, 45f);

            // Sprite.
            UISprite sprite = newCheckBox.AddUIComponent<UISprite>();
            sprite.spriteName = "ToggleBase";
            sprite.size = new Vector2(20f, 20f);
            sprite.relativePosition = Vector3.zero;

            newCheckBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)newCheckBox.checkedBoxObject).spriteName = "ToggleBaseFocused";
            newCheckBox.checkedBoxObject.size = new Vector2(20f, 20f);
            newCheckBox.checkedBoxObject.relativePosition = Vector3.zero;

            // Tooltip.
            newCheckBox.tooltip = tooltip;

            // Trigger filtering changed event if any checkbox is changed.
            newCheckBox.eventCheckChanged += (control, isChecked) => { eventFilteringChanged(this, 0); };

            return newCheckBox;
        }
    }
}