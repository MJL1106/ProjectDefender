using UnityEngine;

/// <summary>
/// A rapid-fire tower that fires fast projectiles from alternating barrels.
/// Sound is limited to play every few shots to reduce noise.
/// </summary>
public class TowerMachineGun : Tower
{
    private MachineGunVisuals machineGunVisuals;
    
    [Header("Machine gun Details")] 
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float damage;
    [SerializeField] private float projectileSpeed;
    [SerializeField] protected int playSoundEveryXShots = 3; // Only plays the attack sound every X shots to reduce noise
    
    [Space]
    [SerializeField] private Vector3 rotationOffset; // Aims slightly above the enemy's center point
    [SerializeField] private Transform[] gunPointSet; // The array of gun barrels to fire from
    private int gunPointIndex;
    private int shotCounter = 0; // Tracks shots for the audio limiting

    protected override void Awake()
    {
        base.Awake();
        machineGunVisuals = GetComponent<MachineGunVisuals>();
        shotCounter = playSoundEveryXShots;
    }

    /// <summary>
    /// Fires a projectile from the current gun barrel and cycles to the next.
    /// </summary>
    protected override void Attack()
    {
        gunPoint = gunPointSet[gunPointIndex];
        Vector3 directionToEnemy = DirectionToEnemyFrom(gunPoint);

        if (Physics.Raycast(gunPoint.position, directionToEnemy, out RaycastHit hitInfo, Mathf.Infinity,
                whatIsTargetable))
        {
            IDamageable damageable = hitInfo.transform.GetComponent<IDamageable>();

            if (damageable == null) return;

            GameObject newProjectile = objectPool.Get(projectilePrefab, gunPoint.position, gunPoint.rotation);
            newProjectile.GetComponent<ProjectileMachineGun>().SetupProjectile(hitInfo.point, damageable, damage, projectileSpeed, objectPool);
        
            machineGunVisuals.RecoilVfx(gunPoint);
            
            // Only play sound every X shots
            shotCounter++;
            if (shotCounter >= playSoundEveryXShots)
            {
                PlayTowerAttackSound();
                shotCounter = 0;
            }

            base.Attack();
            gunPointIndex = (gunPointIndex + 1) % gunPointSet.Length;
        }
    }

    /// <summary>
    /// Rotates the tower head to face the enemy, applying a vertical offset.
    /// </summary>
    protected override void RotateTowardsEnemy()
    {
        if (currentEnemy == null) return;

        Vector3 directionToEnemy = (currentEnemy.CentrePoint() - rotationOffset) - towerHead.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);

        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, lookRotation, rotationSpeed * Time.deltaTime)
            .eulerAngles;
        towerHead.rotation = Quaternion.Euler(rotation);
    }
}