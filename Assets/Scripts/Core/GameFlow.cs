using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Visvang.Fish;
using Visvang.Fishing;
using Visvang.Bait;
using Visvang.Equipment;
using Visvang.Events;
using Visvang.QTE;
using Visvang.Progression;
using Visvang.UI;
using Visvang.Save;
using Visvang.Cloud;
using Visvang.Notifications;

namespace Visvang.Core
{
    /// <summary>
    /// Master game flow controller. Wires all UI buttons to systems,
    /// manages screen transitions, and drives the full gameplay loop.
    /// </summary>
    public class GameFlow : MonoBehaviour
    {
        public static GameFlow Instance { get; private set; }

        private UIReferences ui;
        private DipData selectedDip;
        private bool isCasting;
        private float castHoldTime;
        private bool isFighting;
        private int sessionCatches;
        private float sessionStartTime;
        private float totalWeightCaught;

        // Cast visual
        private Slider castPowerBar;
        private GameObject castBarObject;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(UIReferences uiRefs)
        {
            ui = uiRefs;
            WireButtons();
            SubscribeEvents();
            UpdateLevelDisplay();
        }

        private void WireButtons()
        {
            // Main menu
            WireButton(ui.mainMenuPanel, "StartButton", OnStartFishing);
            WireButton(ui.mainMenuPanel, "MultiplayerButton", OnMultiplayer);
            WireButton(ui.mainMenuPanel, "StatsButton", OnShowStats);

            // Gear setup
            WireButton(ui.gearSetupPanel, "GoFishingButton", OnGoFishing);

            // Fish caught
            WireButton(ui.fishCaughtPanel, "ContinueButton", OnContinueFishing);

            // Pause
            WireButton(ui.pausePanel, "ResumeButton", OnResume);
            WireButton(ui.pausePanel, "QuitButton", OnQuitToMenu);

            // Results
            WireButton(ui.resultsPanel, "BackToMenuButton", OnBackToMenu);

            // Fishing HUD pause button
            WireButton(ui.fishingHUDPanel, "PauseButton", OnPause);
        }

        private void SubscribeEvents()
        {
            if (FishingController.Instance != null)
            {
                FishingController.Instance.OnStateChanged += OnFishingStateChanged;
                FishingController.Instance.OnFishCaught += OnFishCaught;
                FishingController.Instance.OnFishLost += OnFishLost;
                FishingController.Instance.OnRodPulledIn += OnRodPulledIn;
                FishingController.Instance.OnSlimeChanged += OnSlimeChanged;
            }

            var fight = FindObjectOfType<FightController>();
            if (fight != null)
            {
                fight.OnDeathRollStarted += OnDeathRollStart;
                fight.OnDeathRollEnded += OnDeathRollEnd;
                fight.OnTailSlap += OnTailSlap;
                fight.OnQTERequired += OnQTERequired;
                fight.OnLandingComplete += OnLandingComplete;
            }

            var tension = FindObjectOfType<TensionSystem>();
            if (tension != null)
            {
                tension.OnTensionChanged += OnTensionChanged;
                tension.OnLineSnapped += OnLineSnapped;
            }

            if (PapSystem.Instance != null)
            {
                PapSystem.Instance.OnPapChanged += OnPapChanged;
                PapSystem.Instance.OnPapEmpty += OnPapEmpty;
            }

            if (QTEManager.Instance != null)
            {
                QTEManager.Instance.OnQTECompleted += OnQTECompleted;
                QTEManager.Instance.OnGripChanged += OnGripChanged;
            }

            if (ChaosEventManager.Instance != null)
            {
                ChaosEventManager.Instance.OnChaosEventStarted += OnChaosStart;
                ChaosEventManager.Instance.OnChaosEventEnded += OnChaosEnd;
                ChaosEventManager.Instance.OnChaosMessage += ShowChaosMessage;
            }

            if (PlayerProfile.Instance != null)
            {
                PlayerProfile.Instance.OnLevelUp += OnLevelUp;
            }
        }

