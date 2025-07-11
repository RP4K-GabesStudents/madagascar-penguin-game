using UnityEngine;
using UnityEngine.Audio;

namespace Managers
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _audioSource1;
        [SerializeField] private AudioSource _audioSource2;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip _actionMusic;
        [SerializeField] private AudioClip _ambientMusic;
        [SerializeField] private AudioClip _menuMusic;
        // Add more clips as needed

        [Header("Settings")]
        [SerializeField] private float _crossfadeDuration = 3f;
        [SerializeField] private AudioMixerGroup _musicMixerGroup;

        private AudioSource _currentSource;
        private AudioSource _nextSource;
        private bool _isCrossfading;
        private float _crossfadeTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            if (_audioSource1 == null)
            {
                _audioSource1 = gameObject.AddComponent<AudioSource>();
                _audioSource1.outputAudioMixerGroup = _musicMixerGroup;
                _audioSource1.loop = true;
            }

            if (_audioSource2 == null)
            {
                _audioSource2 = gameObject.AddComponent<AudioSource>();
                _audioSource2.outputAudioMixerGroup = _musicMixerGroup;
                _audioSource2.loop = true;
            }

            _currentSource = _audioSource1;
            _nextSource = _audioSource2;
        }

        private void Update()
        {
            if (_isCrossfading)
            {
                _crossfadeTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(_crossfadeTimer / _crossfadeDuration);

                _currentSource.volume = 1f - progress;
                _nextSource.volume = progress;

                if (progress >= 1f)
                {
                    _isCrossfading = false;
                    _currentSource.Stop();
                    _currentSource.volume = 1f; // Reset volume for next use

                    // Swap references
                    AudioSource temp = _currentSource;
                    _currentSource = _nextSource;
                    _nextSource = temp;
                }
            }
        }

        public void PlayActionMusic()
        {
            PlayMusic(_actionMusic);
        }

        public void PlayAmbientMusic()
        {
            PlayMusic(_ambientMusic);
        }

        public void PlayMenuMusic()
        {
            PlayMusic(_menuMusic);
        }

        public void PlayMusic(AudioClip clip, bool forceRestart = false)
        {
            if (clip == null)
            {
                Debug.LogWarning("Tried to play null audio clip");
                return;
            }

            if (_currentSource.clip == clip && _currentSource.isPlaying && !forceRestart)
            {
                // Already playing this clip
                return;
            }

            if (_isCrossfading)
            {
                // If we're already crossfading, stop the current fade
                _isCrossfading = false;
                _currentSource.Stop();
                _currentSource.volume = 1f;
            }

            _nextSource.clip = clip;
            _nextSource.volume = 0f;
            _nextSource.Play();

            _isCrossfading = true;
            _crossfadeTimer = 0f;
        }

        public void StopMusic(float fadeDuration = 0f)
        {
            if (fadeDuration <= 0f)
            {
                _currentSource.Stop();
                if (_isCrossfading)
                {
                    _nextSource.Stop();
                    _isCrossfading = false;
                }
            }
            else
            {
                StartCoroutine(FadeOutMusic(fadeDuration));
            }
        }

        private System.Collections.IEnumerator FadeOutMusic(float duration)
        {
            float startVolume = _currentSource.volume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                _currentSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                if (_isCrossfading)
                {
                    _nextSource.volume = Mathf.Lerp(_nextSource.volume, 0f, timer / duration);
                }
                yield return null;
            }

            _currentSource.Stop();
            if (_isCrossfading)
            {
                _nextSource.Stop();
                _isCrossfading = false;
            }
            _currentSource.volume = startVolume; // Reset volume
        }
    }
}