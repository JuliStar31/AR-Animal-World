using UnityEngine;

namespace ARAnimalWorld
{
    // Moves the glowing line of the scan frame gently up and down.
    [RequireComponent(typeof(RectTransform))]
    public class ScanLineEffect : MonoBehaviour
    {
        public float travel = 220f;
        public float speed = 1.4f;

        private RectTransform rect;
        private Vector2 start;

        private void Awake()
        {
            rect = (RectTransform)transform;
            start = rect.anchoredPosition;
        }

        private void Update()
        {
            rect.anchoredPosition = start + Vector2.up * (Mathf.Sin(Time.unscaledTime * speed) * travel);
        }
    }
}
