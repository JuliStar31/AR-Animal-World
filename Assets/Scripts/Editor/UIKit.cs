using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARAnimalWorld.EditorTools
{
    // Helpers for building the uGUI hierarchy from code. Positions use the
    // mockups' own pixel grid (780 x 1688, top-left origin), so numbers can
    // be read straight off the designs.
    public static class UIKit
    {
        public const float DesignWidth = 780f;
        public const float DesignHeight = 1688f;

        public static readonly Color Sky = Hex("#BFE6FA");
        public static readonly Color Ink = Hex("#124A73");
        public static readonly Color Brown = Hex("#3B2A14");
        public static readonly Color Cream = Hex("#FFF8E7");
        public static readonly Color CreamShadow = Hex("#9FC9E2");
        public static readonly Color Green = Hex("#1B7F3B");
        public static readonly Color GreenDark = Hex("#11592A");
        public static readonly Color Yellow = Hex("#FFC93C");
        public static readonly Color YellowDark = Hex("#D9A21A");
        public static readonly Color Orange = Hex("#F59B3A");
        public static readonly Color OrangeDark = Hex("#C9741F");
        public static readonly Color Halo = Hex("#E3F4FD");
        public static readonly Color PaleBlue = Hex("#E8F6FE");
        public static readonly Color HillBack = Hex("#8ED178");
        public static readonly Color HillFront = Hex("#A8E08A");
        public static readonly Color White = Color.white;

        public static TMP_FontAsset BoldFont;
        public static TMP_FontAsset MediumFont;

        public enum Edge
        {
            Top,
            Middle,
            Bottom
        }

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
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        // Places a rect on the screen using mockup coordinates. Edge says
        // which part of a taller or shorter phone it sticks to: Top items keep
        // their distance from the top, Bottom items from the bottom, Middle
        // items stay centred.
        public static RectTransform Put(RectTransform rect, Edge edge, float x, float y, float w, float h)
        {
            float cx = x + w / 2f - DesignWidth / 2f;
            float cy = y + h / 2f;
            switch (edge)
            {
                case Edge.Top:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(cx, -cy);
                    break;
                case Edge.Bottom:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.anchoredPosition = new Vector2(cx, DesignHeight - cy);
                    break;
                default:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(cx, DesignHeight / 2f - cy);
                    break;
            }
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }

        // Places a child inside its parent, top-left origin, parent pixels.
        public static RectTransform Local(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x + w / 2f, -(y + h / 2f));
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }

        public static Image Shape(string name, Transform parent, Color color, Sprite sprite, float radius = 64f)
        {
            var rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            image.raycastTarget = false;
            if (sprite != null && sprite.border.sqrMagnitude > 0f)
            {
                image.type = Image.Type.Sliced;
                // The rounded sprite has a 64 unit corner at multiplier 1.
                image.pixelsPerUnitMultiplier = Mathf.Max(0.01f, 64f / Mathf.Max(1f, radius));
            }
            return image;
        }

        public static Image Round(string name, Transform parent, Color color, float radius = 64f)
        {
            return Shape(name, parent, color, GeneratedArt.Rounded, radius);
        }

        public static Image Dot(string name, Transform parent, Color color)
        {
            return Shape(name, parent, color, GeneratedArt.Circle);
        }

        public static Image Picture(string name, Transform parent, Sprite sprite, Color? tint = null)
        {
            var image = Shape(name, parent, tint ?? White, sprite);
            image.preserveAspect = true;
            return image;
        }

        // A surface with a solid "lip" under it, like the cards and buttons in
        // the mockups. Returns the face; the container is its parent.
        public static Image Raised(string name, Transform parent, Color face, Color shadow, float lip, float radius,
            out RectTransform container)
        {
            container = Rect(name, parent);
            var under = Round("Shadow", container, shadow, radius);
            Stretch(under.rectTransform, 0f, 0f, lip, 0f);
            var top = Round("Face", container, face, radius);
            Stretch(top.rectTransform, 0f, 0f, 0f, lip);
            return top;
        }

        public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color,
            bool bold = true, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var rect = Rect(name, parent);
            var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
            var font = bold ? BoldFont : MediumFont;
            if (font != null)
            {
                tmp.font = font;
            }
            else if (bold)
            {
                tmp.fontStyle = FontStyles.Bold;
            }
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            return tmp;
        }

        // Pill button with a lip, an optional icon and a label, like
        // "Mulai Belajar" or "Main Kuis".
        public static Button PillButton(string name, Transform parent, Edge edge, float x, float y, float w, float h,
            string label, Color face, Color shadow, Color textColor, Sprite icon = null, float fontSize = 44f, float lip = 12f)
        {
            var top = Raised(name, parent, face, shadow, lip, (h - lip) / 2f, out var container);
            Put(container, edge, x, y, w, h);
            var button = MakeButton(container.gameObject, top);

            var row = Rect("Content", top.transform);
            Stretch(row);
            var text = Text("Label", row, label, fontSize, textColor);
            if (icon != null)
            {
                float iconSize = fontSize * 0.95f;
                float textWidth = EstimateWidth(label, fontSize);
                float total = iconSize + 20f + textWidth;
                var image = Picture("Icon", row, icon, textColor);
                image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                image.rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
                image.rectTransform.anchoredPosition = new Vector2(-total / 2f + iconSize / 2f, 0f);
                text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                text.rectTransform.sizeDelta = new Vector2(textWidth + 40f, h);
                text.rectTransform.anchoredPosition = new Vector2(total / 2f - textWidth / 2f, 0f);
            }
            else
            {
                Stretch(text.rectTransform, 24f, 24f, 0f, 0f);
            }
            return button;
        }

        // Round cream button with a dark chevron, used for "back".
        public static Button BackButton(Transform parent, Sprite chevron, Color shadow, Edge edge = Edge.Top, float x = 48f, float y = 48f)
        {
            var top = Raised("BackButton", parent, Cream, shadow, 8f, 52f, out var container);
            top.sprite = GeneratedArt.Circle;
            top.type = Image.Type.Simple;
            var under = container.GetChild(0).GetComponent<Image>();
            under.sprite = GeneratedArt.Circle;
            under.type = Image.Type.Simple;
            Put(container, edge, x, y, 112f, 120f);
            var icon = Picture("Chevron", top.transform, chevron, Brown);
            Stretch(icon.rectTransform, 30f, 30f, 28f, 28f);
            return MakeButton(container.gameObject, top);
        }

        public static Button MakeButton(GameObject go, Graphic target)
        {
            var hit = go.GetComponent<Image>();
            if (hit == null)
            {
                // Invisible graphic so the whole area (lip included) is tappable.
                hit = go.AddComponent<Image>();
                hit.color = new Color(1f, 1f, 1f, 0f);
            }
            hit.raycastTarget = true;

            var button = go.AddComponent<Button>();
            button.targetGraphic = target;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.86f, 0.86f, 0.86f);
            colors.disabledColor = Color.white; // Screens style disabled states themselves.
            colors.fadeDuration = 0.06f;
            button.colors = colors;
            return button;
        }

        // A star counter pill: star icon and a number.
        public static TextMeshProUGUI StarBadge(Transform parent, Sprite star, float x, float y, float w = 182f, float h = 100f)
        {
            var top = Raised("StarBadge", parent, Cream, CreamShadow, 8f, (h - 8f) / 2f, out var container);
            Put(container, Edge.Top, x, y, w, h);
            Local(Picture("Star", top.transform, star).rectTransform, 26f, (h - 8f) / 2f - 27f, 54f, 54f);
            var count = Text("Count", top.transform, "0", 42f, Brown);
            Local(count.rectTransform, 86f, 0f, w - 100f, h - 8f);
            return count;
        }

        // Soft rolling hills along the bottom of a screen.
        public static void Hills(Transform screen, bool twoLayers)
        {
            if (twoLayers)
            {
                BehindContent(Put(Dot("HillBack", screen, HillBack).rectTransform, Edge.Bottom, -360f, 1165f, 1500f, 1100f));
            }
            BehindContent(Put(Dot("HillFront", screen, HillFront).rectTransform, Edge.Bottom, -260f, 1228f, 1300f, 1000f));
        }

        public static void Cloud(Transform parent, Sprite cloud, Edge edge, float x, float y, float w, float h)
        {
            var image = Picture("Cloud", parent, cloud, new Color(1f, 1f, 1f, 0.75f));
            image.preserveAspect = false;
            BehindContent(Put(image.rectTransform, edge, x, y, w, h));
        }

        // Screen decorations are added after the SafeArea content, which
        // would draw them over the buttons; move them underneath it.
        private static void BehindContent(RectTransform decoration)
        {
            var content = decoration.parent.Find("SafeArea");
            if (content != null)
            {
                decoration.SetSiblingIndex(content.GetSiblingIndex());
            }
        }

        // Rough text width for centring an icon next to a label.
        private static float EstimateWidth(string text, float fontSize)
        {
            return text.Length * fontSize * 0.52f;
        }
    }
}
