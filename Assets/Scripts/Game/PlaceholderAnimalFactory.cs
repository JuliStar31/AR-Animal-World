using System.Collections.Generic;
using UnityEngine;

namespace ARAnimalWorld
{
    // Builds a friendly toy-like animal out of Unity primitives, so the
    // game works before real 3D models exist. The model is about 1 unit
    // tall, stands on y = 0 and faces -Z (towards the viewer).
    // A real prefab in Resources/Models/{modelName} always wins over this.
    public static class PlaceholderAnimalFactory
    {
        private static readonly Color Black = new Color(0.15f, 0.15f, 0.18f);
        private static readonly Color Pink = new Color(1f, 0.6f, 0.68f);
        private static readonly Dictionary<Color, Material> Materials = new Dictionary<Color, Material>();

        public static GameObject LoadOrCreate(AnimalData animal, Transform parent)
        {
            var prefab = Resources.Load<GameObject>("Models/" + animal.modelName);
            GameObject model = prefab != null ? Object.Instantiate(prefab, parent, false) : Create(animal.id, parent);
            if (model.GetComponent<AnimalIdleMotion>() == null)
            {
                model.AddComponent<AnimalIdleMotion>();
            }
            return model;
        }

        public static GameObject Create(AnimalId id, Transform parent)
        {
            var root = new GameObject(id + "Placeholder");
            root.transform.SetParent(parent, false);

            switch (id)
            {
                case AnimalId.Lion:
                    BuildLion(root.transform);
                    break;
                case AnimalId.Elephant:
                    BuildElephant(root.transform);
                    break;
                default:
                    BuildRabbit(root.transform);
                    break;
            }

            return root;
        }

        private static void BuildLion(Transform root)
        {
            var fur = new Color(1f, 0.76f, 0.35f);
            var mane = new Color(0.85f, 0.45f, 0.15f);

            Part(root, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.42f, 0.1f), new Vector3(90f, 0f, 0f), new Vector3(0.42f, 0.38f, 0.42f), fur);
            Legs(root, 0.12f, 0.3f, 0.13f, -0.12f, 0.32f, fur);
            Part(root, PrimitiveType.Sphere, "Mane", new Vector3(0f, 0.72f, -0.22f), Vector3.zero, Vector3.one * 0.56f, mane);
            Part(root, PrimitiveType.Sphere, "Head", new Vector3(0f, 0.72f, -0.36f), Vector3.zero, Vector3.one * 0.38f, fur);
            Part(root, PrimitiveType.Sphere, "EarL", new Vector3(-0.15f, 0.92f, -0.36f), Vector3.zero, Vector3.one * 0.12f, fur);
            Part(root, PrimitiveType.Sphere, "EarR", new Vector3(0.15f, 0.92f, -0.36f), Vector3.zero, Vector3.one * 0.12f, fur);
            Face(root, 0.78f, -0.53f, 0.08f, Black);
            Part(root, PrimitiveType.Cylinder, "Tail", new Vector3(0f, 0.5f, 0.55f), new Vector3(-45f, 0f, 0f), new Vector3(0.05f, 0.18f, 0.05f), fur);
            Part(root, PrimitiveType.Sphere, "TailTip", new Vector3(0f, 0.63f, 0.68f), Vector3.zero, Vector3.one * 0.1f, mane);
        }

