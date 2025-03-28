using UnityEngine;

namespace Managers
{
    public class ColourManager : MonoBehaviour
    {
        public static Color CurColour = Color.red;
        [SerializeField] private float colourChangeSpeed = 0.5f;
        
        private int _stage = 0;
        private float _progress = 0f;

        private void Update()
        {
            _progress += Time.deltaTime * colourChangeSpeed;
            
            if (_progress >= 1f)
            {
                _progress = 0f;
                _stage = (_stage + 1) % 6;
            }
            
            switch (_stage)
            {
                case 0: // Red to Yellow (Increase G)
                    CurColour = new Color(1f, _progress, 0f);
                    break;
                case 1: // Yellow to Green (Decrease R)
                    CurColour = new Color(1f - _progress, 1f, 0f);
                    break;
                case 2: // Green to Cyan (Increase B)
                    CurColour = new Color(0f, 1f, _progress);
                    break;
                case 3: // Cyan to Blue (Decrease G)
                    CurColour = new Color(0f, 1f - _progress, 1f);
                    break;
                case 4: // Blue to Magenta (Increase R)
                    CurColour = new Color(_progress, 0f, 1f);
                    break;
                case 5: // Magenta to Red (Decrease B)
                    CurColour = new Color(1f, 0f, 1f - _progress);
                    break;
            }
        }
    }
}