using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Boss enemy that continuously spawns ground units while flying.
/// Eliminates all spawned units when boss dies.
/// </summary>
public class EnemyFlyingBoss : EnemyFlying
{
    [Header("Boss Details")]
    [SerializeField] private GameObject bossUnitPrefab;
    [SerializeField] private int amountToCreate = 150;
    private int unitsCreated;
    [SerializeField] private float cooldown = .05f; // Time between unit spawns
    private float creationTimer;

    private List<Enemy> createdEnemies = new List<Enemy>();

    protected override void OnEnable()
    {
        base.OnEnable();
        unitsCreated = 0;
    }

    protected override void Update()
    {
        base.Update();

        creationTimer -= Time.deltaTime;

        if (creationTimer < 0 && unitsCreated < amountToCreate)
        {
            creationTimer = cooldown;
            CreateNewBossUnit();
        }
    }

    private void CreateNewBossUnit()
    {
        unitsCreated++;
        GameObject newUnit = objectPool.Get(bossUnitPrefab, transform.position, Quaternion.identity);

        EnemyBossUnit bossUnit = newUnit.GetComponent<EnemyBossUnit>();
        bossUnit.SetupEnemy(GetFinalWaypoint(), this, myPortal);
        
        createdEnemies.Add(bossUnit);
    }

    /// <summary>
    /// Kills all spawned units when boss is eliminated.
    /// </summary>
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