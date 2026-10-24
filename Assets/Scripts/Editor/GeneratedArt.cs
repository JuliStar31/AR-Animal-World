using System.IO;
using UnityEditor;
using UnityEngine;

namespace ARAnimalWorld.EditorTools
{
    // UI sprites. Rounded shapes are drawn here (they need exact 9-slice
    // borders); the illustrations and icons come from Assets/Art/UI, drawn by
    // tools/make_ui_art.py, and are set up as sprites on import.
    public static class GeneratedArt
    {
        public const string Folder = "Assets/Art/Generated";
        public const string UiFolder = "Assets/Art/UI";

        // Rounded rectangle with a 64 px corner inside a 72 px 9-slice
        // border, so at Image.pixelsPerUnitMultiplier 1 the corner radius is
        // 64 canvas units; raise the multiplier for smaller corners.
        public static Sprite Rounded { get; private set; }
        public static Sprite Circle { get; private set; }

        public static void CreateShapes()
        {
            Directory.CreateDirectory(Folder);
            Rounded = SaveSprite("rounded", 192, (x, y, s) => Fill(RoundedRect(x, y, s, 64f)), 72);
            Circle = SaveSprite("circle", 512, (x, y, s) => Fill(RoundedRect(x, y, s, s / 2f)), 0);
        }

        public static Sprite Ui(string name)
        {
            string path = $"{UiFolder}/{name}.png";
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Missing UI art {path}. Run tools/make_ui_art.py.");
                return null;
            }
            return ImportSprite(path, 0);
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
    }
}
