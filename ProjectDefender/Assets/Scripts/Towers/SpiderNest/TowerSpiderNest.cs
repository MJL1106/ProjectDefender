using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A tower that spawns 'spider' projectiles.
/// Spiders rest on the tower and are launched when an enemy is in range.
/// </summary>
public class TowerSpiderNest : Tower
{
    [Header("Spider nest details")] 
    [SerializeField] private GameObject spiderPrefab;
    [SerializeField] private float damage;
    
    [Range(0,1)]
    [SerializeField] private float attackTimeMultiplier = .4f; // % of cooldown spent on the "attack" animation
    [SerializeField] private float reloadTimeMultiplier = .6f; // % of cooldown spent on the "reload" animation
    
    [Space]
    [SerializeField] private Transform[] attachPoint; // Where the spiders rest
    [SerializeField] private Transform[] webSet; // The web visuals
    [SerializeField] private Transform[] attachPointRef; // Reference transforms for attach point positions (for animation)
    
    private GameObject[] activeSpider;
    private int spiderIndex;
    private Vector3 spiderPointOffset = new Vector3(0, -.17f, 0);

    protected override void Start()
    {
        base.Start();
        InitializeSpiders();
        reloadTimeMultiplier = 1 - attackTimeMultiplier;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateAttachPointsPosition();
    }

    /// <summary>
    /// Checks cooldown and if any enemy is in range.
    /// </summary>
    protected override bool CanAttack()
    {
        return Time.time > lastTimeAttacked + attackCooldown && AtLeastOneEnemyAround();
    }

    /// <summary>
    /// Triggers the attack animation and releases a spider.
    /// </summary>
    protected override void Attack()
    {
        base.Attack();
        StartCoroutine(AttackCo());
    }

    /// <summary>
    /// Coroutine to animates the web, sets up the spider, and re-spawns a new one.
    /// </summary>
    private IEnumerator AttackCo()
    {
        Transform currentWeb = webSet[spiderIndex];
        Transform currentAttachPoint = attachPoint[spiderIndex];
        float attackTime = (attackCooldown / 4) * attackTimeMultiplier;
        float reloadTime = (attackCooldown / 4) * reloadTimeMultiplier;
    
        yield return ChangeScaleCo(currentWeb, 1, attackTime);
        
        // Setup and launch the currently attached spider
        activeSpider[spiderIndex].GetComponent<ProjectileSpiderNest>().SetupSpider(
            damage,
            projectileSfx != null ? projectileSfx.clip : null,
            projectileSfxId,
            projectileSfxCooldown,
            maxConcurrentProjectileSfx,
            limitProjectileSfx,
            projectileSfx != null ? projectileSfx.volume : 1f
        );

        yield return ChangeScaleCo(currentWeb, .1f, reloadTime);
    
        // Spawn a new spider to replace the one launched
        Vector3 spawnPos = GetSafeSpawnPosition(currentAttachPoint);
        GameObject newSpider = objectPool.Get(spiderPrefab, spawnPos, Quaternion.identity, null);
    
        // Then move and parent
        newSpider.transform.position = currentAttachPoint.position + spiderPointOffset;
        newSpider.transform.SetParent(currentAttachPoint);
    
        // Disable components while attached
        Collider spiderCollider = newSpider.GetComponent<Collider>();
        if (spiderCollider != null) spiderCollider.enabled = false;
    
        NavMeshAgent agent = newSpider.GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;
    
        activeSpider[spiderIndex] = newSpider;

        spiderIndex = (spiderIndex + 1) % attachPoint.Length;
    }

    /// <summary>
    /// Updates attach points to follow reference transforms (allows for animation).
    /// </summary>
    private void UpdateAttachPointsPosition()
    {
        for (int i = 0; i < attachPoint.Length; i++)
        {
            attachPoint[i].position = attachPointRef[i].position;
        }
    }

    /// <summary>
    /// Pre-spawns the spiders and attaches them to the tower at the start.
    /// </summary>
    private void InitializeSpiders()
    {
        activeSpider = new GameObject[attachPoint.Length];

        for (int i = 0; i < activeSpider.Length; i++)
        {
            Vector3 spawnPos = GetSafeSpawnPosition(attachPoint[i]);
            GameObject newSpider = objectPool.Get(spiderPrefab, spawnPos, Quaternion.identity, null); // Don't parent yet
        
            // Move to correct position after spawn, then parent
            newSpider.transform.position = attachPoint[i].position + spiderPointOffset;
            newSpider.transform.SetParent(attachPoint[i]);
        
            // Disable collider while attached
            Collider spiderCollider = newSpider.GetComponent<Collider>();
            if (spiderCollider != null) spiderCollider.enabled = false;
        
            // Disable NavMeshAgent while attached
            NavMeshAgent agent = newSpider.GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false;
        
            activeSpider[i] = newSpider;
        }
    }
    
    /// <summary>
    /// Gets a safe spawn point with an offset to prevent pooling conflicts.
    /// </summary>
    private Vector3 GetSafeSpawnPosition(Transform attachPoint)
    {
        // Get a position offset away from other nearby towers
        Vector3 basePosition = attachPoint.position + spiderPointOffset;
    
        // Add a small random offset to prevent exact overlaps
        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(-0.2f, 0.2f),
            0,
            UnityEngine.Random.Range(-0.2f, 0.2f)
        );
    
        return basePosition + randomOffset;
    }
    
    /// <summary>
    /// Coroutine to lerp the Y-scale of a transform.
    /// </summary>
    /// <param name="transform">The transform to scale.</param>
    /// <param name="newScale">The target local Y-scale.</param>
    /// <param name="duration">The time in seconds for the lerp.</param>
    private IEnumerator ChangeScaleCo(Transform transform, float newScale, float duration = .25f)
    {
        float time = 0;

        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(1,newScale,1);

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }
}