using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public UIGame inGameUI { get; private set; }
    public WaveManager currentActiveWaveManager;
    private LevelManager levelManager;
    private CameraEffects cameraEffects;
    
    [SerializeField] private int currency;
    [SerializeField] private int maxHp; 
    private int currentHp;
    
    private Transform castleTransform; 

    [Header("Win/Loss Visuals")]
    [SerializeField] private GameObject winFireworksVFX;
    [SerializeField] private GameObject loseSmokeVFX;
    
    
    public int enemiesKilled { get; private set; }

    private bool gameLost;
    public bool IsGameLost() => gameLost;

    private void Awake()
    {
        instance = this;
        
        inGameUI = FindFirstObjectByType<UIGame>(FindObjectsInactive.Include);
        levelManager = FindFirstObjectByType<LevelManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("TowerProjectile"), LayerMask.NameToLayer("TowerProjectile"), true);
    }

    private void Start()
    {
        currentHp = maxHp;

        // Enable if need to test a level using high currency and hp
        /*if (IsTestingLevel())
        {
            currency += 500;
            currentHp += 9999;
        }*/
        
        inGameUI.UpdateHealthPointsUI(currentHp,maxHp);
        inGameUI.UpdateCurrencyUI(currency);
    }

    public void StopMakingEnemies()
    {
        EnemyPortal[] portals = FindObjectsByType<EnemyPortal>(FindObjectsSortMode.None);

        foreach (var portal in portals)
        {
            portal.CanCreateNewEnemies(false);
        }
    }

    public bool IsTestingLevel() => levelManager == null;

    public IEnumerator LevelFailedCo()
    {
        gameLost = true;

        StopWaveProgression();
        DisableAllTowers();
        FreezeAllEnemies();

        if (loseSmokeVFX != null)
        {
            // Find only ACTIVE castles
            Castle activeCastle = FindFirstObjectByType<Castle>(FindObjectsInactive.Exclude);
        
            if (activeCastle != null)
            {
                Vector3 spawnPosition = activeCastle.transform.position;
                spawnPosition.y += 1.5f;
                Debug.Log($"[LevelFailedCo] Spawning smoke at active castle: {activeCastle.gameObject.name} at {spawnPosition}");
                Instantiate(loseSmokeVFX, spawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("[LevelFailedCo] No active castle found to spawn smoke at!");
            }
        }

        yield return new WaitForSeconds(1f);

        yield return ShowGameOverSequence();
    }

    private void StopWaveProgression()
    {
        StopMakingEnemies();
        
        if (currentActiveWaveManager != null)
        {
            currentActiveWaveManager.DeactivateWaveManager();
        }
    }

    private void DisableAllTowers()
    {
        Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        foreach (Tower tower in allTowers)
        {
            if (tower != null) tower.enabled = false;
        }
    }

    private void FreezeAllEnemies()
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                var navAgent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (navAgent != null && navAgent.enabled)
                {
                    navAgent.isStopped = true;
                    navAgent.velocity = Vector3.zero;
                }
                enemy.enabled = false;
            }
        }
    }

    private IEnumerator ShowGameOverSequence()
    {
        if (cameraEffects != null)
        {
            cameraEffects.FocusOnCastle();
            yield return cameraEffects.GetActiveCameraCo();
        }
        
        if (inGameUI != null)
        {
            inGameUI.EnableGameOverUI(true);
        }
    }

    public void LevelCompleted() => StartCoroutine(LevelCompletedCo());

    public IEnumerator LevelCompletedCo()
    {
        bool isFinalLevel = levelManager.HasNoMoreLevels();
    
        if (isFinalLevel && winFireworksVFX != null)
        {
            // Find only ACTIVE castles
            Castle activeCastle = FindFirstObjectByType<Castle>(FindObjectsInactive.Exclude);
        
            if (activeCastle != null)
            {
                Vector3 spawnPosition = activeCastle.transform.position;
                spawnPosition.y += 2f;
                Debug.Log($"[LevelCompletedCo] Spawning fireworks at active castle: {activeCastle.gameObject.name} at {spawnPosition}");
                Quaternion spawnRotation = Quaternion.Euler(-90, 0, 0);
                Instantiate(winFireworksVFX, spawnPosition, spawnRotation);
            }
            else
            {
                Debug.LogWarning("[LevelCompletedCo] No active castle found to spawn fireworks at!");
            }
        }
    
        yield return new WaitForSeconds(1.5f);
    
        cameraEffects.FocusOnCastle();

        yield return cameraEffects.GetActiveCameraCo();

        if (isFinalLevel)
        { 
            inGameUI.EnableVictoryUI(true);
        }
        else
        {
            inGameUI.EnableLevelCompletedUI(true);
            PlayerPrefs.SetInt(levelManager.GetNextLevelName() + " unlocked", 1);
        }
    }

    public void PrepareLevel(int levelCurrency, WaveManager newWaveManager)
    {
        gameLost = false;
        enemiesKilled = 0;
        
        currentActiveWaveManager = newWaveManager;
        currency = levelCurrency;
        currentHp = maxHp;
        
        inGameUI.UpdateHealthPointsUI(currentHp, maxHp);
        inGameUI.UpdateCurrencyUI(currency);
        
        newWaveManager.ActivateWaveManager();
    }

    public void UpdateHp(int value)
    {
        currentHp += value;
        inGameUI.UpdateHealthPointsUI(currentHp,maxHp);
        inGameUI.ShakeHealthUI();
        
        if (currentHp <= 0 && gameLost == false) StartCoroutine(LevelFailedCo());
    }

    public void UpdateEnemiesKilled() => enemiesKilled++;
    

    public void UpdateCurrency(int value)
    {
        currency += value;
        inGameUI.UpdateCurrencyUI(currency);
    }

    public bool HasEnoughCurrency(int price)
    {
        return price <= currency;
    }
}