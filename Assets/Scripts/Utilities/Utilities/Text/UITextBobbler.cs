using System.Collections;
using TMPro;
using UnityEngine;

namespace Utilities.Utilities.Text
{
    public class UITextBobbler : MonoBehaviour
    {
    
        //Generated by ChatGPT
    
        [Tooltip("Amplitude of the wave, how high the characters move.")]
        public float amplitude = 5f;
    
        [Tooltip("Frequency of the wave, how often the characters move.")]
        public float frequency = 1f;
        
        public float initialDelay = 1f;

        private TextMeshProUGUI textMesh;
        private TMP_TextInfo textInfo;
        private Vector3[] vertices;

        private void OnEnable()
        {
            if (!textMesh)
            {
                textMesh = GetComponent<TextMeshProUGUI>();
                textInfo = textMesh.textInfo;
            }
            StartCoroutine(AnimateText());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator AnimateText()
        {
            yield return new WaitForSeconds(initialDelay);
            while (true)
            {
                textMesh.ForceMeshUpdate();
                textInfo = textMesh.textInfo;

                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    if (!textInfo.characterInfo[i].isVisible)
                        continue;

                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                    vertices = textInfo.meshInfo[textInfo.characterInfo[i].materialReferenceIndex].vertices;

                    Vector3 offset = Wobble(Time.time + i);
                    vertices[vertexIndex + 0] += offset;
                    vertices[vertexIndex + 1] += offset;
                    vertices[vertexIndex + 2] += offset;
                    vertices[vertexIndex + 3] += offset;
                }

                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                    textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }

                yield return null;
            }
        }

        Vector2 Wobble(float time)
        {
            return new Vector2(0, Mathf.Sin(time * frequency) * amplitude);
        }
    }
}
