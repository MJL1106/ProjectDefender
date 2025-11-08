using System;
using UnityEngine;

/// <summary>
/// Detects enemies entering stealth radius and adds them to hide list.
/// Attached to child sphere collider of stealth enemy.
/// </summary>
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

    /// <summary>
    /// Manages stealth hide list based on proximity.
    /// Ignores other stealth enemies to prevent recursive hiding.
    /// </summary>
    /// <param name="enemyCollider">The collider of the enemy entering/exiting the trigger.</param>
    /// <param name="addEnemy">True to add the enemy, false to remove it from the list.</param>
    private void AddEnemyToHideList(Collider enemyCollider, bool addEnemy)
    {
        Enemy newEnemy = enemyCollider.GetComponent<Enemy>();
        
        if (newEnemy == null) return;
        if (newEnemy.GetEnemyType() == EnemyType.Stealth) return;

        if (addEnemy) enemy.GetEnemiesToHide().Add(newEnemy);
        else enemy.GetEnemiesToHide().Remove(newEnemy);
    }
}