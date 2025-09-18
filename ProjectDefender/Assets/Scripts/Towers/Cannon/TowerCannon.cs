using Unity.Mathematics;
using UnityEngine;

public class TowerCannon : Tower
{
    [Header("Cannon Details")] 
    [SerializeField] private float timeToTarget = 1.5f;
    [SerializeField] private float damage;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private ParticleSystem attackVFX;


    protected override void Attack()
    {
        base.Attack();

        Vector3 velocity = CalculateLaunchVelocity();
        attackVFX.Play();

        GameObject newProjectile = Instantiate(projectilePrefab, gunPoint.position, Quaternion.identity);
        newProjectile.GetComponent<ProjectileCannon>().SetupProjectile(velocity, damage);
    }
    
    
    // Finds the enemy with the most enemies around it, makes tower cannon useful for destroying large slow groups of enemies
    protected override Enemy FindEnemyWithinRange()
    {
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, attackRange, whatIsEnemy);
        Enemy bestTarget = null;
        int maxNearbyEnemies = 0;

        foreach (Collider enemy in enemiesAround)
        {
            int amountOfEnemiesAround = EnemiesAroundEnemy(enemy.transform);

            if (amountOfEnemiesAround > maxNearbyEnemies)
            {
                maxNearbyEnemies = amountOfEnemiesAround;
                bestTarget = enemy.GetComponent<Enemy>();
            }
        }

        return bestTarget;
    }

    private int EnemiesAroundEnemy(Transform enemyToCheck)
    {
        Collider[] enemiesAround = Physics.OverlapSphere(enemyToCheck.position, .8f, whatIsEnemy);

        return enemiesAround.Length;
    }

    protected override void HandleRotation()
    {
        if (currentEnemy == null) return;
        
        RotateBodyTowardsEnemy();
        FaceLaunchDirection();
    }

    private void FaceLaunchDirection()
    {
        Vector3 attackDirection = CalculateLaunchVelocity();
        Quaternion lookRotation = Quaternion.LookRotation(attackDirection);

        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, lookRotation, rotationSpeed * Time.deltaTime).eulerAngles;

        towerHead.rotation = Quaternion.Euler(rotation.x, towerHead.eulerAngles.y, 0);
    }

    private Vector3 CalculateLaunchVelocity()
    {
        Vector3 direction = currentEnemy.CentrePoint() - gunPoint.position;
        Vector3 directionXZ = new Vector3(direction.x, 0, direction.z);
        Vector3 velocityXZ = directionXZ / timeToTarget;

        // Calculates initial vertical velocity to make the projectile reach the target's height.
        // Makes sure the projectile reaches the target considering gravity over time.
        float yVelocity = (direction.y - (Physics.gravity.y * Mathf.Pow(timeToTarget, 2)) / 2) / timeToTarget;
        Vector3 launchVelocity = velocityXZ + (Vector3.up * yVelocity);

        return launchVelocity;
    }
}
