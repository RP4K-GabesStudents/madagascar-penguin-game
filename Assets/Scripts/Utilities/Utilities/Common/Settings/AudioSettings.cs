using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Utilities.Utilities.Common.Settings
{
    public class AudioSettings : MonoBehaviour, ISettingsMenu
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private VolumeSettings[] volumeSettings;
        public void Save()
        {
            Debug.Log("Audio settings saved");
            foreach (VolumeSettings volume in volumeSettings)
            {
                float volumeValue = Mathf.Log10(Mathf.Clamp(volume.masterVolume.value, 0.0001f, 1f)) * 20f;
                PlayerPrefs.SetFloat(volume.volumePathName, volumeValue);
            }
            PlayerPrefs.Save();
        }

        public void Load()
        {
            Debug.Log("Loading Audio Settings");
            foreach (VolumeSettings volume in volumeSettings)
            {
                float savedVolume = PlayerPrefs.GetFloat(volume.volumePathName, 0f);
                float linearVolume = Mathf.Pow(10f, savedVolume / 20f); // Convert dB back to linear for slider
                volume.masterVolume.value = linearVolume;

                // Ensure we don't add multiple listeners
                volume.masterVolume.onValueChanged.RemoveAllListeners();

                volume.masterVolume.onValueChanged.AddListener(newVolume =>
                {
                    float dbVolume = Mathf.Log10(Mathf.Clamp(newVolume, 0.0001f, 1f)) * 20f;
                    audioMixer.SetFloat(volume.volumePathName, dbVolume);
                });

                // Set initial volume on AudioMixer
                audioMixer.SetFloat(volume.volumePathName, savedVolume);
            }
        }
    }

    [Serializable]
    public struct VolumeSettings
    {
        public Slider masterVolume;
        public string volumePathName;
    }
}