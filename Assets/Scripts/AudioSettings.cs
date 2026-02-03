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
                PlayerPrefs.SetFloat(volume.volumePathName, volume.masterVolume.value);
                Debug.Log($"Audio settings saved: {volume.volumePathName}: {volume.masterVolume.value}");

            }
            PlayerPrefs.Save();
        }

        public void Load()
        {
            Debug.Log("Loading Audio Settings");
            foreach (VolumeSettings volume in volumeSettings)
            {
                float savedVolume = PlayerPrefs.GetFloat(volume.volumePathName, 0.8f); //0-1 range
                Debug.Log($"Loading Audio Settings: {volume.volumePathName}: {savedVolume}");

                float dbVolume = Mathf.Log10(Mathf.Clamp(savedVolume, 0.0001f, 1f)) * 20f;
                audioMixer.SetFloat(volume.volumePathName, dbVolume);
                volume.masterVolume.value = savedVolume;
                
                volume.masterVolume.onValueChanged.AddListener(newVolume =>
                {
                    float x = Mathf.Log10(Mathf.Clamp(newVolume, 0.0001f, 1f)) * 20f;
                    audioMixer.SetFloat(volume.volumePathName, x);
                });
                
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