        // ===== INPUT HANDLING =====

        private void Update()
        {
            if (FishingController.Instance == null) return;
            var state = FishingController.Instance.CurrentState;

            switch (state)
            {
                case FishingState.Idle:
                    HandleIdleInput();
                    break;
                case FishingState.Casting:
                    HandleCastInput();
                    break;
                case FishingState.BiteDetected:
                    HandleBiteInput();
                    break;
                case FishingState.Fighting:
                    HandleFightInput();
                    break;
                case FishingState.RodPulledIn:
                    HandleRodPulledInput();
                    break;
            }

            // Update fight UI in real-time
            if (state == FishingState.Fighting)
                UpdateFightUI();

            // ESC for pause
            if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance.CurrentPhase == GamePhase.Fishing)
                OnPause();
        }

        private void HandleIdleInput()
        {
            if (GetTapDown())
            {
                FishingController.Instance.StartCast();
            }
        }

        private void HandleCastInput()
        {
            if (!isCasting && GetTapHeld())
            {
                isCasting = true;
                castHoldTime = 0f;
                ShowCastBar(true);
            }

            if (isCasting)
            {
                castHoldTime += Time.deltaTime;
                float power = Mathf.PingPong(castHoldTime * 50f, Constants.MAX_CAST_POWER);
                UpdateCastBar(power / Constants.MAX_CAST_POWER);

                if (GetTapUp())
                {
                    isCasting = false;
                    ShowCastBar(false);
                    FishingController.Instance.ReleaseCast(power, 0f);
                    SessionTracker.Instance?.RecordCast();
                }
            }
        }

        private void HandleBiteInput()
        {
            if (GetTapDown())
            {
                FishingController.Instance.Strike();
            }
        }

        private void HandleFightInput()
        {
            // Hold to reel
            if (GetTapHeld())
                FishingController.Instance.ReelFight(1f);

            // W to wash hands
            if (Input.GetKeyDown(KeyCode.W))
                FishingController.Instance.WashHands();

            // Tap for QTE
            if (QTEManager.Instance != null && QTEManager.Instance.IsQTEActive)
            {
                if (GetTapDown())
                    QTEManager.Instance.RegisterTap();
                if (QTEManager.Instance.ActiveQTEType == QTEType.Hold)
                    QTEManager.Instance.RegisterHold(GetTapHeld());
            }
        }

        private void HandleRodPulledInput()
        {
            if (GetTapDown())
            {
                QTEManager.Instance?.RegisterTap();
            }
        }

        // ===== SCREEN TRANSITIONS =====

        private void OnStartFishing()
        {
            ui.HideAll();
            ui.gearSetupPanel.SetActive(true);
            PopulateDipButtons();
            UpdateGearDisplay();
        }

        private void OnMultiplayer()
        {
            ShowMessage("Multiplayer coming soon, my bru!", MessageType.System);
        }

        private void OnShowStats()
        {
            if (PlayerProfile.Instance == null) return;
            var p = PlayerProfile.Instance;
            var save = SaveManager.Instance?.CurrentSave;
            int sessions = save?.statistics.totalSessionsPlayed ?? 0;
            float playTime = save?.statistics.totalPlayTimeSeconds ?? 0f;
            int playMinutes = Mathf.FloorToInt(playTime / 60f);

            string stats = $"Level: {p.PlayerLevel}\n" +
                          $"Total XP: {p.TotalXP}\n" +
                          $"Fish Caught: {p.TotalFishCaught}\n" +
                          $"Fish Lost: {p.TotalFishLost}\n" +
                          $"Barbel: {p.TotalBarbelCaught}\n" +
                          $"Mudfish: {p.TotalMudfishCaught}\n" +
                          $"Heaviest: {p.HeaviestFishWeight:F1}kg ({p.HeaviestFishName})\n" +
                          $"Species Caught: {p.CaughtSpeciesCount}\n" +
                          $"Rods Lost to Barbers: {p.RodsLostToBarbers}\n" +
                          $"Times Slapped: {p.TimesSlapped}\n" +
                          $"Sessions Played: {sessions}\n" +
                          $"Total Play Time: {playMinutes} min";
            ShowMessage(stats, MessageType.System);
        }

