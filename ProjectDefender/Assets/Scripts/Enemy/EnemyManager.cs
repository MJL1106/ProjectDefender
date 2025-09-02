using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveDetails
{
    public int basicEnemy;
    public int fastEnemy;
}

public class EnemyManager : MonoBehaviour
{
    public bool waveCompleted;

    public float timeBetweenWaves = 10;
    public float waveTimer;
    [SerializeField] private WaveDetails[] levelWaves;
    private int waveIndex;

    private float checkInterval = .5f;
    private float nextCheckTime;
    
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicEnemy;
    [SerializeField] private GameObject fastEnemy;
    
    private List<EnemyPortal> enemyPortals;

    private void Awake()
    {
        enemyPortals = new List<EnemyPortal>(FindObjectsByType<EnemyPortal>(FindObjectsSortMode.None));
    }

    private void Start()
    {
        SetupNextWave();
    }

    private void Update()
    {
        HandleWaveCompletion();
        HandleWaveTiming();
    }

    private void HandleWaveCompletion()
    {
        if (ReadyToCheck() == false) return;
        
        if (waveCompleted == false && AllEnemiesDefeated())
        {
            waveCompleted = true;
            waveTimer = timeBetweenWaves;
        }
    }

    private void HandleWaveTiming()
    {
        if (waveCompleted)
        {
            waveTimer -= Time.deltaTime;
            
            if (waveTimer <=0) SetupNextWave();
        }
    }

    public void ForceNextWave()
    {
        if (AllEnemiesDefeated() == false) return;
        
        SetupNextWave();
    }

    [ContextMenu("Setup Next Wave")]
    private void SetupNextWave()
    {
        List<GameObject> newEnemies = NewEnemyWave();
        int portalIndex = 0;

        if (newEnemies == null) return;

        for (int i = 0; i < newEnemies.Count; i++)
        {
            GameObject enemyToAdd = newEnemies[i];
            EnemyPortal portalToReceiveEnemy = enemyPortals[portalIndex];

            portalToReceiveEnemy.AddEnemy(enemyToAdd);

            portalIndex++;

            if (portalIndex >= enemyPortals.Count)
            {
                portalIndex = 0;
            }
        }

        waveCompleted = false;
    }
    private List<GameObject> NewEnemyWave()
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

        waveIndex++;
        
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

    private bool ReadyToCheck()
    {
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            return true;
        }

        return false;
    }
}
