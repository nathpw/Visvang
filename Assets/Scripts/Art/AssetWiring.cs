using UnityEngine;
using Visvang.Core;
using Visvang.Fish;
using Visvang.Bait;
using Visvang.Equipment;

namespace Visvang.Art
{
    /// <summary>
    /// Wires all generated art assets into the game systems.
    /// Call WireAll() after RuntimeDataLoader has loaded all data.
    /// Assigns fish sprites, dip icons, rod icons to ScriptableObject instances.
    /// Applies textures to environment objects. Creates particle effects.
    /// </summary>
    public static class AssetWiring
    {
        public static void WireAll()
        {
            WireFishSprites();
            WireDipIcons();
            WireRodIcons();
            ApplyEnvironmentTextures();
            CreateEnvironmentEffects();

            Debug.Log("[Visvang] All art assets generated and wired.");
        }

        private static void WireFishSprites()
        {
            if (FishDatabase.Instance == null) return;

            // Wire sprites to all fish species
            var allSpecies = System.Enum.GetValues(typeof(FishSpecies));
            foreach (FishSpecies species in allSpecies)
            {
                var fishData = FishDatabase.Instance.GetFishBySpecies(species);
                if (fishData != null)
                {
                    fishData.fishSprite = FishSpriteGenerator.GetSprite(species);
                    fishData.fishPhoto = FishSpriteGenerator.GetPhotoSprite(species);
                }
            }

            Debug.Log($"[AssetWiring] Fish sprites wired for {allSpecies.Length} species");
        }

        private static void WireDipIcons()
        {
            if (BaitManager.Instance == null) return;

            foreach (var dip in BaitManager.Instance.AllDips)
            {
                dip.dipIcon = IconGenerator.GetDipIcon(dip.dipName, dip.dipColor);
            }

            Debug.Log($"[AssetWiring] Dip icons wired for {BaitManager.Instance.AllDips.Count} dips");
        }

        private static void WireRodIcons()
        {
            if (EquipmentManager.Instance == null) return;

            foreach (var rod in EquipmentManager.Instance.GetOwnedRods())
            {
                rod.rodIcon = IconGenerator.GetRodIcon(rod.rodName, rod.tier);
            }

            foreach (var reel in EquipmentManager.Instance.GetOwnedReels())
            {
                reel.reelIcon = IconGenerator.GetReelIcon(reel.reelName, reel.tier);
            }
        }

        private static void ApplyEnvironmentTextures()
        {
            // Water
            var water = GameObject.Find("WaterSurface");
            if (water != null)
            {
                var renderer = water.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material = TextureGenerator.CreateWaterMaterial();
            }

            // Bank
            var bank = GameObject.Find("Bank");
            if (bank != null)
            {
                var renderer = bank.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material = TextureGenerator.CreateDirtMaterial();
            }

            // Bank slope
            var slope = GameObject.Find("BankSlope");
            if (slope != null)
            {
                var renderer = slope.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material = TextureGenerator.CreateDirtMaterial();
            }

            // Far bank
            var farBank = GameObject.Find("FarBank");
            if (farBank != null)
            {
                var renderer = farBank.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material = TextureGenerator.CreateGrassMaterial();
            }
        }

        private static void CreateEnvironmentEffects()
        {
            // Pre-create a splash effect pool at the water surface for cast splashes
            var splashPool = new GameObject("SplashPool");
            splashPool.transform.position = new Vector3(0, 0.1f, 10f);

            // Ambient water ripple particles
            CreateAmbientRipples();
        }

        private static void CreateAmbientRipples()
        {
            var go = new GameObject("AmbientRipples");
            go.transform.position = new Vector3(0, 0.15f, 30f);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 3f;
            main.startSpeed = 0f;
            main.startSize = 0.5f;
            main.startColor = new Color(0.3f, 0.5f, 0.65f, 0.15f);
            main.maxParticles = 20;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 3f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(40f, 0.1f, 30f);

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0, 0.3f, 1, 2f));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.4f, 0.6f, 0.8f), 0f),
                    new GradientColorKey(new Color(0.3f, 0.5f, 0.7f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.15f, 0.3f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = new Color(0.4f, 0.6f, 0.8f, 0.15f);
            renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
        }

        /// <summary>
        /// Spawn a splash effect at a world position (call on cast landing).
        /// </summary>
        public static void PlaySplashAt(Vector3 position)
        {
            var splash = TextureGenerator.CreateSplashEffect(position);
            Object.Destroy(splash, 2f);
        }

        /// <summary>
        /// Spawn sparkle effect (legendary fish catch).
        /// </summary>
        public static void PlaySparkleAt(Vector3 position)
        {
            var sparkle = TextureGenerator.CreateSparkleEffect(position);
            Object.Destroy(sparkle, 4f);
        }

        /// <summary>
        /// Spawn slime effect (mudfish catch).
        /// </summary>
        public static void PlaySlimeAt(Vector3 position)
        {
            var slime = TextureGenerator.CreateSlimeEffect(position);
            Object.Destroy(slime, 3f);
        }
    }
}
