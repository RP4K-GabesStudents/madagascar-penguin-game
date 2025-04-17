using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnableBubbleText : MonoBehaviour
{
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private int minTextSize = 120;
    [SerializeField] private int maxTextSize = 180;
    [SerializeField] private AnimationCurve textScaleCurve;

    private TextMeshProUGUI tmp;

    private void OnEnable()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        StartCoroutine(BubbleTimer());
    }

    private IEnumerator BubbleTimer()
    {
        float t = 0;
        while (t < duration)
        {
            float p = t / duration;
            t += Time.deltaTime;
            tmp.fontSize = Mathf.Lerp(minTextSize, maxTextSize, textScaleCurve.Evaluate(p));
            yield return null;
        }
        tmp.fontSize = Mathf.Lerp(minTextSize, maxTextSize, textScaleCurve.Evaluate(1));
    }


    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
