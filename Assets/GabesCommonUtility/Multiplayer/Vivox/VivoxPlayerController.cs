using System;
using Unity.Services.Vivox;
using UnityEngine;
#if UNITASK_EXISTS
using Cysharp.Threading.Tasks;
#endif

namespace Multiplayer.Vivox
{
    public class VivoxPlayerController : MonoBehaviour
    {
        [SerializeField] private string channelName = "proximity";

        [Header("Loudness")]
        [Tooltip("Higher = the reported SmoothedLoudness chases the raw value faster. 0 = no smoothing (snap).")]
        [SerializeField, Range(0f, 30f)] private float loudnessSmoothing = 12f;
        [Tooltip("Raw AudioEnergy at or above this counts as 'speaking'.")]
        [SerializeField, Range(0f, 1f)] private float speakingThreshold = 0.02f;

        private bool _isPositionTracking;
        private bool _subscribed;
        private VivoxParticipant _selfParticipant;
        private float _smoothedLoudness;

        /// <summary>Raw 0..1 audio energy of the local player this frame. 0 when no live participant.</summary>
        public float Loudness { get; private set; }

        /// <summary>Smoothed 0..1 loudness. Good for driving UI meters or mouth animation.</summary>
        public float SmoothedLoudness => _smoothedLoudness;

        /// <summary>True while raw Loudness is at or above the speaking threshold.</summary>
        public bool IsSpeaking { get; private set; }

        /// <summary>Local participant input-muted state, per Vivox. False when no participant.</summary>
        public bool IsMuted => _selfParticipant != null && _selfParticipant.IsMuted;

        /// <summary>True once 3D position tracking has been established.</summary>
        public bool IsTracking => _isPositionTracking;

        /// <summary>True once the local (self) participant has been resolved in the channel.</summary>
        public bool HasParticipant => _selfParticipant != null;

        /// <summary>Fires when the raw loudness changes (every frame a participant is live), value 0..1.</summary>
        public event Action<float> LoudnessChanged;

        /// <summary>Fires only on speaking start/stop transitions. bool = isSpeaking.</summary>
        public event Action<bool> SpeakingChanged;

#if UNITASK_EXISTS
        private void Start()
        {
            InitializePositionalAudio().Forget();
        }

        private async UniTask InitializePositionalAudio()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            try
            {
                // Wait for Vivox to be ready
                await UniTask.WaitUntil(() =>
                    VivoxService.Instance != null &&
                    VivoxService.Instance.IsLoggedIn,
                    cancellationToken: ct
                );

                // Wait for the channel to be joined
                await UniTask.WaitUntil(() =>
                    VivoxService.Instance.ActiveChannels.Count > 0,
                    cancellationToken: ct
                );

                if (this == null) return;
                OnVivoxReady();
            }
            catch (OperationCanceledException)
            {
                // Destroyed while waiting; nothing to do.
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize positional audio: {e}", this);
            }
        }
#else
        private void Start()
        {
            StartCoroutine(InitializePositionalAudio());
        }

        private IEnumerator InitializePositionalAudio()
        {
            // Wait for Vivox to be ready
            yield return new WaitUntil(() =>
                VivoxService.Instance != null &&
                VivoxService.Instance.IsLoggedIn
            );

            // Wait for the channel to be joined
            yield return new WaitUntil(() =>
                VivoxService.Instance.ActiveChannels.Count > 0
            );

            if (this == null) yield break;
            OnVivoxReady();
        }
#endif

        private void OnVivoxReady()
        {
            // Now safe to set 3D position
            VivoxService.Instance.Set3DPosition(gameObject, channelName, true);
            _isPositionTracking = true;

            CacheSelfParticipant();
            VivoxService.Instance.ParticipantAddedToChannel += OnRosterChanged;
            VivoxService.Instance.ParticipantRemovedFromChannel += OnRosterChanged;
            _subscribed = true;
        }

        private void Update()
        {
            if (_selfParticipant == null)
            {
                // Decay toward silence so meters settle cleanly after the participant drops.
                Loudness = 0f;
                _smoothedLoudness = StepSmoothed(0f);
                SetSpeaking(false);
                return;
            }

            Loudness = (float)_selfParticipant.AudioEnergy;
            LoudnessChanged?.Invoke(Loudness);

            _smoothedLoudness = StepSmoothed(Loudness);
            SetSpeaking(Loudness >= speakingThreshold);
        }

        private float StepSmoothed(float target)
        {
            if (loudnessSmoothing <= 0f) return target;
            return Mathf.Lerp(_smoothedLoudness, target, 1f - Mathf.Exp(-loudnessSmoothing * Time.deltaTime));
        }

        private void SetSpeaking(bool speaking)
        {
            if (speaking == IsSpeaking) return;
            IsSpeaking = speaking;
            SpeakingChanged?.Invoke(speaking);
        }

        private void OnRosterChanged(VivoxParticipant participant)
        {
            // Re-resolve self on any roster change (covers leave/rejoin and late join).
            CacheSelfParticipant();
        }

        private void CacheSelfParticipant()
        {
            _selfParticipant = null;
            if (VivoxService.Instance == null) return;
            if (!VivoxService.Instance.ActiveChannels.TryGetValue(channelName, out var participants)) return;

            foreach (var p in participants)
            {
                if (p.IsSelf)
                {
                    _selfParticipant = p;
                    return;
                }
            }
        }

        private void OnDestroy()
        {
            if (_subscribed && VivoxService.Instance != null)
            {
                VivoxService.Instance.ParticipantAddedToChannel -= OnRosterChanged;
                VivoxService.Instance.ParticipantRemovedFromChannel -= OnRosterChanged;
            }

            // Clean up position tracking
            if (_isPositionTracking && VivoxService.Instance != null)
            {
                try
                {
                    VivoxService.Instance.Set3DPosition(gameObject, channelName, false);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to cleanup positional audio: {e.Message}");
                }
            }
        }
    }
}