using Eflatun.SceneReference;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LobbyUI : MonoBehaviour
{
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button joinLobbyButton;
        [SerializeField] SceneReference gameScene;

        private void Awake()
        {
                createLobbyButton.onClick.AddListener(CreateGame); 
                joinLobbyButton.onClick.AddListener(JoinGame);
        }

        private async void CreateGame()
        {
                await Multiplayer.Instance.CreateLobby();
        }

        private async void JoinGame()
        {
                await Multiplayer.Instance.QuickJoinLobby();
        }

        public static class Loader
        {
                public static void LoadNetword(SceneReference sceneReference)
                {
                        NetworkManager.Singleton.SceneManager.LoadScene(sceneReference.Name, LoadSceneMode.Single);
                }
        }

}