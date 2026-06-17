using System;
using System.Collections;
using Common.Extensions;
using Common.Loading;
using Sequencing.Core;
using UnityEngine;
#if UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
#endif

namespace Sequencing.Managers
{
    /*
     * Runs a chain of IEntrySequence steps in order.
     * With UniTask, each step is awaited as a UniTask and returns the next step.
     * Without it, each step runs as a coroutine and reports its next step via
     * IEntrySequence.Next. Behaviour is otherwise identical.
     *
     * The loading display (none / throbber / full-screen cover) is owned here and
     * wraps the whole chain: it is shown before the first step and hidden after the
     * last. Individual sequence steps no longer touch the LoadingScreen directly.
     */
    public class SequenceEntryPoint : MonoBehaviour
    {
        private enum LoadingDisplay
        {
            None,
            Throbber,
            ScreenCover
        }

        [SerializeField, RequireInterface(typeof(IEntrySequence))]
        private Behaviour start;

        [SerializeField] private bool activateOnStart = true;
        [SerializeField] private bool persist;

        [SerializeField] private float startDelay;

        [Header("Loading Display")]
        [SerializeField] private LoadingDisplay loadingDisplay = LoadingDisplay.None;
        // Optional. If left empty, it falls back to LoadingScreen.Instance.
        [SerializeField] private LoadingScreen loadingScreen;

        private int _chainLength;
        private int _currentLength;
        public float Progress => _chainLength == 0 ? 1f : (float)_currentLength / _chainLength;
        public float StartDelay => startDelay;

        public event Action OnProgress;
        public event Action OnFinish;

        // Re-broadcasts formatted step messages so a loading UI can display them.
        public event Action<string> OnMessage;

        private IEntrySequence _next;

        // Deterministic identity of the whole chain, computed once. Used by
        // NetworkSequenceEntryPoint to validate peers against the server's chain.
        protected long ChainSignature { get; private set; }

        // Exposed so subclasses can walk the chain (e.g. to describe it in a log).
        protected IEntrySequence StartSequence => start as IEntrySequence;

        private void Awake()
        {
            if (persist) DontDestroyOnLoad(gameObject);

            if (start == null)
            {
                Debug.LogError("No start sequence assigned.", this);
                return;
            }

            var startSeq = (IEntrySequence)start;
            _chainLength = CountChain(startSeq);
            ChainSignature = ComputeSignature(startSeq);
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(startDelay);
            if (activateOnStart) ActivateAndForget();
        }

        private int CountChain(IEntrySequence startPoint)
        {
            int count = 0;
            var current = startPoint;
            while (current != null)
            {
                count++;
                current = current.Default;
            }
            return count;
        }

        // FNV-1a 64-bit over each step's token in chain order. Deterministic across
        // processes (unlike string.GetHashCode, which is randomized per run).
        private static long ComputeSignature(IEntrySequence startPoint)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;

            ulong hash = offset;
            var current = startPoint;
            while (current != null)
            {
                string token = (current as IChainSignatureProvider)?.SignatureToken
                               ?? current.GetType().FullName ?? "null";

                foreach (var ch in token)
                {
                    hash = (hash ^ (byte)(ch & 0xFF)) * prime;
                    hash = (hash ^ (byte)((ch >> 8) & 0xFF)) * prime;
                }

                hash = (hash ^ (byte)'|') * prime; // separator so [AB][C] != [A][BC]
                current = current.Default;
            }

            return unchecked((long)hash);
        }

        private void OnDisplayMessage(IEntrySequence sequence, string message)
        {
            string formatted = $"[{GetSequenceLabel(sequence)}] {message}";
            Debug.Log(formatted, gameObject);
            OnMessage?.Invoke(formatted);
        }

        private static string GetSequenceLabel(IEntrySequence sequence)
        {
            return sequence is Component component ? component.gameObject.name : sequence.GetType().Name;
        }

        // Resolves the screen to drive, honouring the serialized override and falling
        // back to the singleton. Returns null (and warns) when a display is requested
        // but no screen exists, so callers can no-op gracefully.
        private LoadingScreen ResolveLoadingScreen()
        {
            if (loadingDisplay == LoadingDisplay.None) return null;

            var screen = loadingScreen != null ? loadingScreen : LoadingScreen.Instance;
            if (screen == null)
            {
                Debug.LogWarning(
                    $"{nameof(SequenceEntryPoint)}: loadingDisplay is {loadingDisplay} but no LoadingScreen was found.",
                    this);
            }
            return screen;
        }

        // Non-awaiting teardown used on early exit (error/cancel) so the screen is
        // never left stuck covered. Safe to call when nothing is showing.
        private void ForceEndLoadingDisplay(LoadingScreen screen)
        {
            if (screen == null) return;
            switch (loadingDisplay)
            {
                case LoadingDisplay.ScreenCover:
                    screen.PlayCloseTransition();
                    break;
                case LoadingDisplay.Throbber:
                    screen.HideThrobber();
                    break;
            }
        }

#if UNITASK
        private CancellationTokenSource _cts;

