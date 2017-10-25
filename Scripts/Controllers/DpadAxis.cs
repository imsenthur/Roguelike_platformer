using UnityEngine;
using UnityEngine.UI;

namespace CLUTCHINPUT
{
    public class DpadAxis : MonoBehaviour
    {
        public string AxisName;
        public float AxisMultiplier;
        public Sprite[] faces;
        public Image buttonface;

        public RectTransform RectTransform { get; private set; }
        public int LastFingerId { get; set; }
        private VirtualAxis _virtualAxis;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            buttonface = gameObject.GetComponent<Image>();
        }

        private void OnEnable()
        {
            _virtualAxis = _virtualAxis ?? new VirtualAxis(AxisName);
            LastFingerId = -1;

            InputManager.RegisterVirtualAxis(_virtualAxis);
        }

        private void OnDisable()
        {
            InputManager.UnregisterVirtualAxis(_virtualAxis);
        }

        public void Press(Vector2 screenPoint, Camera eventCamera, int pointerId)
        {
            buttonface.sprite = faces[0];
            _virtualAxis.Value = Mathf.Clamp(AxisMultiplier, -1f, 1f);
            LastFingerId = pointerId;
        }

        public void TryRelease(int pointerId)
        {
            buttonface.sprite = faces[1];
            if (LastFingerId == pointerId)
            { 
                _virtualAxis.Value = 0f;
                LastFingerId = -1;
            }
        }
    }
}