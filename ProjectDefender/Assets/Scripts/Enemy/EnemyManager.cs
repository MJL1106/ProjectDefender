using System;
using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class WaveDetails
{
    public int basicEnemy;
    public int fastEnemy;
}

public class EnemyManager : MonoBehaviour
{
    public List<EnemyPortal> enemyPortals;
    [SerializeField] private WaveDetails currentWave;
    
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicEnemy;
    [SerializeField] private GameObject fastEnemy;

    private void Awake()
    {
        enemyPortals = new List<EnemyPortal>(FindObjectsByType<EnemyPortal>(FindObjectsSortMode.None));
    }

    [ContextMenu("Setup Next Wave")]
    private void SetupNextWave()
    {
        List<GameObject> newEnemies = NewEnemyWave();
        int portalIndex = 0;

        for (int i = 0; i < newEnemies.Count; i++)
        {
            GameObject enemyToAdd = newEnemies[i];
            EnemyPortal portalToReceiveEnemy = enemyPortals[portalIndex];

            portalToReceiveEnemy.GetEnemyList().Add(enemyToAdd);

            portalIndex++;

            if (portalIndex >= enemyPortals.Count)
            {
                portalIndex = 0;
            }
        }
    }
    private List<GameObject> NewEnemyWave()
    {
        List<GameObject> newEnemyList = new List<GameObject>();

        // Add basic enemies
        for (int i = 0; i < currentWave.basicEnemy; i++)
        {
            newEnemyList.Add(basicEnemy);
        }

        // Add fast enemies  
        for (int i = 0; i < currentWave.fastEnemy; i++)
        {
            newEnemyList.Add(fastEnemy);
        }
        
        return newEnemyList;
    }
}
