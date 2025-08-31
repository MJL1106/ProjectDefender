using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Tower : MonoBehaviour
{
    public Enemy currentEnemy;

    [SerializeField] protected float attackCooldown = 1f;
    protected float lastTimeAttacked;
    
    [Header("Tower Setup")] 
    [SerializeField] protected Transform towerHead;
    [SerializeField] protected float rotationSpeed = 10f;
    private bool canRotate;

    [SerializeField] protected float attackRange = 2.5f;
    [SerializeField] protected LayerMask whatIsEnemy;

    protected virtual void Awake()
    {
        EnableRotation(true);
    }

    protected virtual void Update()
    {
        if (currentEnemy == null)
        {
            currentEnemy = FindRandomEnemyWithinRange();
            return;
        }

        if (CanAttack()) Attack();

        if (Vector3.Distance(currentEnemy.CentrePoint(), transform.position) > attackRange) currentEnemy = null;
        
        RotateTowardsEnemy();
    }

    protected virtual void Attack()
    {
        
    }

    protected bool CanAttack()
    {
        if (currentEnemy == null) return false;
        
        if (Time.time > lastTimeAttacked + attackCooldown)
        {
            lastTimeAttacked = Time.time;
            return true;
        }

        return false;
    }

    public void EnableRotation(bool enable)
    {
        canRotate = enable;
    }

    protected virtual void RotateTowardsEnemy()
    {
        if (!canRotate) return;
        
        if (currentEnemy == null) return;

        Vector3 directionToEnemy = DirectionToEnemyFrom(towerHead);

        Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);

        // Interp between current tower head rotation and desired rotation, convert quaternion to vector with euler angles
        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, lookRotation, rotationSpeed * Time.deltaTime).eulerAngles;

        towerHead.rotation = Quaternion.Euler(rotation);
    }

    protected Enemy FindRandomEnemyWithinRange()
    {
        List<Enemy> possibleTargets = new List<Enemy>();
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, attackRange, whatIsEnemy);

        foreach (Collider enemy in enemiesAround)
        {
            Enemy newEnemy = enemy.GetComponent<Enemy>();
            possibleTargets.Add((newEnemy));
        }

        Enemy newTarget = GetMostAdvancedEnemy(possibleTargets);

        if (newTarget != null) return newTarget;

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
}
