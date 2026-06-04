using Eflatun.SceneReference;
using GabesCommonUtility.GabesCommonUtility.Multiplayer.GameObjects.Sequencing;
using Managers;
using Network.Sequences.Create;
using TMPro;
using UnityEditor.ShortcutManagement;
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
                

                private void OnEnable()
                {
                        LobbySystemManager.Instance.OnClientConnected += UpdateLobby;
                        LobbySystemManager.Instance.OnClientDisconnected += UpdateLobby;
                        LobbySystemManager.Instance.OnLobbyOpened += UpdateLobby;
                        LobbySystemManager.Instance.OnLobbyClosed += OnLobbyClosed;
                        LobbySystemManager.Instance.OnGameStarting += JoinRelay;
                }


                private void OnDisable()
                {
                        LobbySystemManager.Instance.OnClientConnected -= UpdateLobby;
                        LobbySystemManager.Instance.OnClientDisconnected -= UpdateLobby;
                        LobbySystemManager.Instance.OnLobbyOpened -= UpdateLobby;
                        LobbySystemManager.Instance.OnLobbyClosed -= OnLobbyClosed;
                        LobbySystemManager.Instance.OnGameStarting -= JoinRelay;
                }

                private void DisableInput()
                {
                        if (startGameButton != null) startGameButton.interactable = false;
                }

                private void Awake()
                {
                        _sequence = GetComponent<NetcodeSigninSequence>();
                        startGameButton?.onClick.AddListener(StartGame);
                }

                public async void CreateGame()
                {
                        if(!_sequence.IsCompleted) await _sequence.ExecuteSequence();
                        OnTryCreateGameStart.Invoke();
                        await LobbySystemManager.Instance.CreateLobby();
                        
                        UpdateLobby();
                        
                        if (LobbySystemManager.Instance.IsConnected()) OnTryCreateGameSucceed.Invoke();
                        else OnTryCreateGameFail.Invoke();
                }

                private async void StartGame()
                {
                        LobbySystemManager.Instance.StartGame(gameScene.Name, "0");
                }
                //when we press enter
                public async void JoinGameWithCode()
                {
                        JoinGameWithCode(lobbyCodeField.text);
                }

                //when we stop typing optional
                public async void JoinGameWithCode(string code)
                {
                        if(!_sequence.IsCompleted) await _sequence.ExecuteSequence();
                        if (code.Length != 6) return; //default unity code length
                        OnTryJoinGameStart?.Invoke();
                        await LobbySystemManager.Instance.JoinLobby(code);
                        UpdateLobby();
                        
                        if (LobbySystemManager.Instance.IsConnected()) OnTryJoinGameSucceed.Invoke();
                        else OnTryJoinGameFail?.Invoke();
                }

                private void UpdateLobby()
                {
                        if (!LobbySystemManager.Instance.IsConnected()) return;
                        startGameButton.gameObject.SetActive(LobbySystemManager.Instance.IsHost());
                        lobbyText.text = LobbySystemManager.Instance.IsHost().ToString();
                        lobbyCode.text = LobbySystemManager.Instance.CurrentLobby.LobbyCode;
                }

                private void OnLobbyClosed()
                {       
                        startGameButton.gameObject.SetActive(false);
                }

                private async void JoinRelay()
                {
                        DisableInput();
                        if (!LobbySystemManager.Instance.IsHost())
                                await RelayHandler.Instance.JoinRelay(lobbyCodeField.text);
                }
        }
}