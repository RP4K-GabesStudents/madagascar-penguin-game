using UnityEngine;
using Cutscene.Core;

namespace Cutscene.Managers
{
    public class CutsceneManager : MonoBehaviour
    {
        public CutsceneManager Instance { get; private set; }
        private ICutscenes _currentCutscene;
        

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayCutscene(ICutscenes cutscene)
        {
            _currentCutscene = cutscene;
        }

        public ICutscenes GetCurCutscene()
        {
            return _currentCutscene;
        }

        public bool IsCutscenePlaying()
        {
            return false;
        }
        
    }
}
