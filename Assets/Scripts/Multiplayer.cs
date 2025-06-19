using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Random = UnityEngine.Random;

public class Multiplayer : MonoBehaviour
{
    [SerializeField] private string lobbyName = "lobby";
    [SerializeField] private int maxPlayers = 4;

    public static Multiplayer Instance { get; set; }
    public string PlayerName { get; private set; }
    public string PlayerId { get; private set; }

    private const float HeartBeatInterval = 20f;
    private const float UpdateTimeInterval = 65f;
    
    private Lobby _currentLobby;
    
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
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to create lobby: " + e.Message);
        }
    }

    public async Task QuickJoinLobby()
    {
        
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
