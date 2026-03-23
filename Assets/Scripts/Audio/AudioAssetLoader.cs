using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Visvang.Audio
{
    /// <summary>
    /// Auto-assigns audio clips to AudioManager by scanning Assets/Audio/ folders.
    /// Matches files by name convention (e.g. "cast.wav" → castSound field).
    /// Attach to AudioManager GameObject and call LoadAll() or use the [ContextMenu].
    /// </summary>
    public class AudioAssetLoader : MonoBehaviour
    {
        [Header("Audio Folders (assign in Inspector or auto-scan)")]
        [SerializeField] private AudioClip[] sfxClips;
        [SerializeField] private AudioClip[] musicClips;
        [SerializeField] private AudioClip[] ambienceClips;
        [SerializeField] private AudioClip[] uiClips;

        // Filename → AudioManager field name mapping
        private static readonly Dictionary<string, string> sfxMap = new Dictionary<string, string>
        {
            // Fishing SFX
            { "cast", "castSound" },
            { "splash", "splashSound" },
            { "reel", "reelSound" },
            { "bite_alert", "biteAlertSound" },
            { "bite", "biteAlertSound" },
            { "hook_set", "hookSetSound" },
            { "hook", "hookSetSound" },
            { "line_snap", "lineSnapSound" },
            { "snap", "lineSnapSound" },
            { "net_splash", "netSplashSound" },
            { "net", "netSplashSound" },
            { "rod_creak", "rodCreakSound" },
            { "creak", "rodCreakSound" },

            // Fish SFX
            { "barbel_death_roll", "barbelDeathRollSound" },
            { "death_roll", "barbelDeathRollSound" },
            { "barbel_tail_slap", "barbelTailSlapSound" },
            { "tail_slap", "barbelTailSlapSound" },
            { "mudfish_squish", "mudfishSquishSound" },
            { "squish", "mudfishSquishSound" },
            { "fish_splash", "fishSplashSound" },

            // Chaos SFX
            { "chaos_alert", "chaosAlertSound" },
            { "alarm", "chaosAlertSound" },
            { "bird_swoop", "birdSwoopSound" },
            { "bird", "birdSwoopSound" },
            { "rod_in_water", "rodSplashSound" },
            { "rod_splash", "rodSplashSound" },
            { "pap_destroy", "papDestroySound" },
            { "bucket", "papDestroySound" },
        };

        private static readonly Dictionary<string, string> uiMap = new Dictionary<string, string>
        {
            { "button_click", "buttonClickSound" },
            { "click", "buttonClickSound" },
            { "level_up", "levelUpSound" },
            { "levelup", "levelUpSound" },
            { "fanfare", "levelUpSound" },
            { "xp_gain", "xpGainSound" },
            { "xp", "xpGainSound" },
            { "reward", "xpGainSound" },
            { "unlock", "unlockSound" },
            { "achievement", "unlockSound" },
        };

        private static readonly Dictionary<string, string> musicMap = new Dictionary<string, string>
        {
            { "menu_music", "menuMusic" },
            { "menu", "menuMusic" },
            { "fishing_music", "fishingMusic" },
            { "fishing", "fishingMusic" },
            { "calm", "fishingMusic" },
            { "fight_music", "fightMusic" },
            { "fight", "fightMusic" },
            { "battle", "fightMusic" },
            { "tense", "fightMusic" },
            { "victory_stinger", "victoryStinger" },
            { "victory", "victoryStinger" },
            { "win", "victoryStinger" },
        };

        private static readonly Dictionary<string, string> ambienceMap = new Dictionary<string, string>
        {
            { "dam_ambience", "damAmbience" },
            { "dam", "damAmbience" },
            { "lake", "damAmbience" },
            { "daytime", "damAmbience" },
            { "night_ambience", "nightAmbience" },
            { "night", "nightAmbience" },
            { "crickets", "nightAmbience" },
            { "rain_ambience", "rainAmbience" },
            { "rain", "rainAmbience" },
            { "storm", "rainAmbience" },
        };

        /// <summary>
        /// Auto-assign all clips from the serialized arrays to AudioManager.
        /// Call from code or use the context menu in Inspector.
        /// </summary>
        [ContextMenu("Load All Audio Assets")]
        public void LoadAll()
        {
            var audioManager = AudioManager.Instance ?? GetComponent<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError("[AudioAssetLoader] No AudioManager found!");
                return;
            }

            int assigned = 0;

            assigned += AssignClips(audioManager, sfxClips, sfxMap);
            assigned += AssignClips(audioManager, uiClips, uiMap);
            assigned += AssignClips(audioManager, musicClips, musicMap);
            assigned += AssignClips(audioManager, ambienceClips, ambienceMap);

            Debug.Log($"[AudioAssetLoader] Assigned {assigned} audio clips to AudioManager.");
        }

        private int AssignClips(AudioManager manager, AudioClip[] clips, Dictionary<string, string> map)
        {
            if (clips == null) return 0;

            int count = 0;
            foreach (var clip in clips)
            {
                if (clip == null) continue;

                // Try to match clip name to a field
                string cleanName = clip.name.ToLower()
                    .Replace(" ", "_")
                    .Replace("-", "_")
                    .Replace(".", "_");

                string fieldName = null;

                // Exact match first
                if (map.TryGetValue(cleanName, out fieldName))
                {
                    SetAudioField(manager, fieldName, clip);
                    count++;
                    continue;
                }

                // Partial match (clip name contains key)
                foreach (var kvp in map)
                {
                    if (cleanName.Contains(kvp.Key))
                    {
                        SetAudioField(manager, kvp.Value, clip);
                        count++;
                        break;
                    }
                }
            }
            return count;
        }

        private void SetAudioField(AudioManager manager, string fieldName, AudioClip clip)
        {
            var field = typeof(AudioManager).GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(manager, clip);
                Debug.Log($"[AudioAssetLoader] {clip.name} → {fieldName}");
            }
            else
            {
                Debug.LogWarning($"[AudioAssetLoader] Field '{fieldName}' not found on AudioManager");
            }
        }

        /// <summary>
        /// Scan Resources folders for audio and auto-load.
        /// Place audio files in Assets/Resources/Audio/SFX/, Music/, Ambience/, UI/
        /// </summary>
        [ContextMenu("Auto-Scan Resources Folder")]
        public void AutoScanResources()
        {
            sfxClips = Resources.LoadAll<AudioClip>("Audio/SFX");
            musicClips = Resources.LoadAll<AudioClip>("Audio/Music");
            ambienceClips = Resources.LoadAll<AudioClip>("Audio/Ambience");
            uiClips = Resources.LoadAll<AudioClip>("Audio/UI");

            Debug.Log($"[AudioAssetLoader] Scanned Resources: " +
                     $"SFX={sfxClips?.Length ?? 0}, Music={musicClips?.Length ?? 0}, " +
                     $"Ambience={ambienceClips?.Length ?? 0}, UI={uiClips?.Length ?? 0}");

            LoadAll();
        }
    }
}
