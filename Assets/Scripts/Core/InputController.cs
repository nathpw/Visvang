using UnityEngine;
using Visvang.Fishing;
using Visvang.QTE;

namespace Visvang.Core
{
    /// <summary>
    /// Handles all player input and routes it to the appropriate system.
    /// Supports both touch (mobile) and keyboard/mouse (desktop).
    /// </summary>
    public class InputController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float castHoldThreshold = 0.1f;

        private bool isCastHolding;
        private float castHoldTime;
        private bool isReeling;

        private void Update()
        {
            HandleFishingInput();
            HandleQTEInput();
        }

        private void HandleFishingInput()
        {
            var fishing = FishingController.Instance;
            if (fishing == null) return;

            switch (fishing.CurrentState)
            {
                case FishingState.Idle:
                    HandleIdleInput(fishing);
                    break;
                case FishingState.Casting:
                    HandleCastingInput(fishing);
                    break;
                case FishingState.Waiting:
                    HandleWaitingInput(fishing);
                    break;
                case FishingState.BiteDetected:
                    HandleBiteInput(fishing);
                    break;
                case FishingState.Fighting:
                    HandleFightingInput(fishing);
                    break;
                case FishingState.RodPulledIn:
                    HandleRodPulledInput(fishing);
                    break;
            }
        }

        private void HandleIdleInput(FishingController fishing)
        {
            // Tap/click to start cast
            if (GetTapDown())
            {
                fishing.StartCast();
                isCastHolding = true;
                castHoldTime = 0f;
            }
        }

        private void HandleCastingInput(FishingController fishing)
        {
            if (isCastHolding)
            {
                castHoldTime += Time.deltaTime;

                // Release to cast
                if (GetTapUp())
                {
                    isCastHolding = false;
                    float power = Mathf.Clamp01(castHoldTime / 2f) * Constants.MAX_CAST_POWER;
                    fishing.ReleaseCast(power, 0f);
                }
            }
        }

        private void HandleWaitingInput(FishingController fishing)
        {
            // Long press to reel in (give up)
            if (GetTapDown() && Input.GetKey(KeyCode.R))
                fishing.ReelIn();
        }

        private void HandleBiteInput(FishingController fishing)
        {
            // Tap to strike!
            if (GetTapDown())
                fishing.Strike();
        }

        private void HandleFightingInput(FishingController fishing)
        {
            // Hold to reel
            if (GetTapHeld())
            {
                fishing.ReelFight(1f);
                isReeling = true;
            }
            else
            {
                isReeling = false;
            }

            // Double tap to wash hands (mudfish slime)
            if (Input.GetKeyDown(KeyCode.W))
                fishing.WashHands();
        }

        private void HandleRodPulledInput(FishingController fishing)
        {
            // Tap rapidly to retrieve rod
            if (GetTapDown())
            {
                QTEManager.Instance?.RegisterTap();
            }
        }

        private void HandleQTEInput()
        {
            var qte = QTEManager.Instance;
            if (qte == null || !qte.IsQTEActive) return;

            switch (qte.ActiveQTEType)
            {
                case QTEType.Tap:
                case QTEType.GripStrength:
                case QTEType.SlimeWash:
                    if (GetTapDown())
                        qte.RegisterTap();
                    break;

                case QTEType.Hold:
                    qte.RegisterHold(GetTapHeld());
                    break;
            }
        }

        // --- Input Abstraction (supports touch + mouse) ---

        private bool GetTapDown()
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                return true;
            return Input.GetMouseButtonDown(0);
        }

        private bool GetTapUp()
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
                return true;
            return Input.GetMouseButtonUp(0);
        }

        private bool GetTapHeld()
        {
            if (Input.touchCount > 0)
                return true;
            return Input.GetMouseButton(0);
        }
    }
}
