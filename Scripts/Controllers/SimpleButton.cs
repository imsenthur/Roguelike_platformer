using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CLUTCHINPUT
{
    /// <summary>
    /// Simple button class
    /// Handles press, hold and release, just like a normal button
    /// </summary>
    public class SimpleButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        /// <summary>
        /// The name of the button
        /// </summary>
        public string ButtonName = "Jump";
        public Sprite[] faces;
        public Image buttonface;

        /// <summary>
        /// Utility object that is registered in the system
        /// </summary>
        private VirtualButton _virtualButton;
        
        /// <summary>
        /// It's pretty simple here
        /// When we enable, we register our button in the input system
        /// </summary>
        private void OnEnable()
        {
            
            buttonface = gameObject.GetComponent<Image>();
            _virtualButton = _virtualButton ?? new VirtualButton(ButtonName);
            InputManager.RegisterVirtualButton(_virtualButton);
        }

        /// <summary>
        /// When we disable, we unregister our button
        /// </summary>
        private void OnDisable()
        {
            InputManager.UnregisterVirtualButton(_virtualButton);
        }

        /// <summary>
        /// uGUI Event system stuff
        /// It's also utilised by the editor input helper
        /// </summary>
        /// <param name="eventData">Data of the passed event</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            
            buttonface.sprite = faces[1];
            _virtualButton.Release();
        }

        /// <summary>
        /// uGUI Event system stuff
        /// It's also utilised by the editor input helper
        /// </summary>
        /// <param name="eventData">Data of the passed event</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            Vibration.Vibrate(40);
            buttonface.sprite = faces[0];
            _virtualButton.Press();
        }
    }
}