using UnityEngine;

namespace ARAnimalWorld
{
    // Quick bouncy grow-in, played each time the object is enabled.
    public class PopInEffect : MonoBehaviour
    {
        public float duration = 0.35f;
        public float delay;

        private float time;

        private void OnEnable()
        {
            time = -delay;
            transform.localScale = Vector3.zero;
        }

        public void Play()
        {
            OnEnable();
        }

        private void Update()
        {
            if (time > duration)
            {
                return;
            }

            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            // Ease-out-back: overshoots a little, then settles.
            const float c = 1.7f;
            float eased = 1f + (c + 1f) * Mathf.Pow(t - 1f, 3f) + c * Mathf.Pow(t - 1f, 2f);
            transform.localScale = Vector3.one * eased;
        }
    }
}
