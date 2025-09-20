using UnityEngine;

public class TowerMachineGun : Tower
{
    private MachineGunVisuals machineGunVisuals;
    
    [Header("Machine gun Details")] [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField] private float damage;
    [SerializeField] private float projectileSpeed;
    
    [Space]
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private Transform[] gunPointSet;
    private int gunPointIndex;

    protected override void Awake()
    {
        base.Awake();
        machineGunVisuals = GetComponent<MachineGunVisuals>();
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

            GameObject newProjectile = Instantiate(projectilePrefab, gunPoint.position, gunPoint.rotation);
            newProjectile.GetComponent<ProjectileMachineGun>().SetupProjectile(hitInfo.point, damageable, damage, projectileSpeed);
            
            machineGunVisuals.RecoilVfx(gunPoint);

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
