using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Visvang.Core;

namespace Visvang.UI
{
    /// <summary>
    /// Programmatically creates the entire game UI.
    /// Creates Canvas, all panels, buttons, bars, and text elements.
    /// Call Build() after all managers are initialized.
    /// </summary>
    public static class UIBuilder
    {
        private static Canvas mainCanvas;
        private static CanvasScaler scaler;

        // Color palette - SA fishing vibes
        private static readonly Color bgDark = new Color(0.08f, 0.12f, 0.18f, 0.92f);
        private static readonly Color bgPanel = new Color(0.12f, 0.16f, 0.22f, 0.95f);
        private static readonly Color accentGold = new Color(0.95f, 0.75f, 0.15f);
        private static readonly Color accentGreen = new Color(0.2f, 0.75f, 0.3f);
        private static readonly Color accentRed = new Color(0.9f, 0.25f, 0.2f);
        private static readonly Color accentBlue = new Color(0.2f, 0.6f, 0.9f);
        private static readonly Color textWhite = new Color(0.95f, 0.95f, 0.92f);
        private static readonly Color textMuted = new Color(0.6f, 0.65f, 0.7f);
        private static readonly Color barBg = new Color(0.15f, 0.15f, 0.2f, 0.8f);
        private static readonly Color slimeGreen = new Color(0.3f, 0.7f, 0.1f);

        public static UIReferences Build()
        {
            var refs = new UIReferences();

            // Canvas
            var canvasGo = new GameObject("GameCanvas");
            mainCanvas = canvasGo.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 10;

            scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // Event System
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var eventGo = new GameObject("EventSystem");
                eventGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Build all panels
            refs.mainMenuPanel = BuildMainMenu(canvasGo.transform);
            refs.gearSetupPanel = BuildGearSetup(canvasGo.transform);
            refs.fishingHUDPanel = BuildFishingHUD(canvasGo.transform, refs);
            refs.fightPanel = BuildFightPanel(canvasGo.transform, refs);
            refs.fishCaughtPanel = BuildFishCaughtPanel(canvasGo.transform, refs);
            refs.chaosEventPanel = BuildChaosPanel(canvasGo.transform, refs);
            refs.messagePanel = BuildMessagePanel(canvasGo.transform, refs);
            refs.resultsPanel = BuildResultsPanel(canvasGo.transform);
            refs.pausePanel = BuildPausePanel(canvasGo.transform);
            refs.dipSelectionPanel = BuildDipSelectionPanel(canvasGo.transform, refs);

            // Hide all initially except main menu
            refs.HideAll();
            refs.mainMenuPanel.SetActive(true);

            return refs;
        }

        // --- MAIN MENU ---
        private static GameObject BuildMainMenu(Transform parent)
        {
            var panel = CreatePanel(parent, "MainMenuPanel", true);
            var rect = panel.GetComponent<RectTransform>();
            StretchFull(rect);

            // Title
            var title = CreateText(panel.transform, "Title", "VISVANG", 72, accentGold);
            SetAnchored(title, new Vector2(0.5f, 0.78f), new Vector2(800, 100));

            var subtitle = CreateText(panel.transform, "Subtitle", "South Africa's Carp Fishing Chaos Simulator", 24, textMuted);
            SetAnchored(subtitle, new Vector2(0.5f, 0.72f), new Vector2(800, 50));

            // Start button
            var startBtn = CreateButton(panel.transform, "StartButton", "START FISHING", accentGreen, 36);
            SetAnchored(startBtn, new Vector2(0.5f, 0.45f), new Vector2(500, 90));

            // Multiplayer button
            var mpBtn = CreateButton(panel.transform, "MultiplayerButton", "MULTIPLAYER", accentBlue, 28);
            SetAnchored(mpBtn, new Vector2(0.5f, 0.35f), new Vector2(400, 70));

            // Profile/Stats button
            var statsBtn = CreateButton(panel.transform, "StatsButton", "MY STATS", accentGold, 24);
            SetAnchored(statsBtn, new Vector2(0.5f, 0.27f), new Vector2(350, 60));

            // Level info
            var levelText = CreateText(panel.transform, "LevelText", "Level 1 Hengelaar", 20, textMuted);
            SetAnchored(levelText, new Vector2(0.5f, 0.15f), new Vector2(400, 40));

            // Fish emoji decoration
            var deco = CreateText(panel.transform, "Decoration", "--- o<))>< ---", 28, textMuted);
            SetAnchored(deco, new Vector2(0.5f, 0.60f), new Vector2(600, 50));

            return panel;
        }

