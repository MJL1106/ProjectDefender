using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Provides common UI animation coroutines (Shake, ChangePosition, ChangeScale, ChangeColour).
/// </summary>
public class UIAnimator : MonoBehaviour
{
    [Header("UI feedback - Shake Effect")] 
    [SerializeField] private float shakeMagnitude;
    [SerializeField] private float shakeDuration;
    [SerializeField] private float shakeRotationMagnitude;
    [Space] [SerializeField] private float defaultUIScale = 1.5f;
    [SerializeField] private bool scaleChangeAvailable;

    /// <summary>
    /// Triggers a shake animation on a UI RectTransform.
    /// </summary>
    /// <param name="transformToShake">The UI element to shake.</param>
    public void Shake(Transform transformToShake)
    {
        RectTransform rectTransform = transformToShake.GetComponent<RectTransform>();
        StartCoroutine(ShakeCo(rectTransform));
    }

    /// <summary>
    /// Coroutine that applies random offsets and rotation for the shake effect.
    /// </summary>
    /// <param name="rectTransform">The UI element's RectTransform.</param>
    private IEnumerator ShakeCo(RectTransform rectTransform)
    {
        float time = 0;
        Vector3 originalPosition = rectTransform.anchoredPosition;

        float currentScale = rectTransform.localScale.x;

        if (scaleChangeAvailable) StartCoroutine(ChangeScaleCo(rectTransform, currentScale * 1.1f, shakeDuration / 2));

        while (time < shakeDuration)
        {
            float xOffset = Random.Range(-shakeMagnitude, shakeMagnitude);
            float yOffset = Random.Range(-shakeMagnitude, shakeMagnitude);
            float randomRotation = Random.Range(-shakeRotationMagnitude, shakeRotationMagnitude);

            rectTransform.anchoredPosition = originalPosition + new Vector3(xOffset, yOffset);
            rectTransform.localRotation = Quaternion.Euler(0, 0, randomRotation);

            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
        
        if (scaleChangeAvailable) StartCoroutine(ChangeScaleCo(rectTransform, defaultUIScale, shakeDuration / 2));
    }
    
    /// <summary>
    /// Starts a coroutine to move a UI element by a given offset.
    /// </summary>
    /// <param name="myTransform">The UI element to move.</param>
    /// <param name="offset">The anchoredPosition offset to add.</param>
    /// <param name="duration">The time in seconds for the animation.</param>
    public void ChangePosition(Transform myTransform, Vector3 offset, float duration = .1f)
    {
        RectTransform rectTransform = myTransform.GetComponent<RectTransform>();
        StartCoroutine(ChangePositionCo(rectTransform, offset, duration));
    }
    
    /// <summary>
    /// Coroutine to lerp a RectTransform's anchoredPosition.
    /// </summary>
    /// <param name="rectTransform">The UI element's RectTransform.</param>
    /// <param name="offset">The anchoredPosition offset to add.</param>
    /// <param name="duration">The time in seconds for the animation.</param>
    public IEnumerator ChangePositionCo(RectTransform rectTransform, Vector3 offset, float duration = .1f)
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

    /// <summary>
    /// Starts a coroutine to scale a UI element.
    /// </summary>
    /// <param name="transform">The UI element to scale.</param>
    /// <param name="targeScale">The target uniform scale (e.g., 1.1).</param>
    /// <param name="duration">The time in seconds for the animation.</param>
    public void ChangeScale(Transform transform, float targeScale, float duration = 0.25f)
    {
        RectTransform rectTransform = transform.GetComponent<RectTransform>();
        StartCoroutine(ChangeScaleCo(rectTransform, targeScale, duration));
    }

    /// <summary>
    /// Coroutine to lerp a RectTransform's scale.
    /// </summary>
    /// <param name="rectTransform">The UI element's RectTransform.</param>
    /// <param name="newScale">The target uniform scale.</param>
    /// <param name="duration">The time in seconds for the animation.</param>
    public IEnumerator ChangeScaleCo(RectTransform rectTransform, float newScale, float duration = .25f)
    {
        float time = 0;
        Vector3 initialScale = rectTransform.localScale;
        Vector3 targetScale = new Vector3(newScale, newScale, newScale);

        while (time < duration)
        {
            rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        rectTransform.localScale = targetScale;
    }

    /// <summary>
    /// Starts a coroutine to fade an Image's alpha.
    /// </summary>
    /// <param name="image">The UI Image to fade.</param>
    /// <param name="targetAlpha">The target alpha value (0-1).</param>
    /// <param name="duration">The time in seconds for the fade.</param>
    public void ChangeColour(Image image, float targetAlpha, float duration)
    {
        StartCoroutine(ChangeColourCo(image, targetAlpha, duration));
    }
    
    /// <summary>
    /// Coroutine to lerp an Image's color alpha.
    /// </summary>
    /// <param name="image">The UI Image to fade.</param>
    /// <param name="targetAlpha">The target alpha value (0-1).</param>
    /// <param name="duration">The time in seconds for the fade.</param>
    private IEnumerator ChangeColourCo(Image image, float targetAlpha, float duration)
    {
        float time = 0;
        Color currentColor = image.color;
        float startAlpha = currentColor.a;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            image.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);

            time += Time.deltaTime;
            yield return null;
        }
        image.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }
}