        private static void BuildElephant(Transform root)
        {
            var skin = new Color(0.62f, 0.68f, 0.78f);
            var ear = new Color(0.72f, 0.66f, 0.78f);

            Part(root, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.5f, 0.12f), new Vector3(90f, 0f, 0f), new Vector3(0.58f, 0.42f, 0.58f), skin);
            Legs(root, 0.18f, 0.32f, 0.17f, -0.12f, 0.36f, skin);
            Part(root, PrimitiveType.Sphere, "Head", new Vector3(0f, 0.78f, -0.36f), Vector3.zero, Vector3.one * 0.44f, skin);
            Part(root, PrimitiveType.Sphere, "EarL", new Vector3(-0.27f, 0.8f, -0.3f), new Vector3(0f, -20f, 0f), new Vector3(0.06f, 0.4f, 0.32f), ear);
            Part(root, PrimitiveType.Sphere, "EarR", new Vector3(0.27f, 0.8f, -0.3f), new Vector3(0f, 20f, 0f), new Vector3(0.06f, 0.4f, 0.32f), ear);
            Part(root, PrimitiveType.Cylinder, "Trunk", new Vector3(0f, 0.56f, -0.58f), new Vector3(-15f, 0f, 0f), new Vector3(0.1f, 0.2f, 0.1f), skin);
            Part(root, PrimitiveType.Sphere, "TrunkTip", new Vector3(0f, 0.37f, -0.63f), Vector3.zero, Vector3.one * 0.11f, skin);
            Part(root, PrimitiveType.Sphere, "EyeL", new Vector3(-0.1f, 0.86f, -0.56f), Vector3.zero, Vector3.one * 0.07f, Black);
            Part(root, PrimitiveType.Sphere, "EyeR", new Vector3(0.1f, 0.86f, -0.56f), Vector3.zero, Vector3.one * 0.07f, Black);
            Part(root, PrimitiveType.Cylinder, "Tail", new Vector3(0f, 0.5f, 0.68f), new Vector3(-30f, 0f, 0f), new Vector3(0.04f, 0.12f, 0.04f), skin);
        }

        private static void BuildRabbit(Transform root)
        {
            var fur = new Color(0.97f, 0.95f, 0.92f);
            var innerEar = new Color(1f, 0.78f, 0.82f);

            Part(root, PrimitiveType.Sphere, "Body", new Vector3(0f, 0.27f, 0.02f), Vector3.zero, new Vector3(0.44f, 0.44f, 0.5f), fur);
            Part(root, PrimitiveType.Sphere, "FootL", new Vector3(-0.12f, 0.05f, -0.12f), Vector3.zero, new Vector3(0.12f, 0.08f, 0.2f), fur);
            Part(root, PrimitiveType.Sphere, "FootR", new Vector3(0.12f, 0.05f, -0.12f), Vector3.zero, new Vector3(0.12f, 0.08f, 0.2f), fur);
            Part(root, PrimitiveType.Sphere, "Head", new Vector3(0f, 0.56f, -0.16f), Vector3.zero, Vector3.one * 0.32f, fur);
            Part(root, PrimitiveType.Capsule, "EarL", new Vector3(-0.07f, 0.86f, -0.14f), new Vector3(0f, 0f, 10f), new Vector3(0.09f, 0.18f, 0.05f), fur);
            Part(root, PrimitiveType.Capsule, "EarR", new Vector3(0.07f, 0.86f, -0.14f), new Vector3(0f, 0f, -10f), new Vector3(0.09f, 0.18f, 0.05f), fur);
            Part(root, PrimitiveType.Capsule, "EarInnerL", new Vector3(-0.07f, 0.86f, -0.165f), new Vector3(0f, 0f, 10f), new Vector3(0.05f, 0.14f, 0.02f), innerEar);
            Part(root, PrimitiveType.Capsule, "EarInnerR", new Vector3(0.07f, 0.86f, -0.165f), new Vector3(0f, 0f, -10f), new Vector3(0.05f, 0.14f, 0.02f), innerEar);
            Part(root, PrimitiveType.Sphere, "Tail", new Vector3(0f, 0.28f, 0.27f), Vector3.zero, Vector3.one * 0.14f, Color.white);
            Face(root, 0.6f, -0.3f, 0.06f, Pink);
        }

        private static void Legs(Transform root, float thickness, float height, float halfWidth, float frontZ, float backZ, Color color)
        {
            var scale = new Vector3(thickness, height / 2f, thickness);
            float y = height / 2f;
            Part(root, PrimitiveType.Cylinder, "LegFL", new Vector3(-halfWidth, y, frontZ), Vector3.zero, scale, color);
            Part(root, PrimitiveType.Cylinder, "LegFR", new Vector3(halfWidth, y, frontZ), Vector3.zero, scale, color);
            Part(root, PrimitiveType.Cylinder, "LegBL", new Vector3(-halfWidth, y, backZ), Vector3.zero, scale, color);
            Part(root, PrimitiveType.Cylinder, "LegBR", new Vector3(halfWidth, y, backZ), Vector3.zero, scale, color);
        }

        // Two eyes and a nose on the front of the head.
        private static void Face(Transform root, float eyeY, float frontZ, float eyeSpacing, Color noseColor)
        {
            Part(root, PrimitiveType.Sphere, "EyeL", new Vector3(-eyeSpacing, eyeY, frontZ), Vector3.zero, Vector3.one * 0.06f, Black);
            Part(root, PrimitiveType.Sphere, "EyeR", new Vector3(eyeSpacing, eyeY, frontZ), Vector3.zero, Vector3.one * 0.06f, Black);
            Part(root, PrimitiveType.Sphere, "Nose", new Vector3(0f, eyeY - 0.07f, frontZ - 0.01f), Vector3.zero, Vector3.one * 0.06f, noseColor);
        }

        private static void Part(Transform root, PrimitiveType type, string name, Vector3 position, Vector3 euler, Vector3 scale, Color color)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(root, false);
            part.transform.localPosition = position;
            part.transform.localRotation = Quaternion.Euler(euler);
            part.transform.localScale = scale;

            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(collider);
                }
                else
                {
                    Object.DestroyImmediate(collider);
                }
            }

            var renderer = part.GetComponent<Renderer>();
            renderer.sharedMaterial = MaterialFor(color, renderer.sharedMaterial);
        }

        // Copies the primitive's default material so this works with the
        // built-in pipeline and URP without looking up shaders by name.
        private static Material MaterialFor(Color color, Material template)
        {
            if (Materials.TryGetValue(color, out var material) && material != null)
            {
                return material;
            }

            material = new Material(template) { color = color };
            Materials[color] = material;
            return material;
        }
    }
}
