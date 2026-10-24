using UnityEngine;

namespace ARAnimalWorld
{
    // Sizes any model (a real one from an asset pack or a placeholder) so it
    // fits a given size in its parent's space, stands on y = 0 and is
    // centred. Asset packs come in every scale (centimetres, metres, huge
    // Sketchfab scenes), so the AR card and the editor tools both use this.
    public static class ModelFitter
    {
        // Returns the uniform local scale that was applied.
        public static float Fit(GameObject model, float size)
        {
            var t = model.transform;
            t.localScale = Vector3.one;

            if (!TryGetLocalBounds(model, t.parent, out var bounds) || bounds.size.sqrMagnitude <= 0f)
            {
                return 1f;
            }

            float largest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            float scale = size / largest;
            t.localScale = Vector3.one * scale;

            // Bounds were measured at scale 1; scaling happens around the
            // model's pivot, so move the pivot until the scaled bounds are
            // centred on x/z and rest on y = 0.
            var pivot = t.localPosition;
            t.localPosition = new Vector3(
                -(bounds.center.x - pivot.x) * scale,
                -(bounds.min.y - pivot.y) * scale,
                -(bounds.center.z - pivot.z) * scale);
            return scale;
        }

        // Bounds of every renderer under the model, in the parent's space
        // (world space when there is no parent).
        public static bool TryGetLocalBounds(GameObject model, Transform space, out Bounds bounds)
        {
            bounds = default;
            bool any = false;
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                var world = renderer.bounds;
                var c = world.center;
                var e = world.extents;
                for (int i = 0; i < 8; i++)
                {
                    var corner = c + new Vector3((i & 1) == 0 ? -e.x : e.x, (i & 2) == 0 ? -e.y : e.y, (i & 4) == 0 ? -e.z : e.z);
                    var local = space != null ? space.InverseTransformPoint(corner) : corner;
                    if (!any)
                    {
                        bounds = new Bounds(local, Vector3.zero);
                        any = true;
                    }
                    else
                    {
                        bounds.Encapsulate(local);
                    }
                }
            }
            return any;
        }
    }
}
