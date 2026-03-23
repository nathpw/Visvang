using UnityEngine;
using System.Collections.Generic;
using Visvang.Core;

namespace Visvang.Art
{
    /// <summary>
    /// Generates UI icons for dips, rods, reels, accessories, and badges.
    /// All icons are 64x64 pixel art style.
    /// </summary>
    public static class IconGenerator
    {
        private const int SIZE = 64;
        private static Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        // --- DIP BOTTLE ICONS ---

        public static Sprite GetDipIcon(string dipName, Color dipColor)
        {
            string key = $"dip_{dipName}";
            if (cache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] px = new Color[SIZE * SIZE];
            ClearPixels(px);

            // Bottle shape
            Color glass = new Color(0.7f, 0.75f, 0.8f, 0.6f);
            Color cap = new Color(0.3f, 0.3f, 0.35f);

            // Cap
            FillRect(px, SIZE, 24, 52, 40, 58, cap);

            // Neck
            FillRect(px, SIZE, 26, 46, 38, 52, glass);

            // Body
            FillRoundRect(px, SIZE, 16, 8, 48, 46, glass);

            // Liquid fill (based on dip color)
            Color liquid = dipColor;
            liquid.a = 0.9f;
            FillRoundRect(px, SIZE, 18, 10, 46, 36, liquid);

            // Label
            Color labelColor = new Color(0.9f, 0.88f, 0.82f);
            FillRect(px, SIZE, 20, 18, 44, 30, labelColor);

            // Highlight on glass
            for (int y = 20; y < 40; y++)
                SetPixel(px, SIZE, 20, y, new Color(1f, 1f, 1f, 0.3f));

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), Vector2.one * 0.5f, 100f);
            sprite.name = key;
            cache[key] = sprite;
            return sprite;
        }

        // --- ROD ICONS ---

        public static Sprite GetRodIcon(string rodName, RodTier tier)
        {
            string key = $"rod_{rodName}";
            if (cache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] px = new Color[SIZE * SIZE];
            ClearPixels(px);

            Color rodColor = GetTierColor(tier);
            Color handleColor = new Color(0.3f, 0.2f, 0.1f);
            Color guideColor = new Color(0.6f, 0.6f, 0.65f);

            // Rod shaft (diagonal)
            for (int i = 0; i < 50; i++)
            {
                int x = 8 + i;
                int y = 8 + Mathf.RoundToInt(i * 0.8f);
                float thickness = Mathf.Lerp(3f, 1f, (float)i / 50f);
                for (int t = 0; t < (int)thickness; t++)
                {
                    SetPixel(px, SIZE, x, y + t, rodColor);
                    SetPixel(px, SIZE, x, y - t, rodColor);
                }
            }

            // Handle (thicker bottom)
            for (int i = 0; i < 12; i++)
            {
                int x = 8 + i;
                int y = 8 + Mathf.RoundToInt(i * 0.8f);
                for (int t = -2; t <= 2; t++)
                    SetPixel(px, SIZE, x, y + t, handleColor);
            }

            // Reel seat
            FillRect(px, SIZE, 14, 14, 20, 22, guideColor);

            // Line guides (small dots along rod)
            for (int g = 0; g < 4; g++)
            {
                int gx = 22 + g * 9;
                int gy = 16 + Mathf.RoundToInt(g * 7.2f);
                DrawCircleFilled(px, SIZE, gx, gy, 1, guideColor);
            }

            // Tier indicator star
            if (tier == RodTier.Legendary)
                DrawStar(px, SIZE, 50, 50, 6, new Color(1f, 0.85f, 0f));
            else if (tier == RodTier.High)
                DrawStar(px, SIZE, 50, 50, 4, new Color(0.6f, 0.2f, 0.9f));

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), Vector2.one * 0.5f, 100f);
            sprite.name = key;
            cache[key] = sprite;
            return sprite;
        }

        // --- REEL ICONS ---

        public static Sprite GetReelIcon(string reelName, RodTier tier)
        {
            string key = $"reel_{reelName}";
            if (cache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] px = new Color[SIZE * SIZE];
            ClearPixels(px);

            Color metalColor = GetTierColor(tier);
            Color spoolColor = new Color(0.5f, 0.5f, 0.55f);
            Color handleColor = new Color(0.25f, 0.25f, 0.3f);

            // Reel body (circle)
            DrawCircleFilled(px, SIZE, 32, 32, 16, metalColor);
            DrawCircleFilled(px, SIZE, 32, 32, 12, spoolColor);
            DrawCircleFilled(px, SIZE, 32, 32, 4, handleColor);

            // Handle arm
            for (int i = 0; i < 14; i++)
                SetPixel(px, SIZE, 32 + i, 32 + Mathf.RoundToInt(Mathf.Sin(i * 0.2f) * 3), handleColor);
            DrawCircleFilled(px, SIZE, 46, 33, 3, metalColor);

            // Spool lines
            for (int r = 6; r < 12; r++)
            {
                float a = r * 0.7f;
                int lx = 32 + Mathf.RoundToInt(Mathf.Cos(a) * r);
                int ly = 32 + Mathf.RoundToInt(Mathf.Sin(a) * r);
                SetPixel(px, SIZE, lx, ly, new Color(0.7f, 0.7f, 0.7f));
            }

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), Vector2.one * 0.5f, 100f);
            sprite.name = key;
            cache[key] = sprite;
            return sprite;
        }

        // --- BADGE ICONS ---

        public static Sprite GetBadge(string badgeType)
        {
            string key = $"badge_{badgeType}";
            if (cache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] px = new Color[SIZE * SIZE];
            ClearPixels(px);

            switch (badgeType)
            {
                case "firstCatch":
                    DrawCircleFilled(px, SIZE, 32, 32, 24, new Color(0.2f, 0.7f, 0.3f));
                    DrawStar(px, SIZE, 32, 32, 12, Color.white);
                    break;
                case "newRecord":
                    DrawCircleFilled(px, SIZE, 32, 32, 24, new Color(0.9f, 0.2f, 0.2f));
                    // Trophy shape
                    FillRect(px, SIZE, 24, 16, 40, 36, new Color(1f, 0.85f, 0f));
                    FillRect(px, SIZE, 28, 36, 36, 42, new Color(0.8f, 0.65f, 0f));
                    FillRect(px, SIZE, 26, 42, 38, 46, new Color(1f, 0.85f, 0f));
                    break;
                case "legendary":
                    DrawCircleFilled(px, SIZE, 32, 32, 24, new Color(0.15f, 0.1f, 0.3f));
                    DrawStar(px, SIZE, 32, 32, 16, new Color(1f, 0.85f, 0f));
                    DrawStar(px, SIZE, 32, 32, 10, new Color(1f, 0.95f, 0.5f));
                    break;
            }

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), Vector2.one * 0.5f, 100f);
            sprite.name = key;
            cache[key] = sprite;
            return sprite;
        }

        // --- ACCESSORY ICONS ---

        public static Sprite GetAccessoryIcon(string accessoryId)
        {
            string key = $"acc_{accessoryId}";
            if (cache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] px = new Color[SIZE * SIZE];
            ClearPixels(px);

            switch (accessoryId)
            {
                case "specialGloves":
                    // Glove shape
                    FillRoundRect(px, SIZE, 16, 12, 48, 48, new Color(0.6f, 0.4f, 0.15f));
                    FillRect(px, SIZE, 20, 48, 28, 56, new Color(0.6f, 0.4f, 0.15f)); // thumb
                    break;
                case "barbelProofStand":
                    // V-shape stand
                    for (int i = 0; i < 20; i++)
                    {
                        SetPixel(px, SIZE, 22 + i, 50 - i, new Color(0.4f, 0.4f, 0.45f));
                        SetPixel(px, SIZE, 42 - i, 50 - i, new Color(0.4f, 0.4f, 0.45f));
                    }
                    break;
                case "slimeResistantGrip":
                    FillRoundRect(px, SIZE, 20, 16, 44, 48, new Color(0.2f, 0.2f, 0.25f));
                    // Grip texture
                    for (int y = 18; y < 46; y += 4)
                        FillRect(px, SIZE, 22, y, 42, y + 2, new Color(0.35f, 0.35f, 0.4f));
                    break;
                case "betterPapBucket":
                    // Bucket shape
                    FillRect(px, SIZE, 18, 20, 46, 48, new Color(0.6f, 0.6f, 0.65f));
                    FillRect(px, SIZE, 20, 22, 44, 46, new Color(0.9f, 0.85f, 0.7f)); // pap inside
                    // Handle
                    for (int i = 0; i < 20; i++)
                    {
                        int hy = 16 - Mathf.RoundToInt(Mathf.Sin(i / 20f * Mathf.PI) * 8);
                        SetPixel(px, SIZE, 22 + i, hy, new Color(0.4f, 0.4f, 0.45f));
                    }
                    break;
            }

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), Vector2.one * 0.5f, 100f);
            sprite.name = key;
            cache[key] = sprite;
            return sprite;
        }

        // --- HELPERS ---

        private static Color GetTierColor(RodTier tier)
        {
            switch (tier)
            {
                case RodTier.Entry: return new Color(0.5f, 0.5f, 0.5f);
                case RodTier.Mid: return new Color(0.3f, 0.5f, 0.7f);
                case RodTier.High: return new Color(0.6f, 0.2f, 0.8f);
                case RodTier.Legendary: return new Color(1f, 0.8f, 0.1f);
                default: return Color.gray;
            }
        }

        private static void ClearPixels(Color[] px)
        {
            for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        }

        private static void SetPixel(Color[] px, int w, int x, int y, Color c)
        {
            if (x < 0 || x >= w || y < 0 || y >= px.Length / w) return;
            px[y * w + x] = c;
        }

        private static void FillRect(Color[] px, int w, int x0, int y0, int x1, int y1, Color c)
        {
            for (int x = x0; x <= x1; x++)
                for (int y = y0; y <= y1; y++)
                    SetPixel(px, w, x, y, c);
        }

        private static void FillRoundRect(Color[] px, int w, int x0, int y0, int x1, int y1, Color c)
        {
            int r = 3;
            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    // Skip corners
                    bool inCorner = (x < x0 + r && y < y0 + r) || (x > x1 - r && y < y0 + r) ||
                                   (x < x0 + r && y > y1 - r) || (x > x1 - r && y > y1 - r);
                    int cx = x < x0 + r ? x0 + r : (x > x1 - r ? x1 - r : x);
                    int cy = y < y0 + r ? y0 + r : (y > y1 - r ? y1 - r : y);

                    if (!inCorner || (x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                        SetPixel(px, w, x, y, c);
                }
            }
        }

        private static void DrawCircleFilled(Color[] px, int w, int cx, int cy, int radius, Color c)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
                for (int y = cy - radius; y <= cy + radius; y++)
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius)
                        SetPixel(px, w, x, y, c);
        }

        private static void DrawStar(Color[] px, int w, int cx, int cy, int size, Color c)
        {
            // Simple 4-point star
            for (int i = -size; i <= size; i++)
            {
                int thickness = Mathf.Max(1, size / 3 - Mathf.Abs(i) / 2);
                for (int t = -thickness; t <= thickness; t++)
                {
                    SetPixel(px, w, cx + i, cy + t, c);
                    SetPixel(px, w, cx + t, cy + i, c);
                }
            }
            // Diagonal arms
            for (int i = -size / 2; i <= size / 2; i++)
            {
                SetPixel(px, w, cx + i, cy + i, c);
                SetPixel(px, w, cx + i, cy - i, c);
            }
        }
    }
}
