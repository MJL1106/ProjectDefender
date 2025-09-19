using System.Collections;
using UnityEngine;

public class TowerHarpoon : Tower
{
    [Header("Harpoon Details")] 
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private Transform projectileDefaultPosition;
    [SerializeField] private float projectileSpeed = 15;
    private ProjectileHarpoon currentProjectile;

    [Header("Damage Details")] 
    [SerializeField] private float initialDamage = 5;
    [SerializeField] private float damageOverTime = 10;
    [SerializeField] private float overTimeEffectDuration = 4;
    
    [Range(0f, 1f)] 
    [SerializeField] private float slowEffect = .7f;

    private bool busyWithAttack;
    private Coroutine damageOverTimeCo;

    protected override void Awake()
    {
        base.Awake();
        CreateNewProjectile();
    }

    protected override void Attack()
    {
        base.Attack();

        if (Physics.Raycast(gunPoint.position, gunPoint.forward, out RaycastHit hitInfo, Mathf.Infinity,
                whatIsTargetable))
        {
            busyWithAttack = true;
            currentProjectile.SetupProjectile(currentEnemy, projectileSpeed,this);
        }
    }

    public void ActivateAttack()
    {
        currentEnemy.SlowEnemy(slowEffect, overTimeEffectDuration);

        IDamageable damageable = currentEnemy.GetComponent<IDamageable>();
        damageable?.TakeDamage(initialDamage);

        damageOverTimeCo = StartCoroutine(DamageOverTimeCo(damageable));
    }

    private IEnumerator DamageOverTimeCo(IDamageable damageable)
    {
        float time = 0;
        float damageFrequency = overTimeEffectDuration / damageOverTime;
        float damagePerTick = damageOverTime / (overTimeEffectDuration / damageFrequency);
        
        while (time < overTimeEffectDuration)
        {
            damageable?.TakeDamage(damagePerTick);
            yield return new WaitForSeconds(damageFrequency);
            time += damageFrequency;
        }
        ResetAttack();
    }
    
    public void ResetAttack()
    {
        if (damageOverTimeCo != null) StopCoroutine(damageOverTimeCo);
        
        currentEnemy = null;
        lastTimeAttacked = Time.time;
        busyWithAttack = false;
        CreateNewProjectile();
    }

    protected override void LooseTargetIfNeeded()
    {
        if (busyWithAttack == false) base.LooseTargetIfNeeded();
    }

    private void CreateNewProjectile()
    {
        GameObject newProjectile = Instantiate(projectilePrefab, projectileDefaultPosition.position,
            projectileDefaultPosition.rotation, towerHead);

        currentProjectile = newProjectile.GetComponent<ProjectileHarpoon>();
    }

    protected override bool CanAttack()
    {
        return base.CanAttack() && busyWithAttack == false;
    }
}
