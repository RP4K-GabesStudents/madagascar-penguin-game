#if USE_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
#else
using System.Collections;
#endif

namespace Commands.Core
{
    public interface ICommand
    {
        string DisplayName { get; }

#if USE_UNITASK
        UniTask ExecuteAsync(CancellationToken ct = default);
        UniTask UndoAsync(CancellationToken ct = default);
#else
        IEnumerator Execute();
        IEnumerator Undo();
#endif
    }
}