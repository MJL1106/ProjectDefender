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
        myBoss = myNewBoss;
        
        myPortal = myNewPortal;
        myPortal.GetActiveEnemies().Add(gameObject);
        
        savedDestination = destination;
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
}
