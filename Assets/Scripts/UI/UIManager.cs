using UnityEngine;
using Visvang.Core;
using Visvang.Fishing;
using Visvang.Events;

namespace Visvang.UI
{
    /// <summary>
    /// Central UI manager. Activates/deactivates panels based on game phase.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gearSetupPanel;
        [SerializeField] private GameObject fishingHUDPanel;
        [SerializeField] private GameObject fightPanel;
        [SerializeField] private GameObject fishCaughtPanel;
        [SerializeField] private GameObject chaosEventPanel;
        [SerializeField] private GameObject qtePanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject baitSelectionPanel;
        [SerializeField] private GameObject equipmentPanel;

        [Header("Controllers")]
        [SerializeField] private HUDController hudController;
        [SerializeField] private FishCaughtPanel catchPanel;
        [SerializeField] private MessageSystem messageSystem;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;

            if (FishingController.Instance != null)
                FishingController.Instance.OnStateChanged += HandleFishingStateChanged;

            if (ChaosEventManager.Instance != null)
            {
                ChaosEventManager.Instance.OnChaosMessage += ShowChaosMessage;
            }

            HideAllPanels();
            ShowPanel(mainMenuPanel);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;

            if (FishingController.Instance != null)
                FishingController.Instance.OnStateChanged -= HandleFishingStateChanged;

            if (ChaosEventManager.Instance != null)
                ChaosEventManager.Instance.OnChaosMessage -= ShowChaosMessage;
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            HideAllPanels();

            switch (phase)
            {
                case GamePhase.MainMenu:
                    ShowPanel(mainMenuPanel);
                    break;
                case GamePhase.GearSetup:
                    ShowPanel(gearSetupPanel);
                    break;
                case GamePhase.Fishing:
                    ShowPanel(fishingHUDPanel);
                    break;
                case GamePhase.FishFight:
                    ShowPanel(fishingHUDPanel);
                    ShowPanel(fightPanel);
                    break;
                case GamePhase.FishCaught:
                    ShowPanel(fishCaughtPanel);
                    break;
                case GamePhase.ChaosEvent:
                    ShowPanel(fishingHUDPanel);
                    ShowPanel(chaosEventPanel);
                    break;
                case GamePhase.Results:
                    ShowPanel(resultsPanel);
                    break;
                case GamePhase.Paused:
                    ShowPanel(pausePanel);
                    break;
            }
        }

        private void HandleFishingStateChanged(FishingState state)
        {
            if (qtePanel != null)
                qtePanel.SetActive(state == FishingState.Landing || state == FishingState.RodPulledIn);
        }

        private void ShowChaosMessage(string message)
        {
            if (messageSystem != null)
                messageSystem.ShowMessage(message, MessageType.Chaos);
        }

        public void ShowBaitSelection()
        {
            ShowPanel(baitSelectionPanel);
        }

        public void ShowEquipmentSelection()
        {
            ShowPanel(equipmentPanel);
        }

        public void CloseBaitSelection()
        {
            HidePanel(baitSelectionPanel);
        }

        public void CloseEquipmentSelection()
        {
            HidePanel(equipmentPanel);
        }

        private void HideAllPanels()
        {
            HidePanel(mainMenuPanel);
            HidePanel(gearSetupPanel);
            HidePanel(fishingHUDPanel);
            HidePanel(fightPanel);
            HidePanel(fishCaughtPanel);
            HidePanel(chaosEventPanel);
            HidePanel(qtePanel);
            HidePanel(resultsPanel);
            HidePanel(pausePanel);
            HidePanel(baitSelectionPanel);
            HidePanel(equipmentPanel);
        }

        private void ShowPanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(true);
        }

        private void HidePanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
