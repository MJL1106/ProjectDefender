using System;
using UnityEngine;

public class EnemyBossUnit : Enemy
{
    private Vector3 savedDestination;
    private Vector3 lastKnownBossPosition;
    private EnemyFlyingBoss myBoss;

    protected override void Update()
    {
        base.Update();

        if (myBoss != null)
            lastKnownBossPosition = myBoss.transform.position;
    }


    public void SetupEnemy(Vector3 destination, EnemyFlyingBoss myNewBoss, EnemyPortal myNewPortal)
    {
        ResetEnemy();
        ResetMovement();

        myBoss = myNewBoss;
        
        myPortal = myNewPortal;
        myPortal.GetActiveEnemies().Add(gameObject);
        
        savedDestination = destination;
        
        InvokeRepeating(nameof(SnapToBossIfNeeded), .1f,.5f);
    }

    private void ResetMovement()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        agent.enabled = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.tag == "Enemy") return;

        if (Vector3.Distance(transform.position, lastKnownBossPosition) > 2.5f)
        {
            transform.position = lastKnownBossPosition + new Vector3(0, -1, 0);
        }
        
        rb.useGravity = false;
        rb.isKinematic = true;

        agent.enabled = true;
        agent.SetDestination(savedDestination);
    }

    private void SnapToBossIfNeeded()
    {
        if (agent.enabled && !agent.isOnNavMesh)
        {
            if (Vector3.Distance(transform.position, lastKnownBossPosition) > 3f)
            {
                transform.position = lastKnownBossPosition + new Vector3(0, -1, 0);
                ResetMovement();
            }
        }
    }
    
    public override float DistanceToFinishLine()
    {
        return Vector3.Distance(transform.position, GetFinalWaypoint());
    }
}