        // --- GEAR SETUP ---
        private static GameObject BuildGearSetup(Transform parent)
        {
            var panel = CreatePanel(parent, "GearSetupPanel", true);
            StretchFull(panel.GetComponent<RectTransform>());

            var title = CreateText(panel.transform, "Title", "GEAR UP", 48, accentGold);
            SetAnchored(title, new Vector2(0.5f, 0.92f), new Vector2(400, 70));

            // Rod section
            var rodLabel = CreateText(panel.transform, "RodLabel", "ROD:", 24, textWhite);
            SetAnchored(rodLabel, new Vector2(0.2f, 0.78f), new Vector2(200, 40));

            var rodName = CreateText(panel.transform, "RodName", "Makro Plastic Special", 22, accentGold);
            SetAnchored(rodName, new Vector2(0.6f, 0.78f), new Vector2(400, 40));

            // Reel section
            var reelLabel = CreateText(panel.transform, "ReelLabel", "REEL:", 24, textWhite);
            SetAnchored(reelLabel, new Vector2(0.2f, 0.71f), new Vector2(200, 40));

            var reelName = CreateText(panel.transform, "ReelName", "Budget Plastic Reel", 22, accentGold);
            SetAnchored(reelName, new Vector2(0.6f, 0.71f), new Vector2(400, 40));

            // Dip selection
            var dipLabel = CreateText(panel.transform, "DipLabel", "SELECT YOUR DIP:", 28, textWhite);
            SetAnchored(dipLabel, new Vector2(0.5f, 0.60f), new Vector2(500, 50));

            var dipWarning = CreateText(panel.transform, "DipWarning", "", 18, accentRed);
            SetAnchored(dipWarning, new Vector2(0.5f, 0.20f), new Vector2(600, 40));

            // Go fishing button
            var goBtn = CreateButton(panel.transform, "GoFishingButton", "GOOI DIE PAP!", accentGreen, 32);
            SetAnchored(goBtn, new Vector2(0.5f, 0.08f), new Vector2(450, 80));

            return panel;
        }

        // --- FISHING HUD ---
        private static GameObject BuildFishingHUD(Transform parent, UIReferences refs)
        {
            var panel = CreatePanel(parent, "FishingHUDPanel", false);
            StretchFull(panel.GetComponent<RectTransform>());

            // Top bar - time, weather, level
            var topBar = CreatePanel(panel.transform, "TopBar", false);
            var topRect = topBar.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 0.93f);
            topRect.anchorMax = new Vector2(1, 1f);
            topRect.offsetMin = Vector2.zero;
            topRect.offsetMax = Vector2.zero;
            var topImg = topBar.GetComponent<Image>();
            topImg.color = bgDark;

            refs.timeText = CreateText(topBar.transform, "TimeText", "Morning", 20, textWhite).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.timeText.gameObject, new Vector2(0.15f, 0.5f), new Vector2(150, 35));

