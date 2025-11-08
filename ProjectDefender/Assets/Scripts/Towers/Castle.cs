using System;
using UnityEngine;

/// <summary>
/// The player's base. Detects enemies reaching it,
/// applies damage to the GameManager, and removes the enemy.
/// </summary>
public class Castle : MonoBehaviour
{
    private GameManager gameManager;
   
    private void Start()
    {
        gameManager = GameManager.instance; 
    }
   
    /// <summary>
    /// Called when an object enters the castle's trigger.
    /// Checks if it's an enemy and applies damage.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy == null) return;

            int damageToDeal = enemy.GetCastleDamage();
         
            enemy.RemoveEnemy();

            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
         
            if (gameManager != null) gameManager.UpdateHp(-damageToDeal);
        }
    }
}