        private void OnGoFishing()
        {
            if (selectedDip == null)
            {
                ShowMessage("Pick a dip first, bru!", MessageType.System);
                return;
            }

            ui.HideAll();
            ui.fishingHUDPanel.SetActive(true);

            // Start the session
            sessionCatches = 0;
            totalWeightCaught = 0f;
            sessionStartTime = Time.time;

            // Start session tracking (saves to disk on end)
            SessionTracker.Instance?.StartSession();

            // Collect current state to save before starting
            SaveBridge.CollectToSave(SaveManager.Instance?.CurrentSave);

            PapSystem.Instance?.FillBucket();
            PapSystem.Instance?.ApplyDip(selectedDip);

            // Set tension line strength from rod
            var tensionSystem = FindObjectOfType<TensionSystem>();
            if (tensionSystem != null && EquipmentManager.Instance != null)
                tensionSystem.SetLineStrength(EquipmentManager.Instance.GetLineStrength());

            GameManager.Instance?.StartFishingSession();
            FishingController.Instance?.SetState(FishingState.Idle);

            UpdateStatus("TAP to cast your line!");
        }

        private void OnContinueFishing()
        {
            ui.HideAll();
            ui.fishingHUDPanel.SetActive(true);
            FishingController.Instance?.SetState(FishingState.Idle);
            GameManager.Instance?.SetPhase(GamePhase.Fishing);
            UpdateStatus("TAP to cast again!");
        }

        private void OnPause()
        {
            ui.pausePanel.SetActive(true);
            GameManager.Instance?.PauseGame();
        }

        private void OnResume()
        {
            ui.pausePanel.SetActive(false);
            GameManager.Instance?.ResumeGame();
        }

        private void OnQuitToMenu()
        {
            Time.timeScale = 1f;
            FishingController.Instance?.ReelIn();
            ShowResults();
        }

        private void OnBackToMenu()
        {
            ui.HideAll();
            ui.mainMenuPanel.SetActive(true);
            UpdateLevelDisplay();
        }

        // ===== FISHING STATE EVENTS =====

        private void OnFishingStateChanged(FishingState state)
        {
            switch (state)
            {
                case FishingState.Idle:
                    UpdateStatus("TAP to cast!");
                    ui.fightPanel.SetActive(false);
                    ui.castInstructionText.gameObject.SetActive(true);
                    ui.castInstructionText.text = "TAP AND HOLD TO CAST";
                    break;

                case FishingState.Casting:
                    UpdateStatus("HOLD... building power...");
                    ui.castInstructionText.text = "RELEASE TO CAST!";
                    break;

                case FishingState.Waiting:
                    UpdateStatus("Waiting for a bite...");
                    ui.castInstructionText.text = "Watching the rod tips...";
                    break;

                case FishingState.BiteDetected:
                    UpdateStatus("BITE! TAP TO STRIKE!");
                    ui.castInstructionText.text = "TAP NOW!";
                    StartCoroutine(FlashText(ui.statusText, 0.2f));
                    break;

                case FishingState.Fighting:
                    ui.castInstructionText.gameObject.SetActive(false);
                    ui.fightPanel.SetActive(true);
                    isFighting = true;
                    var fish = FishingController.Instance?.CurrentFish;
                    float weight = FishingController.Instance?.CurrentFishWeight ?? 0f;
                    if (fish != null)
                    {
                        ui.fightFishName.text = fish.fishName;
                        ui.fightFishWeight.text = $"{weight:F1} kg";

                        if (fish.IsBarbel())
                            UpdateStatus("BARBEL ON! Hold on tight!");
                        else if (fish.IsMudfish())
                            UpdateStatus("Mudfish... eish.");
                        else
                            UpdateStatus("FISH ON! HOLD to reel!");

                        ui.slimePanel.SetActive(fish.causesSlime);
                    }
                    ui.reelInstruction.text = "HOLD TO REEL!";
                    break;

                case FishingState.Landing:
                    UpdateStatus("LANDING! Don't let go!");
                    break;

                case FishingState.RodPulledIn:
                    UpdateStatus("ROD IN WATER! TAP TAP TAP!");
                    ShowMessage("Your rod got pulled in! Tap rapidly!", MessageType.Chaos);
                    break;
            }
        }

