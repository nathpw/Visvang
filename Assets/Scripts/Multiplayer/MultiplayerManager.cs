using UnityEngine;
using System;
using System.Collections.Generic;
using Visvang.Core;
using Visvang.Fish;

namespace Visvang.Multiplayer
{
    /// <summary>
    /// Manages multiplayer game modes and scoring.
    /// Each mode has different rules for what counts and what disqualifies.
    /// </summary>
    public class MultiplayerManager : MonoBehaviour
    {
        public static MultiplayerManager Instance { get; private set; }

        [Header("Match Settings")]
        [SerializeField] private MultiplayerMode currentMode = MultiplayerMode.None;
        [SerializeField] private float matchTimer;
        [SerializeField] private float matchDuration;
        [SerializeField] private bool matchActive;

        [Header("Players")]
        [SerializeField] private List<PlayerScore> playerScores = new List<PlayerScore>();

        public MultiplayerMode CurrentMode => currentMode;
        public float MatchTimer => matchTimer;
        public float MatchDuration => matchDuration;
        public bool MatchActive => matchActive;
        public List<PlayerScore> PlayerScores => playerScores;

        public event Action<MultiplayerMode> OnMatchStarted;
        public event Action<List<PlayerScore>> OnMatchEnded;
        public event Action<string, float> OnScoreUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!matchActive) return;

            matchTimer -= Time.deltaTime;
            if (matchTimer <= 0f)
                EndMatch();
        }

        public void StartMatch(MultiplayerMode mode, List<string> playerNames)
        {
            currentMode = mode;
            matchActive = true;

            switch (mode)
            {
                case MultiplayerMode.BarberBattle:
                    matchDuration = Constants.BARBER_BATTLE_DURATION;
                    break;
                case MultiplayerMode.MudfishMadness:
                    matchDuration = Constants.MUDFISH_MADNESS_DURATION;
                    break;
                case MultiplayerMode.CarpOnlyLeague:
                    matchDuration = 600f; // 10 minutes
                    break;
                default:
                    matchDuration = 300f;
                    break;
            }

            matchTimer = matchDuration;

            playerScores.Clear();
            foreach (var name in playerNames)
            {
                playerScores.Add(new PlayerScore { playerName = name, score = 0f, catchCount = 0 });
            }

            GameManager.Instance?.SetMultiplayerMode(mode);
            OnMatchStarted?.Invoke(mode);
        }

        public void RecordCatch(string playerName, FishData fish, float weight)
        {
            if (!matchActive) return;

            var player = playerScores.Find(p => p.playerName == playerName);
            if (player == null) return;

            switch (currentMode)
            {
                case MultiplayerMode.BarberBattle:
                    // Only barbel count. Biggest barbel wins.
                    if (fish.IsBarbel())
                    {
                        player.catchCount++;
                        if (weight > player.score)
                            player.score = weight;
                    }
                    break;

                case MultiplayerMode.MudfishMadness:
                    // Catching mudfish = penalty. Longest WITHOUT catching mudfish wins.
                    if (fish.IsMudfish())
                    {
                        player.mudfishCaught++;
                        player.score -= 10f; // Penalty
                    }
                    else
                    {
                        player.catchCount++;
                        player.score += weight;
                    }
                    break;

                case MultiplayerMode.CarpOnlyLeague:
                    // Only carp count. Barbel and mudfish auto-disqualified (not the player, the fish).
                    if (fish.IsCarp())
                    {
                        player.catchCount++;
                        player.score += weight;
                    }
                    break;
            }

            OnScoreUpdated?.Invoke(playerName, player.score);
        }

        public void EndMatch()
        {
            matchActive = false;

            // Sort by score descending
            playerScores.Sort((a, b) => b.score.CompareTo(a.score));

            GameManager.Instance?.SetMultiplayerMode(MultiplayerMode.None);
            OnMatchEnded?.Invoke(playerScores);
        }

        public string GetModeDescription(MultiplayerMode mode)
        {
            switch (mode)
            {
                case MultiplayerMode.BarberBattle:
                    return "Biggest barbel in 5 minutes wins! High risk, high reward.";
                case MultiplayerMode.MudfishMadness:
                    return "Avoid catching mudfish the longest! Every mudfish = penalty.";
                case MultiplayerMode.CarpOnlyLeague:
                    return "Carp only! Barbel and mudfish don't count. Total weight wins.";
                default:
                    return "";
            }
        }
    }

    [System.Serializable]
    public class PlayerScore
    {
        public string playerName;
        public float score;
        public int catchCount;
        public int mudfishCaught;
    }
}