        // Runs after each stage completes, before the next begins. The base does
        // nothing; NetworkSequenceEntryPoint overrides it to hold a barrier until
        // every peer has finished the same stage. Only present in the UniTask path.
        protected virtual UniTask OnStageComplete(int stage, CancellationToken token) => UniTask.CompletedTask;

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public void ActivateAndForget() => Activate().Forget();

        public async UniTaskVoid Activate()
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            var token = _cts.Token;

            LoadingScreen screen = null;
            bool displayEnded = false;

            try
            {
                _currentLength = 0;
                _next = start as IEntrySequence;

                Debug.Log("Starting Loading Sequence...", gameObject);

                screen = ResolveLoadingScreen();
                await BeginLoadingDisplayAsync(screen);

                while (_next != null && !token.IsCancellationRequested)
                {
                    var step = _next;
                    Action<string> handler = msg => OnDisplayMessage(step, msg);
                    step.DisplayMessage += handler;

                    IEntrySequence next;
                    try
                    {
                        next = await step.ExecuteSequence().AttachExternalCancellation(token);

                        _currentLength++;
                        OnProgress?.Invoke();
                    }
                    finally
                    {
                        step.DisplayMessage -= handler;
                    }

                    // Barrier point. No-op in the base; synchronization in the network subclass.
                    await OnStageComplete(_currentLength, token);

                    _next = next;

                    Debug.Log("Success, currently on stage number: " + _currentLength, gameObject);
                }

                if (!token.IsCancellationRequested)
                {
                    await EndLoadingDisplayAsync(screen);
                    displayEnded = true;

                    Debug.Log("Loading Sequence Complete");
                    OnFinish?.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("SequenceEntryPoint sequence canceled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"SequenceEntryPoint encountered an error: {ex}");
            }
            finally
            {
                // If we exited before a clean reveal, do not leave the screen covered.
                if (!displayEnded) ForceEndLoadingDisplay(screen);
            }
        }

        private async UniTask BeginLoadingDisplayAsync(LoadingScreen screen)
        {
            if (screen == null) return;
            switch (loadingDisplay)
            {
                case LoadingDisplay.ScreenCover:
                    await screen.PlayOpenTransitionAsync();
                    break;
                case LoadingDisplay.Throbber:
                    screen.ShowThrobber();
                    break;
            }
        }

        private async UniTask EndLoadingDisplayAsync(LoadingScreen screen)
        {
            if (screen == null) return;
            switch (loadingDisplay)
            {
                case LoadingDisplay.ScreenCover:
                    await screen.PlayCloseTransitionAsync();
                    break;
                case LoadingDisplay.Throbber:
                    screen.HideThrobber();
                    break;
            }
        }
#else
        private Coroutine _running;

        private void OnDestroy()
        {
            // Unity stops coroutines on destruction anyway; explicit for clarity.
            if (_running != null) StopCoroutine(_running);
        }

        public void ActivateAndForget() => _running = StartCoroutine(Activate());

        public IEnumerator Activate()
        {
            _currentLength = 0;
            _next = start as IEntrySequence;

            Debug.Log("Starting Loading Sequence...", gameObject);

            var screen = ResolveLoadingScreen();
            yield return BeginLoadingDisplayRoutine(screen);

            while (_next != null)
            {
                var step = _next;
                Action<string> handler = msg => OnDisplayMessage(step, msg);
                step.DisplayMessage += handler;

                // try/finally is legal around 'yield return' (only try/catch is not),
                // so the unsubscribing runs even if the step throws.
                try
                {
                    yield return step.ExecuteSequence();
                }
                finally
                {
                    step.DisplayMessage -= handler;
                }

                _currentLength++;
                OnProgress?.Invoke();

                // Branch on Next if the step set one, otherwise follow the static chain.
                _next = step.Next ?? step.Default;

                Debug.Log("Success, currently on stage number: " + _currentLength, gameObject);
            }

            yield return EndLoadingDisplayRoutine(screen);

            Debug.Log("Loading Sequence Complete");
            OnFinish?.Invoke();
        }

        private IEnumerator BeginLoadingDisplayRoutine(LoadingScreen screen)
        {
            if (screen == null) yield break;
            switch (loadingDisplay)
            {
                case LoadingDisplay.ScreenCover:
                {
                    bool done = false;
                    screen.PlayOpenTransition(() => done = true);
                    yield return new WaitUntil(() => done);
                    break;
                }
                case LoadingDisplay.Throbber:
                    screen.ShowThrobber();
                    break;
            }
        }

        private IEnumerator EndLoadingDisplayRoutine(LoadingScreen screen)
        {
            if (screen == null) yield break;
            switch (loadingDisplay)
            {
                case LoadingDisplay.ScreenCover:
                {
                    bool done = false;
                    screen.PlayCloseTransition(() => done = true);
                    yield return new WaitUntil(() => done);
                    break;
                }
                case LoadingDisplay.Throbber:
                    screen.HideThrobber();
                    break;
            }
        }
#endif
    }
}