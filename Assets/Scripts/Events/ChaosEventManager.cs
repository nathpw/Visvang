using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Visvang.Core;
using Visvang.Fishing;
using Visvang.Bait;

namespace Visvang.Events
{
    public enum ChaosEventType
    {
        BarberAmbush,
        MudfishSwarm,
        BirdStealsFish,
        TurtleStealsMudfish,
        RodPulledIn,
        PapBucketDestroyed,
        BarberPhotobomb,
        MudfishJumpsFromNet,
        MudfishSlimeHands,
        BarberEatsLeftovers,
        BarberSlap,
        EelTangle,
        WeatherChange,
        BaitThief
    }

    /// <summary>
    /// Manages random chaos events that disrupt gameplay.
    /// Events are checked periodically and triggered based on conditions.
    /// </summary>
    public class ChaosEventManager : MonoBehaviour
    {
        public static ChaosEventManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float checkInterval = Constants.CHAOS_CHECK_INTERVAL;
        [SerializeField] private float baseChance = Constants.CHAOS_BASE_CHANCE;
        [SerializeField] private bool chaosEnabled = true;

        [Header("Active Event")]
        [SerializeField] private ChaosEventType? activeEvent;
        [SerializeField] private float eventTimer;

        private float checkTimer;
        private bool isProcessing;

        public ChaosEventType? ActiveEvent => activeEvent;
        public bool IsEventActive => activeEvent.HasValue;

        public event Action<ChaosEventType> OnChaosEventStarted;
        public event Action<ChaosEventType> OnChaosEventEnded;
        public event Action<string> OnChaosMessage;

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
            if (!chaosEnabled || isProcessing) return;

            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                checkTimer = 0f;
                RollChaosEvent();
            }

