using System;
using Sequencing.Core;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using UnityEngine;
#if UNITASK_EXISTS
using Cysharp.Threading.Tasks;
#endif

namespace Multiplayer.Vivox.Sequencing
{
    public class VivoxSignOutSequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour success;
        [SerializeField] private Behaviour failure;

        [SerializeField] private float timeUntilTimeOut = 90;

        public event Action<string> DisplayMessage;

#if UNITASK_EXISTS
        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("VivoxSignOutSequence ... Waiting for netcode sign in...");
            await UniTask.WhenAny(UniTask.WaitUntil(IsSignedIn), UniTask.WaitForSeconds(timeUntilTimeOut));
            if (!IsSignedIn())
            {
                Debug.LogError("Vivox timed out...", gameObject);
                return failure as IEntrySequence;
            }

            await VivoxService.Instance.InitializeAsync();
            if (VivoxService.Instance.IsLoggedIn)
            {
                await VivoxService.Instance.LogoutAsync();
            }

            return Default;
        }
#else
        public IEnumerator ExecuteSequence(Action<IEntrySequence> onComplete)
        {
            Debug.Log("VivoxSignOutSequence ... Waiting for netcode sign in...");
            float deadline = Time.time + timeUntilTimeOut;
            yield return new WaitUntil(() => IsSignedIn() || Time.time >= deadline);
            if (!IsSignedIn())
            {
                Debug.LogError("Vivox timed out...", gameObject);
                onComplete(failure as IEntrySequence);
                yield break;
            }

            var initTask = VivoxService.Instance.InitializeAsync();
            yield return new WaitUntil(() => initTask.IsCompleted);

            if (VivoxService.Instance.IsLoggedIn)
            {
                var logoutTask = VivoxService.Instance.LogoutAsync();
                yield return new WaitUntil(() => logoutTask.IsCompleted);
            }

            onComplete(Default);
        }
#endif

        private bool IsSignedIn()
        {
            return AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn;
        }

        public IEntrySequence Default => success as IEntrySequence;
        public bool IsCompleted => VivoxService.Instance != null && VivoxService.Instance.IsLoggedIn;

        private void OnDrawGizmos()
        {
            if (success && success is not IEntrySequence)
            {
                Debug.LogError("Success is INVALID", gameObject);
            }

            if (failure && failure is not IEntrySequence)
            {
                Debug.LogError("failure is INVALID", gameObject);
            }
        }
    }
}