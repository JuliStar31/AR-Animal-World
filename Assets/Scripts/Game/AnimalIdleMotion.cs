using UnityEngine;

namespace ARAnimalWorld
{
    // Gentle breathing bob and a slow look left and right, so the animal
    // feels alive without any scary or sudden movement.
    public class AnimalIdleMotion : MonoBehaviour
    {
        public float bobHeight = 0.02f;
        public float bobSpeed = 2f;
        public float lookAngle = 12f;
        public float lookSpeed = 0.6f;

        private Vector3 basePosition;
        private Quaternion baseRotation;
        private float popIn;

        private void OnEnable()
        {
            basePosition = transform.localPosition;
            baseRotation = transform.localRotation;
            popIn = 0f;
        }

        private void Update()
        {
            float t = Time.time;
            popIn = Mathf.Min(1f, popIn + Time.deltaTime * 3f);
            float overshoot = 1f + Mathf.Sin(popIn * Mathf.PI) * 0.12f;

            transform.localPosition = basePosition + Vector3.up * (Mathf.Sin(t * bobSpeed) * bobHeight);
            transform.localRotation = baseRotation * Quaternion.Euler(0f, Mathf.Sin(t * lookSpeed) * lookAngle, 0f);
            transform.localScale = Vector3.one * (popIn * overshoot) * Scale;
        }

        // Base size, so the pop-in effect does not fight other scaling.
        public float Scale { get; set; } = 1f;
    }
}
