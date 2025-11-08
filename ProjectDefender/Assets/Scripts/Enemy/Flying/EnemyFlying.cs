using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Flying enemy that moves directly to castle without waypoints.
/// Can be tracked by harpoon towers for special targeting behavior.
/// </summary>
public class EnemyFlying : Enemy
{
    private List<TowerHarpoon> observingTowers = new List<TowerHarpoon>();
    
    protected override void Start()
    {
        base.Start();
        agent.SetDestination(GetFinalWaypoint());
    }

    public override float DistanceToFinishLine()
    {
        return Vector3.Distance(transform.position, GetFinalWaypoint());
    }

    /// <summary>
    /// Registers a harpoon tower that is tracking this flying enemy.
    /// </summary>
    public void AddObservingTower(TowerHarpoon newTower) 
    {
        if (!observingTowers.Contains(newTower)) observingTowers.Add(newTower);
    }
    
    public void RemoveObservingTower(TowerHarpoon tower)
    {
        observingTowers.Remove(tower);
    }

    /// <summary>
    /// Notifies all tracking harpoon towers when enemy dies.
    /// Removes attached harpoon projectiles before cleanup.
    /// </summary>
    public override void RemoveEnemy()
    {
        // Create copy to avoid modification during iteration
        List<TowerHarpoon> towersToNotify = new List<TowerHarpoon>(observingTowers);
    
        foreach (var tower in towersToNotify)
        {
            if (tower != null) tower.ResetAttack();
        }
    
        observingTowers.Clear();

        foreach (var harpoon in GetComponentsInChildren<ProjectileHarpoon>())
        {
            objectPool.Remove(harpoon.gameObject);
        }
    
        base.RemoveEnemy();
    }
}