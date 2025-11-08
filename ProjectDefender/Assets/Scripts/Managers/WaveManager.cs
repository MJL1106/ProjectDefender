using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

/// <summary>
/// A data class holding the composition of a single enemy wave.
/// Also stores references to new grids or portals for layout changes.
/// </summary>
[System.Serializable]
public class WaveDetails
{
    public GridBuilder nextGrid; // The grid layout to transition to *after* this wave
    public EnemyPortal[] newPortals; // New portals to activate *after* this wave
    public int basicEnemy;
    public int fastEnemy;
    public int swarmEnemy;
    public int heavyEnemy;
    public int stealthEnemy;
    public int flyingEnemy;
    public int flyingBossEnemy;
    public int spiderBossEnemy;
}

/// <summary>
/// Manages the progression of enemy waves within a level.
/// Handles wave timers, enemy spawning, and triggering level layout changes.
/// </summary>
public class WaveManager : MonoBehaviour
{
    private GameManager gameManager;
    private TileAnimator tileAnimator;
    private UIGame inGameUI;
    [SerializeField] private GridBuilder currentGrid;
    [SerializeField] private NavMeshSurface flyingNavSurface;
    [SerializeField] private NavMeshSurface flyingBossNavSurface;
    [SerializeField] private NavMeshSurface droneNavSurface;
    private MeshCollider[] flyingNavColliders;
    private MeshCollider[] flyingBossNavColliders;

    [Header("Wave Details")]
    [SerializeField] private float timeBetweenWaves = 10; // Time in seconds for the countdown timer
    [SerializeField] private float waveTimer;
    [SerializeField] private WaveDetails[] levelWaves;
    [SerializeField] private int waveIndex;

    [Header("Level Update Details")] 
    [SerializeField] private float yOffset = 5; // Vertical distance tiles travel when changing layout
    [SerializeField] private float tileDelay = .1f;
    
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicEnemy;
    [SerializeField] private GameObject fastEnemy;
    [SerializeField] private GameObject swarmEnemy;
    [SerializeField] private GameObject heavyEnemy;
    [SerializeField] private GameObject stealthEnemy;
    [SerializeField] private GameObject flyingEnemy;
    [SerializeField] private GameObject flyingBossEnemy;
    [SerializeField] private GameObject spiderBossEnemy;
    
    private List<EnemyPortal> enemyPortals;
    private bool waveTimerEnabled;
    private bool makingNextWave;
    public bool gameBegan;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        enemyPortals = new List<EnemyPortal>(FindObjectsByType<EnemyPortal>(FindObjectsSortMode.None));
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        inGameUI = FindFirstObjectByType<UIGame>(FindObjectsInactive.Include);

        // Cache colliders for specialized NavMesh baking
        MeshCollider[] allColliders = GetComponentsInChildren<MeshCollider>();
    
        List<MeshCollider> flyingList = new List<MeshCollider>();
        List<MeshCollider> bossList = new List<MeshCollider>();
    
