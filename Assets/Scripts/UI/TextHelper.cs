using UnityEngine;
using UnityEngine.UI;

namespace Visvang.UI
{
    /// <summary>
    /// Bulletproof font loading for all platforms.
    /// Bundles Inter-Regular.ttf in Resources/Fonts/ as guaranteed fallback.
    /// </summary>
    public static class TextHelper
    {
        private static Font cachedFont;

        public static Font GetFont()
        {
            if (cachedFont != null) return cachedFont;

            // 1. Try our bundled font (guaranteed to exist in build)
            cachedFont = Resources.Load<Font>("Fonts/GameFont");
            if (cachedFont != null) return cachedFont;

            // 2. Try Unity built-in fonts
            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (cachedFont != null) return cachedFont;

            cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (cachedFont != null) return cachedFont;

            // 3. Try OS fonts
            try
            {
                var fontNames = Font.GetOSInstalledFontNames();
                if (fontNames != null && fontNames.Length > 0)
                {
                    // Prefer common fonts
                    foreach (var preferred in new[] { "Arial", "Roboto", "DroidSans", "NotoSans", "Helvetica", "sans-serif" })
                    {
                        foreach (var name in fontNames)
                        {
                            if (name.Contains(preferred))
                            {
                                cachedFont = Font.CreateDynamicFontFromOSFont(name, 16);
                                if (cachedFont != null) return cachedFont;
                            }
                        }
                    }
                    // Any font at all
                    cachedFont = Font.CreateDynamicFontFromOSFont(fontNames[0], 16);
                    if (cachedFont != null) return cachedFont;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[TextHelper] OS font fallback failed: {e.Message}");
            }

            Debug.LogError("[TextHelper] No font found at all!");
            return null;
        }
    }
}
