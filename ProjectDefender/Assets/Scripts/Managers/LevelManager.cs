using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private TileAnimator tileAnimator;
    private UI ui;
    private CameraEffects cameraEffects;
    
    private GridBuilder currentActiveGrid;
    public string currentLevelName { get; private set; }

    [Header("Color Change Details")] 
    [SerializeField] private MeshRenderer groundMesh;
    private Color defaultColor;
    
    private void Awake()
    {
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        ui = FindFirstObjectByType<UI>();

        defaultColor = groundMesh.material.color;
        groundMesh.material = new Material(groundMesh.material);
    }

    public void RestartCurrentLevel() => StartCoroutine(LoadLevelCo(currentLevelName));
    public void LoadLevel(string levelName) => StartCoroutine(LoadLevelCo(levelName));
    public void LoadNextLevel() => LoadLevel(GetNextLevelName());
    public void LoadLevelFromMenu(string levelName) => StartCoroutine(LoadLevelFromMenuCo(levelName));

    public void LoadMainMenu()
    {
        StartCoroutine(LoadMainMenuCo());
    }

    private IEnumerator LoadLevelCo(string levelName)
    {
        LevelData upcomingData = Resources.Load<LevelData>("LevelData/" + levelName);
        if (upcomingData == null)
        {
            Debug.LogError("FATAL: Could not find LevelData at 'Resources/LevelData/" + levelName + "'");
            yield break;
        }
    
        StartCoroutine(UpdateBackgroundColorCo(upcomingData.groundMaterial.color, 1.5f));
    
        CleanUpScene();
        ui.EnableInGameUI(false);
    
        cameraEffects.SwitchToGameView();
        yield return tileAnimator.GetActiveCoroutine();
    
        yield return UnloadCurrentScene(); // --- CHANGE HERE ---
        LoadScene(levelName);
    }

    private IEnumerator LoadLevelFromMenuCo(string levelName)
    {
        LevelData upcomingData = Resources.Load<LevelData>("LevelData/" + levelName);
        if (upcomingData == null)
        {
            Debug.LogError("FATAL: Could not find LevelData at 'Resources/LevelData/" + levelName + "'");
            yield break;
        }
    
        StartCoroutine(UpdateBackgroundColorCo(upcomingData.groundMaterial.color, 1.5f));
    
        tileAnimator.ShowMainGrid(false);
        ui.EnableMainMenuUI(false);
    
        cameraEffects.SwitchToGameView();

        yield return tileAnimator.GetActiveCoroutine();
    
        tileAnimator.EnableMainSceneObjects(false);
        
        yield return cameraEffects.GetActiveCameraCo(); 
        
        LoadScene(levelName);
    }

    private IEnumerator LoadMainMenuCo()
    {
        CleanUpScene();
        ui.EnableInGameUI(false);
    
        cameraEffects.SwitchToMenuView();

        yield return tileAnimator.GetActiveCoroutine();
    
        UpdateBackgroundColor(defaultColor);
    
        yield return UnloadCurrentScene();
        
        if (tileAnimator == null)
        {
            tileAnimator = FindFirstObjectByType<TileAnimator>();
        }
    
        if (tileAnimator != null)
        {
            tileAnimator.EnableMainSceneObjects(true);
            cameraEffects.UnselectAllTiles();
            tileAnimator.ShowMainGrid(true);

            yield return tileAnimator.GetActiveCoroutine();
        }

        ui.EnableMainMenuUI(true);
        
        cameraEffects.EnableAllTiles(false);
        cameraEffects.EnableLevelButtonTiles(false);
    }

    private void LoadScene(string sceneNameToLoad)
    {
        currentLevelName = sceneNameToLoad;
        SceneManager.LoadSceneAsync(sceneNameToLoad, LoadSceneMode.Additive);
    }

    private AsyncOperation UnloadCurrentScene() => SceneManager.UnloadSceneAsync(currentLevelName);

    private void CleanUpScene()
    {
        GameManager.instance.StopMakingEnemies();
        GameManager.instance.CleanUpVFX();
        EliminateAllEnemies();
        EliminateAllTowers();
        EliminateAllPreviews();
        
        if (currentActiveGrid != null) tileAnimator.ShowGrid(currentActiveGrid, false);
    }

    private void EliminateAllPreviews()
    {
        TowerPreview[] previews = FindObjectsByType<TowerPreview>(FindObjectsSortMode.None);
        foreach (var preview in previews)
        {
            Destroy(preview.gameObject);
        }
    }

    private void EliminateAllEnemies()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            enemy.RemoveEnemy();
        }
    }

    private void EliminateAllTowers()
    {
        Tower[] towers = FindObjectsByType<Tower>(FindObjectsSortMode.None);

        foreach (Tower tower in towers)
        {
            Destroy(tower.gameObject);
        }
    }

    public void UpdateBackgroundColor(Color targetColor)
    {
        StartCoroutine(UpdateBackgroundColorCo(targetColor, 1.5f));
    }

    private IEnumerator UpdateBackgroundColorCo(Color targetColor, float duration)
    {
        float time = 0;
        Color startColor = groundMesh.material.color;

        while (time < duration)
        {
            Color currentColor = Color.Lerp(startColor, targetColor, time / duration);
            groundMesh.material.color = currentColor;

            time += Time.deltaTime;
            yield return null;
        }

        groundMesh.material.color = targetColor;
    }
    
    public void UpdateCurrentGrid(GridBuilder newGrid) => currentActiveGrid = newGrid;

    public int GetNextLevelIndex() => SceneUtility.GetBuildIndexByScenePath(currentLevelName) + 1;
    public string GetNextLevelName() => "Level_" + GetNextLevelIndex();
    public bool HasNoMoreLevels() => GetNextLevelIndex() >= SceneManager.sceneCountInBuildSettings;
}