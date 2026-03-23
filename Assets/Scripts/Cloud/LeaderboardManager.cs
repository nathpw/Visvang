using UnityEngine;
using System;
using System.Collections.Generic;

namespace Visvang.Cloud
{
    /// <summary>
    /// Global leaderboards stored in Firebase Firestore.
    /// Boards: Heaviest Fish, Most Catches, Highest Level, Biggest Barbel, Most Sessions.
    /// Falls back gracefully when offline — shows "Cloud unavailable" in UI.
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        private Dictionary<LeaderboardType, List<LeaderboardEntry>> cachedBoards = new Dictionary<LeaderboardType, List<LeaderboardEntry>>();
        private Dictionary<LeaderboardType, int> cachedPlayerRanks = new Dictionary<LeaderboardType, int>();

        public event Action<LeaderboardType, List<LeaderboardEntry>> OnLeaderboardLoaded;
        public event Action<LeaderboardType, int> OnPlayerRankLoaded;
        public event Action<string> OnLeaderboardError;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Submit player's current stats to all leaderboards.
        /// Call on session end.
        /// </summary>
        public void SubmitScores(string playerName, int level, int totalCatches,
            float heaviestWeight, string heaviestFishName, int barbelCaught, int sessionsPlayed)
        {
#if FIREBASE_ENABLED
            var cloud = CloudManager.Instance;
            if (cloud == null || !cloud.IsCloudAvailable) return;

            var config = cloud.GetConfig();
            if (config == null || !config.leaderboardsEnabled) return;

            var leaderboardRef = cloud.GetLeaderboardCollection();
            string odcId = cloud.UserId;

            var data = new Dictionary<string, object>
            {
                { "userId", cloud.UserId },
                { "playerName", playerName },
                { "level", level },
                { "totalCatches", totalCatches },
                { "heaviestWeight", heaviestWeight },
                { "heaviestFishName", heaviestFishName ?? "" },
                { "barbelCaught", barbelCaught },
                { "sessionsPlayed", sessionsPlayed },
                { "updatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            leaderboardRef.Document(cloud.UserId).SetAsync(data).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"[Leaderboard] Submit failed: {task.Exception}");
                    OnLeaderboardError?.Invoke("Failed to submit scores");
                }
                else
                {
                    Debug.Log("[Leaderboard] Scores submitted.");
                }
            });
#else
            Debug.Log("[Leaderboard] Offline mode — scores not submitted.");
#endif
        }

        /// <summary>
        /// Fetch a leaderboard. Results come via OnLeaderboardLoaded event.
        /// </summary>
        public void FetchLeaderboard(LeaderboardType type, int limit = 25)
        {
#if FIREBASE_ENABLED
            var cloud = CloudManager.Instance;
            if (cloud == null || !cloud.IsCloudAvailable)
            {
                OnLeaderboardError?.Invoke("Cloud not available");
                return;
            }

            var query = cloud.GetLeaderboardCollection();
            string orderField = GetOrderField(type);

            query.OrderByDescending(orderField).Limit(limit).GetSnapshotAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    OnLeaderboardError?.Invoke("Failed to load leaderboard");
                    return;
                }

                var entries = new List<LeaderboardEntry>();
                int rank = 1;

                foreach (var doc in task.Result.Documents)
                {
                    entries.Add(new LeaderboardEntry
                    {
                        rank = rank++,
                        userId = doc.GetValue<string>("userId"),
                        playerName = doc.GetValue<string>("playerName"),
                        level = doc.ContainsField("level") ? doc.GetValue<int>("level") : 0,
                        score = GetScoreFromDoc(doc, type),
                        detail = GetDetailFromDoc(doc, type),
                        isCurrentPlayer = doc.Id == cloud.UserId
                    });
                }

                cachedBoards[type] = entries;

                UnityMainThread.Execute(() =>
                {
                    OnLeaderboardLoaded?.Invoke(type, entries);
                });
            });
