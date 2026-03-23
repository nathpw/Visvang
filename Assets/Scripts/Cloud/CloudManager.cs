using UnityEngine;
using System;
using System.Collections.Generic;

namespace Visvang.Cloud
{
    /// <summary>
    /// Central cloud services manager. Wraps Firebase Auth, Firestore, and FCM.
    /// All Firebase calls are behind #if FIREBASE_ENABLED so the game compiles and runs
    /// without the Firebase SDK. Enable by adding FIREBASE_ENABLED to Scripting Define Symbols.
    /// </summary>
    public class CloudManager : MonoBehaviour
    {
        public static CloudManager Instance { get; private set; }

        [SerializeField] private CloudConfig config;

        private bool isInitialized;
        private bool isAuthenticated;
        private string userId;
        private string displayName;

        public bool IsInitialized => isInitialized;
        public bool IsAuthenticated => isAuthenticated;
        public bool IsCloudAvailable => isInitialized && isAuthenticated && config != null && config.cloudEnabled;
        public string UserId => userId;
        public string DisplayName => displayName;

        public event Action OnCloudReady;
        public event Action<string> OnCloudError;
        public event Action<string> OnAuthComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Use default config if none assigned
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<CloudConfig>();
            }
        }

        public void Initialize()
        {
#if FIREBASE_ENABLED
            InitializeFirebase();
#else
            Debug.Log("[CloudManager] Firebase not enabled. Running in offline mode. " +
                     "Add FIREBASE_ENABLED to Scripting Define Symbols after importing Firebase SDK.");
            isInitialized = false;
#endif
        }

#if FIREBASE_ENABLED
        private void InitializeFirebase()
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.Result == Firebase.DependencyStatus.Available)
                {
                    isInitialized = true;
                    Debug.Log("[CloudManager] Firebase initialized.");

                    // Auto sign-in anonymously
                    SignInAnonymous();

                    // Register for remote push
                    if (config.remotePushEnabled)
                        InitializeMessaging();
                }
                else
                {
                    Debug.LogError($"[CloudManager] Firebase dependency error: {task.Result}");
                    OnCloudError?.Invoke($"Firebase init failed: {task.Result}");
                }
            });
        }

        // --- AUTH ---

        public void SignInAnonymous()
        {
            Firebase.Auth.FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"[CloudManager] Anonymous sign-in failed: {task.Exception}");
                    OnCloudError?.Invoke("Sign-in failed");
                    return;
                }

                var user = task.Result.User;
                userId = user.UserId;
                displayName = user.DisplayName ?? "Hengelaar";
                isAuthenticated = true;

                Debug.Log($"[CloudManager] Signed in as {userId}");
                OnAuthComplete?.Invoke(userId);
                OnCloudReady?.Invoke();
            });
        }

        public void SignInWithGoogle(string idToken)
        {
            var credential = Firebase.Auth.GoogleAuthProvider.GetCredential(idToken, null);
            Firebase.Auth.FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"[CloudManager] Google sign-in failed: {task.Exception}");
                    OnCloudError?.Invoke("Google sign-in failed");
                    return;
                }

                var user = task.Result.User;
                userId = user.UserId;
                displayName = user.DisplayName ?? "Hengelaar";
                isAuthenticated = true;

                Debug.Log($"[CloudManager] Google sign-in: {displayName} ({userId})");
                OnAuthComplete?.Invoke(userId);
                OnCloudReady?.Invoke();
            });
        }

        // --- FIRESTORE HELPERS ---

        public Firebase.Firestore.FirebaseFirestore GetFirestore()
        {
            return Firebase.Firestore.FirebaseFirestore.DefaultInstance;
        }

        public Firebase.Firestore.DocumentReference GetUserDoc()
        {
            return GetFirestore().Collection(config.usersCollection).Document(userId);
        }

        public Firebase.Firestore.CollectionReference GetLeaderboardCollection()
        {
            return GetFirestore().Collection(config.leaderboardCollection);
        }

        // --- FCM ---

        private void InitializeMessaging()
        {
            Firebase.Messaging.FirebaseMessaging.TokenReceived += (sender, token) =>
            {
                Debug.Log($"[CloudManager] FCM Token: {token.Token}");
            };

            Firebase.Messaging.FirebaseMessaging.MessageReceived += (sender, e) =>
            {
                Debug.Log($"[CloudManager] FCM Message: {e.Message.Notification?.Title}");
            };
        }
#else
        // Stubs when Firebase is not available
        public void SignInAnonymous()
        {
            Debug.Log("[CloudManager] Offline mode — no sign-in available.");
        }

        public void SignInWithGoogle(string idToken)
        {
            Debug.Log("[CloudManager] Offline mode — Google sign-in not available.");
        }
#endif

        public CloudConfig GetConfig()
        {
            return config;
        }

        public void SignOut()
        {
#if FIREBASE_ENABLED
            Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
#endif
            isAuthenticated = false;
            userId = null;
            displayName = null;
        }
    }
}