            if (activeEvent.HasValue)
            {
                eventTimer -= Time.deltaTime;
                if (eventTimer <= 0f)
                    EndActiveEvent();
            }
        }

        private void RollChaosEvent()
        {
            if (activeEvent.HasValue) return;

            float roll = UnityEngine.Random.value;
            if (roll > baseChance) return;

            var gm = GameManager.Instance;
            bool isNight = gm != null && gm.IsNightTime();

            // Weight events based on conditions
            List<WeightedEvent> candidates = new List<WeightedEvent>();

            candidates.Add(new WeightedEvent(ChaosEventType.BirdStealsFish, 0.15f));
            candidates.Add(new WeightedEvent(ChaosEventType.WeatherChange, 0.1f));
            candidates.Add(new WeightedEvent(ChaosEventType.BaitThief, 0.1f));

            if (isNight)
            {
                candidates.Add(new WeightedEvent(ChaosEventType.BarberAmbush, Constants.BARBEL_AMBUSH_NIGHT_CHANCE));
                candidates.Add(new WeightedEvent(ChaosEventType.EelTangle, 0.15f));
            }

            candidates.Add(new WeightedEvent(ChaosEventType.MudfishSwarm, 0.12f));
            candidates.Add(new WeightedEvent(ChaosEventType.TurtleStealsMudfish, 0.08f));

            TriggerEvent(SelectWeightedRandom(candidates));
        }

        public void TriggerEvent(ChaosEventType eventType)
        {
            if (isProcessing) return;

            activeEvent = eventType;
            isProcessing = true;

            OnChaosEventStarted?.Invoke(eventType);
            OnChaosMessage?.Invoke(GetEventMessage(eventType));

            StartCoroutine(ProcessEvent(eventType));
        }

        private IEnumerator ProcessEvent(ChaosEventType eventType)
        {
            switch (eventType)
            {
                case ChaosEventType.BarberAmbush:
                    yield return ProcessBarberAmbush();
                    break;
                case ChaosEventType.MudfishSwarm:
                    yield return ProcessMudfishSwarm();
                    break;
                case ChaosEventType.BirdStealsFish:
                    yield return ProcessBirdSteals();
                    break;
                case ChaosEventType.TurtleStealsMudfish:
                    yield return ProcessTurtleSteals();
                    break;
                case ChaosEventType.BarberPhotobomb:
                    yield return ProcessBarberPhotobomb();
                    break;
                case ChaosEventType.BarberSlap:
                    yield return ProcessBarberSlap();
                    break;
                case ChaosEventType.PapBucketDestroyed:
                    ProcessPapDestruction();
                    break;
                case ChaosEventType.BarberEatsLeftovers:
                    ProcessBarberEatsLeftovers();
                    break;
                case ChaosEventType.WeatherChange:
                    ProcessWeatherChange();
                    break;
                case ChaosEventType.BaitThief:
                    ProcessBaitThief();
                    break;
                default:
                    break;
            }

            isProcessing = false;
            EndActiveEvent();
        }

        // --- Event Implementations ---

        private IEnumerator ProcessBarberAmbush()
        {
            // Barbel destroys pap bucket at night
            OnChaosMessage?.Invoke("A massive barbel just ambushed your pap bucket in the dark!");
            PapSystem.Instance?.DestroyPap(Constants.PAP_BARBEL_DESTROY_AMOUNT);
            yield return new WaitForSeconds(3f);
        }

        private IEnumerator ProcessMudfishSwarm()
        {
            // 60 seconds of nonstop mudfish bites
            OnChaosMessage?.Invoke("MUDFISH SWARM! Eish, they're everywhere for the next 60 seconds!");
            eventTimer = Constants.MUDFISH_SWARM_DURATION;
            // FishSpawner handles increased mudfish rate during swarm
            yield return new WaitForSeconds(Constants.MUDFISH_SWARM_DURATION);
        }

        private IEnumerator ProcessBirdSteals()
        {
            OnChaosMessage?.Invoke("A bird just swooped in! Tap fast to save your catch!");
            // QTE handled by UI/QTEManager
            yield return new WaitForSeconds(5f);
        }

        private IEnumerator ProcessTurtleSteals()
        {
            OnChaosMessage?.Invoke("A turtle is slowly making off with your mudfish... watch it go.");
            yield return new WaitForSeconds(4f);
        }

        private IEnumerator ProcessBarberPhotobomb()
        {
            OnChaosMessage?.Invoke("A barbel just photobombed your carp catch! Classic.");
            yield return new WaitForSeconds(3f);
        }

        private IEnumerator ProcessBarberSlap()
        {
            OnChaosMessage?.Invoke("The barbel just slapped you during landing! Your ancestors felt that one.");
            yield return new WaitForSeconds(Constants.BARBEL_UI_DISORIENT_DURATION);
        }

        private void ProcessPapDestruction()
        {
            PapSystem.Instance?.DestroyPap(Constants.PAP_BARBEL_DESTROY_AMOUNT);
            OnChaosMessage?.Invoke("Your pap bucket took a direct hit!");
        }

        private void ProcessBarberEatsLeftovers()
        {
            PapSystem.Instance?.DestroyPap(20f);
            OnChaosMessage?.Invoke("A barbel ate all your leftover bait. Rude.");
        }

        private void ProcessWeatherChange()
        {
            Weather[] options = { Weather.Cloudy, Weather.Rainy, Weather.Windy, Weather.Stormy };
            Weather newWeather = options[UnityEngine.Random.Range(0, options.Length)];
            GameManager.Instance?.SetWeather(newWeather);
            OnChaosMessage?.Invoke($"Weather changed to {newWeather}! Adjust your strategy.");
        }

        private void ProcessBaitThief()
        {
            PapSystem.Instance?.DestroyPap(10f);
            OnChaosMessage?.Invoke("Something stole your bait while you weren't looking!");
        }

        private void EndActiveEvent()
        {
            if (activeEvent.HasValue)
            {
                var ended = activeEvent.Value;
                activeEvent = null;
                OnChaosEventEnded?.Invoke(ended);
            }
        }

        private string GetEventMessage(ChaosEventType eventType)
        {
            switch (eventType)
            {
                case ChaosEventType.BarberAmbush: return "BARBER AMBUSH!";
                case ChaosEventType.MudfishSwarm: return "MUDFISH MADNESS!";
                case ChaosEventType.BirdStealsFish: return "BIRD INCOMING!";
                case ChaosEventType.TurtleStealsMudfish: return "Turtle theft in progress...";
                case ChaosEventType.BarberPhotobomb: return "PHOTOBOMB!";
                case ChaosEventType.BarberSlap: return "SLAPPED!";
                case ChaosEventType.PapBucketDestroyed: return "PAP DOWN!";
                default: return "Something's happening...";
            }
        }

        private ChaosEventType SelectWeightedRandom(List<WeightedEvent> candidates)
        {
            float total = 0f;
            foreach (var c in candidates) total += c.weight;

            float roll = UnityEngine.Random.Range(0f, total);
            float cumulative = 0f;

            foreach (var c in candidates)
            {
                cumulative += c.weight;
                if (roll <= cumulative)
                    return c.eventType;
            }

            return candidates[candidates.Count - 1].eventType;
        }

        private struct WeightedEvent
        {
            public ChaosEventType eventType;
            public float weight;

            public WeightedEvent(ChaosEventType type, float w)
            {
                eventType = type;
                weight = w;
            }
        }
    }
}
