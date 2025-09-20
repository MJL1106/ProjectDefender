using UnityEngine;

public class EnemyHeavy : Enemy
{
    [Header("Enemy Details")] 
    [SerializeField] private float shieldAmount = 50;
    [SerializeField] private EnemyShield shieldObject;

    protected override void Start()
    {
        base.Start();
        EnableShieldIfNeeded();
    }
    
    private void EnableShieldIfNeeded()
    {
        if (shieldObject != null && shieldAmount > 0)
        {
            shieldObject.gameObject.SetActive(true);
        }
    }

    public override void TakeDamage(float damage)
    {
        if (shieldAmount > 0)
        {
            shieldAmount -= damage;
            shieldObject.ActivateShieldImpact();

            if (shieldAmount <= 0) shieldObject.gameObject.SetActive(false);
        }
        else
        {
            base.TakeDamage(damage);
        }
        
    }
}
