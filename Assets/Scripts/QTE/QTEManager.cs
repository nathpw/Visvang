using UnityEngine;
using System;
using System.Collections;
using Visvang.Core;

namespace Visvang.QTE
{
    /// <summary>
    /// Quick Time Event manager. Handles tap, hold, grip strength, and slime wash QTEs.
    /// Species-specific QTEs triggered during fights and chaos events.
    /// </summary>
    public class QTEManager : MonoBehaviour
    {
        public static QTEManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private bool isQTEActive;
        [SerializeField] private QTEType activeQTEType;
        [SerializeField] private float qteTimer;
        [SerializeField] private float qteProgress;

        [Header("Tap QTE Settings")]
        [SerializeField] private int requiredTaps = 10;
        [SerializeField] private float tapWindow = 3f;

        [Header("Hold QTE Settings")]
        [SerializeField] private float holdDuration = 2f;

        [Header("Grip QTE Settings")]
        [SerializeField] private float gripDecayRate = Constants.GRIP_DECAY_RATE;
        [SerializeField] private float gripRecoveryPerTap = 15f;
        [SerializeField] private float gripThreshold = 30f;

        private int tapCount;
        private float holdTimer;
        private float gripStrength;
        private bool qteSucceeded;

        public bool IsQTEActive => isQTEActive;
        public QTEType ActiveQTEType => activeQTEType;
        public float QTEProgress => qteProgress;
        public float QTETimer => qteTimer;
        public float GripStrength => gripStrength;
        public int TapCount => tapCount;
        public int RequiredTaps => requiredTaps;

        public event Action<QTEType> OnQTEStarted;
        public event Action<QTEType, bool> OnQTECompleted;
        public event Action<float> OnQTEProgressChanged;
        public event Action<float> OnGripChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartQTE(QTEType type)
        {
            if (isQTEActive) return;

            isQTEActive = true;
            activeQTEType = type;
            qteProgress = 0f;
            qteSucceeded = false;
            tapCount = 0;
            holdTimer = 0f;

            switch (type)
            {
                case QTEType.Tap:
                    qteTimer = tapWindow;
                    break;
                case QTEType.Hold:
                    qteTimer = holdDuration + 1f;
                    break;
                case QTEType.GripStrength:
                    qteTimer = Constants.QTE_GRIP_DURATION;
                    gripStrength = Constants.MAX_GRIP_STRENGTH;
                    break;
                case QTEType.SlimeWash:
                    qteTimer = Constants.SLIME_WASH_TIME * 3f;
                    break;
                case QTEType.DirectionalSwipe:
                    qteTimer = 2f;
                    break;
            }

            OnQTEStarted?.Invoke(type);
        }

        private void Update()
        {
            if (!isQTEActive) return;

            qteTimer -= Time.deltaTime;

            switch (activeQTEType)
            {
                case QTEType.Tap:
                    UpdateTapQTE();
                    break;
                case QTEType.Hold:
                    UpdateHoldQTE();
                    break;
                case QTEType.GripStrength:
                    UpdateGripQTE();
                    break;
                case QTEType.SlimeWash:
                    UpdateSlimeWashQTE();
                    break;
            }

            // Time expired
            if (qteTimer <= 0f && !qteSucceeded)
            {
                CompleteQTE(false);
            }
        }

        // --- TAP QTE (Mudfish net escape, bird chase) ---

        private void UpdateTapQTE()
        {
            qteProgress = (float)tapCount / requiredTaps;
            OnQTEProgressChanged?.Invoke(qteProgress);

            if (tapCount >= requiredTaps)
                CompleteQTE(true);
        }

        /// <summary>
        /// Call this when player taps during a Tap QTE.
        /// </summary>
        public void RegisterTap()
        {
            if (!isQTEActive) return;

            if (activeQTEType == QTEType.Tap)
            {
                tapCount++;
            }
            else if (activeQTEType == QTEType.GripStrength)
            {
                gripStrength = Mathf.Min(Constants.MAX_GRIP_STRENGTH, gripStrength + gripRecoveryPerTap);
                OnGripChanged?.Invoke(gripStrength / Constants.MAX_GRIP_STRENGTH);
            }
            else if (activeQTEType == QTEType.SlimeWash)
            {
                qteProgress += 0.15f;
                OnQTEProgressChanged?.Invoke(qteProgress);

                if (qteProgress >= 1f)
                    CompleteQTE(true);
            }
        }

        // --- HOLD QTE (Landing standard fish) ---

        private void UpdateHoldQTE()
        {
            // Player must hold button - progress tracks how long they've held
            qteProgress = holdTimer / holdDuration;
            OnQTEProgressChanged?.Invoke(qteProgress);

            if (holdTimer >= holdDuration)
                CompleteQTE(true);
        }

        /// <summary>
        /// Call each frame the player is holding during a Hold QTE.
        /// </summary>
        public void RegisterHold(bool isHolding)
        {
            if (!isQTEActive || activeQTEType != QTEType.Hold) return;

            if (isHolding)
                holdTimer += Time.deltaTime;
            else
                holdTimer = Mathf.Max(0f, holdTimer - Time.deltaTime * 2f); // Penalty for releasing
        }

        // --- GRIP STRENGTH QTE (Barbel death roll, landing) ---

        private void UpdateGripQTE()
        {
            // Grip decays, player must tap/mash to keep it up
            gripStrength -= gripDecayRate * Time.deltaTime;
            gripStrength = Mathf.Max(0f, gripStrength);

            OnGripChanged?.Invoke(gripStrength / Constants.MAX_GRIP_STRENGTH);

            // Fail if grip drops too low
            if (gripStrength < gripThreshold * 0.3f)
            {
                CompleteQTE(false);
                return;
            }

            // Succeed if timer expires with sufficient grip
            if (qteTimer <= 0f && gripStrength >= gripThreshold)
                CompleteQTE(true);
        }

        // --- SLIME WASH QTE (Mudfish slime on hands) ---

        private void UpdateSlimeWashQTE()
        {
            // Player must tap rapidly to wash slime off
            // Progress handled in RegisterTap
        }

        private void CompleteQTE(bool success)
        {
            qteSucceeded = success;
            isQTEActive = false;

            OnQTECompleted?.Invoke(activeQTEType, success);
        }

        public void CancelQTE()
        {
            isQTEActive = false;
        }
    }
}
