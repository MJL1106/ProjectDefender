using UnityEngine;

public class TowerMachineGun : Tower
{
    private MachineGunVisuals machineGunVisuals;
    
    [Header("Machine gun Details")] 
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float damage;
    [SerializeField] private float projectileSpeed;
    [SerializeField] protected int playSoundEveryXShots = 3;
    
    [Space]
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private Transform[] gunPointSet;
    private int gunPointIndex;
    private int shotCounter = 0;

    protected override void Awake()
    {
        base.Awake();
        machineGunVisuals = GetComponent<MachineGunVisuals>();
        shotCounter = playSoundEveryXShots;
    }

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