using System;
using Eflatun.SceneReference;
using GabesCommonUtility.GabesCommonUtility.Multiplayer.GameObjects.Sequencing;
using Managers;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
        public class LobbyUI : MonoBehaviour
        {
                [Header("In Game")]
                [SerializeField] private Button startGameButton;
                [SerializeField] SceneReference gameScene;
                [SerializeField] private TextMeshProUGUI lobbyText;
                [SerializeField] private TextMeshProUGUI lobbyCode;

                [Header("Join Game")]
                [SerializeField] private TMP_InputField lobbyCodeField;

                public UnityEvent OnTryJoinGameStart;
                public UnityEvent OnTryJoinGameSucceed;
                public UnityEvent OnTryJoinGameFail;

                [Header("Create Game")]
                [SerializeField] private Button hostButton;

                public UnityEvent OnTryCreateGameStart;
                public UnityEvent OnTryCreateGameSucceed;
                public UnityEvent OnTryCreateGameFail;

                private NetcodeSigninSequence _sequence;

                [SerializeField] private LobbyPlayer[] lobbyPlayers;

                [Serializable]
                private class LobbyPlayer
                {
                        [SerializeField] private TextMeshProUGUI name;
                        [SerializeField] private Button button;
                        private Player _current;

                        public void Initialize()
                        {
                                button.onClick.AddListener(RemovePlayer);
                                button.interactable = false;
                                name.text = "";
                        }

                        public void Set(Player player)
                        {
                                if (player == _current) return;
                                _current = player;
                                bool state = player != null;
                                button.interactable = state;
                                name.text = "";
                                if (!state) return;
                                name.text = player.Data["Name"].Value;
                        }

                        public void RemovePlayer()
                        {
                                if (_current != null)
                                        LobbySystemManager.Instance.TryRemovePlayer(_current);
                        }
                }

                private void OnEnable()
                {
                        LobbySystemManager.Instance.OnClientConnected += UpdateLobby;
                        LobbySystemManager.Instance.OnClientDisconnected += UpdateLobby;
                        LobbySystemManager.Instance.OnLobbyOpened += UpdateLobby;
                        LobbySystemManager.Instance.OnLobbyClosed += OnLobbyClosed;
                        LobbySystemManager.Instance.OnGameStarting += OnGameStarting;
                }

                private void OnDisable()
                {
                        LobbySystemManager.Instance.OnClientConnected -= UpdateLobby;
                        LobbySystemManager.Instance.OnClientDisconnected -= UpdateLobby;
                        LobbySystemManager.Instance.OnLobbyOpened -= UpdateLobby;
                        LobbySystemManager.Instance.OnLobbyClosed -= OnLobbyClosed;
                        LobbySystemManager.Instance.OnGameStarting -= OnGameStarting;
                }

                private void Awake()
                {
                        _sequence = GetComponent<NetcodeSigninSequence>();
                        startGameButton?.onClick.AddListener(StartGame);

                        // Players were never initialised, so the remove-buttons never wired up.
                        if (lobbyPlayers != null)
                                foreach (var p in lobbyPlayers)
                                        p.Initialize();
                }

                private void DisableInput()
                {
                        if (startGameButton != null) startGameButton.interactable = false;
                }

                public async void CreateGame()
                {
                        if (!_sequence.IsCompleted) await _sequence.ExecuteSequence();
                        OnTryCreateGameStart.Invoke();
                        await LobbySystemManager.Instance.CreateLobby();

                        UpdateLobby();

                        if (LobbySystemManager.Instance.IsConnected()) OnTryCreateGameSucceed.Invoke();
                        else OnTryCreateGameFail.Invoke();
                }

                private void StartGame()
                {
                        LobbySystemManager.Instance.StartGame(gameScene.Name, "0");
                }

                // when we press enter
                public void JoinGameWithCode()
                {
                        JoinGameWithCode(lobbyCodeField.text);
                }

                // when we stop typing (optional)
                public async void JoinGameWithCode(string code)
                {
                        if (!_sequence.IsCompleted) await _sequence.ExecuteSequence();
                        if (code.Length != 6) return; // default unity code length
                        OnTryJoinGameStart?.Invoke();
                        await LobbySystemManager.Instance.JoinLobby(code);
                        UpdateLobby();

                        if (LobbySystemManager.Instance.IsConnected()) OnTryJoinGameSucceed.Invoke();
                        else OnTryJoinGameFail?.Invoke();
                }

                private void UpdateLobby()
                {
                        if (!LobbySystemManager.Instance.IsConnected()) return;

                        var lobby = LobbySystemManager.Instance.CurrentLobby;

                        bool isHost = LobbySystemManager.Instance.IsHost();
                        startGameButton.gameObject.SetActive(isHost);
                        lobbyText.text = isHost.ToString();
                        lobbyCode.text = lobby.LobbyCode;

                        for (int i = 0; i < lobbyPlayers.Length; i++)
                        {
                                // Guard the index: indexing a List past Count throws,
                                // it does NOT return null.
                                Player p = i < lobby.Players.Count ? lobby.Players[i] : null;
                                lobbyPlayers[i].Set(p);
                        }
                }

                private void OnLobbyClosed()
                {
                        startGameButton.gameObject.SetActive(false);
                }

                // Fired by OnGameStarting. The actual relay join for clients is handled in
                // LobbySystemManager.CheckStartGame with the CORRECT relay code. This used
                // to also call RelayHandler.JoinRelay(lobbyCodeField.text) — the 6-char
                // LOBBY code — which started the client a second time and dropped the
                // connection. It now only updates UI state.
                private void OnGameStarting()
                {
                        Debug.Log("[LobbyUI] Game starting - disabling input.");
                        DisableInput();
                }
        }
}