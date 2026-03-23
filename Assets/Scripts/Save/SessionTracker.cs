using UnityEngine;
using System;
using Visvang.Core;
using Visvang.Fish;
using Visvang.Fishing;
using Visvang.Bait;
using Visvang.Equipment;
using Visvang.Events;

namespace Visvang.Save
{
    /// <summary>
    /// Tracks all stats for the current fishing session.
    /// On session end, writes a SessionRecord to SaveData and triggers save.
    /// </summary>
    public class SessionTracker : MonoBehaviour
    {
        public static SessionTracker Instance { get; private set; }

        private SessionRecord current;
        private float sessionStartRealtime;
        private bool sessionActive;
        private int xpAtSessionStart;

        public SessionRecord Current => current;
        public bool IsSessionActive => sessionActive;
        public float SessionDuration => sessionActive ? Time.realtimeSinceStartup - sessionStartRealtime : 0f;

        public event Action<SessionRecord> OnSessionEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartSession()
        {
            if (sessionActive) EndSession();

            current = new SessionRecord();
            sessionStartRealtime = Time.realtimeSinceStartup;
            sessionActive = true;

            // Stamp start
            current.startTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var save = SaveManager.Instance?.CurrentSave;
            if (save != null)
            {
                current.sessionIndex = save.statistics.totalSessionsPlayed;
                xpAtSessionStart = save.player.totalXP;
            }

            // Record gear used
            var em = EquipmentManager.Instance;
            if (em != null)
            {
                current.rodUsed = em.EquippedRod != null ? em.EquippedRod.rodName : "None";
                current.reelUsed = em.EquippedReel != null ? em.EquippedReel.reelName : "None";
            }

            var bm = BaitManager.Instance;
            if (bm != null && bm.SelectedDip != null)
                current.dipUsed = bm.SelectedDip.dipName;

            // Subscribe to events
            if (FishingController.Instance != null)
            {
                FishingController.Instance.OnFishCaught += OnCatch;
                FishingController.Instance.OnFishLost += OnLost;
            }
            if (ChaosEventManager.Instance != null)
                ChaosEventManager.Instance.OnChaosEventStarted += OnChaos;

            Debug.Log("[SessionTracker] Session started.");
        }

        public SessionRecord EndSession()
        {
            if (!sessionActive) return current;

            sessionActive = false;
            current.endTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            current.durationSeconds = Time.realtimeSinceStartup - sessionStartRealtime;

            // Calculate XP earned
            var save = SaveManager.Instance?.CurrentSave;
            if (save != null)
                current.xpEarned = save.player.totalXP - xpAtSessionStart;

            // Unsubscribe
            if (FishingController.Instance != null)
            {
                FishingController.Instance.OnFishCaught -= OnCatch;
                FishingController.Instance.OnFishLost -= OnLost;
            }
            if (ChaosEventManager.Instance != null)
                ChaosEventManager.Instance.OnChaosEventStarted -= OnChaos;

            // Write session record to save
            if (save != null)
            {
                save.sessionHistory.Add(current);
                save.statistics.totalSessionsPlayed++;
                save.statistics.totalPlayTimeSeconds += current.durationSeconds;
                save.statistics.totalPapBucketsUsed++;
                SaveManager.Instance.Save();
            }

            Debug.Log($"[SessionTracker] Session ended. Duration: {current.durationSeconds:F0}s | " +
                     $"Caught: {current.fishCaught} | Lost: {current.fishLost} | XP: {current.xpEarned}");

            OnSessionEnded?.Invoke(current);
            return current;
        }

        // --- Event Handlers ---

        private void OnCatch(FishData fish, float weight)
        {
            current.fishCaught++;
            current.totalWeightCaught += weight;

            if (weight > current.heaviestCatchWeight)
            {
                current.heaviestCatchWeight = weight;
                current.heaviestCatchName = fish.fishName;
            }

            if (fish.IsMudfish())
                current.mudfishStreak++;
            else
                current.mudfishStreak = 0;

            // Also log to global catch log
            LogCatch(fish, weight);
        }

        private void OnLost(FishData fish)
        {
            current.fishLost++;
        }

        private void OnChaos(ChaosEventType eventType)
        {
            current.chaosEventsTriggered++;
        }

        public void RecordCast()
        {
            current.castsThrown++;
            var save = SaveManager.Instance?.CurrentSave;
            if (save != null)
                save.statistics.totalCastsThrown++;
        }

        public void RecordLineSnap()
        {
            current.linesSnapped++;
            var save = SaveManager.Instance?.CurrentSave;
            if (save != null)
                save.statistics.totalLinesSnapped++;
        }

        private void LogCatch(FishData fish, float weight)
        {
            var save = SaveManager.Instance?.CurrentSave;
            if (save == null) return;

            var entry = new CatchEntry
            {
                fishName = fish.fishName,
                speciesId = (int)fish.species,
                weight = weight,
                wasFirstCatch = !save.progress.caughtSpeciesIds.Contains((int)fish.species),
                wasRecord = weight > save.statistics.heaviestFishWeight,
                dipUsed = current.dipUsed ?? "None",
                rodUsed = current.rodUsed ?? "None",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                sessionIndex = current.sessionIndex
            };

            save.catchLog.Add(entry);

            // Mark species as caught
            if (!save.progress.caughtSpeciesIds.Contains((int)fish.species))
                save.progress.caughtSpeciesIds.Add((int)fish.species);

            // Update global stats
            save.statistics.totalFishCaught++;
            if (fish.IsCarp()) save.statistics.totalCarpCaught++;
            else if (fish.IsBarbel()) save.statistics.totalBarbelCaught++;
            else if (fish.IsMudfish()) save.statistics.totalMudfishCaught++;
            else save.statistics.totalOtherFishCaught++;

            if (weight > save.statistics.heaviestFishWeight)
            {
                save.statistics.heaviestFishWeight = weight;
                save.statistics.heaviestFishName = fish.fishName;
            }

            // Auto-save on every catch
            SaveManager.Instance.MarkDirty();
        }

        private void OnDestroy()
        {
            if (sessionActive)
                EndSession();
        }
    }
}