        private void OnFishCaught(FishData fish, float weight)
        {
            isFighting = false;
            sessionCatches++;
            totalWeightCaught += weight;

            ui.HideAll();
            ui.fishCaughtPanel.SetActive(true);

            // Fill catch panel
            ui.catchFishName.text = fish.fishName;
            ui.catchSpecies.text = fish.species.ToString();
            ui.catchWeight.text = $"{weight:F1} kg";
            ui.catchRarity.text = fish.rarity.ToString();
            ui.catchRarity.color = GetRarityColor(fish.rarity);

            bool isFirst = PlayerProfile.Instance != null && !PlayerProfile.Instance.HasCaughtSpecies(fish.species);
            ui.firstCatchBadge.SetActive(isFirst);
            ui.newRecordBadge.SetActive(PlayerProfile.Instance != null && weight > PlayerProfile.Instance.HeaviestFishWeight);
            ui.legendaryBadge.SetActive(fish.IsLegendary());

            int xp = XPSystem.Instance?.CalculateXP(fish, weight, isFirst) ?? fish.baseXP;
            ui.catchXPText.text = $"+{xp} XP";

            // Record it
            PlayerProfile.Instance?.RecordCatch(fish, weight, 0f);

            // Show SA humor message
            ShowCatchMessage(fish, weight);

            // Cloud: upload catch record
            CloudSaveSync.Instance?.UploadCatchRecord(fish.fishName, weight, fish.species.ToString());

            // Notification: legendary catch
            if (fish.IsLegendary())
                NotificationManager.Instance?.ScheduleLegendaryCatchNotification(fish.fishName);
        }

        private void OnFishLost(FishData fish)
        {
            isFighting = false;
            ui.fightPanel.SetActive(false);
            PlayerProfile.Instance?.RecordFishLost();

            string[] lostMessages = {
                "Gone. Just like your dignity.",
                "That fish is telling its friends about you right now.",
                "Better luck next time, boet.",
                "The dam giveth, the dam taketh away.",
                "That fish just swam off laughing."
            };
            ShowMessage(lostMessages[Random.Range(0, lostMessages.Length)], MessageType.CatchFail);

            UpdateStatus("Fish lost! TAP to cast again.");
            FishingController.Instance?.SetState(FishingState.Idle);
        }

        private void OnRodPulledIn()
        {
            QTEManager.Instance?.StartQTE(QTEType.Tap);
        }

        // ===== FIGHT UI UPDATES =====

        private void UpdateFightUI()
        {
            var fight = FindObjectOfType<FightController>();
            if (fight != null && fight.IsFighting)
            {
                ui.fightProgressBar.value = fight.NormalizedProgress;

                // Disorientation effect
                if (fight.IsUIDisoriented && ui.disorientOverlay != null)
                {
                    ui.disorientOverlay.SetActive(true);
                    ui.reelInstruction.text = "SLAPPED! Can't see!";
                }
                else if (ui.disorientOverlay != null)
                {
                    ui.disorientOverlay.SetActive(false);
                }
            }
        }

