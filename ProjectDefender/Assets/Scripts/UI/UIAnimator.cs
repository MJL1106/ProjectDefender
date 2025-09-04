using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimator : MonoBehaviour
{
    
    public void ChangePosition(Transform myTransform, Vector3 offset, float duration = .1f)
    {
        RectTransform rectTransform = myTransform.GetComponent<RectTransform>();
        StartCoroutine(ChangePositionCo(rectTransform, offset, duration));
    }
    
    private IEnumerator ChangePositionCo(RectTransform rectTransform, Vector3 offset, float duration)
    {
        float time = 0;

        Vector3 initialPosition = rectTransform.anchoredPosition;
        Vector3 targetPosition = initialPosition + offset;

        while (time < duration)
        {
            rectTransform.anchoredPosition = Vector3.Lerp(initialPosition, targetPosition, time / duration);
            time += Time.deltaTime;

            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
    }
}
