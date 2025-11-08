using System;
using UnityEngine;

/// <summary>
/// The harpoon projectile fired by TowerHarpoon.
/// Moves towards the enemy, attaches, and then notifies the tower.
/// </summary>
public class ProjectileHarpoon : MonoBehaviour
{
    private TowerHarpoon myTower;
    private bool isAttached;
    private float speed;
    private Enemy enemy;
    
    [SerializeField] private Transform connectionPoint; // The visual endpoint for the chain

    private void Update()
    {
        if (enemy == null || isAttached) return;
        
        MoveTowardsEnemy();
        
        if (Vector3.Distance(transform.position, enemy.transform.position) < .25f) AttachToEnemy();
    }

    /// <summary>
    /// Moves the projectile towards the target enemy.
    /// </summary>
    private void MoveTowardsEnemy()
    {
        transform.position = Vector3.MoveTowards(transform.position, enemy.transform.position, speed * Time.deltaTime);
        transform.forward = enemy.transform.position - transform.position;
    }

    /// <summary>
    /// Attaches the projectile to the enemy and notifies the tower to activate its attack.
    /// </summary>
    private void AttachToEnemy()
    {
        if (isAttached) return;
        if (enemy == null) return;
        if (myTower == null) return;

        isAttached = true; 
        transform.parent = enemy.transform;
        myTower.ActivateAttack();
    }

    /// <summary>
    /// Initializes the projectile with a target and speed.
    /// Called by the tower when firing.
    /// </summary>
    /// <param name="newEnemy">The target enemy.</param>
    /// <param name="newSpeed">The projectile's travel speed.</param>
    /// <param name="newTower">The tower that fired this projectile.</param>
    public void SetupProjectile(Enemy newEnemy, float newSpeed, TowerHarpoon newTower)
    {
        ResetProjectile();
        
        speed = newSpeed;
        enemy = newEnemy;
        myTower = newTower;
    }
    
    /// <summary>
    /// Resets the projectile's state to be ready for pooling.
    /// </summary>
    public void ResetProjectile()
    {
        isAttached = false;
        enemy = null;
        myTower = null;
    }

    /// <summary>
    /// Returns the visual connection point for the chain.
    /// </summary>
    public Transform GetConnectionPoint()
    {
        if (connectionPoint == null) return transform;

        return connectionPoint;
    }
    
    private void OnDisable()
    {
        isAttached = false;
        enemy = null;
        myTower = null;
    }

}