        private void OnTensionChanged(float normalized)
        {
            if (ui.tensionBar != null)
            {
                ui.tensionBar.value = normalized;
                var fill = ui.tensionBar.fillRect?.GetComponent<Image>();
                if (fill != null)
                {
                    if (normalized < 0.5f)
                        fill.color = Color.Lerp(new Color(0.2f, 0.75f, 0.3f), Color.yellow, normalized * 2f);
                    else
                        fill.color = Color.Lerp(Color.yellow, new Color(0.9f, 0.25f, 0.2f), (normalized - 0.5f) * 2f);
                }
            }
        }

        private void OnLineSnapped()
        {
            ShowMessage("SNAP! Your line broke!", MessageType.CatchFail);
            SessionTracker.Instance?.RecordLineSnap();
        }

        private void OnSlimeChanged(float slime)
        {
            if (ui.slimeBar != null)
                ui.slimeBar.value = slime / Constants.MAX_SLIME_METER;

            if (slime > Constants.MAX_SLIME_METER * 0.7f)
                ui.reelInstruction.text = "SLIMED! Press W to wash!";
        }

        private void OnDeathRollStart()
        {
            ui.deathRollWarning.gameObject.SetActive(true);
            ui.gripPanel.SetActive(true);
            ShowMessage("DEATH ROLL! Mash TAP to keep your grip!", MessageType.Chaos);
        }

        private void OnDeathRollEnd()
        {
            ui.deathRollWarning.gameObject.SetActive(false);
            ui.gripPanel.SetActive(false);
        }

        private void OnTailSlap()
        {
            ShowMessage("TAIL SLAP! Your ancestors felt that!", MessageType.Chaos);
            PlayerProfile.Instance?.RecordSlapped();
        }

        private void OnGripChanged(float normalized)
        {
            if (ui.gripBar != null)
                ui.gripBar.value = normalized;
        }

        // ===== QTE EVENTS =====

        private void OnQTERequired(QTEType type)
        {
            QTEManager.Instance?.StartQTE(type);
            ui.gripPanel.SetActive(type == QTEType.GripStrength);
        }

        private void OnQTECompleted(QTEType type, bool success)
        {
            ui.gripPanel.SetActive(false);

            var fight = FindObjectOfType<FightController>();
            fight?.OnQTEResult(success);

            if (!success && type == QTEType.GripStrength)
                ShowMessage("You lost your grip! The fish is gone!", MessageType.CatchFail);
        }

        private void OnLandingComplete(bool success)
        {
            // Handled by FishingController
        }

        // ===== CHAOS EVENTS =====

        private void OnChaosStart(ChaosEventType eventType)
        {
            ui.chaosEventPanel.SetActive(true);
            ui.chaosTitle.text = eventType.ToString().Replace("_", " ");
        }

        private void OnChaosEnd(ChaosEventType eventType)
        {
            ui.chaosEventPanel.SetActive(false);
        }

        private void ShowChaosMessage(string message)
        {
            if (ui.chaosMessage != null)
                ui.chaosMessage.text = message;
            ShowMessage(message, MessageType.Chaos);
        }

        // ===== PAP EVENTS =====

        private void OnPapChanged(float normalized)
        {
            if (ui.papBar != null)
                ui.papBar.value = normalized;
        }

        private void OnPapEmpty()
        {
            ShowMessage("Your pap bucket is EMPTY! Session over!", MessageType.System);
            StartCoroutine(DelayedAction(3f, ShowResults));
        }

        // ===== PROGRESSION =====

        private void OnLevelUp(int newLevel)
        {
            ShowMessage($"LEVEL UP! You're now Level {newLevel}!", MessageType.Achievement);
            UpdateLevelDisplay();
            NotificationManager.Instance?.ScheduleLevelUpNotification(newLevel);
        }

        // ===== DIP SELECTION =====

