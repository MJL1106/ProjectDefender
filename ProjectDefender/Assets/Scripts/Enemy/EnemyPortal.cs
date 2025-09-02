using System.Collections.Generic;
using UnityEngine;

public class EnemyPortal : MonoBehaviour
{
    [SerializeField] private float spawnCooldown;
    private float spawnTimer;

    public List<GameObject> enemiesToCreate;
    
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
    }

    private GameObject GetRandomEnemy()
    {
        if (enemiesToCreate.Count == 0) return null;
        
        int randomIndex = Random.Range(0, enemiesToCreate.Count);
        GameObject chosenEnemy = enemiesToCreate[randomIndex];
        
        enemiesToCreate.RemoveAt(randomIndex);
        
        return chosenEnemy;
    }

    public List<GameObject> GetEnemyList() { return enemiesToCreate; }
}
