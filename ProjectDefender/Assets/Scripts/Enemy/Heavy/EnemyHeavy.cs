using UnityEngine;

/// <summary>
/// Armored enemy with shield that absorbs damage.
/// Shield must be depleted before enemy takes health damage.
/// </summary>
public class EnemyHeavy : Enemy
{
    [Header("Enemy Details")] 
    private float currentShield = 50;
    [SerializeField] private float maxShield = 50;
    [SerializeField] private EnemyShield shieldObject;

    protected override void OnEnable()
    {
        base.OnEnable();

        currentShield = maxShield;
        EnableShieldIfNeeded();
    }

    private void EnableShieldIfNeeded()
    {
        if (shieldObject != null && currentShield > 0) shieldObject.gameObject.SetActive(true);
    }

    /// <summary>
    /// Applies damage to shield first, then to health when shield depletes.
    /// </summary>
    public override void TakeDamage(float damage)
    {
        if (currentShield > 0)
        {
            currentShield -= damage;
            shieldObject.ActivateShieldImpact();

            if (currentShield <= 0) shieldObject.gameObject.SetActive(false);
        }
        else
        {
            base.TakeDamage(damage);
        }
    }
}