        private void PopulateDipButtons()
        {
            if (ui.dipScrollContent == null || BaitManager.Instance == null) return;

            // Clear old buttons
            foreach (Transform child in ui.dipScrollContent)
                Destroy(child.gameObject);

            var dips = BaitManager.Instance.AllDips;
            int playerLevel = PlayerProfile.Instance?.PlayerLevel ?? 1;

            foreach (var dip in dips)
            {
                bool locked = dip.playerLevelRequired > playerLevel;
                Color btnColor = locked ? new Color(0.3f, 0.3f, 0.3f) : GetDipColor(dip);
                string label = locked
                    ? $"{dip.dipName} (Lvl {dip.playerLevelRequired})"
                    : $"{dip.dipName} - {dip.description}";

                var btnGo = UIBuilder.CreateButton(ui.dipScrollContent, $"Dip_{dip.dipName}", label, btnColor, 18);
                var btnRect = btnGo.GetComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(0, 70);

                if (!locked)
                {
                    var captured = dip;
                    btnGo.GetComponent<Button>().onClick.AddListener(() => SelectDip(captured));
                }
                else
                {
                    btnGo.GetComponent<Button>().interactable = false;
                }
            }
        }

        private void SelectDip(DipData dip)
        {
            selectedDip = dip;
            BaitManager.Instance?.SelectDip(dip);

            // Show warning
            var warning = ui.gearSetupPanel.transform.Find("DipWarning")?.GetComponent<TextMeshProUGUI>();
            if (warning != null)
            {
                string warn = BaitManager.Instance?.GetDipWarning(dip);
                warning.text = warn ?? $"Selected: {dip.dipName}";
                warning.color = warn != null ? new Color(0.9f, 0.25f, 0.2f) : new Color(0.2f, 0.75f, 0.3f);
            }

            ui.dipSelectionPanel.SetActive(false);
            ShowMessage($"Dip selected: {dip.dipName}\n{dip.mixingMessage}", MessageType.System);
        }

        // ===== RESULTS =====

        private void ShowResults()
        {
            FishingController.Instance?.ReelIn();

            // End session tracking and save to disk
            var session = SessionTracker.Instance?.EndSession();

            // Collect all manager state into save data before final write
            SaveBridge.CollectToSave(SaveManager.Instance?.CurrentSave);
            SaveManager.Instance?.Save();

            // Upload to cloud
            CloudSaveSync.Instance?.UploadSave();

            // Submit to leaderboards
            var p = PlayerProfile.Instance;
            if (p != null)
            {
                LeaderboardManager.Instance?.SubmitScores(
                    p.PlayerName, p.PlayerLevel, p.TotalFishCaught,
                    p.HeaviestFishWeight, p.HeaviestFishName,
                    p.TotalBarbelCaught, SaveManager.Instance?.CurrentSave?.statistics.totalSessionsPlayed ?? 0
                );
            }

            // Schedule re-engagement notifications
            NotificationManager.Instance?.ScheduleReengagementNotifications();
            NotificationManager.Instance?.ScheduleSessionReminder(session?.durationSeconds ?? 0f);

            ui.HideAll();
            ui.resultsPanel.SetActive(true);

            float duration = session?.durationSeconds ?? (Time.time - sessionStartTime);
            int minutes = Mathf.FloorToInt(duration / 60f);
            int seconds = Mathf.FloorToInt(duration % 60f);
            int xpEarned = session?.xpEarned ?? 0;
            int caught = session?.fishCaught ?? sessionCatches;
            int lost = session?.fishLost ?? 0;
            float weight = session?.totalWeightCaught ?? totalWeightCaught;
            string heaviest = session != null && session.heaviestCatchWeight > 0
                ? $"{session.heaviestCatchName} ({session.heaviestCatchWeight:F1}kg)"
                : "None";

            var save = SaveManager.Instance?.CurrentSave;
            int totalSessions = save?.statistics.totalSessionsPlayed ?? 0;

            var statsText = ui.resultsPanel.transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>();
            if (statsText != null)
            {
                statsText.text =
                    $"SESSION #{totalSessions} COMPLETE\n" +
                    $"Duration: {minutes}m {seconds}s\n\n" +
                    $"Fish Caught: {caught}\n" +
                    $"Fish Lost: {lost}\n" +
                    $"Total Weight: {weight:F1} kg\n" +
                    $"Biggest This Session: {heaviest}\n" +
                    $"XP Earned: +{xpEarned}\n" +
                    $"Chaos Events: {session?.chaosEventsTriggered ?? 0}\n\n" +
                    $"--- CAREER ---\n" +
                    $"Level: {PlayerProfile.Instance?.PlayerLevel ?? 1}\n" +
                    $"Total Catches: {PlayerProfile.Instance?.TotalFishCaught ?? 0}\n" +
                    $"All-Time Heaviest: {PlayerProfile.Instance?.HeaviestFishWeight ?? 0:F1} kg\n" +
                    $"Species Discovered: {PlayerProfile.Instance?.CaughtSpeciesCount ?? 0}/16\n\n" +
                    $"Progress saved!";
            }

            GameManager.Instance?.EndFishingSession();
        }

