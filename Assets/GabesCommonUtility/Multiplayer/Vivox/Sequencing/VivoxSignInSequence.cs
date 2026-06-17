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
    public class VivoxSignInSequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour success;
        [SerializeField] private Behaviour failure;

        [SerializeField] private float timeUntilTimeOut = 90;
        [SerializeField] private bool useDebugEcho;
        [SerializeField] private bool usePositionalAudio;
        [SerializeField] private ChatCapability chatCapability;

        public event Action<string> DisplayMessage;

#if UNITASK_EXISTS
        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("VivoxInitializationSequence ... Waiting for netcode sign in...");
            await UniTask.WhenAny(UniTask.WaitUntil(IsSignedIn), UniTask.WaitForSeconds(timeUntilTimeOut));
            if (!IsSignedIn())
            {
                Debug.LogError("Vivox timed out...", gameObject);
                return failure as IEntrySequence;
            }

            try
            {
                await VivoxService.Instance.InitializeAsync();
                string username = AuthenticationService.Instance.PlayerName;
                if (string.IsNullOrEmpty(username)) username = string.IsNullOrEmpty(AuthenticationService.Instance.PlayerId) ? "INVALID USER NAME" : "ID: " + AuthenticationService.Instance.PlayerId;

                LoginOptions options = new LoginOptions
                {
                    DisplayName = username,
                    EnableTTS = true
                };

                await VivoxService.Instance.LoginAsync(options);
            }
            catch (Exception e)
            {
                Debug.LogError("Vivox failed to init: " + e, gameObject);
                return failure as IEntrySequence;
            }

            if (useDebugEcho)
            {
                await VivoxService.Instance.JoinEchoChannelAsync("DEBUG", chatCapability);
                return Default;
            }

            if (usePositionalAudio)
            {
                Debug.LogWarning("Using temporary vivox 3D default settings");
                Channel3DProperties props = new Channel3DProperties();
                await VivoxService.Instance.JoinPositionalChannelAsync("proximity", chatCapability, props, new ChannelOptions());
            }

            return Default;
        }
#else
        public IEnumerator ExecuteSequence(Action<IEntrySequence> onComplete)
        {
            Debug.Log("VivoxInitializationSequence ... Waiting for netcode sign in...");
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
            if (initTask.IsFaulted)
            {
                Debug.LogError("Vivox failed to init: " + initTask.Exception, gameObject);
                onComplete(failure as IEntrySequence);
                yield break;
            }

            string username = AuthenticationService.Instance.PlayerName;
            if (string.IsNullOrEmpty(username)) username = string.IsNullOrEmpty(AuthenticationService.Instance.PlayerId) ? "INVALID USER NAME" : "ID: " + AuthenticationService.Instance.PlayerId;

            LoginOptions options = new LoginOptions
            {
                DisplayName = username,
                EnableTTS = true
            };

            var loginTask = VivoxService.Instance.LoginAsync(options);
            yield return new WaitUntil(() => loginTask.IsCompleted);
            if (loginTask.IsFaulted)
            {
                Debug.LogError("Vivox failed to init: " + loginTask.Exception, gameObject);
                onComplete(failure as IEntrySequence);
                yield break;
            }

            if (useDebugEcho)
            {
                var echoTask = VivoxService.Instance.JoinEchoChannelAsync("DEBUG", chatCapability);
                yield return new WaitUntil(() => echoTask.IsCompleted);
                onComplete(Default);
                yield break;
            }

            if (usePositionalAudio)
            {
                Debug.LogWarning("Using temporary vivox 3D default settings");
                Channel3DProperties props = new Channel3DProperties();
                var joinTask = VivoxService.Instance.JoinPositionalChannelAsync("proximity", chatCapability, props, new ChannelOptions());
                yield return new WaitUntil(() => joinTask.IsCompleted);
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