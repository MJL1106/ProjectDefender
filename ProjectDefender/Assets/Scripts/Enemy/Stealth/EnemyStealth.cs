using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stealth enemy that hides itself and nearby enemies.
/// Creates smoke VFX while hiding ability is active.
/// Can be revealed by detection towers to disable hiding.
/// </summary>
public class EnemyStealth : Enemy
{
    [Header("Stealth enemy details")]
    [SerializeField] private List<Enemy> enemiesToHide;
    [SerializeField] private float hideDuration = .5f;
    [SerializeField] private ParticleSystem smokeFx;
    private bool canHideEnemies = true;

    private void HideItself() => HideEnemy(hideDuration);

    /// <summary>
    /// Applies stealth effect to all nearby enemies in hide area.
    /// Removes dead enemies from list before processing.
    /// </summary>
    private void HideEnemies()
    {
        if (canHideEnemies == false) return;
        
        enemiesToHide.RemoveAll(enemy => enemy == null || enemy.IsDead());
        
        foreach (Enemy enemy in enemiesToHide)
        {
            enemy.HideEnemy(hideDuration);
        }
    }

    public List<Enemy> GetEnemiesToHide() => enemiesToHide;

    public void EnableSmoke(bool enable)
    {
        if (enable)
        {
            smokeFx.Clear();
            smokeFx.Play();
        }
        else
        {
            smokeFx.Stop();
            smokeFx.Clear();
        }
    }

    /// <summary>
    /// Disables hiding ability and smoke effect when revealed.
    /// Overrides base to add smoke control and hide prevention.
    /// </summary>
    protected override IEnumerator DisableHideCo(float duration)
    {
        EnableSmoke(false);
        canBeHidden = false;
        canHideEnemies = false;
        
        yield return new WaitForSeconds(duration);

        EnableSmoke(true);
        canBeHidden = true;
        canHideEnemies = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    
        canBeHidden = true;
        canHideEnemies = true;
        EnableSmoke(true);
    
        enemiesToHide.Clear();
    
        InvokeRepeating(nameof(HideItself), .1f, hideDuration);
        InvokeRepeating(nameof(HideEnemies), .1f, hideDuration);
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
    
        EnableSmoke(false);
        enemiesToHide.Clear();
    }
}