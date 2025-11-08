using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerFan : Tower
{
    [Header("Fan Details")] 
    [SerializeField] private float revealFrequency = .1f;
    [SerializeField] private float revealDuration = 1f;
    
    private List<Enemy> enemiesToReveal = new List<Enemy>();
    private ForwardAttackDisplay display;

    protected override void Awake()
    {
        base.Awake();
        
        // Disable the display lines once tower is built (only used in editor/preview)
        display = GetComponent<ForwardAttackDisplay>();
        if (display != null) display.CreateLines(false, 0);
        
        InvokeRepeating(nameof(RevealEnemies), .1f, revealFrequency);
        PlayTowerAttackSound();
    }

    private void RevealEnemies()
    {
        enemiesToReveal.RemoveAll(enemy => enemy == null || enemy.IsDead() || !enemy.gameObject.activeInHierarchy);
        
        foreach (var enemy in enemiesToReveal)
        {
            if (enemy != null) enemy.DisableHide(revealDuration);
        }
    }

    public void AddEnemyToReveal(Enemy enemy) => enemiesToReveal.Add(enemy);
    public void RemoveEnemyToReveal(Enemy enemy) => enemiesToReveal.Remove(enemy);

    private void OnValidate()
    {
        // Show lines in editor for design purposes
        ForwardAttackDisplay editorDisplay = GetComponent<ForwardAttackDisplay>();
        if (editorDisplay != null) editorDisplay.CreateLines(true, attackRange);
    }
}