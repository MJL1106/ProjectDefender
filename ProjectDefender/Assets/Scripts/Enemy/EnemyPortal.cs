using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Spawns enemies for a wave and provides them with a waypoint path.
/// Manages the spawn queue, tracks active enemies, and links to the WaveManager.
/// </summary>
public class EnemyPortal : MonoBehaviour
{
    private ObjectPoolManager objectPool;
    
    [SerializeField] private WaveManager myWaveManager;
    [SerializeField] private float spawnCooldown; // Time between individual enemy spawns
    private float spawnTimer;
    private bool canCreateEnemies = true;

    [Space] 
    [SerializeField] private ParticleSystem flyPortalVfx; // Spawn point and VFX for flying enemies

    private Coroutine flyPortalVfxCo;
    
    [Space]
    
    [SerializeField] private List<Waypoint> waypointList;
    public Vector3[] currentWaypoints { get; private set; } // The path passed to enemies

    private List<GameObject> enemiesToCreate = new List<GameObject>(); // Queue of enemy prefabs to spawn this wave
    private List<GameObject> activeEnemies = new List<GameObject>(); // List of enemies currently spawned and alive

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

    /// <summary>
    /// Assigns the WaveManager, called during wave setup.
    /// </summary>
    public void AssignWaveManager(WaveManager newWaveManager) => myWaveManager = newWaveManager;
    
    /// <summary>
    /// Checks if spawn timer has elapsed and enemies are in queue.
    /// </summary>
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
    
    /// <summary>
    /// Spawns a single enemy from the pool and assigns its path.
    /// </summary>
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

    /// <summary>
    /// Repositions flying enemies to the dedicated fly portal and plays VFX.
    /// </summary>
    private void PlaceEnemyAtFlyPortalIfNeeded(GameObject newEnemy, EnemyType enemyType)
    {
        if (enemyType != EnemyType.Flying) return;

        if (flyPortalVfxCo != null) StopCoroutine(flyPortalVfxCo);

        flyPortalVfxCo = StartCoroutine(EnableFlyPortalVfxCo());
        newEnemy.transform.position = flyPortalVfx.transform.position;
    }

    /// <summary>
    /// Plays the flying portal VFX for a short duration.
    /// </summary>
    private IEnumerator EnableFlyPortalVfxCo()
    {
        flyPortalVfx.Play();

        yield return new WaitForSeconds(2);
        
        flyPortalVfx.Stop();
    }

    /// <summary>
    /// Selects, removes, and returns a random enemy prefab from the spawn queue.
    /// </summary>
    private GameObject GetRandomEnemy()
    {
        if (enemiesToCreate.Count == 0) return null;
        
        int randomIndex = Random.Range(0, enemiesToCreate.Count);
        GameObject chosenEnemy = enemiesToCreate[randomIndex];
        
        enemiesToCreate.RemoveAt(randomIndex);
        
        return chosenEnemy;
    }

    /// <summary>
    /// Adds an enemy prefab to the spawn queue for this wave.
    /// </summary>
    public void AddEnemy(GameObject enemyToAdd) => enemiesToCreate.Add(enemyToAdd);
    
    public List<GameObject> GetActiveEnemies() => activeEnemies;
    public bool HasEnemiesToSpawn()=> enemiesToCreate.Count > 0;
    
    /// <summary>
    /// Toggles the portal's ability to spawn new enemies.
    /// </summary>
    public void CanCreateNewEnemies(bool canCreate) => canCreateEnemies = canCreate;


    /// <summary>
    /// Removes an enemy from the active list (on death/despawn).
    /// Triggers WaveManager to check for wave completion.
    /// </summary>
    public void RemoveActiveEnemy(GameObject enemyToRemove)
    {
        if (activeEnemies.Contains(enemyToRemove)) activeEnemies.Remove(enemyToRemove);
        
        myWaveManager.CheckIfWaveCompleted();
    }

    /// <summary>
    /// Populates the waypoint list from child 'Waypoint' objects.
    /// Called from Awake and via [ContextMenu].
    /// </summary>
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