        // ===== HELPERS =====

        private void UpdateStatus(string text)
        {
            if (ui.statusText != null)
                ui.statusText.text = text;
        }

        private void UpdateLevelDisplay()
        {
            if (PlayerProfile.Instance == null) return;

            var levelText = ui.mainMenuPanel?.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            if (levelText != null)
                levelText.text = $"Level {PlayerProfile.Instance.PlayerLevel} {PlayerProfile.Instance.PlayerName}";

            if (ui.levelText != null)
                ui.levelText.text = $"Level {PlayerProfile.Instance.PlayerLevel}";
        }

        private void UpdateGearDisplay()
        {
            var em = EquipmentManager.Instance;
            if (em == null) return;

            var rodName = ui.gearSetupPanel.transform.Find("RodName")?.GetComponent<TextMeshProUGUI>();
            if (rodName != null && em.EquippedRod != null)
                rodName.text = em.EquippedRod.rodName;

            var reelName = ui.gearSetupPanel.transform.Find("ReelName")?.GetComponent<TextMeshProUGUI>();
            if (reelName != null && em.EquippedReel != null)
                reelName.text = em.EquippedReel.reelName;
        }

        private void ShowCastBar(bool show)
        {
            if (castBarObject == null && show)
            {
                castPowerBar = UIBuilder.CreateProgressBar(
                    ui.fishingHUDPanel.transform, "CastPowerBar",
                    new Color(0.95f, 0.75f, 0.15f), "POWER",
                    new Vector2(0.5f, 0.30f), new Vector2(500, 40));
                castBarObject = castPowerBar.gameObject;
            }

            if (castBarObject != null)
                castBarObject.SetActive(show);
        }

        private void UpdateCastBar(float normalized)
        {
            if (castPowerBar != null)
                castPowerBar.value = normalized;
        }

        private void ShowMessage(string text, MessageType type)
        {
            if (ui.messagePanel == null || ui.messageText == null) return;

            ui.messageText.text = text;
            switch (type)
            {
                case MessageType.CatchSuccess: ui.messageText.color = new Color(0.2f, 0.9f, 0.3f); break;
                case MessageType.CatchFail: ui.messageText.color = new Color(0.9f, 0.3f, 0.2f); break;
                case MessageType.Chaos: ui.messageText.color = new Color(1f, 0.6f, 0f); break;
                case MessageType.Achievement: ui.messageText.color = new Color(1f, 0.85f, 0f); break;
                default: ui.messageText.color = new Color(0.95f, 0.95f, 0.92f); break;
            }

            StartCoroutine(ShowMessageCoroutine());
        }

