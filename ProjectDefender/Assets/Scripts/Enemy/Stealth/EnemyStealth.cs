using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStealth : Enemy
{
    [Header("Stealth enemy details")]
    [SerializeField] private List<Enemy> enemiesToHide;
    [SerializeField] private float hideDuration = .5f;
    [SerializeField] private ParticleSystem smokeFx;
    private bool canHideEnemies = true;

    protected override void Awake()
    {
        base.Awake();
        InvokeRepeating(nameof(HideItself), .1f, hideDuration);
        InvokeRepeating(nameof(HideEnemies), .1f, hideDuration);
    }

    private void HideItself() => HideEnemy(hideDuration);

    private void HideEnemies()
    {
        if (canHideEnemies == false) return;
        
        foreach (Enemy enemy in enemiesToHide)
        {
            enemy.HideEnemy(hideDuration);
        }
    }

    public List<Enemy> GetEnemiesToHide() => enemiesToHide;

    public void EnableSmoke(bool enable)
    {
        if (enable && !smokeFx.isPlaying)
            smokeFx.Play();
        else if (!enable && smokeFx.isPlaying)
            smokeFx.Stop();
    }

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
}
