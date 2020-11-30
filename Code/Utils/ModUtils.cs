using System;
using System.Reflection;
using ColossalFramework.Plugins;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Class that manages interactions with other mods, including compatibility and functionality checks.
    /// </summary>
    internal static class ModUtils
    {
        // RICO methods.
        internal static MethodInfo ricoPopManaged;
        internal static MethodInfo ricoClearWorkplace;


        /// <summary>
        /// Checks to see if another mod is installed, based on a provided assembly name.
        /// Case-sensitive!  PloppableRICO is not the same as ploppablerico!
        /// </summary>
        /// <param name="assemblyName">Name of the mod assembly</param>
        /// <param name="enabledOnly">True if the mod needs to be enabled for the purposes of this check; false if it doesn't matter</param>
        /// <returns>True if the mod is installed (and, if enabledOnly is true, is also enabled), false otherwise</returns>
        internal static bool IsModInstalled(string assemblyName, bool enabledOnly = false)
        {
            // Iterate through the full list of plugins.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    if (assembly.GetName().Name.Equals(assemblyName))
                    {
                        Debugging.Message("found mod assembly " + assemblyName);
                        if (enabledOnly)
                        {
                            return plugin.isEnabled;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            // If we've made it here, then we haven't found a matching assembly.
            return false;
        }


        /// <summary>
        /// Checks to see whether the given prefab is currently having its population controlled by Ploppable RICO Revisited.
        /// Here as a separate method on its own to avoid issues with unfound binaries breaking other methods.
        /// </summary>
        /// <param name="prefab">Prefab to check</param>
        /// <returns>True if Ploppable RICO is managing this prefab, false otherwise.</returns>
        internal static bool CheckRICO(BuildingInfo prefab)
        {
            // If we haven't got the RICO method by reflection, the answer is always false.
            if (ricoPopManaged != null)
            {
                object result = ricoPopManaged.Invoke(null, new object[] { prefab });

                if (result is bool)
                {
                    return (bool)result;
                }
            }

            // Default result.
            return false;
        }


        /// <summary>
        /// Uses reflection to find the IsRICOPopManaged and ClearWorkplaceCache methods of Ploppable RICO Revisited.
        /// If successful, sets ricoPopManaged and ricoClearWorkplace fields.
        /// </summary>
        internal static void RICOReflection()
        {
            // Iterate through each loaded plugin assembly.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    if (assembly.GetName().Name.Equals("ploppablerico") && plugin.isEnabled)
                    {
                        Debugging.Message("Found Ploppable RICO Revisited");

                        // Found ploppablerico.dll that's part of an enabled plugin; try to get its ModUtils class.
                        Type ricoModUtils = assembly.GetType("PloppableRICO.Interfaces");

                        if (ricoModUtils != null)
                        {
                            // Try to get IsRICOPopManaged method.
                            ricoPopManaged = ricoModUtils.GetMethod("IsRICOPopManaged", BindingFlags.Public | BindingFlags.Static);
                            if (ricoPopManaged != null)
                            {
                                // Success!
                                Debugging.Message("found IsRICOPopManaged");
                            }

                            // Try to get ClearWorkplaceCache method.
                            ricoClearWorkplace = ricoModUtils.GetMethod("ClearWorkplaceCache", BindingFlags.Public | BindingFlags.Static);
                            if (ricoClearWorkplace != null)
                            {
                                // Success!
                                Debugging.Message("found RICO ClearWorkplaceCache");
                            }
                        }

                        // At this point, we're done; return.
                        return;
                    }
                }
            }

            // If we got here, we were unsuccessful.
            Debugging.Message("Ploppable RICO Revisited not found");
        }
    }
}
