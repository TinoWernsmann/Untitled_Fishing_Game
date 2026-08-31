using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using XRInputDevice = UnityEngine.XR.InputDevice;

namespace Manager.Haptics
{
    /// <summary>
    /// Centralized haptic manager for XR controller rumble.
    /// Designed as a singleton with simple gameplay-facing calls.
    /// </summary>
    public class HapticManager : MonoBehaviour
    {
        private enum HapticMode
        {
            None,
            FishFight,
            OneShot
        }

        public static HapticManager Instance { get; private set; }

        [Header("Haptics (tunable)")]
        [SerializeField] private float castDuration = 0.45f;
        [SerializeField] private float castAmplitude = 0.8f;

        [SerializeField] private float biteDuration = 0.14f;
        [SerializeField] private float biteAmplitude = 0.9f;

        [SerializeField] private float fishFightBase = 0.09f;

        private const float FishFightPulseStrength = 0.05f;
        private const float FishFightPulseFrequency = 2.4f;

        [SerializeField] private float lineBreakDuration = 0.08f;
        [SerializeField] private float lineBreakAmplitude = 1f;

        [Header("Device Discovery")]
        [SerializeField] private float deviceRefreshInterval = 0.5f;

        private Coroutine runningRoutine;
        private HapticMode currentMode = HapticMode.None;

        private readonly List<XRInputDevice> controllerDevices = new List<XRInputDevice>();
        private float nextDeviceRefreshAt = 0f;

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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                StopAllHaptics();
                Instance = null;
            }
        }

        // --- Public API ---
        public void PlayCastHaptics()
        {
            StartRoutine(StartCoroutine(OneShotAllDevices(castAmplitude, castDuration)), HapticMode.OneShot);
        }

        public void PlayBiteHaptics()
        {
            StartRoutine(StartCoroutine(OneShotAllDevices(biteAmplitude, biteDuration)), HapticMode.OneShot);
        }

        public void StartFishFightHaptics()
        {
            if (currentMode == HapticMode.FishFight && runningRoutine != null)
            {
                return;
            }

            StartRoutine(StartCoroutine(FishFightPulseRoutine()), HapticMode.FishFight);
        }

        public void StopFishFightHaptics()
        {
            StopRoutine();
        }

        public void PlayLineBreakHaptics()
        {
            StartRoutine(StartCoroutine(LineBreakBurstRoutine()), HapticMode.OneShot);
        }

        // --- Internal helpers ---
        private void StartRoutine(Coroutine c, HapticMode mode)
        {
            StopRoutine();
            runningRoutine = c;
            currentMode = mode;
        }

        private void StopRoutine()
        {
            if (runningRoutine != null)
            {
                StopCoroutine(runningRoutine);
                runningRoutine = null;
            }

            currentMode = HapticMode.None;
            StopAllHaptics();
        }

        private IEnumerator FishFightPulseRoutine()
        {
            while (true)
            {
                float frequency = Mathf.Max(0.1f, FishFightPulseFrequency);
                float pulse = (Mathf.Sin(Time.unscaledTime * frequency * Mathf.PI * 2f) + 1f) * 0.5f;
                float amplitude = fishFightBase + Mathf.Lerp(-FishFightPulseStrength, FishFightPulseStrength, pulse);
                SendHapticToAll(Mathf.Clamp01(amplitude), Time.deltaTime);
                yield return null;
            }
        }

        private IEnumerator OneShotAllDevices(float amplitude, float duration)
        {
            SendHapticToAll(amplitude, duration);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                yield return null;
            }
            StopAllHaptics();
        }

        private IEnumerator LineBreakBurstRoutine()
        {
            const int pulseCount = 3;
            const float gapDuration = 0.03f;

            float pulseAmplitude = Mathf.Clamp01(lineBreakAmplitude);
            float pulseDuration = Mathf.Max(0.03f, lineBreakDuration);

            for (int i = 0; i < pulseCount; i++)
            {
                SendHapticToAll(pulseAmplitude, pulseDuration);

                float t = 0f;
                while (t < pulseDuration)
                {
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }

                StopAllHaptics();

                if (i < pulseCount - 1)
                {
                    float gap = 0f;
                    while (gap < gapDuration)
                    {
                        gap += Time.unscaledDeltaTime;
                        yield return null;
                    }
                }
            }
        }

        private void SendHapticToAll(float amplitude, float duration)
        {
            RefreshControllerDevices(false);

            foreach (var d in controllerDevices)
            {
                if (d.isValid)
                {
                    HapticCapabilities caps;
                    if (d.TryGetHapticCapabilities(out caps) && caps.supportsImpulse)
                    {
                        uint channel = 0;
                        d.SendHapticImpulse(channel, Mathf.Clamp01(amplitude), duration);
                    }
                }
            }
        }

        private void StopAllHaptics()
        {
            RefreshControllerDevices(true);

            foreach (var d in controllerDevices)
            {
                if (d.isValid)
                {
                    HapticCapabilities caps;
                    if (d.TryGetHapticCapabilities(out caps) && caps.supportsImpulse)
                    {
                        d.StopHaptics();
                    }
                }
            }
        }

        private void RefreshControllerDevices(bool force)
        {
            if (!force && Time.unscaledTime < nextDeviceRefreshAt)
            {
                return;
            }

            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller,
                controllerDevices);

            nextDeviceRefreshAt = Time.unscaledTime + Mathf.Max(0.1f, deviceRefreshInterval);
        }
    }
}
