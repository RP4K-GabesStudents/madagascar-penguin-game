using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Multiplayer.GameObjects;
using Multiplayer.GameObjects;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
using Random = UnityEngine.Random;

namespace Managers
{
    // Runs early so LobbyUI.OnEnable can safely read Instance.
    [DefaultExecutionOrder(-1000)]
    public class LobbySystemManager : MonoBehaviour, IPlayerNameProvider
    {
        private const string Tag = "[Lobby]";

        [SerializeField] private string lobbyName = "lobby";
        [SerializeField] private int maxPlayers = 4;

        private Unity.Services.Lobbies.Models.Player _playerObject;

        public static LobbySystemManager Instance { get; private set; }
        public string PlayerName { get; private set; }
        public string PlayerId { get; private set; }
        public Lobby CurrentLobby { get; private set; }

        private const float LobbyHeartBeatInterval = 20f;
        private const int PollTimer = 1100;

        public event Action OnClientConnected;
        public event Action OnClientDisconnected;
        public event Action OnLobbyClosed;
        public event Action OnLobbyOpened;
        public event Action OnGameStarting;

        private readonly CountdownTimer _heartBeatTimer = new CountdownTimer(LobbyHeartBeatInterval);
        private readonly LobbyEventCallbacks _events = new();

        public bool IsHost() => CurrentLobby != null && CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;
        public bool IsConnected() => CurrentLobby != null;

        /// <summary>
        /// IPlayerNameProvider: resolve a player's display name from the lobby roster
        /// by their stable UGS playerId (lobby Player.Id). The host retains CurrentLobby
        /// into the game, so this works server-side for every player, including those
        /// the selection sequence auto-assigns. Returns false if not found.
        /// </summary>
        public bool TryGetPlayerName(string playerId, out string playerName)
        {
            playerName = null;
            if (string.IsNullOrEmpty(playerId) || CurrentLobby?.Players == null)
                return false;

            foreach (var player in CurrentLobby.Players)
            {
                if (player.Id != playerId) continue;
                if (player.Data != null && player.Data.TryGetValue("Name", out var nameData)
                    && !string.IsNullOrEmpty(nameData.Value))
                {
                    playerName = nameData.Value;
                    return true;
                }
                return false; // matched the player but no usable name
            }
            return false;
        }

        #region Unity lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.Log($"{Tag} Duplicate LobbySystemManager destroyed.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Let the selection sequence resolve player names from the lobby roster
            // without the multiplayer package depending on this assembly.
            PlayerNameDirectory.Active = this;

            // Subscribe ONCE here (not in OnEnable, which can fire repeatedly and
            // double-subscribe). These callbacks fire for whichever lobby we are
            // currently subscribed to via SubscribeToLobbyEventsAsync.
            _events.DataChanged += CheckStartGame;
            _events.LobbyChanged += OnLobbyChanged;

            Application.quitting += LeaveLobby;
        }

        private async void Start()
        {
            await Authenticate();

            _heartBeatTimer.OnTimerStop += () =>
            {
                _ = HandleHeartBeatAsync();
                _heartBeatTimer.Start();
            };
        }

        private void OnDestroy()
        {
            if (Instance != this) return; // a destroyed duplicate must not detach the real one
            if (PlayerNameDirectory.Active == (IPlayerNameProvider)this)
                PlayerNameDirectory.Active = null;
            Application.quitting -= LeaveLobby;
        }

        #endregion

        #region Lobby event callbacks

        private void OnLobbyChanged(ILobbyChanges changes)
        {
            if (changes.LobbyDeleted)
            {
                Debug.Log($"{Tag} Lobby deleted.");
                CurrentLobby = null;
                OnLobbyClosed?.Invoke();
                return;
            }

            // CRITICAL: the callback gives us a DIFF, it does NOT update CurrentLobby.
            // Apply it so the UI reads the new player list, not the stale one.
            if (CurrentLobby != null)
                changes.ApplyToLobby(CurrentLobby);

            if (changes.PlayerJoined.Changed)
            {
                Debug.Log($"{Tag} Player joined event. Players now: {CurrentLobby?.Players.Count}");
                OnClientConnected?.Invoke();
            }

            if (changes.PlayerLeft.Changed)
            {
                Debug.Log($"{Tag} Player left event. Players now: {CurrentLobby?.Players.Count}");
                OnClientDisconnected?.Invoke();
            }
        }

