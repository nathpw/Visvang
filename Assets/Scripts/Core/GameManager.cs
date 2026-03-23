using UnityEngine;
using System;
using Visvang.Fishing;
using Visvang.Events;
using Visvang.Progression;
using Visvang.Audio;

namespace Visvang.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GamePhase currentPhase = GamePhase.MainMenu;
        [SerializeField] private TimeOfDay timeOfDay = TimeOfDay.Morning;
        [SerializeField] private Weather currentWeather = Weather.Clear;
        [SerializeField] private MultiplayerMode multiplayerMode = MultiplayerMode.None;

        [Header("Time Settings")]
        [SerializeField] private float gameTimeScale = 60f;
        [SerializeField] private float dayDurationMinutes = 24f;

        private float gameTimeClock;
        private bool isPaused;

        public GamePhase CurrentPhase => currentPhase;
        public TimeOfDay CurrentTimeOfDay => timeOfDay;
        public Weather CurrentWeather => currentWeather;
        public MultiplayerMode CurrentMultiplayerMode => multiplayerMode;
        public bool IsPaused => isPaused;
        public float GameTimeClock => gameTimeClock;

        public event Action<GamePhase> OnPhaseChanged;
        public event Action<TimeOfDay> OnTimeOfDayChanged;
        public event Action<Weather> OnWeatherChanged;

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

        private void Update()
        {
            if (isPaused) return;

            UpdateGameTime();
        }

        private void UpdateGameTime()
        {
            gameTimeClock += Time.deltaTime * gameTimeScale;

            float hoursInDay = dayDurationMinutes * 60f;
            if (gameTimeClock >= hoursInDay)
                gameTimeClock -= hoursInDay;

            TimeOfDay newTimeOfDay = CalculateTimeOfDay();
            if (newTimeOfDay != timeOfDay)
            {
                timeOfDay = newTimeOfDay;
                OnTimeOfDayChanged?.Invoke(timeOfDay);
            }
        }

        private TimeOfDay CalculateTimeOfDay()
        {
            float normalizedTime = gameTimeClock / (dayDurationMinutes * 60f);
            float hour = normalizedTime * 24f;

            if (hour < 5f) return TimeOfDay.Night;
            if (hour < 7f) return TimeOfDay.EarlyMorning;
            if (hour < 10f) return TimeOfDay.Morning;
            if (hour < 14f) return TimeOfDay.Midday;
            if (hour < 17f) return TimeOfDay.Afternoon;
            if (hour < 20f) return TimeOfDay.Evening;
            return TimeOfDay.Night;
        }

        public void SetPhase(GamePhase newPhase)
        {
            if (currentPhase == newPhase) return;

            currentPhase = newPhase;
            OnPhaseChanged?.Invoke(currentPhase);
        }

        public void SetWeather(Weather weather)
        {
            if (currentWeather == weather) return;

            currentWeather = weather;
            OnWeatherChanged?.Invoke(currentWeather);
        }

        public void SetMultiplayerMode(MultiplayerMode mode)
        {
            multiplayerMode = mode;
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
            SetPhase(GamePhase.Paused);
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            SetPhase(GamePhase.Fishing);
        }

        public void StartFishingSession()
        {
            gameTimeClock = 6f * 60f * 60f / gameTimeScale;
            SetPhase(GamePhase.Fishing);
        }

        public void EndFishingSession()
        {
            SetPhase(GamePhase.Results);
        }

        public bool IsNightTime()
        {
            return timeOfDay == TimeOfDay.Night;
        }
    }
}
