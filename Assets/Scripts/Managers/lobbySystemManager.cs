using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public class LobbySystemManager : MonoBehaviour
    {
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
        
        private CountdownTimer _heartBeatTimer = new CountdownTimer(LobbyHeartBeatInterval);
        
        private readonly LobbyEventCallbacks _events = new();
        public bool IsHost() => CurrentLobby != null && CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;
        private void OnEnable()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
    
            _events.DataChanged += CheckStartGame;
            _events.LobbyChanged += changes =>
            {
                if (changes.LobbyDeleted)
                {
                    Debug.Log("Hey does this lobby closed happen twice when deleted?");
                    OnLobbyClosed?.Invoke();
                }
                else if (changes.PlayerJoined.Changed)
                {
                    OnClientConnected?.Invoke();
                }
                else if (changes.PlayerLeft.Changed)
                {
                    OnClientDisconnected?.Invoke();
                }
            };
            Application.quitting += LeaveLobby;
        }
        private async void CheckStartGame(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> obj)
        {
            
            if (CurrentLobby.Data["RelayCode"].Value != "0")
            {
                Debug.Log("HEARD: Game starting request: " + NetworkManager.ServerClientId + " --> " + CurrentLobby.Data["RelayCode"].Value);
                Debug.LogError("Gabe come back");
                //RelayHandler.Instance.SetLocalServerInfo(BitConverter.GetBytes(int.Parse(_playerObject.Data["Penguin"])));
                await RelayHandler.Instance.JoinRelay(CurrentLobby.Data["RelayCode"].Value);
                OnGameStarting?.Invoke();
                CurrentLobby = null;
            }
        }
        private void OnDisable()
        {
            Application.quitting -= LeaveLobby;
        }
        private void OnDestroy()
        {
            Application.quitting -= LeaveLobby;
        }
        private async void Start()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            await Authenticate();

            _heartBeatTimer.OnTimerStop += () =>
            {
                _ = HandleHeartBeatAsync();
                _heartBeatTimer.Start();
            };
        }
        private async Task HandleHeartBeatAsync()
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
                Debug.Log("Sent heartbeat ping to lobby: " + CurrentLobby.Name);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Failed to heartbeat lobby:" + e.Message);
            }
        }
        private async Task Authenticate()
        {
            await Authenticate("Player" + Random.Range(0, 1000));
        }
        private async Task Authenticate(string playerName)
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                InitializationOptions options = new InitializationOptions();
                options.SetProfile(playerName);

                await UnityServices.InitializeAsync(options);
            }

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed In as" + AuthenticationService.Instance.PlayerId);
            };

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                PlayerId = AuthenticationService.Instance.PlayerId;
                PlayerName = playerName;
            }
        
            Dictionary<string, PlayerDataObject> d = new Dictionary<string, PlayerDataObject>()
            {
                {
                    "Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)
                },
                {
                    "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0")
                },
                {
                    "Penguin", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0")
                }
            };
            
            _playerObject = new Unity.Services.Lobbies.Models.Player()
            {
                Data = d
            };
        }
        public async Task<bool> CreateLobby()
        {
            try
            {
                CreateLobbyOptions option = new CreateLobbyOptions
                {
                    Player = _playerObject,
                    IsPrivate = false,
                    Data = new()
                    {
                        { "Map", new DataObject(DataObject.VisibilityOptions.Public, "") },
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "0") },
                        { "Difficulty", new DataObject(DataObject.VisibilityOptions.Public, "0") }
                    }
                };

                CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, option);
                Debug.Log("Created Lobby: " + CurrentLobby.Name + "with code " + CurrentLobby.LobbyCode);

                _heartBeatTimer.Start();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Failed to create lobby: " + e.Message);
                return false;
            }
            OnLobbyOpened?.Invoke();
            return true;
        }
        public async Task QuickJoinLobby()
        {
            try
            {
                QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
                {
                    Filter = new List<QueryFilter>()
                    {
                        new(QueryFilter.FieldOptions.IsLocked, "0",
                            QueryFilter.OpOptions.EQ) // Make sure lobby is not locked
                    },
                    Player = _playerObject // We are the local player
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
                            Debug.LogError("Failed to quick join: " + e.Reason);
                            //await SceneManager.LoadSceneAsync(0);
                            return;
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to quick join: " + e.Reason);
                        //await SceneManager.LoadSceneAsync(0);
                        return;
                    }
                }
                catch (LobbyServiceException exception)
                {
                    Debug.LogError("Failed to quick join: " + exception.Message);
                    return;
                }
            }
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, _events);
            OnLobbyOpened?.Invoke();
        }
        public async Task<bool> JoinLobby(string inputCode)
        {
            try
            {
                JoinLobbyByCodeOptions options = new()
                {
                    Player = _playerObject
                };
                CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(inputCode, options);
                
                OnLobbyOpened?.Invoke();
                _heartBeatTimer.Start();
            }
            catch (LobbyServiceException e)
            {
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
                    Debug.Log("Destroying an empty lobby.");
                    await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
                    OnLobbyClosed?.Invoke();
                    return;
                }
                
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
                CurrentLobby = null;
                Debug.Log("I have left the lobby, later nerds!");
                OnLobbyClosed?.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("sadness" + e.Reason);
            }
        }
        public async void StartGame(string map, string diff)
        {
            if (!IsHost()) return;
    
            try
            {
                Debug.Log("Starting game!");
                
                string relayCode = await RelayHandler.Instance.CreateRelay(CurrentLobby.Players.Count);

                CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions()
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "Map", new DataObject(DataObject.VisibilityOptions.Public, map) },
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
                        { "Difficulty", new DataObject(DataObject.VisibilityOptions.Public, diff)}
                        
                    },
                    IsLocked = true,
                });
        
                await WaitForAllClientsToConnect();
                OnGameStarting?.Invoke();
                
                Debug.Log("Beginning Network loading!");
                //NOTE: there's a return type here that may be useful
                NetworkManager.Singleton.SceneManager.LoadScene(map, LoadSceneMode.Single);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to start the game: " + e);
            }

        }
        private async Task WaitForAllClientsToConnect()
        {
            int expectedClients = CurrentLobby.Players.Count;

            int attempts = 10; // Number of retries before timing out
            while (attempts-- > 0)
            {
                int connectedClients = NetworkManager.Singleton.ConnectedClientsIds.Count;
                Debug.Log($"Connected Clients: {connectedClients}/{expectedClients}");

                if (connectedClients == expectedClients)
                {
                    Debug.Log("All clients are connected!");

                    return;
                }

                await Task.Delay(PollTimer);
            }

            Debug.LogWarning(
                $"Not all clients connected in time. {NetworkManager.Singleton.ConnectedClientsIds.Count}/{CurrentLobby.Players.Count}");
        }
        public async Task UpdateLobby(string map, string diff)
        {
            if (CurrentLobby == null || !IsHost())
            {
                Debug.LogError("you are a dumb goober");
                return;
            }
            try
            {
                CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new ()
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "Map", new DataObject(DataObject.VisibilityOptions.Public, map) },
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "0") },
                        { "Difficulty", new DataObject(DataObject.VisibilityOptions.Public, diff) }
                    }
                });
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("you are a dumb goober" + e.Message);
            }
        }
        public async Task UpdatePlayer(string penguin, string ready)
        {
            if (CurrentLobby == null)
            {
                Debug.LogError("you are a dumb goober");
                return;
            }
            try
            {
                CurrentLobby = await LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, _playerObject.Id, new ()
                {
                    Data = new()
                    {
                        {
                            "Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerObject.Data["Name"].Value)
                        },
                        {
                            "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ready)
                        },
                        {
                            "Penguin", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, penguin)
                        }
                    }
                });
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("update player has stopped working" + e.Reason);
            }
        }
    }
}