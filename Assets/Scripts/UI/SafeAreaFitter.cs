using UnityEngine;

namespace ARAnimalWorld
{
    // Keeps content inside the phone's safe area (notches, rounded corners).
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private Rect applied;

        private void Update()
        {
            Rect safe = Screen.safeArea;
            if (safe == applied || Screen.width == 0 || Screen.height == 0)
            {
                return;
            }

            applied = safe;
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rect.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
