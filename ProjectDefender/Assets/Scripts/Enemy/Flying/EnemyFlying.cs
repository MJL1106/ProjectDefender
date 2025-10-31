using System.Collections.Generic;
using UnityEngine;

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

    public void AddObservingTower(TowerHarpoon newTower) 
    {
        if (!observingTowers.Contains(newTower))
        {
            observingTowers.Add(newTower);
        }
    }
    
    public void RemoveObservingTower(TowerHarpoon tower)
    {
        observingTowers.Remove(tower);
    }

    public override void RemoveEnemy()
    {
        // Create a copy to avoid modification during iteration
        List<TowerHarpoon> towersToNotify = new List<TowerHarpoon>(observingTowers);
    
        foreach (var tower in towersToNotify)
        {
            if (tower != null)
            {
                // The tower will handle removing itself from the observing list
                tower.ResetAttack();
            }
        }
    
        observingTowers.Clear();

        foreach (var harpoon in GetComponentsInChildren<ProjectileHarpoon>())
        {
            objectPool.Remove(harpoon.gameObject);
        }
    
        base.RemoveEnemy();
    }
}
