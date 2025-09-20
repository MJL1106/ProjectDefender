using System.Collections.Generic;
using UnityEngine;

public class EnemyFlyingBoss : EnemyFlying
{
    [Header("Boss Details")] [SerializeField]
    private GameObject bossUnitPrefab;

    [SerializeField] private int amountToCreate = 150;
    [SerializeField] private float cooldown = .05f;
    private float creationTimer;

    private List<Enemy> createdEnemies = new List<Enemy>();

    protected override void Update()
    {
        base.Update();

        creationTimer -= Time.deltaTime;

        if (creationTimer < 0 && amountToCreate > 0)
        {
            creationTimer = cooldown;
            CreateNewBossUnit();
        }
    }

    protected override void Start()
    {
        
    }

    private void CreateNewBossUnit()
    {
        amountToCreate--;
        GameObject newUnit = Instantiate(bossUnitPrefab, transform.position, Quaternion.identity);

        EnemyBossUnit bossUnit = newUnit.GetComponent<EnemyBossUnit>();
        
        bossUnit.SetupEnemy(GetFinalWaypoint(), this, myPortal);
        
        createdEnemies.Add(bossUnit);
    }

    private void EliminateAllUnits()
    {
        foreach (Enemy enemy in createdEnemies)
        {
            enemy.Die();
        }
    }

    public override void Die()
    {
        EliminateAllUnits();
        base.Die();
    }
}
