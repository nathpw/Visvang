using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace Visvang.Notifications
{
    /// <summary>
    /// Manages local push notifications for player re-engagement.
    /// Schedules SA-flavored fishing notifications on session end and app background.
    /// Requires com.unity.mobile.notifications package.
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        private const string CHANNEL_ID = "visvang_main";
        private const string CHANNEL_NAME = "Visvang";
        private const string CHANNEL_DESC = "Fishing notifications";

        private bool initialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            if (initialized) return;

#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel
            {
                Id = CHANNEL_ID,
                Name = CHANNEL_NAME,
                Description = CHANNEL_DESC,
                Importance = Importance.Default
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif

#if UNITY_IOS
            StartCoroutine(RequestIOSPermission());
#endif

            initialized = true;
            Debug.Log("[NotificationManager] Initialized.");
        }

#if UNITY_IOS
        private System.Collections.IEnumerator RequestIOSPermission()
        {
            var request = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound, true);
            while (!request.IsFinished)
                yield return null;

            Debug.Log($"[NotificationManager] iOS permission: {request.Granted}");
        }
#endif

        /// <summary>
        /// Cancel all pending notifications (call when player opens the app).
        /// </summary>
        public void CancelAll()
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllNotifications();
#endif
#if UNITY_IOS
            iOSNotificationCenter.RemoveAllScheduledNotifications();
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
            iOSNotificationCenter.ApplicationBadge = 0;
#endif
        }

        /// <summary>
        /// Schedule all re-engagement notifications (call on session end or app background).
        /// </summary>
        public void ScheduleReengagementNotifications()
        {
            CancelAll();

            // 2 hours: "Come back" reminder
            ScheduleNotification(
                "The fish are biting, bru!",
                GetRandomComeBackMessage(),
                TimeSpan.FromHours(2)
            );

            // 8 hours: morning/evening fishing
            ScheduleNotification(
                "Time to gooi some pap!",
                GetRandomFishingTimeMessage(),
                TimeSpan.FromHours(8)
            );

            // 24 hours: daily reminder
            ScheduleNotification(
                "Your rod is gathering dust...",
                GetRandomDailyMessage(),
                TimeSpan.FromHours(24)
            );

            // 3 days: longer absence
            ScheduleNotification(
                "The dam misses you!",
                "Your pap bucket is cold and the barbel are getting comfortable. Come show them who's boss!",
                TimeSpan.FromDays(3)
            );

            // 7 days: desperate
            ScheduleNotification(
                "Even the mudfish are asking about you",
                "It's been a week. The dam is not the same without you, bru. One more cast?",
                TimeSpan.FromDays(7)
            );
        }

        /// <summary>
        /// Schedule a notification after catching a legendary fish.
        /// </summary>
        public void ScheduleLegendaryCatchNotification(string fishName)
        {
            ScheduleNotification(
                "Legend status achieved!",
                $"You caught a {fishName}! Come share your brag photo with the boytjies!",
                TimeSpan.FromMinutes(30)
            );
        }

        /// <summary>
        /// Schedule a level-up congratulations (fires after player closes app post-level-up).
        /// </summary>
        public void ScheduleLevelUpNotification(int newLevel)
        {
            ScheduleNotification(
                $"Level {newLevel} unlocked!",
                $"New gear awaits! Level {newLevel} means better rods, new dips, and bigger fish. Come check it out!",
                TimeSpan.FromHours(1)
            );
        }

        /// <summary>
        /// Schedule session reminder based on player patterns.
        /// </summary>
        public void ScheduleSessionReminder(float lastSessionDuration)
        {
            if (lastSessionDuration < 120f)
            {
                // Short session — remind sooner
                ScheduleNotification(
                    "Quick session? Come back for a proper one!",
                    "Last time was too short. The carp were just warming up!",
                    TimeSpan.FromHours(4)
                );
            }
        }

        // --- Core Scheduling ---

        private void ScheduleNotification(string title, string body, TimeSpan delay)
        {
#if UNITY_ANDROID
            var notification = new AndroidNotification
            {
                Title = title,
                Text = body,
                FireTime = DateTime.Now + delay,
                SmallIcon = "icon_small",
                LargeIcon = "icon_large"
            };
            AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);
#endif

#if UNITY_IOS
            var notification = new iOSNotification
            {
                Identifier = $"visvang_{Guid.NewGuid():N}",
                Title = title,
                Body = body,
                ShowInForeground = false,
                CategoryIdentifier = "visvang",
                Trigger = new iOSNotificationTimeIntervalTrigger
                {
                    TimeInterval = delay,
                    Repeats = false
                }
            };
            iOSNotificationCenter.ScheduleNotification(notification);
#endif

#if UNITY_EDITOR
            Debug.Log($"[Notification] Scheduled in {delay.TotalHours:F1}h: {title} — {body}");
#endif
        }

        // --- SA-Flavored Message Banks ---

        private string GetRandomComeBackMessage()
        {
            string[] messages = {
                "Your pap is getting cold and the carp are circling. Don't leave them hanging!",
                "A barbel just broke someone else's rod. That could've been YOUR fight!",
                "The mudfish are multiplying while you're gone. They need catching, bru.",
                "Your boytjies are bragging about their catches. Time to show them up!",
                "The dam won't fish itself, my china. Get back here!",
                "Someone just caught a 15kg barbel on YOUR spot. Just saying."
            };
            return messages[UnityEngine.Random.Range(0, messages.Length)];
        }

        private string GetRandomFishingTimeMessage()
        {
            string[] messages = {
                "Early morning bite is ON! The carp are feeding. Get your pap ready!",
                "Evening session? The barbel come out at dusk. Prepare for chaos!",
                "Perfect fishing weather right now. Your rod is waiting.",
                "The dam is calm and the fish are hungry. Time to gooi!"
            };
            return messages[UnityEngine.Random.Range(0, messages.Length)];
        }

        private string GetRandomDailyMessage()
        {
            string[] messages = {
                "24 hours without fishing? That's not like you, bru.",
                "The pap bucket misses you. The barbel don't. Come remind them who's boss.",
                "Daily catch challenge: Can you beat yesterday's record?",
                "Your fishing chair is getting lonely at the dam."
            };
            return messages[UnityEngine.Random.Range(0, messages.Length)];
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                ScheduleReengagementNotifications();
            else
                CancelAll();
        }
    }
}
