using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A utility tower that continuously reveals nearby stealthed enemies.
/// Uses a trigger collider (TowerFanRevealArea) to detect enemies.
/// </summary>
public class TowerFan : Tower
{
    [Header("Fan Details")] 
    [SerializeField] private float revealFrequency = .1f; // How often to apply the reveal effect
    [SerializeField] private float revealDuration = 1f; // How long the reveal effect lasts on an enemy
    
    private List<Enemy> enemiesToReveal = new List<Enemy>();
    private ForwardAttackDisplay display;

    /// <summary>
    /// Disables the editor-only attack display and starts the reveal behavior.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        
        // Disable the display lines once tower is built (only used in editor/preview)
        display = GetComponent<ForwardAttackDisplay>();
        if (display != null) display.CreateLines(false, 0);
        
        InvokeRepeating(nameof(RevealEnemies), .1f, revealFrequency);
    }

    public override void TowerPlaced()
    {
        base.TowerPlaced();
        PlayTowerAttackSound();
    }

    /// <summary>
    /// Iterates through the list of enemies in the trigger and applies the 'DisableHide' effect.
    /// Cleans the list of null or dead enemies.
    /// </summary>
    private void RevealEnemies()
    {
        enemiesToReveal.RemoveAll(enemy => enemy == null || enemy.IsDead() || !enemy.gameObject.activeInHierarchy);
        
        foreach (var enemy in enemiesToReveal)
        {
            if (enemy != null) enemy.DisableHide(revealDuration);
        }
    }

    /// <summary>
    /// Adds an enemy to the list for continuous revealing.
    /// </summary>
    public void AddEnemyToReveal(Enemy enemy) => enemiesToReveal.Add(enemy);
    
    /// <summary>
    /// Removes an enemy from the reveal list.
    /// </summary>
    public void RemoveEnemyToReveal(Enemy enemy) => enemiesToReveal.Remove(enemy);

    /// <summary>
    /// Editor-only. Updates the forward attack display when values are changed.
    /// </summary>
    private void OnValidate()
    {
        // Show lines in editor for design purposes
        ForwardAttackDisplay editorDisplay = GetComponent<ForwardAttackDisplay>();
        if (editorDisplay != null) editorDisplay.CreateLines(true, attackRange);
    }
}