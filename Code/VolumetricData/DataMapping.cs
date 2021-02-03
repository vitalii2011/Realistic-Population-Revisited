namespace RealPop2
{
    /// <summary>
    /// Class for mapping serives to DataStore structures.
    /// </summary>
    internal class DataMapping
    {
        // Number of data structures (service/subservice combinations) to map.
        internal const int numData = 16;

        // Mapping arrays.
        internal ItemClass.Service[] services;
        internal ItemClass.SubService[] subServices;
        internal int[][][] dataArrays;


        /// <summary>
        /// Constructor - initialises arrays.
        /// </summary>
        internal DataMapping()
        {
            // Services.
            services = new ItemClass.Service[numData]
            {
                ItemClass.Service.Residential,
                ItemClass.Service.Residential,
                ItemClass.Service.Residential,
                ItemClass.Service.Residential,
                ItemClass.Service.Commercial,
                ItemClass.Service.Commercial,
                ItemClass.Service.Commercial,
                ItemClass.Service.Commercial,
                ItemClass.Service.Commercial,
                ItemClass.Service.Office,
                ItemClass.Service.Office,
                ItemClass.Service.Industrial,
                ItemClass.Service.Industrial,
                ItemClass.Service.Industrial,
                ItemClass.Service.Industrial,
                ItemClass.Service.Industrial
            };

            // Sub-services.
            subServices = new ItemClass.SubService[numData]
            {
                ItemClass.SubService.ResidentialLow,
                ItemClass.SubService.ResidentialHigh,
                ItemClass.SubService.ResidentialLowEco,
                ItemClass.SubService.ResidentialHighEco,
                ItemClass.SubService.CommercialLow,
                ItemClass.SubService.CommercialHigh,
                ItemClass.SubService.CommercialEco,
                ItemClass.SubService.CommercialLeisure,
                ItemClass.SubService.CommercialTourist,
                ItemClass.SubService.OfficeGeneric,
                ItemClass.SubService.OfficeHightech,
                ItemClass.SubService.IndustrialGeneric,
                ItemClass.SubService.IndustrialFarming,
                ItemClass.SubService.IndustrialForestry,
                ItemClass.SubService.IndustrialOil,
                ItemClass.SubService.IndustrialOre
            };

            // Data arrays.
            dataArrays = new int[numData][][]
            {
                DataStore.residentialLow,
                DataStore.residentialHigh,
                DataStore.resEcoLow,
                DataStore.resEcoHigh,
                DataStore.commercialLow,
                DataStore.commercialHigh,
                DataStore.commercialEco,
                DataStore.commercialLeisure,
                DataStore.commercialTourist,
                DataStore.office,
                DataStore.officeHighTech,
                DataStore.industry,
                DataStore.industry_farm,
                DataStore.industry_forest,
                DataStore.industry_oil,
                DataStore.industry_ore
            };
        }

        
        /// <summary>
        /// Returns the DataStore array for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="subService">Sub-service</param>
        /// <returns>DataStore array (null if no match)</returns>
        internal int[][] GetArray(ItemClass.Service service, ItemClass.SubService subService)
        {
            // Iterate through each data structure.
            for (int i = 0; i < numData; ++i)
            {
                // Look for a service and sub-service match.
                if (service == services[i] && subService == subServices[i])
                {
                    // Matched - return the relevant DataStore array.
                    return dataArrays[i];
                }
            }

            // If we got here, no match was found; return null.
            return null;
        }
    }
}
