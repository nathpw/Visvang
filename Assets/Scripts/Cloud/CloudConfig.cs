using UnityEngine;

namespace Visvang.Cloud
{
    /// <summary>
    /// Firebase / cloud configuration.
    /// SETUP REQUIRED:
    /// 1. Create a Firebase project at https://console.firebase.google.com
    /// 2. Add Android app (package: com.visvang.fishing) and/or iOS app
    /// 3. Download google-services.json → Assets/
    /// 4. Download GoogleService-Info.plist → Assets/ (iOS)
    /// 5. Import Firebase Unity SDK: https://firebase.google.com/docs/unity/setup
    ///    - FirebaseAuth.unitypackage
    ///    - FirebaseFirestore.unitypackage
    ///    - FirebaseMessaging.unitypackage (for remote push)
    /// 6. Set FIREBASE_ENABLED scripting define symbol in Player Settings
    ///
    /// Without Firebase SDK imported, the game runs fully offline using local save.
    /// All cloud features gracefully degrade — no crashes, just local-only mode.
    /// </summary>
    [CreateAssetMenu(fileName = "CloudConfig", menuName = "Visvang/Cloud Config")]
    public class CloudConfig : ScriptableObject
    {
        [Header("Feature Toggles")]
        [Tooltip("Master toggle for all cloud features")]
        public bool cloudEnabled = true;

        [Tooltip("Sync save data to Firestore")]
        public bool cloudSaveEnabled = true;

        [Tooltip("Upload scores to global leaderboards")]
        public bool leaderboardsEnabled = true;

        [Tooltip("Firebase Cloud Messaging for remote push")]
        public bool remotePushEnabled = true;

        [Header("Firestore Collections")]
        public string usersCollection = "users";
        public string leaderboardCollection = "leaderboards";
        public string sessionsCollection = "sessions";
        public string catchLogCollection = "catches";

        [Header("Leaderboard Settings")]
        public int leaderboardPageSize = 25;
        public int maxLeaderboardEntries = 1000;

        [Header("Sync Settings")]
        [Tooltip("Seconds between cloud sync attempts")]
        public float cloudSyncInterval = 120f;

        [Tooltip("Sync on every catch or only on session end")]
        public bool syncOnEveryCatch = false;
    }
}
