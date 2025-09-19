using System;
using UnityEngine;

public class ProjectileHarpoon : MonoBehaviour
{
    private TowerHarpoon myTower;
    private bool isAttached;
    private float speed;
    private Enemy enemy;

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
        isAttached = true;
        transform.parent = enemy.transform;
        myTower.ActivateAttack();
    }

    public void SetupProjectile(Enemy newEnemy, float newSpeed, TowerHarpoon newTower)
    {
        speed = newSpeed;
        enemy = newEnemy;
        myTower = newTower;
    }
}