        private IEnumerator ShowMessageCoroutine()
        {
            ui.messagePanel.SetActive(true);

            // Fade in
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                if (ui.messageCanvasGroup != null)
                    ui.messageCanvasGroup.alpha = t / 0.3f;
                yield return null;
            }

            yield return new WaitForSecondsRealtime(3f);

            // Fade out
            t = 0f;
            while (t < 0.5f)
            {
                t += Time.unscaledDeltaTime;
                if (ui.messageCanvasGroup != null)
                    ui.messageCanvasGroup.alpha = 1f - (t / 0.5f);
                yield return null;
            }

            ui.messagePanel.SetActive(false);
        }

        private void ShowCatchMessage(FishData fish, float weight)
        {
            string[] barbelMsgs = {
                "He nearly took YOU with the rod, my bru!",
                "That barber gave you a hiding!",
                "Your ancestors felt that strike.",
                "Your arms are going to be sore tomorrow, my china."
            };
            string[] mudfishMsgs = {
                "Mudfish again? Shame.",
                "At this point, you're fishing for disappointment.",
                "This fish owes you nothing.",
                "That slime will never come off."
            };
            string[] carpMsgs = {
                "Lekker! That's a beauty carp!",
                "Now THAT'S what we came for, bru!",
                "Papgooi pays off! Nice carp!",
                "Get the camera! That's a boytjie!"
            };
            string[] legendaryMsgs = {
                "LEGENDARY CATCH! History books, bru!",
                "Screenshot this. Nobody's going to believe you."
            };

            string msg;
            if (fish.IsLegendary()) msg = legendaryMsgs[Random.Range(0, legendaryMsgs.Length)];
            else if (fish.IsBarbel()) msg = barbelMsgs[Random.Range(0, barbelMsgs.Length)];
            else if (fish.IsMudfish()) msg = mudfishMsgs[Random.Range(0, mudfishMsgs.Length)];
            else msg = carpMsgs[Random.Range(0, carpMsgs.Length)];

            ShowMessage(msg, MessageType.CatchSuccess);
        }

        private Color GetDipColor(DipData dip)
        {
            switch (dip.category)
            {
                case DipCategory.BarbelAttractor: return new Color(0.5f, 0.15f, 0.15f);
                case DipCategory.MudfishAttractor: return new Color(0.5f, 0.4f, 0.15f);
                case DipCategory.MudfishRepellent: return new Color(0.15f, 0.15f, 0.4f);
                case DipCategory.CarpSpecialist: return new Color(0.4f, 0.35f, 0.1f);
                default: return new Color(0.2f, 0.35f, 0.15f);
            }
        }

        private Color GetRarityColor(FishRarity rarity)
        {
            switch (rarity)
            {
                case FishRarity.Common: return Color.white;
                case FishRarity.Uncommon: return Color.green;
                case FishRarity.Rare: return new Color(0.2f, 0.5f, 1f);
                case FishRarity.Epic: return new Color(0.6f, 0.2f, 0.9f);
                case FishRarity.Legendary: return new Color(1f, 0.85f, 0f);
                default: return Color.white;
            }
        }

        private IEnumerator FlashText(TextMeshProUGUI text, float interval)
        {
            for (int i = 0; i < 6; i++)
            {
                text.enabled = !text.enabled;
                yield return new WaitForSeconds(interval);
            }
            text.enabled = true;
        }

        private IEnumerator DelayedAction(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        private void WireButton(GameObject panel, string buttonName, UnityEngine.Events.UnityAction action)
        {
            if (panel == null) return;
            var btnTransform = panel.transform.Find(buttonName);
            if (btnTransform == null) return;
            var btn = btnTransform.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(action);
        }

        private bool GetTapDown()
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
            return Input.GetMouseButtonDown(0);
        }

        private bool GetTapUp()
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) return true;
            return Input.GetMouseButtonUp(0);
        }

        private bool GetTapHeld()
        {
            if (Input.touchCount > 0) return true;
            return Input.GetMouseButton(0);
        }
    }
}
