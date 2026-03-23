using UnityEngine;
using System.Collections.Generic;
using Visvang.Core;

namespace Visvang.Art
{
    /// <summary>
    /// Generates pixel-art style fish sprites procedurally using Texture2D.
    /// Each species has a unique silhouette, color palette, and distinguishing features.
    /// Sprites are 128x64 pixels with transparency.
    /// </summary>
    public static class FishSpriteGenerator
    {
        private const int WIDTH = 128;
        private const int HEIGHT = 64;

        private static Dictionary<FishSpecies, Sprite> cache = new Dictionary<FishSpecies, Sprite>();

        public static Sprite GetSprite(FishSpecies species)
        {
            if (cache.TryGetValue(species, out Sprite cached))
                return cached;

            Texture2D tex = GenerateFishTexture(species);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, WIDTH, HEIGHT), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = $"Fish_{species}";
            cache[species] = sprite;
            return sprite;
        }

        public static Sprite GetPhotoSprite(FishSpecies species)
        {
            // Larger version for catch screen
            int photoW = 256, photoH = 128;
            Texture2D tex = GenerateFishTexture(species, photoW, photoH);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, photoW, photoH), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = $"FishPhoto_{species}";
            return sprite;
        }

        private static Texture2D GenerateFishTexture(FishSpecies species, int w = WIDTH, int h = HEIGHT)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point; // Pixel art look
            Color[] pixels = new Color[w * h];

            // Clear to transparent
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;

            FishPalette palette = GetPalette(species);
            DrawFishBody(pixels, w, h, palette, species);
            DrawFishFeatures(pixels, w, h, palette, species);

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static void DrawFishBody(Color[] pixels, int w, int h, FishPalette p, FishSpecies species)
        {
            float cx = w * 0.45f;
            float cy = h * 0.5f;
            float bodyW = w * GetBodyWidthRatio(species);
            float bodyH = h * GetBodyHeightRatio(species);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float dx = (x - cx) / bodyW;
                    float dy = (y - cy) / bodyH;

                    // Ellipse body
                    float dist = dx * dx + dy * dy;
                    if (dist < 1f)
                    {
                        // Body color with shading
                        float shade = 1f - dy * 0.3f; // Lighter on top
                        Color bodyColor = Color.Lerp(p.belly, p.body, (dy + 1f) * 0.5f) * shade;
                        bodyColor.a = 1f;

                        // Scale pattern
                        if (HasScales(species))
                        {
                            float scalePattern = Mathf.Sin(x * 0.8f) * Mathf.Sin(y * 0.8f);
                            if (scalePattern > 0.3f)
                                bodyColor = Color.Lerp(bodyColor, p.scales, 0.2f);
                        }

                        // Spots for mirror carp
                        if (species == FishSpecies.MirrorCarp)
                        {
                            float spotPattern = Mathf.PerlinNoise(x * 0.15f, y * 0.15f);
                            if (spotPattern > 0.65f)
                                bodyColor = Color.Lerp(bodyColor, p.accent, 0.4f);
                        }

                        SetPixel(pixels, w, x, y, bodyColor);
                    }
                }
            }

            // Tail fin
            DrawTail(pixels, w, h, cx + bodyW * 0.85f, cy, bodyH * 0.6f, p, species);

            // Dorsal fin (top)
            DrawDorsalFin(pixels, w, h, cx - bodyW * 0.1f, cy - bodyH * 0.8f, bodyW * 0.4f, bodyH * 0.35f, p);

            // Pectoral fin (side)
            DrawPectoralFin(pixels, w, h, cx - bodyW * 0.2f, cy + bodyH * 0.3f, bodyW * 0.2f, bodyH * 0.3f, p);

            // Outline
            DrawOutline(pixels, w, h, p.outline);
        }

        private static void DrawFishFeatures(Color[] pixels, int w, int h, FishPalette p, FishSpecies species)
        {
            float cx = w * 0.45f;
            float cy = h * 0.5f;
            float bodyW = w * GetBodyWidthRatio(species);

            // Eye
            int eyeX = Mathf.RoundToInt(cx - bodyW * 0.6f);
            int eyeY = Mathf.RoundToInt(cy - h * 0.05f);
            DrawCircle(pixels, w, h, eyeX, eyeY, 3, Color.white);
            DrawCircle(pixels, w, h, eyeX, eyeY, 1, Color.black);

            // Mouth
            int mouthX = Mathf.RoundToInt(cx - bodyW * 0.85f);
            int mouthY = Mathf.RoundToInt(cy + 1);
            SetPixel(pixels, w, mouthX, mouthY, p.outline);
            SetPixel(pixels, w, mouthX + 1, mouthY + 1, p.outline);

            // Species-specific features
            switch (species)
            {
                case FishSpecies.Barbel:
                case FishSpecies.FlatNoseRiverBarber:
                    // Whiskers (barbels)
                    DrawLine(pixels, w, h, mouthX, mouthY, mouthX - 12, mouthY + 6, p.outline);
                    DrawLine(pixels, w, h, mouthX, mouthY - 1, mouthX - 10, mouthY - 5, p.outline);
                    // Wider, flatter head for flat-nose
                    if (species == FishSpecies.FlatNoseRiverBarber)
                    {
                        for (int dx = -2; dx <= 2; dx++)
                            SetPixel(pixels, w, mouthX + dx, mouthY - 3, p.outline);
                    }
                    break;

                case FishSpecies.Mudfish:
                    // Slime drips
                    for (int i = 0; i < 5; i++)
                    {
                        int sx = Mathf.RoundToInt(cx - bodyW * 0.3f + i * bodyW * 0.15f);
                        int sy = Mathf.RoundToInt(cy + h * GetBodyHeightRatio(species) * 0.8f);
                        DrawLine(pixels, w, h, sx, sy, sx + Random.Range(-1, 2), sy + Random.Range(3, 7), p.accent);
                    }
                    break;

                case FishSpecies.BoknesGoldenCarp:
                    // Golden glow/sparkles
                    for (int i = 0; i < 12; i++)
                    {
                        int sx = Mathf.RoundToInt(cx + Random.Range(-bodyW * 0.6f, bodyW * 0.6f));
                        int sy = Mathf.RoundToInt(cy + Random.Range(-h * 0.25f, h * 0.25f));
                        Color sparkle = new Color(1f, 0.95f, 0.5f, 0.8f);
                        SetPixel(pixels, w, sx, sy, sparkle);
                        SetPixel(pixels, w, sx + 1, sy, sparkle * 0.7f);
                        SetPixel(pixels, w, sx, sy + 1, sparkle * 0.7f);
                    }
                    break;

                case FishSpecies.Eel:
                    // No extra features - the body shape handles it
                    break;

                case FishSpecies.Yellowfish:
                    // Yellow lateral line
                    for (int lx = Mathf.RoundToInt(cx - bodyW * 0.5f); lx < Mathf.RoundToInt(cx + bodyW * 0.7f); lx++)
                    {
                        SetPixel(pixels, w, lx, Mathf.RoundToInt(cy), p.accent);
                        SetPixel(pixels, w, lx, Mathf.RoundToInt(cy) + 1, p.accent * 0.8f);
                    }
                    break;

                case FishSpecies.LeatherCarp:
                    // Smooth - no scales, just slight shine
                    int shineY = Mathf.RoundToInt(cy - h * 0.1f);
                    for (int lx = Mathf.RoundToInt(cx - bodyW * 0.3f); lx < Mathf.RoundToInt(cx + bodyW * 0.3f); lx++)
                    {
                        Color shine = Color.Lerp(GetPixel(pixels, w, lx, shineY), Color.white, 0.2f);
                        SetPixel(pixels, w, lx, shineY, shine);
                    }
                    break;
            }
        }

        // --- Drawing Helpers ---

        private static void DrawTail(Color[] pixels, int w, int h, float tx, float cy, float tailH, FishPalette p, FishSpecies species)
        {
            int tailWidth = species == FishSpecies.Eel ? 4 : 12;
            for (int x = 0; x < tailWidth; x++)
            {
                float spread = (float)x / tailWidth;
                int topY = Mathf.RoundToInt(cy - tailH * spread);
                int botY = Mathf.RoundToInt(cy + tailH * spread);

                for (int y = topY; y <= botY; y++)
                {
                    Color c = Color.Lerp(p.fin, p.body, 0.3f);
                    c.a = 1f;
                    SetPixel(pixels, w, Mathf.RoundToInt(tx) + x, y, c);
                }
            }
        }

        private static void DrawDorsalFin(Color[] pixels, int w, int h, float fx, float fy, float finW, float finH, FishPalette p)
        {
            for (int x = 0; x < (int)finW; x++)
            {
                float t = (float)x / finW;
                int finHeight = Mathf.RoundToInt(finH * Mathf.Sin(t * Mathf.PI));
                for (int y = 0; y < finHeight; y++)
                {
                    Color c = Color.Lerp(p.fin, p.body, (float)y / finHeight * 0.5f);
                    c.a = 0.9f;
                    SetPixel(pixels, w, Mathf.RoundToInt(fx) + x, Mathf.RoundToInt(fy) - y, c);
                }
            }
        }

        private static void DrawPectoralFin(Color[] pixels, int w, int h, float fx, float fy, float finW, float finH, FishPalette p)
        {
            for (int x = 0; x < (int)finW; x++)
            {
                float t = (float)x / finW;
                int finHeight = Mathf.RoundToInt(finH * (1f - t));
                for (int y = 0; y < finHeight; y++)
                {
                    Color c = p.fin;
                    c.a = 0.8f;
                    SetPixel(pixels, w, Mathf.RoundToInt(fx) - x, Mathf.RoundToInt(fy) + y, c);
                }
            }
        }

        private static void DrawOutline(Color[] pixels, int w, int h, Color outlineColor)
        {
            Color[] copy = (Color[])pixels.Clone();
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    if (copy[y * w + x].a < 0.1f)
                    {
                        // Check neighbors for opaque pixel
                        bool hasNeighbor = copy[(y - 1) * w + x].a > 0.5f ||
                                          copy[(y + 1) * w + x].a > 0.5f ||
                                          copy[y * w + x - 1].a > 0.5f ||
                                          copy[y * w + x + 1].a > 0.5f;
                        if (hasNeighbor)
                        {
                            outlineColor.a = 1f;
                            pixels[y * w + x] = outlineColor;
                        }
                    }
                }
            }
        }

        private static void DrawCircle(Color[] pixels, int w, int h, int cx, int cy, int radius, Color color)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                for (int y = cy - radius; y <= cy + radius; y++)
                {
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius)
                        SetPixel(pixels, w, x, y, color);
                }
            }
        }

        private static void DrawLine(Color[] pixels, int w, int h, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            while (true)
            {
                SetPixel(pixels, w, x0, y0, color);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        private static void SetPixel(Color[] pixels, int w, int x, int y, Color color)
        {
            if (x < 0 || x >= w || y < 0 || y >= pixels.Length / w) return;
            pixels[y * w + x] = color;
        }

        private static Color GetPixel(Color[] pixels, int w, int x, int y)
        {
            if (x < 0 || x >= w || y < 0 || y >= pixels.Length / w) return Color.clear;
            return pixels[y * w + x];
        }

        // --- Species Properties ---

        private static float GetBodyWidthRatio(FishSpecies s)
        {
            switch (s)
            {
                case FishSpecies.Eel: return 0.55f;
                case FishSpecies.Barbel:
                case FishSpecies.FlatNoseRiverBarber: return 0.42f;
                case FishSpecies.Graskarp: return 0.43f;
                case FishSpecies.Kurper: return 0.3f;
                case FishSpecies.WildSmallCarp: return 0.32f;
                default: return 0.38f;
            }
        }

        private static float GetBodyHeightRatio(FishSpecies s)
        {
            switch (s)
            {
                case FishSpecies.Eel: return 0.15f;
                case FishSpecies.Barbel:
                case FishSpecies.FlatNoseRiverBarber: return 0.3f;
                case FishSpecies.Mudfish: return 0.25f;
                case FishSpecies.Kurper: return 0.4f;
                case FishSpecies.Tilapia: return 0.38f;
                default: return 0.35f;
            }
        }

        private static bool HasScales(FishSpecies s)
        {
            return s != FishSpecies.LeatherCarp && s != FishSpecies.Eel && s != FishSpecies.Mudfish
                && s != FishSpecies.Barbel && s != FishSpecies.FlatNoseRiverBarber;
        }

        // --- Color Palettes ---

        private static FishPalette GetPalette(FishSpecies species)
        {
            switch (species)
            {
                case FishSpecies.CommonCarp:
                    return new FishPalette(
                        new Color(0.55f, 0.45f, 0.25f), new Color(0.75f, 0.65f, 0.35f),
                        new Color(0.85f, 0.8f, 0.6f), new Color(0.6f, 0.5f, 0.3f),
                        new Color(0.45f, 0.35f, 0.2f), new Color(0.3f, 0.25f, 0.15f));

                case FishSpecies.MirrorCarp:
                    return new FishPalette(
                        new Color(0.5f, 0.4f, 0.2f), new Color(0.7f, 0.55f, 0.3f),
                        new Color(0.85f, 0.75f, 0.5f), new Color(0.8f, 0.7f, 0.4f),
                        new Color(0.5f, 0.4f, 0.25f), new Color(0.3f, 0.2f, 0.1f));

                case FishSpecies.LeatherCarp:
                    return new FishPalette(
                        new Color(0.4f, 0.35f, 0.25f), new Color(0.55f, 0.45f, 0.3f),
                        new Color(0.7f, 0.65f, 0.5f), new Color(0.5f, 0.4f, 0.3f),
                        new Color(0.4f, 0.3f, 0.2f), new Color(0.25f, 0.2f, 0.1f));

                case FishSpecies.GhostCarp:
                    return new FishPalette(
                        new Color(0.85f, 0.8f, 0.7f), new Color(0.92f, 0.88f, 0.8f),
                        new Color(0.95f, 0.93f, 0.88f), new Color(0.9f, 0.85f, 0.75f),
                        new Color(0.7f, 0.65f, 0.55f), new Color(0.4f, 0.38f, 0.32f));

                case FishSpecies.WildSmallCarp:
                    return new FishPalette(
                        new Color(0.45f, 0.4f, 0.2f), new Color(0.6f, 0.5f, 0.25f),
                        new Color(0.75f, 0.7f, 0.5f), new Color(0.55f, 0.45f, 0.25f),
                        new Color(0.4f, 0.35f, 0.2f), new Color(0.25f, 0.2f, 0.1f));

                case FishSpecies.BoknesGoldenCarp:
                    return new FishPalette(
                        new Color(0.95f, 0.75f, 0.1f), new Color(1f, 0.85f, 0.2f),
                        new Color(1f, 0.95f, 0.6f), new Color(0.9f, 0.7f, 0.15f),
                        new Color(0.85f, 0.6f, 0.1f), new Color(0.5f, 0.35f, 0.05f));

                case FishSpecies.Barbel:
                    return new FishPalette(
                        new Color(0.3f, 0.28f, 0.25f), new Color(0.45f, 0.4f, 0.35f),
                        new Color(0.65f, 0.6f, 0.5f), new Color(0.35f, 0.3f, 0.25f),
                        new Color(0.4f, 0.35f, 0.3f), new Color(0.15f, 0.12f, 0.1f));

                case FishSpecies.FlatNoseRiverBarber:
                    return new FishPalette(
                        new Color(0.25f, 0.22f, 0.2f), new Color(0.35f, 0.3f, 0.25f),
                        new Color(0.5f, 0.45f, 0.35f), new Color(0.3f, 0.25f, 0.2f),
                        new Color(0.35f, 0.3f, 0.25f), new Color(0.1f, 0.08f, 0.06f));

                case FishSpecies.Mudfish:
                    return new FishPalette(
                        new Color(0.35f, 0.3f, 0.15f), new Color(0.45f, 0.4f, 0.2f),
                        new Color(0.6f, 0.55f, 0.35f), new Color(0.5f, 0.55f, 0.2f),
                        new Color(0.35f, 0.3f, 0.15f), new Color(0.2f, 0.18f, 0.08f));

                case FishSpecies.Kurper:
                    return new FishPalette(
                        new Color(0.3f, 0.4f, 0.3f), new Color(0.4f, 0.55f, 0.35f),
                        new Color(0.6f, 0.7f, 0.5f), new Color(0.35f, 0.45f, 0.3f),
                        new Color(0.3f, 0.4f, 0.25f), new Color(0.15f, 0.2f, 0.1f));

                case FishSpecies.Tilapia:
                    return new FishPalette(
                        new Color(0.4f, 0.45f, 0.35f), new Color(0.55f, 0.6f, 0.45f),
                        new Color(0.7f, 0.75f, 0.6f), new Color(0.45f, 0.5f, 0.35f),
                        new Color(0.35f, 0.4f, 0.3f), new Color(0.2f, 0.22f, 0.15f));

                case FishSpecies.Bass:
                    return new FishPalette(
                        new Color(0.2f, 0.35f, 0.15f), new Color(0.3f, 0.5f, 0.2f),
                        new Color(0.7f, 0.75f, 0.55f), new Color(0.25f, 0.4f, 0.15f),
                        new Color(0.25f, 0.35f, 0.15f), new Color(0.1f, 0.15f, 0.05f));

                case FishSpecies.Graskarp:
                    return new FishPalette(
                        new Color(0.35f, 0.4f, 0.25f), new Color(0.5f, 0.55f, 0.35f),
                        new Color(0.7f, 0.72f, 0.55f), new Color(0.4f, 0.45f, 0.3f),
                        new Color(0.35f, 0.4f, 0.25f), new Color(0.18f, 0.2f, 0.1f));

                case FishSpecies.Yellowfish:
                    return new FishPalette(
                        new Color(0.5f, 0.45f, 0.2f), new Color(0.7f, 0.65f, 0.25f),
                        new Color(0.85f, 0.8f, 0.5f), new Color(0.9f, 0.8f, 0.2f),
                        new Color(0.55f, 0.5f, 0.2f), new Color(0.3f, 0.25f, 0.1f));

                case FishSpecies.Eel:
                    return new FishPalette(
                        new Color(0.2f, 0.2f, 0.15f), new Color(0.3f, 0.28f, 0.2f),
                        new Color(0.45f, 0.4f, 0.3f), new Color(0.35f, 0.3f, 0.2f),
                        new Color(0.25f, 0.22f, 0.15f), new Color(0.1f, 0.08f, 0.05f));

                default:
                    return new FishPalette(
                        new Color(0.5f, 0.5f, 0.5f), new Color(0.7f, 0.7f, 0.7f),
                        new Color(0.85f, 0.85f, 0.85f), new Color(0.6f, 0.6f, 0.6f),
                        new Color(0.4f, 0.4f, 0.4f), new Color(0.2f, 0.2f, 0.2f));
            }
        }

        private struct FishPalette
        {
            public Color body;
            public Color scales;
            public Color belly;
            public Color accent;
            public Color fin;
            public Color outline;

            public FishPalette(Color body, Color scales, Color belly, Color accent, Color fin, Color outline)
            {
                this.body = body;
                this.scales = scales;
                this.belly = belly;
                this.accent = accent;
                this.fin = fin;
                this.outline = outline;
            }
        }
    }
}
