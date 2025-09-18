using System.Collections.Generic;
using UnityEngine;

public class TowerHammer : Tower
{
    private HammerVisuals hammerVisuals;

    [Header("Hammer Details")] 
    [Range(0,1)]
    [SerializeField] private float slowMultiplier = .4f;
    [SerializeField] private float slowDuration;
    
    
    protected override void Awake()
    {
        base.Awake();
        hammerVisuals = GetComponent<HammerVisuals>();
    }

    protected override void Update()
    {
        if (towerActive == false) return;
        
        if (CanAttack()) Attack();
        ;
    }

    protected override void Attack()
    {
        base.Attack();

        if (hammerVisuals == null) return;
        
        hammerVisuals.HammerAttackAnimation();
        hammerVisuals.PlayAttackAnimation();

        foreach (var enemy in ValidEnemyTargets())
        {
            enemy.SlowEnemy(slowMultiplier, slowDuration);
        }
    }

    private List<Enemy> ValidEnemyTargets()
    {
        List<Enemy> targets = new List<Enemy>();
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, attackRange, whatIsTargetable);

        foreach (Collider enemy in enemiesAround)
        {
            Enemy newEnemy = enemy.GetComponent<Enemy>();
            
            if (newEnemy != null) targets.Add(newEnemy);
        }

        return targets;
    }

    protected override bool CanAttack()
    {
        return Time.time > lastTimeAttacked + attackCooldown && AtLeastOneEnemyAround();
    }


}
