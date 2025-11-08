using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton manager for core game state.
/// Tracks currency, castle health, win/loss conditions, and scene-level setup.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public UIGame inGameUI { get; private set; }
    public UISettings settingsUI { get; private set; }
    public WaveManager currentActiveWaveManager;
    private LevelManager levelManager;
    private CameraEffects cameraEffects;
    
    [SerializeField] private int currency;
    [SerializeField] private int maxHp; // Starting and maximum health of the castle
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
        settingsUI = FindFirstObjectByType<UISettings>(FindObjectsInactive.Include);
        levelManager = FindFirstObjectByType<LevelManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("TowerProjectile"), LayerMask.NameToLayer("TowerProjectile"), true);
    }

    private void Start()
    {
        currentHp = maxHp;
    
        inGameUI.UpdateHealthPointsUI(currentHp,maxHp);
        inGameUI.UpdateCurrencyUI(currency);

        // Enable settings UI temporarily to apply settings
        if (settingsUI != null)
        {
            settingsUI.gameObject.SetActive(true);
            settingsUI.ApplyAllSettingsOnStartup();
            settingsUI.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Tells all active EnemyPortals to stop spawning new enemies.
    /// </summary>
    public void StopMakingEnemies()
    {
        EnemyPortal[] portals = FindObjectsByType<EnemyPortal>(FindObjectsSortMode.None);

        foreach (var portal in portals)
        {
            portal.CanCreateNewEnemies(false);
        }
    }

    /// <summary>
    /// Checks if the game is in a test scene (no LevelManager) or a real level.
    /// </summary>
    public bool IsTestingLevel() => levelManager == null;

    /// <summary>
    /// Triggers the entire game over sequence.
    /// Stops waves, freezes units, shows VFX, and brings up the game over UI.
    /// </summary>
    public IEnumerator LevelFailedCo()
    {
        gameLost = true;

        StopWaveProgression();
        DisableAllTowers();
        FreezeAllEnemies();

        if (loseSmokeVFX != null)
        {
            Castle activeCastle = FindFirstObjectByType<Castle>(FindObjectsInactive.Exclude);
        
            if (activeCastle != null)
            {
                Vector3 spawnPosition = activeCastle.transform.position;
                spawnPosition.y += 1.3f;
                Instantiate(loseSmokeVFX, spawnPosition, Quaternion.identity);
            }
        }

        yield return new WaitForSeconds(1f);

        yield return ShowGameOverSequence();
    }

    /// <summary>
    /// Stops all wave timers and enemy spawners.
    /// </summary>
    private void StopWaveProgression()
    {
        StopMakingEnemies();
        
        if (currentActiveWaveManager != null) currentActiveWaveManager.DeactivateWaveManager();
        
    }

    /// <summary>
    /// Disables all active towers in the scene.
    /// </summary>
    private void DisableAllTowers()
    {
        Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        foreach (Tower tower in allTowers)
        {
            if (tower != null) tower.enabled = false;
        }
    }

    /// <summary>
    /// Stops all active enemies from moving or acting.
    /// </summary>
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

    /// <summary>
    /// Focuses the camera on the castle and shows the game over UI.
    /// </summary>
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

    /// <summary>
    /// Public wrapper to start the level completion coroutine.
    /// </summary>
    public void LevelCompleted() => StartCoroutine(LevelCompletedCo());

    /// <summary>
    /// Triggers the level complete sequence.
    /// Plays VFX, focuses camera, shows victory UI, and unlocks the next level.
    /// </summary>
    public IEnumerator LevelCompletedCo()
    {
        bool isFinalLevel = levelManager.HasNoMoreLevels();
        
        if (!isFinalLevel)
        {
            PlayerPrefs.SetInt(levelManager.GetNextLevelName() + " unlocked", 1);
            PlayerPrefs.Save();
        }
    
        if (isFinalLevel && winFireworksVFX != null)
        {
            Castle activeCastle = FindFirstObjectByType<Castle>(FindObjectsInactive.Exclude);
        
            if (activeCastle != null)
            {
                Vector3 spawnPosition = activeCastle.transform.position;
                spawnPosition.y += 2f;
                Quaternion spawnRotation = Quaternion.Euler(-90, 0, 0);
                Instantiate(winFireworksVFX, spawnPosition, spawnRotation);
            }
        }
    
        yield return new WaitForSeconds(1.5f);
    
        cameraEffects.FocusOnCastle();

        yield return cameraEffects.GetActiveCameraCo();

        if (isFinalLevel) inGameUI.EnableVictoryUI(true);
        else inGameUI.EnableLevelCompletedUI(true);
    }

    /// <summary>
    /// Called by LevelSetup to initialize game state for the new level.
    /// </summary>
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
    
    /// <summary>
    /// Destroys all objects on the "VFX" layer.
    /// Used for scene cleanup during level transitions.
    /// </summary>
    public void CleanUpVFX()
    {
        int vfxLayer = LayerMask.NameToLayer("VFX");
    
        ParticleSystem[] allParticles = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
    
        foreach (ParticleSystem ps in allParticles)
        {
            if (ps.gameObject.layer == vfxLayer) Destroy(ps.gameObject);
        }
    }

    /// <summary>
    /// Updates the castle's health points, updates UI, and checks for game over.
    /// </summary>
    /// <param name="value">The amount to add to health. Can be negative for damage.</param>
    public void UpdateHp(int value)
    {
        currentHp += value;
        inGameUI.UpdateHealthPointsUI(currentHp,maxHp);
        inGameUI.ShakeHealthUI();
        
        if (currentHp <= 0 && gameLost == false) StartCoroutine(LevelFailedCo());
    }

    /// <summary>
    /// Increments the count of enemies killed.
    /// </summary>
    public void UpdateEnemiesKilled() => enemiesKilled++;
    

    /// <summary>
    /// Updates the player's currency and refreshes the UI.
    /// </summary>
    public void UpdateCurrency(int value)
    {
        currency += value;
        inGameUI.UpdateCurrencyUI(currency);
    }

    /// <summary>
    // Checks if the player has enough currency for a purchase.
    /// </summary>
    public bool HasEnoughCurrency(int price)
    {
        return price <= currency;
    }
}