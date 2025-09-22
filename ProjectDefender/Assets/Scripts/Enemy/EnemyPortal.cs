using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyPortal : MonoBehaviour
{
    private ObjectPoolManager objectPool;
    
    [SerializeField] private WaveManager myWaveManager;
    [SerializeField] private float spawnCooldown;
    private float spawnTimer;
    private bool canCreateEnemies = true;

    [Space] 
    [SerializeField] private ParticleSystem flyPortalVfx;

    private Coroutine flyPortalVfxCo;
    
    [Space]
    
    [SerializeField] private List<Waypoint> waypointList;
    public Vector3[] currentWaypoints { get; private set; }

    private List<GameObject> enemiesToCreate = new List<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Awake()
    {
        CollectWaypoints();
    }

    private void Update()
    {
        if (CanMakeNewEnemy()) CreateEnemy();
    }

    private void Start()
    {
        objectPool = ObjectPoolManager.instance;
    }

    public void AssignWaveManager(WaveManager newWaveManager) => myWaveManager = newWaveManager;
    private bool CanMakeNewEnemy()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0 && enemiesToCreate.Count > 0)
        {
            spawnTimer = spawnCooldown;
            return true;
        }

        return false;
    }
    private void CreateEnemy()
    {
        if (!canCreateEnemies) return;
        
        GameObject randomEnemy = GetRandomEnemy();
        if (randomEnemy == null) return;
        
        GameObject newEnemy = objectPool.Get(randomEnemy, transform.position, Quaternion.identity);

        Enemy enemyScript = newEnemy.GetComponent<Enemy>();
        enemyScript.SetupEnemy(this);
        
        PlaceEnemyAtFlyPortalIfNeeded(newEnemy,enemyScript.GetEnemyType());
        activeEnemies.Add(newEnemy);
    }

    private void PlaceEnemyAtFlyPortalIfNeeded(GameObject newEnemy, EnemyType enemyType)
    {
        if (enemyType != EnemyType.Flying) return;

        if (flyPortalVfxCo != null) StopCoroutine(flyPortalVfxCo);

        flyPortalVfxCo = StartCoroutine(EnableFlyPortalVfxCo());
        newEnemy.transform.position = flyPortalVfx.transform.position;
    }

    private IEnumerator EnableFlyPortalVfxCo()
    {
        flyPortalVfx.Play();

        yield return new WaitForSeconds(2);
        
        flyPortalVfx.Stop();
    }

    private GameObject GetRandomEnemy()
    {
        if (enemiesToCreate.Count == 0) return null;
        
        int randomIndex = Random.Range(0, enemiesToCreate.Count);
        GameObject chosenEnemy = enemiesToCreate[randomIndex];
        
        enemiesToCreate.RemoveAt(randomIndex);
        
        return chosenEnemy;
    }

    public void AddEnemy(GameObject enemyToAdd) => enemiesToCreate.Add(enemyToAdd);
    public List<GameObject> GetActiveEnemies() => activeEnemies;
    public void CanCreateNewEnemies(bool canCreate) => canCreateEnemies = canCreate;

    public void RemoveActiveEnemy(GameObject enemyToRemove)
    {
        if (activeEnemies.Contains(enemyToRemove))
        {
            activeEnemies.Remove(enemyToRemove);
        }
        
        myWaveManager.CheckIfWaveCompleted();
    }

    [ContextMenu("Add waypoints")]
    private void CollectWaypoints()
    {
        waypointList = new List<Waypoint>();

        foreach (Transform child in transform)
        {
            Waypoint waypoint = child.GetComponent<Waypoint>();
            
            if (waypoint != null) waypointList.Add(waypoint);
        }

        currentWaypoints = new Vector3[waypointList.Count];

        for (int i = 0; i < currentWaypoints.Length; i++)
        {
            currentWaypoints[i] = waypointList[i].transform.position;
        }
    }
}
