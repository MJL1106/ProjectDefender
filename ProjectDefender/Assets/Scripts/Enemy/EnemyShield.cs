using System;
using System.Collections;
using UnityEngine;

public class EnemyShield : MonoBehaviour, IDamageable
{
    [SerializeField] private float currentShieldAmount;

    [Header("Impact Details")] [SerializeField]
    private Material shieldMaterial;

    [SerializeField] private float defaultShieldGlow = 1;
    [SerializeField] private float impactShieldGlow = 3;
    [SerializeField] private float impactScaleMultiplier = .97f;
    [SerializeField] private float impactSpeed;
    [SerializeField] private float impactResetDuration  = .1f;

    private float defaultScale;
    private string shieldFresnelParameter = "_FresnelPower";
    private Coroutine currentCo;

    private void Start()
    {
        defaultScale = transform.localScale.x;
    }

    public void SetupShield(int shieldAmount)
    {
        currentShieldAmount = shieldAmount;
    }
    
    public void TakeDamage(int damage)
    {
        currentShieldAmount -= damage;
        ActivateShieldImpact();
        
        if (currentShieldAmount <= 0) gameObject.SetActive(false);
    }

    private void ActivateShieldImpact()
    {
        if (currentCo != null) StopCoroutine(currentCo);

        StartCoroutine(ImpactCo());
    }

    private IEnumerator ImpactCo()
    {
        yield return StartCoroutine(ShieldChangeCo(impactShieldGlow, defaultScale * impactScaleMultiplier,impactSpeed));

        StartCoroutine(ShieldChangeCo(defaultShieldGlow,defaultScale, impactResetDuration));
    }

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
