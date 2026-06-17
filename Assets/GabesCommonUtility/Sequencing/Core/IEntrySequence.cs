using System;
#if UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Collections;
#endif

namespace Sequencing.Core
{
    public interface IEntrySequence
    {
        // Raised by a step to surface a human-readable message. The entry point
        // subscribes per-step and prefixes a label, e.g. "[Lobby] Could not connect".
        public event Action<string> DisplayMessage;

        public IEntrySequence Default { get; }
        public bool IsCompleted { get; }

#if UNITASK
        // The returned step is the next one to run (may differ from Default to branch).
        public UniTask<IEntrySequence> ExecuteSequence();
#else
        // Coroutines can't return a value, so the step reports its next step
        // through Next, set before the coroutine ends. Null means "follow Default".
        // Equivalent mapping: UniTask 'return X' == coroutine 'Next = X'.
        public IEnumerator ExecuteSequence();
        public IEntrySequence Next { get; }
#endif
    }
}