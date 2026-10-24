using UnityEngine;

namespace ARAnimalWorld
{
    // Puts an animal under a parent: the real model from
    // Resources/Models/{modelName} when there is one, otherwise the
    // placeholder. It is sized to fit `size`, turned by `euler`, and given
    // the gentle idle bob. Used by the printed cards and the picture
    // recognizer.
    public static class AnimalModelSpawner
    {
        public static GameObject Spawn(AnimalData animal, Transform parent, float size, Vector3 euler, Vector3 offset, GameObject prefab = null)
        {
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("Models/" + animal.modelName);
            }

            var model = prefab != null
                ? Object.Instantiate(prefab, parent)
                : PlaceholderAnimalFactory.Create(animal.id, parent);
            model.transform.localRotation = Quaternion.Euler(euler);
            model.transform.localPosition = Vector3.zero;

            // Models come in any scale, and one many times larger than
            // `size` puts the camera inside it, so nothing shows at all.
            float scale = ModelFitter.Fit(model, size);
            model.transform.localPosition += offset;

            var idle = model.GetComponent<AnimalIdleMotion>();
            if (idle == null)
            {
                idle = model.AddComponent<AnimalIdleMotion>();
            }
            idle.Scale = scale;
            idle.bobHeight = 0.02f * size;
            return model;
        }
    }
}
