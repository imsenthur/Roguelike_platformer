using UnityEngine;
using UnityEngine.EventSystems;


namespace CLUTCHINPUT
{
    public class Dpad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public DpadAxis[] DpadAxis;
        PointerEventData eventData;
        /// <summary>
        /// Current event camera reference. Needed for the sake of Unity Remote input
        /// </summary>
        public Camera CurrentEventCamera { get; set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            Vibration.Vibrate(30);
            
            CurrentEventCamera = eventData.pressEventCamera ?? CurrentEventCamera;

            foreach (var dpadAxis in DpadAxis)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(dpadAxis.RectTransform, eventData.position,
                    CurrentEventCamera))
                {
                    dpadAxis.Press(eventData.position, CurrentEventCamera, eventData.pointerId);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            foreach (var dpadAxis in DpadAxis)
            {
                dpadAxis.TryRelease(eventData.pointerId);
            }
        }

        private void OnDisable()
        {
            foreach (var dpadAxis in DpadAxis)
            {
                dpadAxis.TryRelease(-1);
            }
        }

    }
}