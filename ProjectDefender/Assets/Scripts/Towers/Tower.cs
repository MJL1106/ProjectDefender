using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// Base class for all towers. Handles targeting, rotation, attack cooldowns,
/// and deactivation (e.g., from EMPs).
/// </summary>
public class Tower : MonoBehaviour
{
    protected ObjectPoolManager objectPool;
    public Enemy currentEnemy;

    // Spider Boss EMP
    protected bool towerActive = true;
    protected Coroutine deactivatedTowerCo;
    protected GameObject currentEmpVfx;

    [Tooltip("Enabling this allows tower to change target between attacks")] 
    [SerializeField] private bool dynamicTargetChange;
    [SerializeField] protected float attackCooldown = 1f;
    protected float lastTimeAttacked;

    [Header("Tower Setup")] 
    [SerializeField] protected EnemyType enemyPriorityType = EnemyType.None; // Prioritizes this enemy type when multiple are in range
    [SerializeField] protected Transform towerHead; // Rotates on X and Y axis
    [SerializeField] protected Transform towerBody; // Rotates on Y axis only
    [SerializeField] protected Transform gunPoint; // Spawn point for projectiles
    [SerializeField] protected float rotationSpeed = 10f;

    [SerializeField] protected float attackRange = 2.5f;
    [SerializeField] protected LayerMask whatIsEnemy;
    [SerializeField] protected LayerMask whatIsTargetable; // Specific layers for targeting (e.g., for Fan tower)
    
    [Tooltip("Handles showing the correct preview for the fan tower")] 
    public bool towerAttacksForward;

    [Space] 
    private float targetCheckInterval = .1f; // How often (in seconds) to re-scan for a new target
    private float lastTimeCheckedTarget;
    protected Collider[] allocatedColliders = new Collider[100]; // Pre-allocated array to avoid GC

    [Header("Tower SFX Details")] 
    [SerializeField] protected AudioSource towerAttackSfx;
    [SerializeField] protected bool limitTowerSfx = false; // Use tower-instance limiting (max concurrent towers)
    [SerializeField] protected string towerSfxId = "";
    [SerializeField] protected float towerSfxCooldown = 0.3f;
    [SerializeField] protected int maxConcurrentTowerSfx = 3;

