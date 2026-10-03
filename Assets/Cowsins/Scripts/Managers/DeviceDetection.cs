using UnityEngine;
using UnityEngine.InputSystem;

namespace cowsins2D
{
    public class DeviceDetection : MonoBehaviour
    {
        public enum InputMode
        {
            Keyboard, Controller, Mobile
        }
        public InputMode mode { get; private set; }

        public static DeviceDetection Instance;

        bool controllerInputReceived = false;

        [SerializeField] private GameObject touchInputUI;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Update() => DetectInputs();

        public void DetectInputs()
        {
            // Detects if any input has been received from the keyboard or the mouse
            bool KeyboardInputReceived = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
                                         (Mouse.current != null && (Mouse.current.delta.ReadValue() != Vector2.zero || 
                                          Mouse.current.leftButton.isPressed || 
                                          Mouse.current.rightButton.isPressed || 
                                          Mouse.current.middleButton.isPressed));

            if (KeyboardInputReceived)
            {
                controllerInputReceived = false;
            }
            else
            {
                InputDevice device = Gamepad.current ?? (InputDevice)Joystick.current;

                if (device != null)
                {
                    // Loop through each control on the detected device to see if it is being used
                    foreach (InputControl control in device.allControls)
                    {
                        // Some controls (like stick axes) aren't "buttons" so we check for their activation
                        if (control.IsPressed())
                        {
                            controllerInputReceived = true;
                            break;
                        }
                    }
                }
            }

            // Update the current device detected accordingly
            if (controllerInputReceived) mode = InputMode.Controller;
            else if (KeyboardInputReceived) mode = InputMode.Keyboard;
        }
    }
}
