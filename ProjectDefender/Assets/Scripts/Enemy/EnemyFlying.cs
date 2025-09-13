using UnityEngine;

public class EnemyFlying : Enemy
{
    protected override void Start()
    {
        base.Start();

        agent.SetDestination(GetFinalWaypoint());
    }
}
