using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyPortal : MonoBehaviour
{
    [SerializeField] private float spawnCooldown;
    private float spawnTimer;
    
    [Space]
    
    [SerializeField] private List<Waypoint> waypointList;

    private List<GameObject> enemiesToCreate = new List<GameObject>();

    private void Awake()
    {
        CollectWaypoints();
    }

    private void Update()
    {
        if (CanMakeNewEnemy()) CreateEnemy();
    }

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
        if (enemiesToCreate.Count == 0) return;
        
        GameObject randomEnemy = GetRandomEnemy();
        
        if (randomEnemy == null)
        {
            return;
        }
        
        GameObject newEnemy = Instantiate(randomEnemy, transform.position, Quaternion.identity);

        Enemy enemyScript = newEnemy.GetComponent<Enemy>();
        enemyScript.SetupEnemy(waypointList);
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

    [ContextMenu("Add waypoints")]
    private void CollectWaypoints()
    {
        waypointList = new List<Waypoint>();

        foreach (Transform child in transform)
        {
            Waypoint waypoint = child.GetComponent<Waypoint>();
            
            if (waypoint != null) waypointList.Add(waypoint);
        }
    }
}
