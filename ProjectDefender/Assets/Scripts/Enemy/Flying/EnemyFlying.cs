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

    public void AddObservingTower(TowerHarpoon newTower) => observingTowers.Add(newTower);

    public override void DestroyEnemy()
    {
        foreach (var tower in observingTowers)
        {
            tower.ResetAttack();
        }

        foreach (var harpoon in GetComponentsInChildren<ProjectileHarpoon>())
        {
            objectPool.Remove(harpoon.gameObject);
        }
        
        base.DestroyEnemy();
    }
}
