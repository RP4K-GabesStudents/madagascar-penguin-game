using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;

namespace Utilities.Vivox
{
    public class LoadSceneSequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour next;

        [SerializeField] private SceneReference selectionScene;
        
        [SerializeField] private LoadSceneMode loadType = LoadSceneMode.Single;
        
        public IEntrySequence Default => next as IEntrySequence;
        public bool IsCompleted => SceneManager.GetActiveScene().buildIndex == selectionScene.BuildIndex;

        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {

            await SceneManager.LoadSceneAsync(selectionScene.BuildIndex, loadType);

            return Default;
        }

        private void OnDrawGizmos()
        {
            if (next && Default == null)
            {
                Debug.LogError("Success is INVALID", gameObject);
            }
        }
    }
}