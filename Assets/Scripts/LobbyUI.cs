using System;
using Eflatun.SceneReference;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LobbyUI : MonoBehaviour
{
        [SerializeField] private Button quickJoinButton;
        [SerializeField] private Button startGameButton;
        [SerializeField] SceneReference gameScene;
        [SerializeField] private TextMeshProUGUI lobbyText;

        private void OnEnable()
        {
                LobbySystemManager.Instance.OnClientConnected += UpdateLobby;
                LobbySystemManager.Instance.OnClientDisconnected += UpdateLobby;
                LobbySystemManager.Instance.OnLobbyOpened += UpdateLobby;
                LobbySystemManager.Instance.OnLobbyClosed += UpdateLobby;
                LobbySystemManager.Instance.OnGameStarting += DisableInput;
        }


        private void OnDisable()
        {
                LobbySystemManager.Instance.OnClientConnected -= UpdateLobby;
                LobbySystemManager.Instance.OnClientDisconnected -= UpdateLobby;
                LobbySystemManager.Instance.OnLobbyOpened -= UpdateLobby;
                LobbySystemManager.Instance.OnLobbyClosed -= UpdateLobby;
                LobbySystemManager.Instance.OnGameStarting -= DisableInput;
        }

        private void DisableInput()
        {
                startGameButton.interactable = false;
        }

        private void Awake()
        {
                quickJoinButton.onClick.AddListener(JoinGame); 
                startGameButton.onClick.AddListener(StartGame);
                UpdateLobby();
        }

        private async void StartGame()
        {
                LobbySystemManager.Instance.StartGame(gameScene.Name, "0");
        }

        private async void JoinGame()
        {
                await LobbySystemManager.Instance.QuickJoinLobby();
                UpdateLobby();
        }

        private void UpdateLobby()
        {
                startGameButton.gameObject.SetActive(LobbySystemManager.Instance.IsHost());
                startGameButton.interactable = true;
                lobbyText.text = LobbySystemManager.Instance.IsHost().ToString();
        }
}