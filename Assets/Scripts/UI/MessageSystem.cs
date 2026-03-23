using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Visvang.Core;
using Visvang.Fish;

namespace Visvang.UI
{
    public enum MessageType
    {
        CatchSuccess,
        CatchFail,
        Chaos,
        Achievement,
        System,
        Humour
    }

    /// <summary>
    /// Displays South African-flavored messages for catches, events, and humor.
    /// Messages queue and display one at a time with animations.
    /// </summary>
    public class MessageSystem : MonoBehaviour
    {
        public static MessageSystem Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private CanvasGroup messageCanvasGroup;

        [Header("Settings")]
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        private Queue<QueuedMessage> messageQueue = new Queue<QueuedMessage>();
        private bool isDisplaying;

        // --- SA CATCH MESSAGES ---

        private static readonly string[] barbelCatchMessages = new[]
        {
            "He nearly took YOU with the rod, my bru!",
            "That barber gave you a hiding!",
            "Your ancestors felt that strike.",
            "Jislaaik! That's a monster barbel!",
            "You vs the barbel. Score: 1-0. Barely.",
            "That barber fought like it owed money.",
            "Your arms are going to be sore tomorrow, my china.",
            "The other side of the dam heard that fight!"
        };

        private static readonly string[] mudfishCatchMessages = new[]
        {
            "Mudfish again? Shame.",
            "At this point, you're fishing for disappointment.",
            "This fish owes you nothing.",
            "That slime will never come off.",
            "Another mudfish. Your boytjie is laughing.",
            "Eish, mudfish. The universe is trolling you.",
            "Congrats on your... mud trophy.",
            "Even the mudfish looks embarrassed to be here.",
            "You've been mudfish'd. Again."
        };

        private static readonly string[] carpCatchMessages = new[]
        {
            "Lekker! That's a beauty carp!",
            "Now THAT'S what we came for, bru!",
            "Papgooi pays off! Nice carp!",
            "Your pap recipe is working, my man!",
            "Get the camera! That's a boytjie!",
            "Carp in the net! Time for a brag photo.",
            "That carp put up a good fight. Respect.",
            "The dam is delivering today!"
        };

        private static readonly string[] legendaryMessages = new[]
        {
            "LEGENDARY CATCH! This one's going in the history books!",
            "BOKNES GOLDEN CARP?! You absolute legend!",
            "The fishing gods have blessed you today!",
            "FLAT-NOSE RIVER BARBER! They said it was a myth!",
            "Screenshot this. Nobody's going to believe you."
        };

        private static readonly string[] fishLostMessages = new[]
        {
            "Gone. Just like your dignity.",
            "The one that got away... again.",
            "That fish is telling its friends about you right now.",
            "Snap! There goes the line and your hopes.",
            "The dam giveth, the dam taketh away.",
            "Better luck next time, boet.",
            "That fish just swam off laughing.",
            "Your line broke and so did your spirit."
        };

        private static readonly string[] rodPulledMessages = new[]
        {
            "YOUR ROD! The barbel took it! DIVE! DIVE!",
            "Rod's in the water, bru! Quick!",
            "That barbel just yoinked your whole setup!",
            "Your rod is swimming. This is not a drill."
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ShowMessage(string message, MessageType type)
        {
            messageQueue.Enqueue(new QueuedMessage { text = message, type = type });

            if (!isDisplaying)
                StartCoroutine(ProcessMessageQueue());
        }

        public void ShowCatchMessage(FishData fish, float weight)
        {
            string message;

            if (fish.IsLegendary())
                message = legendaryMessages[Random.Range(0, legendaryMessages.Length)];
            else if (fish.IsBarbel())
                message = barbelCatchMessages[Random.Range(0, barbelCatchMessages.Length)];
            else if (fish.IsMudfish())
                message = mudfishCatchMessages[Random.Range(0, mudfishCatchMessages.Length)];
            else
                message = carpCatchMessages[Random.Range(0, carpCatchMessages.Length)];

            message += $"\n{fish.fishName} - {weight:F1}kg";

            ShowMessage(message, MessageType.CatchSuccess);
        }

        public void ShowFishLostMessage()
        {
            string message = fishLostMessages[Random.Range(0, fishLostMessages.Length)];
            ShowMessage(message, MessageType.CatchFail);
        }

        public void ShowRodPulledMessage()
        {
            string message = rodPulledMessages[Random.Range(0, rodPulledMessages.Length)];
            ShowMessage(message, MessageType.Chaos);
        }

        private IEnumerator ProcessMessageQueue()
        {
            isDisplaying = true;

            while (messageQueue.Count > 0)
            {
                var msg = messageQueue.Dequeue();
                yield return DisplayMessage(msg);
            }

            isDisplaying = false;
        }

        private IEnumerator DisplayMessage(QueuedMessage msg)
        {
            if (messageText != null)
                messageText.text = msg.text;

            if (messagePanel != null)
                messagePanel.SetActive(true);

            // Apply color based on type
            if (messageText != null)
            {
                switch (msg.type)
                {
                    case MessageType.CatchSuccess: messageText.color = new Color(0.2f, 0.9f, 0.3f); break;
                    case MessageType.CatchFail: messageText.color = new Color(0.9f, 0.3f, 0.2f); break;
                    case MessageType.Chaos: messageText.color = new Color(1f, 0.6f, 0f); break;
                    case MessageType.Achievement: messageText.color = new Color(1f, 0.85f, 0f); break;
                    default: messageText.color = Color.white; break;
                }
            }

            // Fade in
            yield return FadeCanvasGroup(0f, 1f, fadeInDuration);

            // Display
            yield return new WaitForSeconds(displayDuration);

            // Fade out
            yield return FadeCanvasGroup(1f, 0f, fadeOutDuration);

            if (messagePanel != null)
                messagePanel.SetActive(false);
        }

        private IEnumerator FadeCanvasGroup(float from, float to, float duration)
        {
            if (messageCanvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                messageCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            messageCanvasGroup.alpha = to;
        }

        private struct QueuedMessage
        {
            public string text;
            public MessageType type;
        }
    }
}