            refs.weatherText = CreateText(topBar.transform, "WeatherText", "Clear", 20, textWhite).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.weatherText.gameObject, new Vector2(0.5f, 0.5f), new Vector2(150, 35));

            refs.levelText = CreateText(topBar.transform, "LevelText", "Level 1", 20, accentGold).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.levelText.gameObject, new Vector2(0.85f, 0.5f), new Vector2(150, 35));

            // Pap bucket bar (bottom left)
            refs.papBar = CreateProgressBar(panel.transform, "PapBar", accentGold, "Pap: 100%", new Vector2(0.18f, 0.06f), new Vector2(250, 35));

            // Status text (center)
            refs.statusText = CreateText(panel.transform, "StatusText", "Ready to cast", 28, textWhite).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.statusText.gameObject, new Vector2(0.5f, 0.50f), new Vector2(600, 60));

            // Cast instruction
            refs.castInstructionText = CreateText(panel.transform, "CastInstruction", "TAP AND HOLD TO CAST", 22, accentGold).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.castInstructionText.gameObject, new Vector2(0.5f, 0.42f), new Vector2(500, 40));

            // XP bar (top, under the top bar)
            refs.xpBar = CreateProgressBar(panel.transform, "XPBar", accentBlue, "XP", new Vector2(0.5f, 0.905f), new Vector2(800, 12));

            // Pause button
            var pauseBtn = CreateButton(panel.transform, "PauseButton", "||", new Color(0.4f, 0.4f, 0.4f), 24);
            SetAnchored(pauseBtn, new Vector2(0.95f, 0.06f), new Vector2(60, 60));

            return panel;
        }

        // --- FIGHT PANEL (overlays on HUD) ---
        private static GameObject BuildFightPanel(Transform parent, UIReferences refs)
        {
            var panel = CreatePanel(parent, "FightPanel", false);
            StretchFull(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0); // Transparent overlay

            // Fish name and weight
            refs.fightFishName = CreateText(panel.transform, "FishName", "", 32, accentGold).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.fightFishName.gameObject, new Vector2(0.5f, 0.88f), new Vector2(500, 50));

            refs.fightFishWeight = CreateText(panel.transform, "FishWeight", "", 22, textWhite).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.fightFishWeight.gameObject, new Vector2(0.5f, 0.84f), new Vector2(300, 35));

            // Tension bar
            var tensionLabel = CreateText(panel.transform, "TensionLabel", "LINE TENSION", 16, textMuted);
            SetAnchored(tensionLabel, new Vector2(0.5f, 0.20f), new Vector2(300, 25));
            refs.tensionBar = CreateProgressBar(panel.transform, "TensionBar", accentGreen, "", new Vector2(0.5f, 0.17f), new Vector2(600, 30));

            // Fight progress bar
            var progressLabel = CreateText(panel.transform, "ProgressLabel", "REEL PROGRESS", 16, textMuted);
            SetAnchored(progressLabel, new Vector2(0.5f, 0.78f), new Vector2(300, 25));
            refs.fightProgressBar = CreateProgressBar(panel.transform, "FightProgress", accentBlue, "", new Vector2(0.5f, 0.75f), new Vector2(600, 25));

            // Slime meter
            refs.slimePanel = new GameObject("SlimePanel");
            refs.slimePanel.transform.SetParent(panel.transform, false);
            var slimeLabel = CreateText(refs.slimePanel.transform, "SlimeLabel", "SLIME", 14, slimeGreen);
            SetAnchored(slimeLabel, new Vector2(0.5f, 0.7f), new Vector2(200, 25));
            refs.slimeBar = CreateProgressBar(refs.slimePanel.transform, "SlimeBar", slimeGreen, "", new Vector2(0.5f, 0.3f), new Vector2(200, 20));
            var slimeRect = refs.slimePanel.AddComponent<RectTransform>();
            SetAnchored(refs.slimePanel, new Vector2(0.88f, 0.30f), new Vector2(150, 80));
            refs.slimePanel.SetActive(false);

            // Grip bar
            refs.gripPanel = new GameObject("GripPanel");
            refs.gripPanel.transform.SetParent(panel.transform, false);
            var gripLabel = CreateText(refs.gripPanel.transform, "GripLabel", "GRIP!", 18, accentRed);
            SetAnchored(gripLabel, new Vector2(0.5f, 0.7f), new Vector2(200, 30));
            refs.gripBar = CreateProgressBar(refs.gripPanel.transform, "GripBar", accentRed, "", new Vector2(0.5f, 0.3f), new Vector2(250, 25));
            refs.gripPanel.AddComponent<RectTransform>();
            SetAnchored(refs.gripPanel, new Vector2(0.5f, 0.55f), new Vector2(300, 100));
            refs.gripPanel.SetActive(false);

            // Death roll warning
            refs.deathRollWarning = CreateText(panel.transform, "DeathRollWarning", "DEATH ROLL! HOLD ON!", 36, accentRed).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.deathRollWarning.gameObject, new Vector2(0.5f, 0.55f), new Vector2(600, 60));
            refs.deathRollWarning.gameObject.SetActive(false);

            // Disorient overlay
            refs.disorientOverlay = CreatePanel(panel.transform, "DisorientOverlay", false);
            refs.disorientOverlay.GetComponent<Image>().color = new Color(1f, 0.5f, 0f, 0.3f);
            StretchFull(refs.disorientOverlay.GetComponent<RectTransform>());
            refs.disorientOverlay.SetActive(false);

            // Reel instruction
            refs.reelInstruction = CreateText(panel.transform, "ReelInstruction", "HOLD TO REEL!", 26, textWhite).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.reelInstruction.gameObject, new Vector2(0.5f, 0.40f), new Vector2(400, 50));

            return panel;
        }

        // --- FISH CAUGHT PANEL ---
        private static GameObject BuildFishCaughtPanel(Transform parent, UIReferences refs)
        {
            var panel = CreatePanel(parent, "FishCaughtPanel", true);
            StretchFull(panel.GetComponent<RectTransform>());

            var title = CreateText(panel.transform, "CatchTitle", "FISH CAUGHT!", 48, accentGold);
            SetAnchored(title, new Vector2(0.5f, 0.88f), new Vector2(600, 70));

            // Fish frame area
            var frame = CreatePanel(panel.transform, "FishFrame", true);
            frame.GetComponent<Image>().color = new Color(0.1f, 0.15f, 0.25f);
            SetAnchored(frame, new Vector2(0.5f, 0.62f), new Vector2(400, 300));

            refs.catchFishName = CreateText(frame.transform, "CatchFishName", "", 36, accentGold).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.catchFishName.gameObject, new Vector2(0.5f, 0.85f), new Vector2(350, 50));

            refs.catchSpecies = CreateText(frame.transform, "CatchSpecies", "", 22, textMuted).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.catchSpecies.gameObject, new Vector2(0.5f, 0.7f), new Vector2(350, 35));

            refs.catchWeight = CreateText(frame.transform, "CatchWeight", "", 32, textWhite).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.catchWeight.gameObject, new Vector2(0.5f, 0.45f), new Vector2(350, 50));

            refs.catchRarity = CreateText(frame.transform, "CatchRarity", "", 24, accentGold).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.catchRarity.gameObject, new Vector2(0.5f, 0.25f), new Vector2(350, 40));

            // Badges
            refs.firstCatchBadge = CreateText(panel.transform, "FirstCatchBadge", "FIRST CATCH!", 20, accentGreen);
            SetAnchored(refs.firstCatchBadge, new Vector2(0.25f, 0.45f), new Vector2(200, 35));
            refs.firstCatchBadge.SetActive(false);

            refs.newRecordBadge = CreateText(panel.transform, "NewRecordBadge", "NEW RECORD!", 20, accentRed);
            SetAnchored(refs.newRecordBadge, new Vector2(0.75f, 0.45f), new Vector2(200, 35));
            refs.newRecordBadge.SetActive(false);

            refs.legendaryBadge = CreateText(panel.transform, "LegendaryBadge", "LEGENDARY!", 28, accentGold);
            SetAnchored(refs.legendaryBadge, new Vector2(0.5f, 0.48f), new Vector2(300, 45));
            refs.legendaryBadge.SetActive(false);

            // XP gained
            refs.catchXPText = CreateText(panel.transform, "XPGained", "+50 XP", 32, accentBlue).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.catchXPText.gameObject, new Vector2(0.5f, 0.35f), new Vector2(300, 50));

            // Continue button
            var continueBtn = CreateButton(panel.transform, "ContinueButton", "KEEP FISHING", accentGreen, 28);
            SetAnchored(continueBtn, new Vector2(0.5f, 0.10f), new Vector2(400, 70));

            return panel;
        }

        // --- CHAOS EVENT PANEL ---
        private static GameObject BuildChaosPanel(Transform parent, UIReferences refs)
        {
            var panel = CreatePanel(parent, "ChaosPanel", false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.35f);
            rect.anchorMax = new Vector2(0.95f, 0.65f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.15f, 0.05f, 0.05f, 0.95f);

            // Add outline
            var outline = panel.AddComponent<Outline>();
            outline.effectColor = accentRed;
            outline.effectDistance = new Vector2(3, 3);

            refs.chaosTitle = CreateText(panel.transform, "ChaosTitle", "CHAOS EVENT!", 36, accentRed).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.chaosTitle.gameObject, new Vector2(0.5f, 0.75f), new Vector2(500, 50));

            refs.chaosMessage = CreateText(panel.transform, "ChaosMessage", "", 22, textWhite).GetComponent<TextMeshProUGUI>();
            SetAnchored(refs.chaosMessage.gameObject, new Vector2(0.5f, 0.40f), new Vector2(600, 80));

            return panel;
        }

        // --- MESSAGE PANEL (floating messages) ---
        private static GameObject BuildMessagePanel(Transform parent, UIReferences refs)
        {
            var panel = new GameObject("MessagePanel");
            panel.transform.SetParent(parent, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.05f, 0.7f);
            panelRect.anchorMax = new Vector2(0.95f, 0.85f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var canvasGroup = panel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.08f, 0.15f, 0.85f);

            refs.messageText = CreateText(panel.transform, "MessageText", "", 24, textWhite).GetComponent<TextMeshProUGUI>();
            var msgRect = refs.messageText.GetComponent<RectTransform>();
            StretchFull(msgRect);
            msgRect.offsetMin = new Vector2(20, 10);
            msgRect.offsetMax = new Vector2(-20, -10);
            refs.messageText.alignment = TextAlignmentOptions.Center;

            refs.messageCanvasGroup = canvasGroup;

            panel.SetActive(false);
            return panel;
        }

        // --- RESULTS PANEL ---
        private static GameObject BuildResultsPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "ResultsPanel", true);
            StretchFull(panel.GetComponent<RectTransform>());

            var title = CreateText(panel.transform, "Title", "SESSION RESULTS", 42, accentGold);
            SetAnchored(title, new Vector2(0.5f, 0.88f), new Vector2(600, 60));

            var statsText = CreateText(panel.transform, "StatsText", "", 22, textWhite);
            SetAnchored(statsText, new Vector2(0.5f, 0.55f), new Vector2(600, 400));

            var menuBtn = CreateButton(panel.transform, "BackToMenuButton", "BACK TO MENU", accentGold, 28);
            SetAnchored(menuBtn, new Vector2(0.5f, 0.10f), new Vector2(400, 70));

            return panel;
        }

        // --- PAUSE PANEL ---
        private static GameObject BuildPausePanel(Transform parent)
        {
            var panel = CreatePanel(parent, "PausePanel", true);
            StretchFull(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            var title = CreateText(panel.transform, "Title", "PAUSED", 48, textWhite);
            SetAnchored(title, new Vector2(0.5f, 0.65f), new Vector2(400, 70));

            var resumeBtn = CreateButton(panel.transform, "ResumeButton", "RESUME", accentGreen, 32);
            SetAnchored(resumeBtn, new Vector2(0.5f, 0.45f), new Vector2(350, 70));

            var quitBtn = CreateButton(panel.transform, "QuitButton", "QUIT TO MENU", accentRed, 28);
            SetAnchored(quitBtn, new Vector2(0.5f, 0.32f), new Vector2(350, 60));

            return panel;
        }

        // --- DIP SELECTION PANEL ---
        private static GameObject BuildDipSelectionPanel(Transform parent, UIReferences refs)
        {
            var panel = CreatePanel(parent, "DipSelectionPanel", true);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.05f, 0.15f);
            panelRect.anchorMax = new Vector2(0.95f, 0.85f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var title = CreateText(panel.transform, "Title", "CHOOSE YOUR DIP", 32, accentGold);
            SetAnchored(title, new Vector2(0.5f, 0.92f), new Vector2(500, 50));

            // Scroll content for dip buttons (created dynamically by GameFlow)
            var scrollGo = new GameObject("DipScrollContent");
            scrollGo.transform.SetParent(panel.transform, false);
            var scrollRect = scrollGo.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.10f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.85f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;

            var vlg = scrollGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(10, 10, 10, 10);

            var csf = scrollGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            refs.dipScrollContent = scrollGo.transform;

            return panel;
        }

        // === HELPER METHODS ===

        public static GameObject CreatePanel(Transform parent, string name, bool hasBackground)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();

            if (hasBackground)
            {
                var img = go.AddComponent<Image>();
                img.color = bgPanel;
            }

            return go;
        }

        public static GameObject CreateText(Transform parent, string name, string text, int fontSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return go;
        }

        public static GameObject CreateButton(Transform parent, string name, string label, Color bgColor, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            StretchFull(textRect);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return go;
        }

        public static Slider CreateProgressBar(Transform parent, string name, Color fillColor, string label, Vector2 anchorPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            SetAnchored(go, anchorPos, size);

            // Background
            var bgImg = go.AddComponent<Image>();
            bgImg.color = barBg;

            // Fill area
            var fillArea = new GameObject("FillArea");
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            StretchFull(fillAreaRect);
            fillAreaRect.offsetMin = new Vector2(2, 2);
            fillAreaRect.offsetMax = new Vector2(-2, -2);

            // Fill
            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;

            // Slider component
            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;

            // Label text
            if (!string.IsNullOrEmpty(label))
            {
                var labelGo = CreateText(go.transform, "Label", label, 14, textWhite);
                var labelRect = labelGo.GetComponent<RectTransform>();
                StretchFull(labelRect);
            }

            return slider;
        }

        public static void SetAnchored(GameObject go, Vector2 anchorCenter, Vector2 size)
        {
            var rect = go.GetComponent<RectTransform>();
            if (rect == null) rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorCenter;
            rect.anchorMax = anchorCenter;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
        }

        public static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    /// <summary>
    /// Holds references to all UI elements created by UIBuilder.
    /// Passed to GameFlow and UI controllers.
    /// </summary>
    public class UIReferences
    {
        // Panels
        public GameObject mainMenuPanel;
        public GameObject gearSetupPanel;
        public GameObject fishingHUDPanel;
        public GameObject fightPanel;
        public GameObject fishCaughtPanel;
        public GameObject chaosEventPanel;
        public GameObject messagePanel;
        public GameObject resultsPanel;
        public GameObject pausePanel;
        public GameObject dipSelectionPanel;

        // HUD elements
        public TextMeshProUGUI timeText;
        public TextMeshProUGUI weatherText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI castInstructionText;
        public Slider papBar;
        public Slider xpBar;

        // Fight elements
        public TextMeshProUGUI fightFishName;
        public TextMeshProUGUI fightFishWeight;
        public Slider tensionBar;
        public Slider fightProgressBar;
        public GameObject slimePanel;
        public Slider slimeBar;
        public GameObject gripPanel;
        public Slider gripBar;
        public TextMeshProUGUI deathRollWarning;
        public GameObject disorientOverlay;
        public TextMeshProUGUI reelInstruction;

        // Catch panel
        public TextMeshProUGUI catchFishName;
        public TextMeshProUGUI catchSpecies;
        public TextMeshProUGUI catchWeight;
        public TextMeshProUGUI catchRarity;
        public TextMeshProUGUI catchXPText;
        public GameObject firstCatchBadge;
        public GameObject newRecordBadge;
        public GameObject legendaryBadge;

        // Chaos
        public TextMeshProUGUI chaosTitle;
        public TextMeshProUGUI chaosMessage;

        // Messages
        public TextMeshProUGUI messageText;
        public CanvasGroup messageCanvasGroup;

        // Dip selection
        public Transform dipScrollContent;

        public void HideAll()
        {
            mainMenuPanel?.SetActive(false);
            gearSetupPanel?.SetActive(false);
            fishingHUDPanel?.SetActive(false);
            fightPanel?.SetActive(false);
            fishCaughtPanel?.SetActive(false);
            chaosEventPanel?.SetActive(false);
            messagePanel?.SetActive(false);
            resultsPanel?.SetActive(false);
            pausePanel?.SetActive(false);
            dipSelectionPanel?.SetActive(false);
        }
    }
}