    [Header("Projectile SFX Details")]
    [SerializeField] protected AudioSource projectileSfx;
    [SerializeField] protected bool limitProjectileSfx = false; // Use standard (max concurrent sounds) limiting
    [SerializeField] protected string projectileSfxId = "";
    [SerializeField] protected float projectileSfxCooldown = 0.3f;
    [SerializeField] protected int maxConcurrentProjectileSfx = 3;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        objectPool = ObjectPoolManager.instance;
    }

    protected virtual void FixedUpdate()
    {
        if (towerActive == false) return;
        
        LooseTargetIfNeeded();
        UpdateTargetIfNeeded();
        HandleRotation();
        
        if (CanAttack()) AttemptToAttack();
    }

    /// <summary>
    /// Checks if the current target has moved out of range.
    /// If so, clears the current target.
    /// </summary>
    protected virtual void LooseTargetIfNeeded()
    {
        if (currentEnemy == null) return;
        
        if (Vector3.Distance(currentEnemy.CentrePoint(), transform.position) > attackRange) currentEnemy = null;
    }

    /// <summary>
    /// Temporarily deactivates the tower.
    /// Used by Spider Boss EMP.
    /// </summary>
    /// <param name="duration">The time in seconds the tower will be inactive.</param>
    /// <param name="empVxPrefab">The VFX prefab to instantiate at the tower's location.</param>
    public void DeactivateTower(float duration, GameObject empVxPrefab)
    {
        if (deactivatedTowerCo != null) StopCoroutine(deactivatedTowerCo);
        
        if (currentEmpVfx != null) objectPool.Remove(currentEmpVfx);

        currentEmpVfx = objectPool.Get(empVxPrefab, transform.position + new Vector3(0, .5f, 0), Quaternion.identity);
        deactivatedTowerCo = StartCoroutine(DeactivateTowerCo(duration));
    }

    private IEnumerator DeactivateTowerCo(float duration)
    {
        towerActive = false;

        yield return new WaitForSeconds(duration);

        towerActive = true;
        lastTimeAttacked = Time.time;
        objectPool.Remove(currentEmpVfx);
    }
    
    private void UpdateTargetIfNeeded()
    {
        if (dynamicTargetChange == false && currentEnemy != null) return;

        if (Time.time > lastTimeCheckedTarget + targetCheckInterval || currentEnemy == null)
        {
            lastTimeCheckedTarget = Time.time;
            currentEnemy = FindEnemyWithinRange();
        }
    }

    /// <summary>
    /// Checks if the attack cooldown has elapsed and a target exists.
    /// </summary>
    protected virtual bool CanAttack()
    {
        return Time.time > lastTimeAttacked + attackCooldown && currentEnemy != null;
    }
    
    /// <summary>
    /// Final checks before committing to an attack on the current target.
    /// Ensures the target is not null and is active.
    /// </summary>
    protected void AttemptToAttack()
    {
        if (currentEnemy == null) return;
        
        if (!currentEnemy.gameObject.activeSelf)
        {
            currentEnemy = null;
            return;
        }

        Attack();
    }

    /// <summary>
    /// The core attack logic. Overridden by child tower classes.
    /// Resets the attack timer.
    /// </summary>
    protected virtual void Attack()
    {
        lastTimeAttacked = Time.time;
    }
    
    /// <summary>
    /// Plays the tower's attack sound.
    /// Handles advanced limiting based on the number of *towers* playing, not just concurrent sounds.
    /// </summary>
    protected void PlayTowerAttackSound()
    {
        if (towerAttackSfx == null || towerAttackSfx.clip == null) return;

        Vector3 soundPosition = gunPoint != null ? gunPoint.position : transform.position;

        if (limitTowerSfx && !string.IsNullOrEmpty(towerSfxId))
        {
            // Check if this tower type can play (based on max concurrent TOWERS, not sounds)
            if (!AudioManager.instance.CanTowerPlaySound(towerSfxId, GetInstanceID(), maxConcurrentTowerSfx))
            {
                return; // Too many towers of this type already playing
            }
        
            // Register this tower as playing
            AudioManager.instance.RegisterTowerSound(towerSfxId, GetInstanceID(), towerAttackSfx.clip.length);
        
            // Play the sound normally (no limiting on the sound itself)
            AudioManager.instance?.PlaySFXOneShot(
                towerAttackSfx.clip, 
                soundPosition, 
                true, 
                towerAttackSfx.volume
            );
        }
        else
        {
            AudioManager.instance?.PlaySFXOneShot(
                towerAttackSfx.clip, 
                soundPosition, 
                true, 
                towerAttackSfx.volume
            );
        }
    }

    /// <summary>
    /// Main rotation handler, called every frame.
    /// Updates both head and body rotation.
    /// </summary>
    protected virtual void HandleRotation()
    {
        RotateTowardsEnemy();
        RotateBodyTowardsEnemy();
    }
    
    /// <summary>
    /// Rotates the 'towerHead' transform (vertical and horizontal) to face the enemy.
    /// </summary>
    protected virtual void RotateTowardsEnemy()
    {
        if (currentEnemy == null || towerHead == null) return;

        Vector3 directionToEnemy = DirectionToEnemyFrom(towerHead);

        Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);

        // Interp between current tower head rotation and desired rotation, convert quaternion to vector with euler angles
        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, lookRotation, rotationSpeed * Time.deltaTime).eulerAngles;

        towerHead.rotation = Quaternion.Euler(rotation);
    }
    
    /// <summary>
    /// Rotates the 'towerBody' transform (horizontal only) to face the enemy.
    /// </summary>
    protected void RotateBodyTowardsEnemy()
    {
        if (towerBody == null || currentEnemy == null) return;

        Vector3 directionToEnemy = DirectionToEnemyFrom(towerBody);
        directionToEnemy.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);
        towerBody.rotation = Quaternion.Slerp(towerBody.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Scans for enemies within range using OverlapSphereNonAlloc.
    /// Prioritizes based on 'enemyPriorityType' and then 'most advanced' (closest to finish).
    /// </summary>
    protected virtual Enemy FindEnemyWithinRange()
    {
        List<Enemy> priorityTargets = new List<Enemy>();
        List<Enemy> possibleTargets = new List<Enemy>();
        
        int enemiesAround =
            Physics.OverlapSphereNonAlloc(transform.position, attackRange, allocatedColliders, whatIsEnemy);

        for (int i = 0; i < enemiesAround; i++)
        {
            Enemy newEnemy = allocatedColliders[i].GetComponent<Enemy>();

            if (newEnemy == null) continue;

            float distanceToEnemy = Vector3.Distance(transform.position, newEnemy.transform.position);

            if (distanceToEnemy > attackRange) continue;
            
            EnemyType newEnemyType = newEnemy.GetEnemyType();

            if (newEnemyType == enemyPriorityType) 
                priorityTargets.Add(newEnemy); 
            else 
                possibleTargets.Add(newEnemy);
        }

        if (priorityTargets.Count > 0) return GetMostAdvancedEnemy(priorityTargets);

        if (possibleTargets.Count > 0) return GetMostAdvancedEnemy(possibleTargets);

        return null;
    }

    /// <summary>
    /// Finds the enemy with the lowest remaining distance to the castle.
    /// </summary>
    /// <param name="targets">The list of enemies to check.</param>
    /// <returns>The enemy closest to the finish line.</returns>
    private Enemy GetMostAdvancedEnemy(List<Enemy> targets)
    {
        Enemy mostAdvancedEnemy = null;
        float minRemainingDistance = float.MaxValue;
        
        foreach (Enemy enemy in targets)
        {
            float remainingDistance = enemy.DistanceToFinishLine();
            
            if (remainingDistance < minRemainingDistance)
            {
                minRemainingDistance = remainingDistance;
                mostAdvancedEnemy = enemy;
            }
        }
        return mostAdvancedEnemy;
    }
    
    /// <summary>
    /// Calculates the normalized direction vector from a point to the enemy's center.
    /// </summary>
    /// <param name="startPoint">The transform to calculate the direction from (e.g., tower head).</param>
    /// <returns>A normalized direction vector.</returns>
    protected Vector3 DirectionToEnemyFrom(Transform startPoint)
    {
        return (currentEnemy.CentrePoint() - startPoint.position).normalized;
    }
    
    /// <summary>
    /// Quick check to see if any enemy is within the attack range.
    /// </summary>
    protected bool AtLeastOneEnemyAround()
    {
        int enemyColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRange,allocatedColliders, whatIsEnemy);
        return enemyColliders > 0;
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    public float GetAttackRange() => attackRange;

    public float GetAttackRadius() => attackRange;

    public float GetAttackCooldown() => attackCooldown;
}