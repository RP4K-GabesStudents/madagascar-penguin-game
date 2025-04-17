using System.Collections;
using TMPro;
using UnityEngine;

public class EnableAlphaText : MonoBehaviour
{
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private int startAlpha = 0;
    [SerializeField] private int endAlpha = 1;
    [SerializeField] private AnimationCurve alphaCurveScale;

    private TextMeshProUGUI tmp;

    private void OnEnable()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        StartCoroutine(BubbleTimer());
    }

    private IEnumerator BubbleTimer()
    {
        float t = 0;
        Color c = tmp.color;
        while (t < duration)
        {
            float p = t / duration;
            t += Time.deltaTime;
            c.a  = Mathf.LerpUnclamped(startAlpha, endAlpha, alphaCurveScale.Evaluate(p));
            tmp.color = c;
            yield return null;
        }
        c.a = Mathf.LerpUnclamped(startAlpha, endAlpha, alphaCurveScale.Evaluate(1));
        tmp.color = c;
    }


    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
