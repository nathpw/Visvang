using UnityEngine;
using UnityEngine.UI;

namespace Visvang.UI
{
    /// <summary>
    /// Creates UI Text components using built-in Unity UI (not TextMeshPro).
    /// TMP requires Essential Resources imported which may not exist in fresh projects.
    /// This uses UnityEngine.UI.Text which always works.
    /// </summary>
    public static class TextHelper
    {
        private static Font cachedFont;

        public static Font GetFont()
        {
            if (cachedFont != null) return cachedFont;

            // Try to load Arial (built-in)
            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (cachedFont == null)
                cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (cachedFont == null)
                cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 16);
            if (cachedFont == null)
            {
                // Last resort: find any font
                var fonts = Font.GetOSInstalledFontNames();
                if (fonts.Length > 0)
                    cachedFont = Font.CreateDynamicFontFromOSFont(fonts[0], 16);
            }

            return cachedFont;
        }

        /// <summary>
        /// Create a UI Text element (built-in, no TMP dependency).
        /// </summary>
        public static Text CreateText(Transform parent, string name, string content, int fontSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.font = GetFont();
            text.supportRichText = true;

            // Add best-fit for readability
            text.resizeTextForBestFit = false;

            return text;
        }
    }
}
