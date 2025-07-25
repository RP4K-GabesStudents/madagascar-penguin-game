using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Managers.Game
{
    public class LoadingManager : MonoBehaviour
    {
        public static LoadingManager Instance { get; private set; }
        private TextMeshProUGUI _textMeshPro;
        [SerializeField] private float fadeTime;

        private void Awake()
        {
            if (Instance&& Instance != this)
            {
                Destroy(gameObject);
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void LateUpdate()
        {
            _textMeshPro.text = "<color=#"+ColorUtility.ToHtmlStringRGB(ColourManager.CurColour)+">" + _textMeshPro.text + "</color>";
        }

        public void OnTextAppear()
        {
            _textMeshPro.enabled = true;
            StartCoroutine(OnTextAppearFade());
        }

        private IEnumerator OnTextAppearFade()
        {
            float time = fadeTime;
            while (time > 0)
            {
                time -= Time.deltaTime;
                _textMeshPro.alpha = 1 - time / fadeTime;
                yield return null;
            }
            _textMeshPro.alpha = 1;
        }

        public void OnTextDisappear()
        {
            
        }
    }
}
