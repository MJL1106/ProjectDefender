using System;
using UnityEngine;

/// <summary>
/// A helper component attached to the fan's trigger collider.
/// Detects enemies entering and exiting the reveal area.
/// </summary>
public class TowerFanRevealArea : MonoBehaviour
{
    private TowerFan tower;

    private void Awake()
    {
        tower = GetComponentInParent<TowerFan>();
    }

    /// <summary>
    /// Adds enemies to the parent tower's reveal list when they enter.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        
        if (enemy != null) tower.AddEnemyToReveal(enemy);
    }

    /// <summary>
    /// Removes enemies from the parent tower's reveal list when they exit.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        
        if (enemy != null) tower.RemoveEnemyToReveal(enemy);
    }
}