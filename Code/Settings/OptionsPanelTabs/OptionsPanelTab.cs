using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Base class for options panel tabs.
    /// </summary>
    internal abstract class OptionsPanelTab
    {
        // Status flag.
        protected bool isSetup = false;

        // Panel reference.
        protected UIPanel panel;


        /// <summary>
        /// Performs initial setup; called when panel first becomes visible.
        /// </summary>
        internal abstract void Setup();
    }
}