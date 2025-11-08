using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An AOE tower that slams the ground, slowing all nearby enemies.
/// Does not require a target, but attacks if at least one enemy is in range.
/// </summary>
public class TowerHammer : Tower
{
    private HammerVisuals hammerVisuals;

    [Header("Hammer Details")] 
    [Range(0,1)]
    [SerializeField] private float slowMultiplier = .4f; // e.g., 0.4 means 60% slow
    [SerializeField] private float slowDuration;
    
    
    protected override void Awake()
    {
        base.Awake();
        hammerVisuals = GetComponent<HammerVisuals>();
    }

    /// <summary>
    /// Overrides FixedUpdate to attack if able, without needing a specific target.
    /// </summary>
    protected override void FixedUpdate()
    {
        if (towerActive == false) return;
        
        if (CanAttack()) Attack();
        
    }

    /// <summary>
    /// Triggers the hammer slam animation and applies a slow effect to all enemies in range.
    /// </summary>
    protected override void Attack()
    {
        base.Attack();

        if (hammerVisuals == null) return;
        
        hammerVisuals.HammerAttackAnimation();
        hammerVisuals.PlayAttackAnimation();
        PlayTowerAttackSound();

        foreach (var enemy in ValidEnemyTargets())
        {
            enemy.SlowEnemy(slowMultiplier, slowDuration);
        }
    }

    /// <summary>
    /// Gets a list of all valid enemies within the tower's attack range.
    /// </summary>
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

    /// <summary>
    /// Can attack as long as the cooldown is met and at least one enemy is nearby.
    /// </summary>
    protected override bool CanAttack()
    {
        return Time.time > lastTimeAttacked + attackCooldown && AtLeastOneEnemyAround();
    }
}