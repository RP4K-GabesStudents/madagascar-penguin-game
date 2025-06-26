using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;


[System.Serializable]
public enum EncryptionType
{
    Dtls,
    Wss
}

public class Multiplayer : MonoBehaviour
{
    [SerializeField] private string lobbyName = "lobby";
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] EncryptionType encryptionType = EncryptionType.Dtls;
    

    public static Multiplayer Instance { get; set; }
    public string PlayerName { get; private set; }
    public string PlayerId { get; private set; }

    
    
    private Lobby _currentLobby;
    private string ConnectionType => encryptionType == EncryptionType.Dtls ? DtlsEncryption : WssEncryption;
    
    private const float LobbyHeartBeatInterval = 20f;
    private const float LobbyPollInterval = 65f;
    private const string KeyJoinCode = "RelayJoinCode";
    private const string DtlsEncryption = "Dtls";
    private const string WssEncryption = "Wss";

    private CountdownTimer _heartBeatTimer = new CountdownTimer(LobbyHeartBeatInterval);
    private CountdownTimer _pollForUpdatesTimer = new CountdownTimer(LobbyPollInterval);
    
    
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
            HandleHeartBeatAsync();
            _heartBeatTimer.Start();
        };
        _pollForUpdatesTimer.OnTimerStop += () =>
        {
            HandlePollForUpdatesAsync();
            _pollForUpdatesTimer.Start();
        };
    }

    private async Task HandleHeartBeatAsync()
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            Debug.Log("Sent heartbeat ping to lobby: " + _currentLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to heartbeat lobby:" + e.Message);
        }
    }
    private async Task HandlePollForUpdatesAsync()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
            Debug.Log("Sent heartbeat ping to lobby: " + _currentLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to heartbeat lobby:" + e.Message);
        }
    }

    private async Task Authenticate()
    {
        await Authenticatee("Player" + Random.Range(0, 1000));
    }

    private async Task Authenticatee(string playerName)
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
    }
    
    
    public async Task CreateLobby()
    {
        try
        {
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation);

            CreateLobbyOptions option = new CreateLobbyOptions
            {
                IsPrivate = false
            };
            
            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, option);
            Debug.Log("Created Lobby: " + _currentLobby.Name + "with code " + _currentLobby.LobbyCode );
            
            _heartBeatTimer.Start();
            _pollForUpdatesTimer.Start();

            await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>
                {
                    {KeyJoinCode, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
                }
            });
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, ConnectionType));

            NetworkManager.Singleton.StartHost();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to create lobby: " + e.Message);
        }
    }

    public async Task QuickJoinLobby()
    {
        try
        {
            _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            _pollForUpdatesTimer.Start();
            
            string relayJoinCode = _currentLobby.Data[KeyJoinCode].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, ConnectionType));

            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to allocate relay:" + e.Message);
            return default;
        }
    }

    async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("failed to get relay join code:" + e.Message);
            return default;
        }
    }

    async Task<JoinAllocation> JoinRelay(string relayJoinCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("failed to join relay:" + e.Message);
            return default;
        }
    }
    
    
}
