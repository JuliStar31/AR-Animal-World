using System.IO;
using UnityEditor;
using UnityEngine;

namespace ARAnimalWorld.EditorTools
{
    // Draws the simple UI shapes (rounded cards, circles, stars, scan frame)
    // and renders an icon of each animal, saving them as sprites under
    // Assets/Art/Generated. Run again any time to refresh them, for example
    // after adding real models to Resources/Models.
    public static class GeneratedArt
    {
        public const string Folder = "Assets/Art/Generated";

        public static Sprite Rounded { get; private set; }
        public static Sprite Circle { get; private set; }
        public static Sprite Star { get; private set; }
        public static Sprite Frame { get; private set; }

        public static void CreateShapes()
        {
            Directory.CreateDirectory(Folder);

            Rounded = SaveSprite("rounded", 192, (x, y, s) => Fill(RoundedRect(x, y, s, 64f)), 72);
            Circle = SaveSprite("circle", 256, (x, y, s) => Fill(RoundedRect(x, y, s, s / 2f)), 0);
            Frame = SaveSprite("scan_frame", 256, (x, y, s) => Outline(RoundedRect(x, y, s, 70f), 14f), 90);
            Star = SaveSprite("star", 256, StarAlpha, 0);
        }

        public static Sprite[] RenderAnimalIcons()
        {
            Directory.CreateDirectory(Folder);
            var icons = new Sprite[AnimalCatalog.Animals.Count];

            var stage = new GameObject("IconStage") { hideFlags = HideFlags.HideAndDontSave };
            stage.transform.position = new Vector3(0f, -1000f, 0f);

            var lightGo = new GameObject("IconLight") { hideFlags = HideFlags.HideAndDontSave };
            lightGo.transform.SetParent(stage.transform, false);
            lightGo.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;

            var camGo = new GameObject("IconCamera") { hideFlags = HideFlags.HideAndDontSave };
            camGo.transform.SetParent(stage.transform, false);
            var cam = camGo.AddComponent<Camera>();
            ConfigurePreviewCamera(cam);
            cam.enabled = false;

            var rt = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            cam.targetTexture = rt;

            try
            {
                for (int i = 0; i < icons.Length; i++)
                {
                    var animal = AnimalCatalog.Animals[i];
                    var holder = new GameObject("Holder") { hideFlags = HideFlags.HideAndDontSave };
                    holder.transform.SetParent(stage.transform, false);
                    holder.transform.localRotation = Quaternion.Euler(0f, -25f, 0f);
                    var model = PlaceholderAnimalFactory.LoadOrCreate(animal, holder.transform);
                    foreach (var t in model.GetComponentsInChildren<Transform>(true))
                    {
                        t.gameObject.hideFlags = HideFlags.HideAndDontSave;
                    }

                    cam.Render();

                    var previous = RenderTexture.active;
                    RenderTexture.active = rt;
                    var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                    tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    tex.Apply();
                    RenderTexture.active = previous;

                    string path = $"{Folder}/icon_{animal.id.ToString().ToLowerInvariant()}.png";
                    File.WriteAllBytes(path, tex.EncodeToPNG());
                    Object.DestroyImmediate(tex);
                    Object.DestroyImmediate(holder);

                    icons[i] = ImportSprite(path, 0);
                }
            }
            finally
            {
                cam.targetTexture = null;
                rt.Release();
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(stage);
            }

            return icons;
        }

        // Shared by the icon renderer and the in-game AnimalPreview camera.
        public static void ConfigurePreviewCamera(Camera cam)
        {
            cam.transform.localPosition = new Vector3(0f, 0.9f, -2.6f);
            cam.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
            cam.fieldOfView = 32f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 10f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
        }

        private delegate float AlphaFunc(float x, float y, float size);

        private static Sprite SaveSprite(string name, int size, AlphaFunc alpha, int border)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha(x + 0.5f, y + 0.5f, size)) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            string path = $"{Folder}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            return ImportSprite(path, border);
        }

        private static Sprite ImportSprite(string path, int border)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // Signed distance to a rounded square filling the texture (negative inside).
        private static float RoundedRect(float x, float y, float size, float radius)
        {
            float half = size / 2f;
            float qx = Mathf.Abs(x - half) - (half - radius - 1f);
            float qy = Mathf.Abs(y - half) - (half - radius - 1f);
            float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }

        private static float Fill(float distance)
        {
            return 0.5f - distance;
        }

        private static float Outline(float distance, float thickness)
        {
            return 0.5f - (Mathf.Abs(distance + thickness / 2f) - thickness / 2f);
        }

        // Five-point star, 4x4 supersampled for smooth edges.
        private static float StarAlpha(float x, float y, float size)
        {
            float hits = 0f;
            for (int sy = 0; sy < 4; sy++)
            {
                for (int sx = 0; sx < 4; sx++)
                {
                    float px = x - 0.5f + (sx + 0.5f) / 4f;
                    float py = y - 0.5f + (sy + 0.5f) / 4f;
                    if (InsideStar(px / size * 2f - 1f, py / size * 2f - 1f))
                    {
                        hits += 1f;
                    }
                }
            }
            return hits / 16f;
        }

        private static bool InsideStar(float x, float y)
        {
            const int points = 10;
            var verts = new Vector2[points];
            for (int i = 0; i < points; i++)
            {
                float angle = Mathf.PI / 2f + i * Mathf.PI * 2f / points;
                float radius = i % 2 == 0 ? 0.95f : 0.42f;
                verts[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius - 0.05f);
            }

            bool inside = false;
            for (int i = 0, j = points - 1; i < points; j = i++)
            {
                if ((verts[i].y > y) != (verts[j].y > y) &&
                    x < (verts[j].x - verts[i].x) * (y - verts[i].y) / (verts[j].y - verts[i].y) + verts[i].x)
                {
                    inside = !inside;
                }
            }
            return inside;
        }
    }
}
