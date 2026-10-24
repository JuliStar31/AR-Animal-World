using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARAnimalWorld.EditorTools
{
    // Small helpers for building the uGUI hierarchy from code, using the
    // colors from docs/UI-Concept.md. Positions are in the 1080x1920
    // reference resolution of the canvas.
    public static class UIKit
    {
        public static readonly Color SkyBlue = Hex("#DDF4FF");
        public static readonly Color BlueInk = Hex("#3A7DCA");
        public static readonly Color FreshGreen = Hex("#74C96E");
        public static readonly Color GrassDark = Hex("#2D9C62");
        public static readonly Color SunYellow = Hex("#FFD95B");
        public static readonly Color Cream = Hex("#FFF9F1");
        public static readonly Color Peach = Hex("#FFB36C");
        public static readonly Color SoftOrange = Hex("#FF8C5A");
        public static readonly Color White = Color.white;
        public static readonly Color TextDark = Hex("#4A4A4A");
        public static readonly Color SoftGray = Hex("#E7EEF5");

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static RectTransform Stretch(RectTransform rect, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        // Anchors to a point of the parent: (0.5, 1) is top centre, (0.5, 0)
        // bottom centre, (0.5, 0.5) the middle. position is measured from it.
        public static RectTransform Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static Image Panel(string name, Transform parent, Color color, Sprite sprite = null, bool shadow = false)
        {
            var rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = sprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
                image.preserveAspect = sprite.border.sqrMagnitude <= 0f && sprite != GeneratedArt.Circle;
            }
            if (shadow)
            {
                AddShadow(rect.gameObject);
            }
            return image;
        }

        public static Image Icon(string name, Transform parent, Sprite sprite, Vector2 anchor, Vector2 position, float size)
        {
            var image = Panel(name, parent, White, sprite);
            image.preserveAspect = true;
            image.raycastTarget = false;
            Place(image.rectTransform, anchor, position, new Vector2(size, size));
            return image;
        }

        public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color,
            FontStyles style = FontStyles.Normal, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var rect = Rect(name, parent);
            var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            return tmp;
        }

        public static Button Button(string name, Transform parent, string label, Color background, Color textColor,
            Vector2 anchor, Vector2 position, Vector2 size, float fontSize = 60f, Sprite sprite = null)
        {
            var image = Panel(name, parent, background, sprite != null ? sprite : GeneratedArt.Rounded, true);
            Place(image.rectTransform, anchor, position, size);

            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.88f, 0.88f, 0.88f);
            colors.disabledColor = Color.white; // QuizScreen fades disabled answers itself.
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            if (!string.IsNullOrEmpty(label))
            {
                var text = Text("Label", image.transform, label, fontSize, textColor, FontStyles.Bold);
                Stretch(text.rectTransform, 20f, 20f, 10f, 10f);
            }
            return button;
        }

        // Light bordered button for secondary actions.
        public static Button SecondaryButton(string name, Transform parent, string label, Color accent,
            Vector2 anchor, Vector2 position, Vector2 size, float fontSize = 54f)
        {
            var button = Button(name, parent, label, Cream, accent, anchor, position, size, fontSize);
            var outline = button.gameObject.AddComponent<Outline>();
            outline.effectColor = accent;
            outline.effectDistance = new Vector2(5f, -5f);
            return button;
        }

        public static Button CircleButton(string name, Transform parent, string label, Color background, Color textColor,
            Vector2 anchor, Vector2 position, float size, float fontSize)
        {
            return Button(name, parent, label, background, textColor, anchor, position, new Vector2(size, size), fontSize, GeneratedArt.Circle);
        }

        public static void AddShadow(GameObject go)
        {
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.16f, 0.3f, 0.45f, 0.18f);
            shadow.effectDistance = new Vector2(0f, -10f);
        }

        // A pill with a star icon and a number, e.g. the star counter.
        public static TextMeshProUGUI StarBadge(Transform parent, Vector2 anchor, Vector2 position)
        {
            var badge = Panel("StarBadge", parent, White, GeneratedArt.Rounded, true);
            Place(badge.rectTransform, anchor, position, new Vector2(260f, 120f));
            var star = Icon("Star", badge.transform, GeneratedArt.Star, new Vector2(0f, 0.5f), new Vector2(70f, 0f), 84f);
            star.color = SunYellow;
            var count = Text("Count", badge.transform, "0", 60f, TextDark, FontStyles.Bold);
            Place(count.rectTransform, new Vector2(1f, 0.5f), new Vector2(-80f, 0f), new Vector2(140f, 100f));
            return count;
        }
    }
}
