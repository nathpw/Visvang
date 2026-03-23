using UnityEngine;
using System.Collections.Generic;

namespace Visvang.Audio
{
    /// <summary>
    /// Singleton audio manager for SFX, ambiance, and music.
    /// Handles fishing sounds, UI clicks, chaos event alerts, and ambient dam sounds.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;

        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip fishingMusic;
        [SerializeField] private AudioClip fightMusic;
        [SerializeField] private AudioClip victoryStinger;

        [Header("Ambience")]
        [SerializeField] private AudioClip damAmbience;
        [SerializeField] private AudioClip nightAmbience;
        [SerializeField] private AudioClip rainAmbience;

        [Header("Fishing SFX")]
        [SerializeField] private AudioClip castSound;
        [SerializeField] private AudioClip splashSound;
        [SerializeField] private AudioClip reelSound;
        [SerializeField] private AudioClip biteAlertSound;
        [SerializeField] private AudioClip hookSetSound;
        [SerializeField] private AudioClip lineSnapSound;
        [SerializeField] private AudioClip netSplashSound;
        [SerializeField] private AudioClip rodCreakSound;

        [Header("Fish SFX")]
        [SerializeField] private AudioClip barbelDeathRollSound;
        [SerializeField] private AudioClip barbelTailSlapSound;
        [SerializeField] private AudioClip mudfishSquishSound;
        [SerializeField] private AudioClip fishSplashSound;

        [Header("Chaos SFX")]
        [SerializeField] private AudioClip chaosAlertSound;
        [SerializeField] private AudioClip birdSwoopSound;
        [SerializeField] private AudioClip rodSplashSound;
        [SerializeField] private AudioClip papDestroySound;

        [Header("UI SFX")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip levelUpSound;
        [SerializeField] private AudioClip xpGainSound;
        [SerializeField] private AudioClip unlockSound;

        [Header("Volume Settings")]
        [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.8f;
        [SerializeField] [Range(0f, 1f)] private float ambienceVolume = 0.4f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // --- Music ---

        public void PlayMenuMusic() => PlayMusic(menuMusic);
        public void PlayFishingMusic() => PlayMusic(fishingMusic);
        public void PlayFightMusic() => PlayMusic(fightMusic);

        public void PlayVictoryStinger()
        {
            if (victoryStinger != null)
                sfxSource?.PlayOneShot(victoryStinger, sfxVolume * masterVolume);
        }

        private void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.loop = true;
            musicSource.Play();
        }

        // --- Ambience ---

        public void PlayDamAmbience() => PlayAmbience(damAmbience);
        public void PlayNightAmbience() => PlayAmbience(nightAmbience);
        public void PlayRainAmbience() => PlayAmbience(rainAmbience);

        private void PlayAmbience(AudioClip clip)
        {
            if (ambienceSource == null || clip == null) return;

            ambienceSource.clip = clip;
            ambienceSource.volume = ambienceVolume * masterVolume;
            ambienceSource.loop = true;
            ambienceSource.Play();
        }

        // --- SFX ---

        public void PlayCast() => PlaySFX(castSound);
        public void PlaySplash() => PlaySFX(splashSound);
        public void PlayReel() => PlaySFX(reelSound);
        public void PlayBiteAlert() => PlaySFX(biteAlertSound);
        public void PlayHookSet() => PlaySFX(hookSetSound);
        public void PlayLineSnap() => PlaySFX(lineSnapSound);
        public void PlayNetSplash() => PlaySFX(netSplashSound);
        public void PlayRodCreak() => PlaySFX(rodCreakSound);

        public void PlayBarbelDeathRoll() => PlaySFX(barbelDeathRollSound);
        public void PlayBarbelTailSlap() => PlaySFX(barbelTailSlapSound);
        public void PlayMudfishSquish() => PlaySFX(mudfishSquishSound);
        public void PlayFishSplash() => PlaySFX(fishSplashSound);

        public void PlayChaosAlert() => PlaySFX(chaosAlertSound);
        public void PlayBirdSwoop() => PlaySFX(birdSwoopSound);
        public void PlayRodInWater() => PlaySFX(rodSplashSound);
        public void PlayPapDestroy() => PlaySFX(papDestroySound);

        public void PlayButtonClick() => PlayUI(buttonClickSound);
        public void PlayLevelUp() => PlayUI(levelUpSound);
        public void PlayXPGain() => PlayUI(xpGainSound);
        public void PlayUnlock() => PlayUI(unlockSound);

        private void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }

        private void PlayUI(AudioClip clip)
        {
            if (uiSource == null || clip == null) return;
            uiSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }

        // --- Volume ---

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetAmbienceVolume(float volume)
        {
            ambienceVolume = Mathf.Clamp01(volume);
            if (ambienceSource != null) ambienceSource.volume = ambienceVolume * masterVolume;
        }

        private void UpdateVolumes()
        {
            if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
            if (ambienceSource != null) ambienceSource.volume = ambienceVolume * masterVolume;
        }

        public void StopAll()
        {
            musicSource?.Stop();
            ambienceSource?.Stop();
            sfxSource?.Stop();
        }
    }
}
