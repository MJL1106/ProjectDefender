using UnityEngine;

public class EnemyFlying : Enemy
{
    protected override void Start()
    {
        base.Start();

        agent.SetDestination(GetFinalWaypoint());
    }

    public override float DistanceToFinishLine()
    {
        return Vector3.Distance(transform.position, GetFinalWaypoint());
    }
}
