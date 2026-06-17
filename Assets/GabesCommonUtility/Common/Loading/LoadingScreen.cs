using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Loading
{
    public class LoadingScreen : MonoBehaviour
    {
        
        private static readonly int FillMatID = Shader.PropertyToID("_Fill");

        
        [SerializeField] private Image transitionImage;
        [SerializeField] private GameObject textBlocks;

        [Header("Throbber")]
        // Optional lightweight spinner shown instead of the full screen cover.
        [SerializeField] private GameObject throbber;
        // Degrees per second. Leave at 0 to drive the spin from an Animator instead.
        [SerializeField] private float throbberRotateSpeed = 0f;
        
        [SerializeField] private float closeTime = 1f;
        [SerializeField] private float openTime = 1f;
        [SerializeField] private AnimationCurve closeCurve;
        [SerializeField] private AnimationCurve openCurve;

        private Material _transitionMaterial;
        private static LoadingScreen _instance;
        private Canvas _canvas;

        // Tracks the in-flight transition so a new one can cancel it cleanly
        private Coroutine _activeTransition;
        // Tracks the awaiter for the in-flight transition so it is always resolved
        private TaskCompletionSource<bool> _activeTcs;

        private bool _throbberActive;

        public static LoadingScreen Instance => _instance;

        private void Awake()
        {
            // Handle singleton with DontDestroyOnLoad for prefab persistence
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Ensure we have a Canvas component
            _canvas = GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 9999; // Ensure it renders on top
                
                // Add required components for Canvas
                if (GetComponent<CanvasScaler>() == null)
                {
                    gameObject.AddComponent<CanvasScaler>();
                }
                if (GetComponent<GraphicRaycaster>() == null)
                {
                    gameObject.AddComponent<GraphicRaycaster>();
                }
            }
            
            if (transitionImage != null)
            {
                // Create instance of material to avoid modifying the shared material
                _transitionMaterial = new Material(transitionImage.material);
                transitionImage.material = _transitionMaterial;
            }
            
            SetActive(false);
        }

        private void Start()
        {
            // A duplicate instance is destroyed in Awake, but its Start still runs
            // once before the deferred Destroy takes effect. Bail out so it does not
            // touch shared/persistent state.
            if (_instance != this)
            {
                return;
            }

            // Start with the screen open (filled)
            if (_transitionMaterial != null)
            {
                _transitionMaterial.SetFloat(FillMatID, openCurve.Evaluate(1));
            }
            
            if (textBlocks != null)
            {
                textBlocks.SetActive(true);
            }

            if (throbber != null)
            {
                throbber.SetActive(false);
            }
        }

        private void Update()
        {
            if (_throbberActive && throbber != null && throbberRotateSpeed != 0f)
            {
                // Unscaled so it keeps spinning even if Time.timeScale is 0 during a load.
                throbber.transform.Rotate(0f, 0f, -throbberRotateSpeed * Time.unscaledDeltaTime);
            }
        }

        public void SetActive(bool isActive) => _canvas.enabled = isActive;

        /// <summary>
        /// Shows the lightweight throbber without covering the scene.
        /// Mutually exclusive with the screen-cover transitions.
        /// </summary>
        public void ShowThrobber()
        {
            _throbberActive = true;

            // Make sure the full-screen cover graphic is not obscuring the throbber.
            if (transitionImage != null) transitionImage.enabled = false;
            if (throbber != null) throbber.SetActive(true);

            SetActive(true);
        }

        /// <summary>
        /// Hides the throbber and disables the canvas.
        /// </summary>
        public void HideThrobber()
        {
            _throbberActive = false;

            if (throbber != null) throbber.SetActive(false);
            // Restore the cover graphic for any later screen-cover transition.
            if (transitionImage != null) transitionImage.enabled = true;

            SetActive(false);
        }

        /// <summary>
        /// Plays the closing transition (reveals the scene behind) - Awaitable.
        /// Result is true if it finished naturally, false if superseded or destroyed.
        /// </summary>
        public Task<bool> PlayCloseTransitionAsync()
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            BeginTransition(closeTime, closeCurve, false, null, tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Plays the opening transition (covers the scene) - Awaitable.
        /// Result is true if it finished naturally, false if superseded or destroyed.
        /// </summary>
        public Task<bool> PlayOpenTransitionAsync()
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            BeginTransition(openTime, openCurve, true, null, tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Plays the closing transition (reveals the scene behind) - Callback version
        /// </summary>
        public void PlayCloseTransition(System.Action onComplete = null)
        {
            BeginTransition(closeTime, closeCurve, false, onComplete, null);
        }

        /// <summary>
        /// Plays the opening transition (covers the scene) - Callback version
        /// </summary>
        public void PlayOpenTransition(System.Action onComplete = null)
        {
            BeginTransition(openTime, openCurve, true, onComplete, null);
        }

        private void BeginTransition(float duration, AnimationCurve curve, bool isOpen,
                                     System.Action onComplete, TaskCompletionSource<bool> tcs)
        {
            // Cancel any in-flight transition so two of them do not fight over _Fill.
            if (_activeTransition != null)
            {
                StopCoroutine(_activeTransition);
                _activeTransition = null;
            }

            // Release the previous awaiter so a pending await is never left hanging.
            if (_activeTcs != null)
            {
                _activeTcs.TrySetResult(false);
                _activeTcs = null;
            }

            _activeTcs = tcs;

            _activeTransition = StartCoroutine(TransitionScreen(duration, curve, isOpen, () =>
            {
                _activeTransition = null;
                if (_activeTcs == tcs)
                {
                    _activeTcs = null;
                }

                tcs?.TrySetResult(true);
                onComplete?.Invoke();
            }));
        }

        private IEnumerator TransitionScreen(float duration, AnimationCurve curve, bool isOpen, System.Action onComplete)
        {
            SetActive(true);

            // A screen-cover transition owns the canvas; make sure the throbber is
            // not also showing and the cover graphic is enabled.
            _throbberActive = false;
            if (throbber != null) throbber.SetActive(false);
            if (transitionImage != null) transitionImage.enabled = true;
            
            float elapsed = 0;

            if (!isOpen && textBlocks != null)
            {
                textBlocks.SetActive(false);
            }

            if (_transitionMaterial != null)
            {
                _transitionMaterial.SetFloat(FillMatID, curve.Evaluate(0));
            }
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float perc = elapsed / duration;
                float eval = curve.Evaluate(perc);

                if (_transitionMaterial != null)
                {
                    _transitionMaterial.SetFloat(FillMatID, eval);
                }
                
                yield return null;
            }

            if (isOpen && textBlocks != null)
            {
                textBlocks.SetActive(true);
            }

            if (_transitionMaterial != null)
            {
                _transitionMaterial.SetFloat(FillMatID, curve.Evaluate(1));
            }

            onComplete?.Invoke();
            
            SetActive(isOpen); 

        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            // Release any awaiter so a pending await does not hang forever.
            if (_activeTcs != null)
            {
                _activeTcs.TrySetResult(false);
                _activeTcs = null;
            }
            
            // Clean up instantiated material
            if (_transitionMaterial != null)
            {
                Destroy(_transitionMaterial);
            }
        }
    }
}