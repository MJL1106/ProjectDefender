using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[System.Serializable]
public class WaveDetails
{
    public GridBuilder nextGrid;
    public EnemyPortal[] newPortals;
    public int basicEnemy;
    public int fastEnemy;
    public int swarmEnemy;
    public int heavyEnemy;
    public int stealthEnemy;
    public int flyingEnemy;
    public int flyingBossEnemy;
    public int spiderBossEnemy;
}

public class WaveManager : MonoBehaviour
{
    private GameManager gameManager;
    private TileAnimator tileAnimator;
    private UIGame inGameUI;
    [SerializeField] private GridBuilder currentGrid;
    [SerializeField] private NavMeshSurface flyingNavSurface;
    [SerializeField] private NavMeshSurface droneNavSurface;
    [SerializeField] private MeshCollider[] flyingNavColliders;

    [Header("Wave Details")]
    [SerializeField] private float timeBetweenWaves = 10;
    [SerializeField] private float waveTimer;
    [SerializeField] private WaveDetails[] levelWaves;
    [SerializeField] private int waveIndex;

    [Header("Level Update Details")] [SerializeField]
    private float yOffset = 5;
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

        flyingNavColliders = GetComponentsInChildren<MeshCollider>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ActivateWaveManager();
        }
        
        if (gameBegan == false) return;
        
        HandleWaveTimer();
    }

    [ContextMenu("Activate wave manager")]
    public void ActivateWaveManager()
    {
        gameBegan = true;
        inGameUI = gameManager.inGameUI;
        EnableWaveTimer(true);
    }

    public void DeactivateWaveManager() => gameBegan = false;

    public void CheckIfWaveCompleted()
    {
        if (gameBegan == false) return;
        
        if (AllEnemiesDefeated() == false || makingNextWave) return;

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
    
    public void StartNewWave()
    {
        UpdateNavMeshes();
        GiveEnemiesToPortals();
        EnableWaveTimer(false);
        makingNextWave = false;
    }
    
    private void HandleWaveTimer()
    {
        if (waveTimerEnabled == false) return;
        
        waveTimer -= Time.deltaTime;
        inGameUI.UpdateWaveTimerUI(waveTimer);

        if (waveTimer <= 0) StartNewWave();
        
    }
    
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

    private void AttemptToUpdateLayout() => UpdateLevelLayout(levelWaves[waveIndex]);

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
    
    private void AddTile(TileSlot newTile)
    {
        newTile.gameObject.SetActive(true);
        newTile.transform.position += new Vector3(0, -yOffset, 0);
        newTile.transform.parent = currentGrid.transform;

        Vector3 targetPosition = newTile.transform.position + new Vector3(0, yOffset, 0);
        
        tileAnimator.DissolveTile(true, newTile.transform);
        tileAnimator.MoveTile(newTile.transform, targetPosition, true);
    }

    private void RemoveTile(TileSlot tileToRemove)
    {
        Vector3 targetPosition = tileToRemove.transform.position + new Vector3(0, -yOffset, 0);
        
        tileAnimator.DissolveTile(false, tileToRemove.transform);
        tileAnimator.MoveTile(tileToRemove.transform, targetPosition, false);
        
        Destroy(tileToRemove.gameObject, 3);
    }

    private void EnableWaveTimer(bool enable)
    {
        if (waveTimerEnabled == enable) return;

        waveTimer = timeBetweenWaves;
        waveTimerEnabled = enable;
        inGameUI.EnableWaveTimer(enable);
    }
    
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

    private void UpdateNavMeshes()
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
        
        currentGrid.UpdateNavMesh();
        droneNavSurface.BuildNavMesh();
    }

    public void UpdateDroneNavMesh() => droneNavSurface.BuildNavMesh();
    
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
    

    private bool AllEnemiesDefeated()
    {
        foreach (EnemyPortal portal in enemyPortals)
        {
            if (portal.GetActiveEnemies().Count > 0)
            {
                return false;
            }
        }

        return true;
    }

    private bool HasNewLayout() => waveIndex < levelWaves.Length && levelWaves[waveIndex].nextGrid != null;

    private bool HasNoMoreWaves() => waveIndex >= levelWaves.Length;
}
