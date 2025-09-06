using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveDetails
{
    public GridBuilder nextGrid;
    public EnemyPortal[] newPortals;
    public int basicEnemy;
    public int fastEnemy;
}

public class WaveManager : MonoBehaviour
{
    private UIGame inGameUI;
    
    [SerializeField] private GridBuilder currentGrid;

    [Header("Wave Details")]
    [SerializeField] private float timeBetweenWaves = 10;
    [SerializeField] private float waveTimer;
    [SerializeField] private WaveDetails[] levelWaves;
    [SerializeField] private int waveIndex;
    
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicEnemy;
    [SerializeField] private GameObject fastEnemy;
    
    private List<EnemyPortal> enemyPortals;
    private bool waveTimerEnabled;
    private bool makingNextWave;
    public bool gameBegan;

    private void Awake()
    {
        enemyPortals = new List<EnemyPortal>(FindObjectsByType<EnemyPortal>(FindObjectsSortMode.None));
        inGameUI = FindFirstObjectByType<UIGame>(FindObjectsInactive.Include);
    }

    private void Update()
    {
        if (gameBegan == false) return;
        
        HandleWaveTimer();
    }

    [ContextMenu("Activate wave manager")]
    public void ActivateWaveManager()
    {
        gameBegan = true;
        EnableWaveTimer(true);
    }

    public WaveDetails[] GetLevelWaves() => levelWaves;

    public void CheckIfWaveCompleted()
    {
        if (AllEnemiesDefeated() == false || makingNextWave) return;

        makingNextWave = true;
        
        CheckForNewLevelLayout();
        EnableWaveTimer(true);
    }

    private void HandleWaveTimer()
    {
        if (waveTimerEnabled == false) return;
        
        waveTimer -= Time.deltaTime;
        inGameUI.UpdateWaveTimerUI(waveTimer);

        if (waveTimer <= 0) StartNewWave();
        
    }

    public void StartNewWave()
    {
        waveIndex++;
        
        GiveEnemiesToPortals();
        EnableWaveTimer(false);

        makingNextWave = false;
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

    private void CheckForNewLevelLayout()
    {
        if (waveIndex >= levelWaves.Length) return;

        WaveDetails nextWave = levelWaves[waveIndex];

        if (nextWave.nextGrid != null)
        {
            UpdateLevelTiles(nextWave.nextGrid);
            EnableNewPortals(nextWave.newPortals);
        }
        currentGrid.UpdateNavMesh();
    }

    private void UpdateLevelTiles(GridBuilder nextGrid)
    {
        
        List<GameObject> grid = currentGrid.GetTileSetup();
        List<GameObject> newGrid = nextGrid.GetTileSetup();

        for (int i = 0; i < grid.Count; i++) // Current grid and next grid have to be the same size, they are duplicated.
        {
            TileSlot currentTile = grid[i].GetComponent<TileSlot>();
            TileSlot newTile = newGrid[i].GetComponent<TileSlot>();

            bool shouldBeUpdated = currentTile.GetMesh() != newTile.GetMesh() ||
                                   currentTile.GetMaterial() != newTile.GetMaterial() ||
                                   currentTile.GetAllChildren().Count != newTile.GetAllChildren().Count ||
                                   currentTile.transform.rotation != newTile.transform.rotation;

            if (shouldBeUpdated)
            {
                currentTile.gameObject.SetActive(false);

                newTile.gameObject.SetActive(true);
                newTile.transform.parent = currentGrid.transform;

                grid[i] = newTile.gameObject;
                Destroy(currentTile.gameObject);
            }
        }
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
            portal.AssignWaveManager(this);
            portal.gameObject.SetActive(true);
            enemyPortals.Add(portal);
        }
    }
    
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
        
        return newEnemyList;
    }

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
}
