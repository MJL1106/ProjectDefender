using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Visual effect handler for enemy shield.
/// Creates pulsing/shrinking animation when shield takes damage.
/// </summary>
public class EnemyShield : MonoBehaviour
{
    [Header("Impact Details")] 
    [SerializeField] private Material shieldMaterial;

    [SerializeField] private float defaultShieldGlow = 1;
    [SerializeField] private float impactShieldGlow = 3;
    [SerializeField] private float impactScaleMultiplier = .97f;
    [SerializeField] private float impactSpeed;
    [SerializeField] private float impactResetDuration = .1f;

    private float defaultScale;
    private string shieldFresnelParameter = "_FresnelPower";
    private Coroutine currentCo;

    private void Start()
    {
        defaultScale = transform.localScale.x;
        shieldMaterial = Instantiate(shieldMaterial);
    }
    
    /// <summary>
    /// Triggers shield impact animation (glow and shrink).
    /// Called when shield takes damage.
    /// </summary>
    public void ActivateShieldImpact()
    {
        if (currentCo != null) StopCoroutine(currentCo);
        StartCoroutine(ImpactCo());
    }

    private IEnumerator ImpactCo()
    {
        yield return StartCoroutine(ShieldChangeCo(impactShieldGlow, defaultScale * impactScaleMultiplier, impactSpeed));
        StartCoroutine(ShieldChangeCo(defaultShieldGlow, defaultScale, impactResetDuration));
    }

    /// <summary>
    /// Interpolates shield material glow and scale over time.
    /// </summary>
    private IEnumerator ShieldChangeCo(float targetGlow, float targetScale, float duration)
    {
        float time = 0;
        float startGlow = shieldMaterial.GetFloat(shieldFresnelParameter);
        Vector3 initialScale = transform.localScale;
        Vector3 newTargetScale = new Vector3(targetScale, targetScale, targetScale);

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, newTargetScale, time / duration);
            
            float newGlow = Mathf.Lerp(startGlow, targetGlow, time / duration);
            shieldMaterial.SetFloat(shieldFresnelParameter, newGlow);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = newTargetScale;
        shieldMaterial.SetFloat(shieldFresnelParameter, targetGlow);
    }
}