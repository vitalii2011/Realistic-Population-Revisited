using System;
using UnityEngine;
using ICities;


namespace RealisticPopulationRevisited
{
    public class UIThreading : ThreadingExtensionBase
    {
        private bool _processed = false;


        /// <summary>
        /// Look for keypress to open GUI.
        /// </summary>
        /// <param name="realTimeDelta"></param>
        /// <param name="simulationTimeDelta"></param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // CHeck
            if (Input.GetKey(KeyCode.E) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                // Cancel if key input is already queued for processing.
                if (_processed) return;

                _processed = true;

                try
                {
                    UIBuildingDetails.instance.Toggle();
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