#else
            // Return mock data in offline mode
            var mockEntries = GetMockLeaderboard(type);
            cachedBoards[type] = mockEntries;
            OnLeaderboardLoaded?.Invoke(type, mockEntries);
#endif
        }

        public List<LeaderboardEntry> GetCachedBoard(LeaderboardType type)
        {
            cachedBoards.TryGetValue(type, out var entries);
            return entries;
        }

#if FIREBASE_ENABLED
        private string GetOrderField(LeaderboardType type)
        {
            switch (type)
            {
                case LeaderboardType.HeaviestFish: return "heaviestWeight";
                case LeaderboardType.MostCatches: return "totalCatches";
                case LeaderboardType.HighestLevel: return "level";
                case LeaderboardType.BiggestBarbel: return "barbelCaught";
                case LeaderboardType.MostSessions: return "sessionsPlayed";
                default: return "totalCatches";
            }
        }

        private float GetScoreFromDoc(Firebase.Firestore.DocumentSnapshot doc, LeaderboardType type)
        {
            switch (type)
            {
                case LeaderboardType.HeaviestFish:
                    return doc.ContainsField("heaviestWeight") ? doc.GetValue<float>("heaviestWeight") : 0f;
                case LeaderboardType.MostCatches:
                    return doc.ContainsField("totalCatches") ? doc.GetValue<int>("totalCatches") : 0;
                case LeaderboardType.HighestLevel:
                    return doc.ContainsField("level") ? doc.GetValue<int>("level") : 0;
                case LeaderboardType.BiggestBarbel:
                    return doc.ContainsField("barbelCaught") ? doc.GetValue<int>("barbelCaught") : 0;
                case LeaderboardType.MostSessions:
                    return doc.ContainsField("sessionsPlayed") ? doc.GetValue<int>("sessionsPlayed") : 0;
                default: return 0;
            }
        }

        private string GetDetailFromDoc(Firebase.Firestore.DocumentSnapshot doc, LeaderboardType type)
        {
            if (type == LeaderboardType.HeaviestFish && doc.ContainsField("heaviestFishName"))
                return doc.GetValue<string>("heaviestFishName");
            return "";
        }
#endif

        private List<LeaderboardEntry> GetMockLeaderboard(LeaderboardType type)
        {
            // Offline mock data so the UI still looks populated
            string[] names = { "Oom Frik", "Tannie Bessie", "Boet van Wyk", "Skommel",
                             "Die Barber King", "Papgooi Pete", "Mudfish Mike" };

            var entries = new List<LeaderboardEntry>();
            for (int i = 0; i < names.Length; i++)
            {
                entries.Add(new LeaderboardEntry
                {
                    rank = i + 1,
                    userId = $"mock_{i}",
                    playerName = names[i],
                    level = UnityEngine.Random.Range(5, 40),
                    score = GetMockScore(type, i),
                    detail = type == LeaderboardType.HeaviestFish ? "Common Carp" : "",
                    isCurrentPlayer = false
                });
            }
            return entries;
        }

        private float GetMockScore(LeaderboardType type, int rank)
        {
            switch (type)
            {
                case LeaderboardType.HeaviestFish: return 25f - rank * 2.5f;
                case LeaderboardType.MostCatches: return 500 - rank * 50;
                case LeaderboardType.HighestLevel: return 45 - rank * 5;
                case LeaderboardType.BiggestBarbel: return 30 - rank * 3;
                case LeaderboardType.MostSessions: return 100 - rank * 10;
                default: return 0;
            }
        }
    }

    public enum LeaderboardType
    {
        HeaviestFish,
        MostCatches,
        HighestLevel,
        BiggestBarbel,
        MostSessions
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public int rank;
        public string userId;
        public string playerName;
        public int level;
        public float score;
        public string detail;
        public bool isCurrentPlayer;
    }
}