        private async void CheckStartGame(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> changedValues)
        {
            // The host created the relay itself (StartHost). It must NOT also try to
            // join the relay as a client, or it tears down its own connection.
            if (IsHost())
            {
                Debug.Log($"{Tag} DataChanged ignored (we are the host).");
                return;
            }

            if (!changedValues.TryGetValue("RelayCode", out var data))
                return;

            string code = data.Value.Value;
            Debug.Log($"{Tag} RelayCode changed -> '{code}'");

            if (string.IsNullOrEmpty(code) || code == "0")
                return;

            Debug.Log($"{Tag} Game starting. Client joining relay with code: {code}");
            bool joined = await RelayHandler.Instance.JoinRelay(code);
            if (!joined)
            {
                Debug.LogError($"{Tag} Client failed to join relay with code {code}");
                return;
            }

            OnGameStarting?.Invoke();
            CurrentLobby = null; // handed off to Netcode; drop the lobby handle on the client.
        }

        #endregion

        #region Heartbeat / auth

        private async Task HandleHeartBeatAsync()
        {
            if (CurrentLobby == null || !IsHost()) return; // only the host keeps the lobby alive
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
                Debug.Log($"{Tag} Heartbeat -> {CurrentLobby.Name}");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"{Tag} Heartbeat failed: {e.Message}");
            }
        }

        private async Task Authenticate() => await Authenticate("Player" + Random.Range(0, 1000));

        private async Task Authenticate(string playerName)
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                var options = new InitializationOptions();
                options.SetProfile(playerName);
                await UnityServices.InitializeAsync(options);
            }

            AuthenticationService.Instance.SignedIn += () =>
                Debug.Log($"{Tag} Signed in as {AuthenticationService.Instance.PlayerId}");

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            // Set these unconditionally (the original only set them on first sign-in).
            PlayerId = AuthenticationService.Instance.PlayerId;
            PlayerName = playerName;

            var d = new Dictionary<string, PlayerDataObject>
            {
                { "Name",    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                { "Ready",   new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0") },
                { "Penguin", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0") }
            };

            _playerObject = new Unity.Services.Lobbies.Models.Player { Data = d };
            Debug.Log($"{Tag} Authenticated. PlayerId={PlayerId}");
        }

        #endregion

        #region Lobby connection

        private async Task SubscribeToLobby()
        {
            if (CurrentLobby == null) return;
            try
            {
                await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, _events);
                Debug.Log($"{Tag} Subscribed to lobby events for {CurrentLobby.Id}");
            }
            catch (Exception e)
            {
                Debug.LogError($"{Tag} Failed to subscribe to lobby events: {e}");
            }
        }

        public async Task<bool> CreateLobby()
        {
            try
            {
                var option = new CreateLobbyOptions
                {
                    Player = _playerObject,
                    IsPrivate = false,
                    Data = new()
                    {
                        { "Map",        new DataObject(DataObject.VisibilityOptions.Public, "") },
                        { "RelayCode",  new DataObject(DataObject.VisibilityOptions.Member, "0") },
                        { "Difficulty", new DataObject(DataObject.VisibilityOptions.Public, "0") }
                    }
                };

                CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, option);
                Debug.Log($"{Tag} Created lobby '{CurrentLobby.Name}' code {CurrentLobby.LobbyCode} id {CurrentLobby.Id}");

                await SubscribeToLobby();
                _heartBeatTimer.Start();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"{Tag} Failed to create lobby: {e.Message}");
                return false;
            }

            OnLobbyOpened?.Invoke();
            return true;
        }

        public async Task QuickJoinLobby()
        {
            try
            {
                var options = new QuickJoinLobbyOptions
                {
                    Filter = new List<QueryFilter>
                    {
                        new(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ)
                    },
                    Player = _playerObject
                };

                CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            }
            catch (LobbyServiceException e)
            {
                try
                {
                    if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
                    {
                        bool result = await CreateLobby();
                        if (!result)
                        {
                            Debug.LogError($"{Tag} Failed to quick join (and create fallback failed): {e.Reason}");
                            return;
                        }
                        return; // CreateLobby already subscribed + invoked OnLobbyOpened
                    }

                    Debug.LogError($"{Tag} Failed to quick join: {e.Reason}");
                    return;
                }
                catch (LobbyServiceException exception)
                {
                    Debug.LogError($"{Tag} Failed to quick join: {exception.Message}");
                    return;
                }
            }

            await SubscribeToLobby();
            OnLobbyOpened?.Invoke();
        }

        public async Task<bool> JoinLobby(string inputCode)
        {
            try
            {
                var options = new JoinLobbyByCodeOptions { Player = _playerObject };
                CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(inputCode, options);
                Debug.Log($"{Tag} Joined lobby '{CurrentLobby.Name}' id {CurrentLobby.Id} as {AuthenticationService.Instance.PlayerId}");

                // CRITICAL FIX: without this the client never receives DataChanged,
                // so it never hears the RelayCode and never joins the relay.
                await SubscribeToLobby();

                OnLobbyOpened?.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"{Tag} Failed to join lobby '{inputCode}': {e.Reason} / {e.Message}");
                return false;
            }

            return true;
        }

        public async void LeaveLobby()
        {
            if (CurrentLobby == null) return;
            _heartBeatTimer.Stop();
            try
            {
                if (CurrentLobby.Players.Count == 1)
                {
                    Debug.Log($"{Tag} Destroying an empty lobby.");
                    await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
                    CurrentLobby = null;
                    OnLobbyClosed?.Invoke();
                    return;
                }

                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
                CurrentLobby = null;
                Debug.Log($"{Tag} Left the lobby.");
                OnLobbyClosed?.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"{Tag} LeaveLobby failed: {e.Reason}");
            }
        }

        #endregion

        #region Game start

        public async void StartGame(string map, string diff)
        {
            if (!IsHost())
            {
                Debug.LogWarning($"{Tag} StartGame called by non-host; ignored.");
                return;
            }

            try
            {
                Debug.Log($"{Tag} Host starting game. Creating relay...");

                // Relay maxConnections = number of PEERS (everyone except the host).
                int peers = Mathf.Max(1, maxPlayers - 1);
                string relayCode = await RelayHandler.Instance.CreateRelay(peers);
                if (string.IsNullOrEmpty(relayCode))
                {
                    Debug.LogError($"{Tag} CreateRelay returned no code; aborting StartGame.");
                    return;
                }
                Debug.Log($"{Tag} Relay created. Code: {relayCode}");

                CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "Map",        new DataObject(DataObject.VisibilityOptions.Public, map) },
                        { "RelayCode",  new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
                        { "Difficulty", new DataObject(DataObject.VisibilityOptions.Public, diff) }
                    },
                    IsLocked = true
                });
                Debug.Log($"{Tag} Lobby updated with relay code; waiting for clients to connect...");

                await WaitForAllClientsToConnect();
                OnGameStarting?.Invoke();

                Debug.Log($"{Tag} Loading network scene '{map}'.");
                NetworkManager.Singleton.SceneManager.LoadScene(map, LoadSceneMode.Single);
            }
            catch (Exception e)
            {
                Debug.LogError($"{Tag} Failed to start the game: {e}");
            }
        }

        private async Task WaitForAllClientsToConnect()
        {
            int expected = CurrentLobby.Players.Count; // host + clients
            int attempts = 10;

            while (attempts-- > 0)
            {
                int connected = NetworkManager.Singleton.ConnectedClientsIds.Count;
                Debug.Log($"{Tag} Waiting for clients: {connected}/{expected}");

                if (connected >= expected)
                {
                    Debug.Log($"{Tag} All clients connected.");
                    return;
                }

                await Task.Delay(PollTimer);
            }

            Debug.LogWarning($"{Tag} Timed out waiting for clients: " +
                             $"{NetworkManager.Singleton.ConnectedClientsIds.Count}/{expected}. Loading anyway.");
        }

        #endregion

        #region Lobby / player updates

        public async Task UpdateLobby(string map, string diff)
        {
            if (CurrentLobby == null || !IsHost())
            {
                Debug.LogError($"{Tag} UpdateLobby called without host rights / lobby.");
                return;
            }
            try
            {
                CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new()
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "Map",        new DataObject(DataObject.VisibilityOptions.Public, map) },
                        { "RelayCode",  new DataObject(DataObject.VisibilityOptions.Member, "0") },
                        { "Difficulty", new DataObject(DataObject.VisibilityOptions.Public, diff) }
                    }
                });
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"{Tag} UpdateLobby failed: {e.Message}");
            }
        }

        public async Task UpdatePlayer(string penguin, string ready)
        {
            if (CurrentLobby == null)
            {
                Debug.LogError($"{Tag} UpdatePlayer called without a lobby.");
                return;
            }
            try
            {
                // Use the authenticated player id (the local _playerObject.Id is null
                // until the service assigns it inside CurrentLobby.Players).
                string myId = AuthenticationService.Instance.PlayerId;
                CurrentLobby = await LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, myId, new()
                {
                    Data = new()
                    {
                        { "Name",    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerObject.Data["Name"].Value) },
                        { "Ready",   new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ready) },
                        { "Penguin", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, penguin) }
                    }
                });
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"{Tag} UpdatePlayer failed: {e.Reason}");
            }
        }

        public async UniTask<bool> TryRemovePlayer(Player currentPlayer)
        {
            if (!IsHost() || currentPlayer == null || currentPlayer.Id == CurrentLobby.HostId)
                return false;
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, currentPlayer.Id);
                Debug.Log($"{Tag} Removed player {currentPlayer.Id}");
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"{Tag} Failed to remove player: {e.Reason}");
                return false;
            }
        }

        #endregion
    }
}