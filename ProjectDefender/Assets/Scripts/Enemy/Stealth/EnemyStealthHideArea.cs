using System;
using UnityEngine;

public class EnemyStealthHideArea : MonoBehaviour
{
    private EnemyStealth enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyStealth>();
    }

    private void OnTriggerEnter(Collider other)
    {
        AddEnemyToHideList(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        AddEnemyToHideList(other, false);
    }

    private void AddEnemyToHideList(Collider enemyCollider, bool addEnemy)
    {
        Enemy newEnemy = enemyCollider.GetComponent<Enemy>();
        
        if (newEnemy == null) return;

        if (newEnemy.GetEnemyType() == EnemyType.Stealth) return;

        if (addEnemy)
            enemy.GetEnemiesToHide().Add(newEnemy);
        else
            enemy.GetEnemiesToHide().Remove(newEnemy);
    }
}
