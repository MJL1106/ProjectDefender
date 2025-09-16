using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Tower : MonoBehaviour
{
    public Enemy currentEnemy;

    protected bool towerActive = true;
    protected Coroutine deactivatedTowerCo;
    protected GameObject currentEmpVfx;

    [Tooltip("Enabling this allows tower to change target between attacks")] 
    [SerializeField] private bool dynamicTargetChange;
    [SerializeField] protected float attackCooldown = 1f;
    protected float lastTimeAttacked;

    [Header("Tower Setup")] 
    [SerializeField] protected EnemyType enemyPriorityType = EnemyType.None;
    [SerializeField] protected Transform towerHead;
    [SerializeField] protected Transform gunPoint;
    [SerializeField] protected float rotationSpeed = 10f;

    [SerializeField] protected float attackRange = 2.5f;
    [SerializeField] protected LayerMask whatIsEnemy;
    [SerializeField] protected LayerMask whatIsTargetable;

    [Space] 
    private float targetCheckInterval = .1f;
    private float lastTimeCheckedTarget;

    [Header("SFX Details")] [SerializeField]
    protected AudioSource attackSfx;

    protected virtual void Awake()
    {
        
    }

    protected virtual void Update()
    {
        if (towerActive == false) return;
        
        LooseTargetIfNeeded();
        UpdateTargetIfNeeded();
        HandleRotation();
        
        if (CanAttack()) Attack();
    }

    private void LooseTargetIfNeeded()
    {
        if (currentEnemy == null) return;
        
        if (Vector3.Distance(currentEnemy.CentrePoint(), transform.position) > attackRange) currentEnemy = null;
    }

    public void DeactivateTower(float duration, GameObject empVxPrefab)
    {
        if (deactivatedTowerCo != null) StopCoroutine(deactivatedTowerCo);
        
        if (currentEmpVfx != null) Destroy(currentEmpVfx);

        currentEmpVfx = Instantiate(empVxPrefab, transform.position + new Vector3(0, .5f, 0), Quaternion.identity);
        deactivatedTowerCo = StartCoroutine(DeactivateTowerCo(duration));
    }

    private IEnumerator DeactivateTowerCo(float duration)
    {
        towerActive = false;

        yield return new WaitForSeconds(duration);

        towerActive = true;
        lastTimeAttacked = Time.time;
        Destroy(currentEmpVfx);
    }
    
    private void UpdateTargetIfNeeded()
    {
        if (currentEnemy == null)
        {
            currentEnemy = FindEnemyWithinRange();
            return;
        }
        
        if (dynamicTargetChange == false) return;

        if (Time.time > lastTimeCheckedTarget + targetCheckInterval)
        {
            lastTimeCheckedTarget = Time.time;
            currentEnemy = FindEnemyWithinRange();
        } 
    }

    protected virtual void Attack()
    {
        lastTimeAttacked = Time.time;
    }

    protected bool CanAttack()
    {
        return Time.time > lastTimeAttacked + attackCooldown && currentEnemy != null;
    }

    protected virtual void HandleRotation()
    {
        RotateTowardsEnemy();
    }
    
    protected virtual void RotateTowardsEnemy()
    {
        if (currentEnemy == null || towerHead == null) return;

        Vector3 directionToEnemy = DirectionToEnemyFrom(towerHead);

        Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);

        // Interp between current tower head rotation and desired rotation, convert quaternion to vector with euler angles
        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, lookRotation, rotationSpeed * Time.deltaTime).eulerAngles;

        towerHead.rotation = Quaternion.Euler(rotation);
    }

    protected virtual Enemy FindEnemyWithinRange()
    {
        List<Enemy> priorityTargets = new List<Enemy>();
        List<Enemy> possibleTargets = new List<Enemy>();
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, attackRange, whatIsEnemy);

        foreach (Collider enemy in enemiesAround)
        {
            Enemy newEnemy = enemy.GetComponent<Enemy>();

            if (newEnemy == null) continue;
            
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
    
    protected Vector3 DirectionToEnemyFrom(Transform startPoint)
    {
        return (currentEnemy.CentrePoint() - startPoint.position).normalized;
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    public float GetAttackRange() => attackRange;

    public float GetAttackRadius() => attackRange;
}
