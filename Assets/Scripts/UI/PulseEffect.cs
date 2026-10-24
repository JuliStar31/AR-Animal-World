using UnityEngine;

namespace ARAnimalWorld
{
    // Soft breathing scale, used on the scan frame and reward stars.
    public class PulseEffect : MonoBehaviour
    {
        public float amount = 0.06f;
        public float speed = 2.5f;

        private Vector3 baseScale;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            transform.localScale = baseScale * (1f + Mathf.Sin(Time.unscaledTime * speed) * amount);
        }
    }
}
