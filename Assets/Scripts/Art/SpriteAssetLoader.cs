using UnityEngine;
using System.Collections.Generic;
using Visvang.Core;
using Visvang.Fish;
using Visvang.Bait;
using Visvang.Equipment;

namespace Visvang.Art
{
    /// <summary>
    /// Scans Resources/Sprites/ for real art assets and assigns them to game data.
    /// If real sprites are found, they override the procedurally generated ones.
    ///
    /// File naming convention (place in Assets/Resources/Sprites/Fish/):
    ///   common_carp.png, mirror_carp.png, barbel.png, mudfish.png, etc.
    ///
    /// For dip icons (place in Assets/Resources/Sprites/UI/):
    ///   dip_garlic.png, dip_bloodworm.png, dip_vanilla.png, etc.
    ///
    /// For rod icons:
    ///   rod_makro_plastic_special.png, rod_sensation_carp_rod.png, etc.
    /// </summary>
    public static class SpriteAssetLoader
    {
        /// <summary>
        /// Scan Resources folders and override procedural sprites with real art where available.
        /// Call AFTER AssetWiring.WireAll() so procedural fallbacks are already in place.
        /// </summary>
        public static int LoadAndOverride()
        {
            int overridden = 0;
            overridden += LoadFishSprites();
            overridden += LoadDipIcons();
            overridden += LoadRodIcons();
            overridden += LoadReelIcons();
            overridden += LoadUISprites();

            if (overridden > 0)
                Debug.Log($"[SpriteAssetLoader] Overrode {overridden} procedural sprites with real art.");
            else
                Debug.Log("[SpriteAssetLoader] No real art found in Resources/Sprites/. Using procedural art.");

            return overridden;
        }

        private static int LoadFishSprites()
        {
            if (FishDatabase.Instance == null) return 0;

            int count = 0;
            var fishSprites = Resources.LoadAll<Sprite>("Sprites/Fish");

            // Map: normalized name → species
            var nameMap = new Dictionary<string, FishSpecies>
            {
                { "common_carp", FishSpecies.CommonCarp },
                { "commoncarp", FishSpecies.CommonCarp },
                { "mirror_carp", FishSpecies.MirrorCarp },
                { "mirrorcarp", FishSpecies.MirrorCarp },
                { "leather_carp", FishSpecies.LeatherCarp },
                { "leathercarp", FishSpecies.LeatherCarp },
                { "ghost_carp", FishSpecies.GhostCarp },
                { "ghostcarp", FishSpecies.GhostCarp },
                { "wild_small_carp", FishSpecies.WildSmallCarp },
                { "wildsmallcarp", FishSpecies.WildSmallCarp },
                { "small_carp", FishSpecies.WildSmallCarp },
                { "boknes_golden_carp", FishSpecies.BoknesGoldenCarp },
                { "boknes", FishSpecies.BoknesGoldenCarp },
                { "golden_carp", FishSpecies.BoknesGoldenCarp },
                { "barbel", FishSpecies.Barbel },
                { "catfish", FishSpecies.Barbel },
                { "flat_nose", FishSpecies.FlatNoseRiverBarber },
                { "flatnose", FishSpecies.FlatNoseRiverBarber },
                { "river_barber", FishSpecies.FlatNoseRiverBarber },
                { "mudfish", FishSpecies.Mudfish },
                { "kurper", FishSpecies.Kurper },
                { "tilapia", FishSpecies.Tilapia },
                { "bass", FishSpecies.Bass },
                { "graskarp", FishSpecies.Graskarp },
                { "grass_carp", FishSpecies.Graskarp },
                { "yellowfish", FishSpecies.Yellowfish },
                { "yellow_fish", FishSpecies.Yellowfish },
                { "eel", FishSpecies.Eel },
            };

            foreach (var sprite in fishSprites)
            {
                string normalized = sprite.name.ToLower().Replace(" ", "_").Replace("-", "_");

                foreach (var kvp in nameMap)
                {
                    if (normalized.Contains(kvp.Key))
                    {
                        var fishData = FishDatabase.Instance.GetFishBySpecies(kvp.Value);
                        if (fishData != null)
                        {
                            // Check if it's a photo version (larger) or icon
                            if (normalized.Contains("photo") || normalized.Contains("large") || sprite.rect.width > 200)
                                fishData.fishPhoto = sprite;
                            else
                                fishData.fishSprite = sprite;

                            count++;
                        }
                        break;
                    }
                }
            }

            return count;
        }

        private static int LoadDipIcons()
        {
            if (BaitManager.Instance == null) return 0;

            int count = 0;
            var dipSprites = Resources.LoadAll<Sprite>("Sprites/UI");

            foreach (var sprite in dipSprites)
            {
                string normalized = sprite.name.ToLower().Replace(" ", "_").Replace("-", "_");
                if (!normalized.StartsWith("dip_")) continue;

                string dipKey = normalized.Replace("dip_", "");

                foreach (var dip in BaitManager.Instance.AllDips)
                {
                    string dipNormalized = dip.dipName.ToLower().Replace(" ", "_").Replace("'", "");
                    if (dipNormalized.Contains(dipKey) || dipKey.Contains(dipNormalized))
                    {
                        dip.dipIcon = sprite;
                        count++;
                        break;
                    }
                }
            }

            return count;
        }

        private static int LoadRodIcons()
        {
            if (EquipmentManager.Instance == null) return 0;

            int count = 0;
            var rodSprites = Resources.LoadAll<Sprite>("Sprites/UI");

            foreach (var sprite in rodSprites)
            {
                string normalized = sprite.name.ToLower().Replace(" ", "_").Replace("-", "_");
                if (!normalized.StartsWith("rod_")) continue;

                string rodKey = normalized.Replace("rod_", "");

                foreach (var rod in EquipmentManager.Instance.GetOwnedRods())
                {
                    string rodNormalized = rod.rodName.ToLower().Replace(" ", "_").Replace("'", "");
                    if (rodNormalized.Contains(rodKey) || rodKey.Contains(rodNormalized))
                    {
                        rod.rodIcon = sprite;
                        count++;
                        break;
                    }
                }
            }

            return count;
        }

        private static int LoadReelIcons()
        {
            if (EquipmentManager.Instance == null) return 0;

            int count = 0;
            var reelSprites = Resources.LoadAll<Sprite>("Sprites/UI");

            foreach (var sprite in reelSprites)
            {
                string normalized = sprite.name.ToLower().Replace(" ", "_").Replace("-", "_");
                if (!normalized.StartsWith("reel_")) continue;

                string reelKey = normalized.Replace("reel_", "");

                foreach (var reel in EquipmentManager.Instance.GetOwnedReels())
                {
                    string reelNormalized = reel.reelName.ToLower().Replace(" ", "_").Replace("'", "");
                    if (reelNormalized.Contains(reelKey) || reelKey.Contains(reelNormalized))
                    {
                        reel.reelIcon = sprite;
                        count++;
                        break;
                    }
                }
            }

            return count;
        }

        private static int LoadUISprites()
        {
            // Load any generic UI sprites (backgrounds, frames, etc.)
            var uiSprites = Resources.LoadAll<Sprite>("Sprites/UI");
            return 0; // UI sprites are used directly by UIBuilder if available
        }
    }
}
