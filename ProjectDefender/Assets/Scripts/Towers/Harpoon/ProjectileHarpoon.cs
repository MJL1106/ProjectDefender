using System;
using UnityEngine;

public class ProjectileHarpoon : MonoBehaviour
{
    private TowerHarpoon myTower;
    private bool isAttached;
    private float speed;
    private Enemy enemy;
    
    [SerializeField] private Transform connectionPoint;

    private void Update()
    {
        if (enemy == null || isAttached) return;
        
        MoveTowardsEnemy();
        
        if (Vector3.Distance(transform.position, enemy.transform.position) < .25f) AttachToEnemy();
    }

    private void MoveTowardsEnemy()
    {
        transform.position = Vector3.MoveTowards(transform.position, enemy.transform.position, speed * Time.deltaTime);
        transform.forward = enemy.transform.position - transform.position;
    }

    private void AttachToEnemy()
    {
        if (isAttached) return;

        if (enemy == null) return;
    
        if (myTower == null) return;

        isAttached = true; 
        transform.parent = enemy.transform;
        myTower.ActivateAttack();
    }

    public void SetupProjectile(Enemy newEnemy, float newSpeed, TowerHarpoon newTower)
    {
        ResetProjectile();
        
        speed = newSpeed;
        enemy = newEnemy;
        myTower = newTower;
    }
    
    public void ResetProjectile()
    {
        isAttached = false;
        enemy = null;
        myTower = null;
    }

    public Transform GetConnectionPoint()
    {
        if (connectionPoint == null) return transform;

        return connectionPoint;
    }
    
    private void OnDisable()
    {
        isAttached = false;
        enemy = null;
        myTower = null;
    }

}
