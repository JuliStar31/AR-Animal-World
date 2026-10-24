using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace ARAnimalWorld.EditorTools
{
    // Turns the Fredoka TTFs in Assets/Fonts into TextMeshPro font assets
    // (created once, then reused). The game's text is plain ASCII, which is
    // baked into the atlas so it shows even if dynamic glyphs are unavailable.
    public static class FontAssets
    {
        private const string Ascii = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

        public static TMP_FontAsset Get(string fontName)
        {
            string fontPath = $"Assets/Fonts/{fontName}.ttf";
            string assetPath = $"Assets/Fonts/{fontName} SDF.asset";

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (font == null)
            {
                Debug.LogWarning($"Font {fontPath} not found; using the TextMeshPro default font.");
                return null;
            }

            var asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, false);
            if (asset == null)
            {
                return null;
            }

            asset.name = fontName + " SDF";
            AssetDatabase.CreateAsset(asset, assetPath);
            AddSubAssets(asset);
            asset.TryAddCharacters(Ascii, out _);
            AddSubAssets(asset);

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        // The atlas textures and material must live inside the font asset
        // file, or they are lost when the scene is saved.
        private static void AddSubAssets(TMP_FontAsset asset)
        {
            foreach (var texture in asset.atlasTextures)
            {
                if (texture != null && !AssetDatabase.Contains(texture))
                {
                    texture.name = asset.name + " Atlas";
                    AssetDatabase.AddObjectToAsset(texture, asset);
                }
            }

            if (asset.material != null && !AssetDatabase.Contains(asset.material))
            {
                asset.material.name = asset.name + " Material";
                AssetDatabase.AddObjectToAsset(asset.material, asset);
            }
        }
    }
}
