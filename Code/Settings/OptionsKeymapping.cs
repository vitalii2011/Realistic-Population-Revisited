using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    public class OptionsKeymapping : UICustomControl
    {
        // Components.
        UILabel label;
        UIButton button;

        // State flag.
        private bool isPrimed = false;


        /// <summary>
        /// The current hotkey settings as ColossalFramework InputKey
        /// </summary>
        /// </summary>
        private InputKey CurrentHotkey
        {
            get => SavedInputKey.Encode(UIThreading.hotKey, UIThreading.hotCtrl, UIThreading.hotShift, UIThreading.hotAlt);

            set
            {
                UIThreading.hotKey = (KeyCode)(value & 0xFFFFFFF);
                UIThreading.hotCtrl = (value & 0x40000000) != 0;
                UIThreading.hotShift = (value & 0x20000000) != 0;
                UIThreading.hotAlt = (value & 0x10000000) != 0;
            }
        }


        /// <summary>
        /// Setup this control
        /// Called by Unity immediately before the first update.
        /// </summary>
        public void Start()
        {
            // Get the template from the game and attach it here.
            UIPanel uIPanel = component.AttachUIComponent(UITemplateManager.GetAsGameObject("KeyBindingTemplate")) as UIPanel;

            // Find our sub-components.
            label = uIPanel.Find<UILabel>("Name");
            button = uIPanel.Find<UIButton>("Binding");

            // Attach our event handlers.
            button.eventKeyDown += (control, keyEvent) => OnKeyDown(keyEvent);
            button.eventMouseDown += (control, mouseEvent) => OnMouseDown(mouseEvent);

            // Set label and button text.
            label.text = Translations.Translate("RPR_OPT_KEY");
            button.text = SavedInputKey.ToLocalizedString("KEYNAME", CurrentHotkey);
        }


        /// <summary>
        /// KeyDown event handler to record the new hotkey.
        /// </summary>
        /// <param name="keyEvent">Keypress event parameter</param>
        public void OnKeyDown(UIKeyEventParameter keyEvent)
        {
            Debugging.Message("keydown " + isPrimed);

            // Only do this if we're primed and the keypress isn't a modifier key.
            if (isPrimed && !IsModifierKey(keyEvent.keycode))
            {
                // Variables.
                InputKey inputKey;

                // Use the event.
                keyEvent.Use();

                // If escape was entered, we don't change the code.
                if (keyEvent.keycode == KeyCode.Escape)
                {
                    inputKey = CurrentHotkey;
                }
                else
                {
                    // If backspace was pressed, then we blank the input; otherwise, encode the keypress.
                    inputKey = (keyEvent.keycode == KeyCode.Backspace) ? SavedInputKey.Empty : SavedInputKey.Encode(keyEvent.keycode, keyEvent.control, keyEvent.shift, keyEvent.alt);
                }

                // Apply settings and save.
                CurrentHotkey = inputKey;
                SettingsUtils.SaveSettings();

                // Set the label for the new hotkey.
                button.text = SavedInputKey.ToLocalizedString("KEYNAME", inputKey);

                // Remove priming.
                UIView.PopModal();
                isPrimed = false;
            }
        }


        /// <summary>
        /// MouseDown event handler to handle mouse clicks; primarily used to prime hotkey entry.
        /// </summary>
        /// <param name="mouseEvent">Mouse button event parameter</param>
        public void OnMouseDown(UIMouseEventParameter mouseEvent)
        {
            // Use the event.
            mouseEvent.Use();

            // Check to see if we're already primed for hotkey entry.
            if (isPrimed)
            {
                // We were already primed; reset the button text and cancel priming.
                button.text = SavedInputKey.ToLocalizedString("KEYNAME", CurrentHotkey);
                UIView.PopModal();
                isPrimed = false;
            }
            else
            {
                // We weren't already primed - set our text and focus the button.
                //(mouseEvent.source as UIButton).buttonsMask = (UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3);
                button.text = Translations.Translate("RPR_OPT_PRS");
                button.Focus();

                // Prime for new keybinding entry.
                isPrimed = true;
                UIView.PushModal(button);
            }
        }


        /// <summary>
        /// Checks to see if the given keycode is a modifier key.
        /// </summary>
        /// <param name="code">Leycode to check</param>
        /// <returns>True if the key is a modifier key, false otherwise</returns>
        private bool IsModifierKey(KeyCode keyCode)
        {
            return (keyCode == KeyCode.LeftControl || keyCode == KeyCode.RightControl || keyCode == KeyCode.LeftShift || keyCode == KeyCode.RightShift || keyCode == KeyCode.LeftAlt || keyCode == KeyCode.RightAlt || keyCode == KeyCode.AltGr);
        }
    }
}