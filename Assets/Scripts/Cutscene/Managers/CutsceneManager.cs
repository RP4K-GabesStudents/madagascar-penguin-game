using UnityEngine;

namespace Cutscene
{
    public class CutsceneManager : MonoBehaviour
    {
        public CutsceneManager Instance { get; private set; }
        private Cutscenes.Cutscene _currentCutscene;
        

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

        public void PlayCutscene(Cutscenes.Cutscene cutscene)
        {
            _currentCutscene = cutscene;
        }

        public Cutscenes.Cutscene GetCurCutscene()
        {
            return _currentCutscene;
        }

        public bool IsCutscenePlaying()
        {
            return false;
        }
        
    }
}
