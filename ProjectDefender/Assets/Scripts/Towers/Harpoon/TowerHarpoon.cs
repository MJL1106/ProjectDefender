using System.Collections;
using UnityEngine;

public class TowerHarpoon : Tower
{
    private HarpoonVisuals harpoonVisuals;
    
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

    private bool reachedTarget;
    private bool busyWithAttack;
    private bool isProcessingHit;
    private Coroutine damageOverTimeCo;
    
    protected override void Awake()
    {
        base.Awake();
        currentProjectile = GetComponentInChildren<ProjectileHarpoon>();
        harpoonVisuals = GetComponent<HarpoonVisuals>();
    }

    protected override void Attack()
    {
        base.Attack();

        if (Physics.Raycast(gunPoint.position, gunPoint.forward, out RaycastHit hitInfo, Mathf.Infinity,
                whatIsTargetable))
        {
            currentEnemy = hitInfo.collider.GetComponent<Enemy>();
            busyWithAttack = true;
            reachedTarget = false; 
            isProcessingHit = false; 
            
            currentProjectile.SetupProjectile(currentEnemy, projectileSpeed, this);
            harpoonVisuals.EnableChainVisuals(true, currentProjectile.GetConnectionPoint());
            
            PlayTowerAttackSound();
            
        
            Invoke(nameof(ResetAttackIfMissed), 1f);
        }
    }

    public void ActivateAttack()
    {
        isProcessingHit = true;
        reachedTarget = true;
    
        CancelInvoke(nameof(ResetAttackIfMissed));
    
        if (currentEnemy == null || currentEnemy.IsDead())
        {
            ResetAttack();
            return;
        }
    
        var enemyFlying = currentEnemy.GetComponent<EnemyFlying>();
        if (enemyFlying != null)
        {
            enemyFlying.AddObservingTower(this);
        }
        
        currentEnemy.SlowEnemy(slowEffect, overTimeEffectDuration);
        
        if (harpoonVisuals != null)
        {
            harpoonVisuals.CreateElectrifyVFX(currentEnemy.transform);
        }

        IDamageable damageable = currentEnemy.GetComponent<IDamageable>();
        damageable?.TakeDamage(initialDamage);

        if (damageOverTimeCo != null)
        {
            StopCoroutine(damageOverTimeCo);
        }
        
        damageOverTimeCo = StartCoroutine(DamageOverTimeCo(damageable));
        isProcessingHit = false;
    }

    private IEnumerator DamageOverTimeCo(IDamageable damageable)
    {
        float time = 0;
        float damageFrequency = overTimeEffectDuration / damageOverTime;
        float damagePerTick = damageOverTime / (overTimeEffectDuration / damageFrequency);
    
        while (time < overTimeEffectDuration)
        {
            if (damageable == null || (currentEnemy != null && currentEnemy.IsDead()))
            {
                break;
            }
            
            damageable?.TakeDamage(damagePerTick);
            yield return new WaitForSeconds(damageFrequency);
            time += damageFrequency;
        }
    
        ResetAttack();
    }
    
    public void ResetAttack()
    {
        if (!busyWithAttack && !reachedTarget && damageOverTimeCo == null)
        {
            return;
        }
    
        CancelInvoke(nameof(ResetAttackIfMissed));
    
        if (damageOverTimeCo != null) 
        {
            StopCoroutine(damageOverTimeCo);
            damageOverTimeCo = null;
        }
    
        if (currentProjectile != null)
        {
            currentProjectile.ResetProjectile();
        }
    
        if (currentEnemy != null)
        {
            var enemyFlying = currentEnemy.GetComponent<EnemyFlying>();
            if (enemyFlying != null)
            {
                enemyFlying.RemoveObservingTower(this);
            }
        }
        
        if (towerAttackSfx != null) AudioManager.instance?.FadeOutSFX(towerAttackSfx, 0.2f);
        
        busyWithAttack = false;
        reachedTarget = false;
        isProcessingHit = false;
        currentEnemy = null;
        lastTimeAttacked = Time.time;
    
        harpoonVisuals.EnableChainVisuals(false);
        
        CreateNewProjectile();
    }

    protected override void LooseTargetIfNeeded()
    {
        if (busyWithAttack == false) 
        {
            base.LooseTargetIfNeeded();
        }
    }

    private void CreateNewProjectile()
    {
        if (currentProjectile != null && currentProjectile.gameObject.activeSelf)
        {
            objectPool.Remove(currentProjectile.gameObject);
        }
        
        GameObject newProjectile = objectPool.Get(projectilePrefab, projectileDefaultPosition.position,
            projectileDefaultPosition.rotation, towerHead);

        currentProjectile = newProjectile.GetComponent<ProjectileHarpoon>();
    }

    private void ResetAttackIfMissed()
    {
        if (reachedTarget || isProcessingHit)
        {
            return;
        }
    
        if (currentProjectile != null)
        {
            currentProjectile.ResetProjectile();
            objectPool.Remove(currentProjectile.gameObject);
            currentProjectile = null; 
        }
    
        ResetAttack();
    }
    
    protected override bool CanAttack()
    {
        if (busyWithAttack) return false;
        return base.CanAttack();
    }
}