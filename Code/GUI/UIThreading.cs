using System;
using UnityEngine;
using ICities;
using System.Security.Policy;

namespace RealisticPopulationRevisited
{
    public class UIThreading : ThreadingExtensionBase
    {
        // Key settings.
        public static KeyCode hotKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), "E");
        public static bool hotCtrl = false;
        public static bool hotAlt = true;
        public static bool hotShift = false;

        // Flag.
        private bool _processed = false;


        /// <summary>
        /// Look for keypress to open GUI.
        /// </summary>
        /// <param name="realTimeDelta"></param>
        /// <param name="simulationTimeDelta"></param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // Check keypress according to settings.
            if (Input.GetKey(hotKey) && (!hotAlt || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                && (!hotCtrl || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                && (!hotShift || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                // Cancel if key input is already queued for processing.
                if (_processed) return;

                _processed = true;

                try
                {
                    BuildingDetailsPanel.Open();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

            }
            else
            {
                // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                _processed = false;
            }
        }
    }

}