        foreach (var collider in allColliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("FlyEnemyBoss_Road"))
            {
                bossList.Add(collider);
            }
            else if (collider.gameObject.layer == LayerMask.NameToLayer("FlyEnemy_Road"))
            {
                flyingList.Add(collider);
            }
        }
    
        flyingNavColliders = flyingList.ToArray();
        flyingBossNavColliders = bossList.ToArray();
    }

    private void Update()
    {
        if (gameBegan == false) return;
        
        HandleWaveTimer();
    }

    /// <summary>
    /// Starts the wave progression for the level.
    /// Called by GameManager when the level is prepared.
    /// </summary>
    [ContextMenu("Activate wave manager")]
    public void ActivateWaveManager()
    {
        gameBegan = true;
        inGameUI = gameManager.inGameUI;
        EnableWaveTimer(true);
    }

    /// <summary>
    /// Stops all wave timers and logic.
    /// Called by GameManager on game over or level exit.
    /// </summary>
    public void DeactivateWaveManager()
    {
        gameBegan = false;
        waveTimerEnabled = false;
    
        // Hide the wave timer UI when deactivating
        if (inGameUI != null)
        {
            inGameUI.EnableWaveTimer(false);
        }
    }

    /// <summary>
    /// Checks if all enemies are defeated and portals are empty.
    /// If so, advances the wave index and triggers the next wave or level completion.
    /// </summary>
    public void CheckIfWaveCompleted()
    {
        if (gameBegan == false || gameManager.IsGameLost()) return;
        
        if (AllEnemiesDefeated() == false || AllPortalsFinishedSpawning() == false || makingNextWave) return;

        makingNextWave = true;
        waveIndex++;

        if (HasNoMoreWaves())
        {
            gameManager.LevelCompleted();
            return;
        }

        if (HasNewLayout())
        {   
            AttemptToUpdateLayout();
        }
        else
        {
            EnableWaveTimer(true);
        }
        
    }
    
    /// <summary>
    /// Starts the next wave.
    /// Updates NavMeshes, gives enemies to portals, and stops the countdown timer.
    /// </summary>
    public void StartNewWave()
    {
        if (gameManager.IsGameLost()) return;
        
        UpdateNavMeshes();
        GiveEnemiesToPortals();
        EnableWaveTimer(false);
        makingNextWave = false;
    }
    
    /// <summary>
    /// Manages the countdown timer between waves.
    /// </summary>
    private void HandleWaveTimer()
    {
        if (waveTimerEnabled == false) return;
        
        if (gameManager.IsGameLost()) 
        {
            waveTimerEnabled = false;
            inGameUI.EnableWaveTimer(false);
            return;
        }
        
        waveTimer -= Time.deltaTime;
        inGameUI.UpdateWaveTimerUI(waveTimer);

        if (waveTimer <= 0) StartNewWave();
        
    }
    
    /// <summary>
    /// Distributes the enemy prefabs for the current wave to the active portals.
    /// </summary>
    private void GiveEnemiesToPortals()
    {
        List<GameObject> newEnemies = GetNewEnemies();
        int portalIndex = 0;

        if (newEnemies == null) return;

        for (int i = 0; i < newEnemies.Count; i++)
        {
            GameObject enemyToAdd = newEnemies[i];
            EnemyPortal portalToReceiveEnemy = enemyPortals[portalIndex];

            portalToReceiveEnemy.AddEnemy(enemyToAdd);

            portalIndex++;

            if (portalIndex >= enemyPortals.Count) portalIndex = 0;
        }
    }
    
    /// <summary>
    /// Checks if all enemy portals have finished spawning their queued enemies.
    /// </summary>
    private bool AllPortalsFinishedSpawning()
    {
        foreach (EnemyPortal portal in enemyPortals)
        {
            if (portal.HasEnemiesToSpawn()) return false;
        }
        return true;
    }

    /// <summary>
    /// Triggers the level layout update based on the next wave's data.
    /// </summary>
    private void AttemptToUpdateLayout() => UpdateLevelLayout(levelWaves[waveIndex]);

    /// <summary>
    /// Compares the current grid to the next wave's grid and identifies tiles to be added/removed.
    /// </summary>
    private void UpdateLevelLayout(WaveDetails nextWave)
    {
        GridBuilder nextGrid = nextWave.nextGrid;
        List<GameObject> grid = currentGrid.GetTileSetup();
        List<GameObject> newGrid = nextGrid.GetTileSetup();

        if (grid.Count != newGrid.Count)
        {
            Debug.LogWarning("Current grid and new grid have different size.");
            return;
        }

        List<TileSlot> tilesToRemove = new List<TileSlot>();
        List<TileSlot> tilesToAdd = new List<TileSlot>();

        for (int i = 0; i < grid.Count; i++) // Current grid and next grid have to be the same size, they are duplicated.
        {
            TileSlot currentTile = grid[i].GetComponent<TileSlot>();
            TileSlot newTile = newGrid[i].GetComponent<TileSlot>();

            bool shouldBeUpdated = currentTile.GetMesh() != newTile.GetMesh() ||
                                   currentTile.GetOriginalMaterial() != newTile.GetOriginalMaterial() ||
                                   currentTile.GetAllChildren().Count != newTile.GetAllChildren().Count ||
                                   currentTile.transform.rotation != newTile.transform.rotation;

            if (shouldBeUpdated)
            {
                tilesToRemove.Add(currentTile);
                tilesToAdd.Add(newTile);

                grid[i] = newTile.gameObject;
            }
        }

        StartCoroutine(RebuildLevelCo(tilesToRemove, tilesToAdd, nextWave, tileDelay));
    }

    /// <summary>
    /// Coroutine to animate the removal of old tiles and addition of new tiles.
    /// </summary>
    private IEnumerator RebuildLevelCo(List<TileSlot> tilesToRemove, List<TileSlot> tilesToAdd, WaveDetails waveDetails, float delay)
    {
        for (int i = 0; i < tilesToRemove.Count; i++)
        {
            yield return new WaitForSeconds(delay);
            RemoveTile(tilesToRemove[i]);
        }

        for (int i = 0; i < tilesToAdd.Count; i++)
        {
            yield return new WaitForSeconds(delay);
            AddTile(tilesToAdd[i]);
        }
        
        EnableNewPortals(waveDetails.newPortals);
        EnableWaveTimer(true);
    }
    
    /// <summary>
    /// Animates a new tile moving up and dissolving in.
    /// </summary>
    private void AddTile(TileSlot newTile)
    {
        newTile.gameObject.SetActive(true);
        newTile.transform.position += new Vector3(0, -yOffset, 0);
        newTile.transform.parent = currentGrid.transform;

        Vector3 targetPosition = newTile.transform.position + new Vector3(0, yOffset, 0);
        
        tileAnimator.DissolveTile(true, newTile.transform);
        tileAnimator.MoveTile(newTile.transform, targetPosition, true);
    }

    /// <summary>
    /// Animates an old tile moving down and dissolving out.
    /// </summary>
    private void RemoveTile(TileSlot tileToRemove)
    {
        Vector3 targetPosition = tileToRemove.transform.position + new Vector3(0, -yOffset, 0);
        
        tileAnimator.DissolveTile(false, tileToRemove.transform);
        tileAnimator.MoveTile(tileToRemove.transform, targetPosition, false);
        
        Destroy(tileToRemove.gameObject, 3);
    }

    /// <summary>
    /// Enables or disables the between-wave countdown timer.
    /// </summary>
    private void EnableWaveTimer(bool enable)
    {
        if (enable && gameManager.IsGameLost()) return;
        
        if (waveTimerEnabled == enable) return;

        waveTimer = timeBetweenWaves;
        waveTimerEnabled = enable;
        inGameUI.EnableWaveTimer(enable);
    }
    
    /// <summary>
    /// Activates and registers new enemy portals defined in the wave data.
    /// </summary>
    private void EnableNewPortals(EnemyPortal[] newPortals)
    {
        foreach (EnemyPortal portal in newPortals)
        {
            portal.CanCreateNewEnemies(true);
            portal.AssignWaveManager(this);
            portal.gameObject.SetActive(true);
            enemyPortals.Add(portal);
        }
    }

    /// <summary>
    /// Rebuilds all NavMesh surfaces in the level.
    /// </summary>
    private void UpdateNavMeshes()
    {
        UpdateNavMeshForFlyingEnemies();

        currentGrid.UpdateNavMesh();
        droneNavSurface.BuildNavMesh();
    }

    /// <summary>
    /// Rebuilds the NavMesh for flying enemies by temporarily enabling their path colliders.
    /// </summary>
    private void UpdateNavMeshForFlyingEnemies()
    {
        foreach (var myCollider in flyingNavColliders)
        {
            myCollider.enabled = true;
        }
        
        flyingNavSurface.BuildNavMesh();
        
        foreach (var myCollider in flyingNavColliders)
        {
            myCollider.enabled = false;
        }

        if (flyingBossNavColliders == null || flyingBossNavSurface == null) return;
        
        foreach (var myCollider in flyingBossNavColliders)
        {
            myCollider.enabled = true;
        }
        
        flyingBossNavSurface.BuildNavMesh();
        
        foreach (var myCollider in flyingBossNavColliders)
        {
            myCollider.enabled = false;
        }
    }
    
    /// <summary>
    /// Generates a list of enemy prefabs based on the current wave's data.
    /// </summary>
    private List<GameObject> GetNewEnemies()
    {
        if (waveIndex >= levelWaves.Length)
        {
            Debug.Log("You have no more waves");
            return null;
        }
        
        List<GameObject> newEnemyList = new List<GameObject>();

        // Add basic enemies
        for (int i = 0; i < levelWaves[waveIndex].basicEnemy; i++)
        {
            newEnemyList.Add(basicEnemy);
        }

        // Add fast enemies  
        for (int i = 0; i < levelWaves[waveIndex].fastEnemy; i++)
        {
            newEnemyList.Add(fastEnemy);
        }
        
        // Add swarm enemy
        for (int i = 0; i < levelWaves[waveIndex].swarmEnemy; i++)
        {
            newEnemyList.Add(swarmEnemy);
        }
        
        // Add heavy enemy
        for (int i = 0; i < levelWaves[waveIndex].heavyEnemy; i++)
        {
            newEnemyList.Add(heavyEnemy);
        }
        
        // Add stealth enemy
        for (int i = 0; i < levelWaves[waveIndex].stealthEnemy; i++)
        {
            newEnemyList.Add(stealthEnemy);
        }
        
        // Add flying enemy
        for (int i = 0; i < levelWaves[waveIndex].flyingEnemy; i++)
        {
            newEnemyList.Add(flyingEnemy);
        }
        
        // Add flying boss enemy
        for (int i = 0; i < levelWaves[waveIndex].flyingBossEnemy; i++)
        {
            newEnemyList.Add(flyingBossEnemy);
        }
        
        // Add spider boss enemy
        for (int i = 0; i < levelWaves[waveIndex].spiderBossEnemy; i++)
        {
            newEnemyList.Add(spiderBossEnemy);
        }
        
        return newEnemyList;
    }
    
    public WaveDetails[] GetLevelWaves() => levelWaves;
    public int GetCurrentWaveIndex() => waveIndex;
    

    /// <summary>
    /// Checks if all active enemies in all portals have been defeated.
    /// </summary>
    private bool AllEnemiesDefeated()
    {
        foreach (EnemyPortal portal in enemyPortals)
        {
            if (portal.GetActiveEnemies().Count > 0) return false;
        }

        return true;
    }
    
    /// <summary>
    /// Checks if the next wave has a new grid layout defined.
    /// </summary>
    private bool HasNewLayout() => waveIndex < levelWaves.Length && levelWaves[waveIndex].nextGrid != null;

    /// <summary>
    /// Checks if the level is out of waves.
    /// </summary>
    private bool HasNoMoreWaves() => waveIndex >= levelWaves.Length;
}