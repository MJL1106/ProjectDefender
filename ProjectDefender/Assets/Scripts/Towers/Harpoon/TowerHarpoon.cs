using UnityEngine;

public class TowerHarpoon : Tower
{
    [Header("Harpoon Details")] [SerializeField]
    private GameObject projectilePrefab;

    [SerializeField] private Transform projectileDefaultPosition;
    [SerializeField] private float projectileSpeed = 15;
    private ProjectileHarpoon currentProjectile;

    protected override void Awake()
    {
        base.Awake();
        CreateNewProjectile();
    }

    protected override void Attack()
    {
        base.Attack();

        if (Physics.Raycast(gunPoint.position, gunPoint.forward, out RaycastHit hitInfo, Mathf.Infinity,
                whatIsTargetable))
        {
            currentProjectile.SetupProjectile(currentEnemy, projectileSpeed);
        }
    }

    private void CreateNewProjectile()
    {
        GameObject newProjectile = Instantiate(projectilePrefab, projectileDefaultPosition.position,
            projectileDefaultPosition.rotation, towerHead);

        currentProjectile = newProjectile.GetComponent<ProjectileHarpoon>();
    }
}
