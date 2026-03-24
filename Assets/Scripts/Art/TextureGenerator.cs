using UnityEngine;

namespace Visvang.Art
{
    /// <summary>
    /// Generates environment textures: water, grass, dirt, bark, sand.
    /// Also provides particle system creation for splash, sparkle effects.
    /// </summary>
    public static class TextureGenerator
    {
        // --- WATER ---

        public static Material CreateWaterMaterial()
        {
            Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;

            Color[] pixels = new Color[256 * 256];
            Color deepBlue = new Color(0.08f, 0.22f, 0.35f);
            Color midBlue = new Color(0.12f, 0.32f, 0.45f);
            Color lightBlue = new Color(0.18f, 0.4f, 0.52f);

            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    float noise1 = Mathf.PerlinNoise(x * 0.02f, y * 0.02f);
                    float noise2 = Mathf.PerlinNoise(x * 0.05f + 100f, y * 0.05f + 100f);
                    float combined = noise1 * 0.6f + noise2 * 0.4f;

                    Color c = Color.Lerp(deepBlue, midBlue, combined);

                    // Ripple highlights
                    float ripple = Mathf.Sin(x * 0.15f + y * 0.1f) * Mathf.Sin(y * 0.12f);
                    if (ripple > 0.6f)
                        c = Color.Lerp(c, lightBlue, (ripple - 0.6f) * 2f);

                    // Caustic pattern
                    float caustic = Mathf.PerlinNoise(x * 0.08f + 50f, y * 0.08f + 50f);
                    if (caustic > 0.65f)
                        c = Color.Lerp(c, new Color(0.2f, 0.45f, 0.55f), (caustic - 0.65f) * 1.5f);

                    c.a = 0.85f;
                    pixels[y * 256 + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            Material mat = new Material(Visvang.Core.ShaderHelper.GetOpaque());
            mat.mainTexture = tex;
            mat.SetFloat("_Metallic", 0.3f);
            mat.SetFloat("_Glossiness", 0.85f);
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            mat.name = "WaterMaterial";
            return mat;
        }

        // --- GRASS ---

        public static Material CreateGrassMaterial()
        {
            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;

            Color[] pixels = new Color[128 * 128];
            Color baseGreen = new Color(0.18f, 0.35f, 0.12f);
            Color lightGreen = new Color(0.25f, 0.45f, 0.15f);
            Color darkGreen = new Color(0.12f, 0.25f, 0.08f);

            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.06f, y * 0.06f);
                    Color c = Color.Lerp(darkGreen, lightGreen, noise);

                    // Grass blade pattern
                    float blade = Mathf.PerlinNoise(x * 0.3f, y * 0.15f);
                    if (blade > 0.55f)
                        c = Color.Lerp(c, lightGreen, (blade - 0.55f) * 2f);

                    // Dirt patches
                    float dirt = Mathf.PerlinNoise(x * 0.03f + 200f, y * 0.03f + 200f);
                    if (dirt > 0.7f)
                        c = Color.Lerp(c, new Color(0.3f, 0.22f, 0.12f), (dirt - 0.7f) * 2f);

                    c.a = 1f;
                    pixels[y * 128 + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            Material mat = new Material(Visvang.Core.ShaderHelper.GetOpaque());
            mat.mainTexture = tex;
            mat.name = "GrassMaterial";
            return mat;
        }

        // --- DIRT / BANK ---

        public static Material CreateDirtMaterial()
        {
            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;

            Color[] pixels = new Color[128 * 128];
            Color baseDirt = new Color(0.35f, 0.25f, 0.15f);
            Color lightDirt = new Color(0.45f, 0.35f, 0.2f);
            Color darkDirt = new Color(0.25f, 0.18f, 0.1f);

            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                    Color c = Color.Lerp(darkDirt, lightDirt, noise);

                    // Pebble spots
                    float pebble = Mathf.PerlinNoise(x * 0.2f + 300f, y * 0.2f + 300f);
                    if (pebble > 0.7f)
                        c = Color.Lerp(c, new Color(0.5f, 0.45f, 0.38f), 0.4f);

                    c.a = 1f;
                    pixels[y * 128 + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            Material mat = new Material(Visvang.Core.ShaderHelper.GetOpaque());
            mat.mainTexture = tex;
            mat.name = "DirtMaterial";
            return mat;
        }

        // --- PARTICLE SYSTEMS ---

        public static GameObject CreateSplashEffect(Vector3 position)
        {
            var go = new GameObject("SplashEffect");
            go.transform.position = position;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.8f;
            main.startSpeed = 4f;
            main.startSize = 0.15f;
            main.startColor = new Color(0.4f, 0.6f, 0.8f, 0.7f);
            main.maxParticles = 30;
            main.duration = 0.3f;
            main.loop = false;
            main.gravityModifier = 2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 20));

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.3f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.5f, 0.7f, 0.9f), 0f),
                    new GradientColorKey(new Color(0.3f, 0.5f, 0.7f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.8f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = grad;

            // Use default particle material
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Visvang.Core.ShaderHelper.GetParticle());
            renderer.material.color = new Color(0.5f, 0.7f, 0.9f, 0.7f);

            return go;
        }

        public static GameObject CreateSparkleEffect(Vector3 position)
        {
            var go = new GameObject("SparkleEffect");
            go.transform.position = position;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 1f;
            main.startSize = 0.1f;
            main.startColor = new Color(1f, 0.9f, 0.4f, 0.9f);
            main.maxParticles = 50;
            main.duration = 2f;
            main.loop = false;
            main.gravityModifier = -0.3f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 25;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.EaseInOut(0, 0.5f, 0.5f, 1f));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.95f, 0.5f), 0f),
                    new GradientColorKey(new Color(1f, 0.8f, 0.2f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.2f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Visvang.Core.ShaderHelper.GetParticle());
            renderer.material.color = new Color(1f, 0.95f, 0.5f);

            return go;
        }

        public static GameObject CreateSlimeEffect(Vector3 position)
        {
            var go = new GameObject("SlimeEffect");
            go.transform.position = position;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 2f;
            main.startSpeed = 0.5f;
            main.startSize = 0.2f;
            main.startColor = new Color(0.3f, 0.6f, 0.1f, 0.8f);
            main.maxParticles = 15;
            main.duration = 1f;
            main.loop = false;
            main.gravityModifier = 1.5f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 10));

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;
            shape.radius = 0.2f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Visvang.Core.ShaderHelper.GetParticle());
            renderer.material.color = new Color(0.3f, 0.6f, 0.1f, 0.8f);

            return go;
        }
    }
}
