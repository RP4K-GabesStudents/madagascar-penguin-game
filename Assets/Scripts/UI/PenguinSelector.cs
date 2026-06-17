using System.Collections;
using Game.Characters.World;
using Multiplayer.GameObjects;
using TMPro;
using UI.Transition;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace UI
{
    public class PenguinSelector : MonoBehaviour
    {
        [SerializeField] private PenguinSelectorStats selectorStats;
        [SerializeField] private GenericCharacter target;
        [SerializeField] private TextMeshPro textObject;

        [Header("Objects")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Light[] backingLights;
        [SerializeField] private Light[] frontLights;
        [SerializeField] private Animator penguinAnimator;
        [SerializeField] private Renderer targetRenderer; // replaces shared-material writes

        [SerializeField] private CinemachineCamera cinemachineCamera;

        // How fast _IsOccupied eases toward its target (units per second).
        [SerializeField] private float occupiedLerpSpeed = 6f;

        private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
        private static readonly int IsOccupiedID = Shader.PropertyToID("_IsOccupied");

        private CinemachineBrain _main;
        private MaterialPropertyBlock _mpb;
        private float _t;

        private ulong _prefabId;
        private string _baseName;
        private bool _subscribed;

        // Eased occupied state. _occupiedTarget is the authoritative 0/1 from the
        // store; _occupied chases it so the material transitions smoothly like
        // _Intensity does, instead of snapping.
        private float _occupied;
        private float _occupiedTarget;

        /// <summary>True if any player currently holds this character (from the store).</summary>
        public bool IsTaken { get; private set; }

        public GenericCharacter Character => target;

        /// <summary>Prefab-id hash of this selector's character (0 if none).</summary>
        public ulong PrefabId => _prefabId;

        protected void Start()
        {
            _main ??= Camera.main.GetComponent<CinemachineBrain>();
            _mpb ??= new MaterialPropertyBlock();

            _baseName = target.name;
            textObject.text = _baseName;

            var netObj = target.GetComponent<NetworkObject>();
            _prefabId = netObj ? netObj.PrefabIdHash : 0;

            _occupied = 0f;
            _occupiedTarget = 0f;
            SetMaterialFloat(IsOccupiedID, 0);
            TrySubscribe();
        }

        private void OnEnable()
        {
            _mpb ??= new MaterialPropertyBlock();
            textObject.alpha = 1;
            textObject.gameObject.SetActive(false);
            EvaluateLights(0, 0);
            TrySubscribe();
        }

        // The store spawns via a sequence step on the networked scene and may not
        // exist when this selector's Start/OnEnable runs (SelectionScene loads
        // additively). Keep retrying until we latch on; cheap no-op once subscribed.
        private void Update()
        {
            if (!_subscribed) TrySubscribe();

            // Ease _IsOccupied toward its target, mirroring how _Intensity ramps.
            if (!Mathf.Approximately(_occupied, _occupiedTarget))
            {
                _occupied = Mathf.MoveTowards(_occupied, _occupiedTarget,
                                              occupiedLerpSpeed * Time.deltaTime);
                SetMaterialFloat(IsOccupiedID, _occupied);
            }
        }

        private void OnDestroy() => Unsubscribe();

        private void TrySubscribe()
        {
            if (_subscribed) return;
            var store = CharacterSelectionStore.Instance;
            if (!store) return; // store spawns via sequence; retried from Update.
            store.TakenList.OnListChanged += OnTakenChanged;
            _subscribed = true;
            RefreshFromStore(store);
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            var store = CharacterSelectionStore.Instance;
            if (store) store.TakenList.OnListChanged -= OnTakenChanged;
            _subscribed = false;
        }

        private void OnTakenChanged(NetworkListEvent<CharacterSelectionStore.Taken> _)
            => RefreshFromStore(CharacterSelectionStore.Instance);

        private void RefreshFromStore(CharacterSelectionStore store)
        {
            foreach (var taken in store.TakenList)
            {
                if (taken.PrefabId == _prefabId)
                {
                    IsTaken = true;
                    textObject.text = $"{_baseName}\n<size=50%>Selected by {taken.PlayerName}</size>";
                    _occupiedTarget = 1f; // Update() lerps the material toward this.
                    return;
                }
            }
            IsTaken = false;
            textObject.text = _baseName;
            _occupiedTarget = 0f;
        }

        public void Select()
        {
            _main ??= Camera.main.GetComponent<CinemachineBrain>();
            StartCoroutine(FadeIn());
            cinemachineCamera.enabled = true;
        }

        // Snap straight to the fully-selected state with no blend wait. Used for the
        // first selector when the scene loads so it is lit immediately.
        public void SelectInstant()
        {
            _main ??= Camera.main.GetComponent<CinemachineBrain>();
            StopAllCoroutines();
            cinemachineCamera.enabled = true;

            _t = selectorStats.MaxLightTime;
            textObject.gameObject.SetActive(true);
            textObject.alpha = 1;
            EvaluateLights(1, 1);
        }

        public void Deselect()
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
            cinemachineCamera.enabled = false;
        }

        private IEnumerator FadeOut()
        {
            EvaluateLights(1, 1);
            textObject.alpha = 1;
            while (_t >= 0)
            {
                _t -= Time.deltaTime * 2;
                EvaluateLights(Mathf.Clamp01(_t / selectorStats.FrontLightTime),
                               Mathf.Clamp01(_t / selectorStats.BackLightsTime));
                textObject.alpha = _t / selectorStats.MaxLightTime;
                yield return null;
            }

            _t = 0;
            textObject.alpha = 1;
            textObject.gameObject.SetActive(false);
            EvaluateLights(0, 0);
        }

        private IEnumerator FadeIn()
        {
            EvaluateLights(0, 0);
            yield return new WaitForSeconds(_main.DefaultBlend.BlendTime);

            audioSource.PlayOneShot(selectorStats.LightActiveSound);
            textObject.gameObject.SetActive(true);

            while (_t <= selectorStats.MaxLightTime)
            {
                _t += Time.deltaTime;
                EvaluateLights(Mathf.Clamp01(_t / selectorStats.FrontLightTime),
                               Mathf.Clamp01(_t / selectorStats.BackLightsTime));
                yield return null;
            }

            _t = selectorStats.MaxLightTime;
            EvaluateLights(1, 1);
        }

        private void EvaluateLights(float percent1, float percent2)
        {
            float intensity = selectorStats.FrontLights.Evaluate(percent1);
            foreach (Light l in frontLights) l.intensity = intensity;

            intensity = selectorStats.BackLights.Evaluate(percent2);
            foreach (Light l in backingLights) l.intensity = intensity;

            SetMaterialFloat(IntensityID, selectorStats.MaterialIntensity.Evaluate(percent2));
        }

        // Per-renderer override; never mutates the shared material asset, so multiple
        // selectors sharing a material don't fight over _Intensity / _IsSelected.
        private void SetMaterialFloat(int id, float value)
        {
            if (!targetRenderer) return;
            targetRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(id, value);
            targetRenderer.SetPropertyBlock(_mpb);
        }

        public void ChooseCharacter()
        {
            Debug.Log("implement on chosen effect");
        }

        public void UnChooseCharacter()
        {

        }
    }
}