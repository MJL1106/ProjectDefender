using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages scene loading and transitions between the menu and levels.
/// Handles cleanup of old level objects before loading the new one.
/// </summary>
public class LevelManager : MonoBehaviour
{
    private TileAnimator tileAnimator;
    private UI ui;
    private CameraEffects cameraEffects;
    
    private GridBuilder currentActiveGrid;
    public string currentLevelName { get; private set; }

    [Header("Color Change Details")] 
    [SerializeField] private MeshRenderer groundMesh; // The main ground plane to change color
    private Color defaultColor;
    
    private void Awake()
    {
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        ui = FindFirstObjectByType<UI>();

        defaultColor = groundMesh.material.color;
        groundMesh.material = new Material(groundMesh.material);
    }

    /// <summary>
    /// Reloads the currently active level scene.
    /// </summary>
    public void RestartCurrentLevel() => StartCoroutine(LoadLevelCo(currentLevelName));
    
    /// <summary>
    /// Loads a level by name. (Used for "Retry" or "Next Level").
    /// </summary>
    public void LoadLevel(string levelName) => StartCoroutine(LoadLevelCo(levelName));
    
    /// <summary>
    /// Loads the level with the next sequential build index.
    /// </summary>
    public void LoadNextLevel() => LoadLevel(GetNextLevelName());
    
    /// <summary>
    /// Loads a level from the main menu, running menu-to-game transitions.
    /// </summary>
    public void LoadLevelFromMenu(string levelName) => StartCoroutine(LoadLevelFromMenuCo(levelName));

    /// <summary>
    /// Loads the main menu scene, running game-to-menu transitions.
    /// </summary>
    public void LoadMainMenu()
    {
        StartCoroutine(LoadMainMenuCo());
    }

    /// <summary>
    /// Coroutine for loading a level from within another level (e.g., restart, next).
    /// </summary>
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
    
        yield return UnloadCurrentScene();
        LoadScene(levelName);
    }

    /// <summary>
    /// Coroutine for loading a level from the main menu.
    /// </summary>
    private IEnumerator LoadLevelFromMenuCo(string levelName)
    {
        LevelData upcomingData = Resources.Load<LevelData>("LevelData/" + levelName);
        if (upcomingData == null)
        {
            Debug.LogError("FATAL: Could not find LevelData at 'Resources/LevelData/" + levelName + "'");
            yield break;
        }
    
    
        tileAnimator.ShowMainGrid(false);
        ui.EnableMainMenuUI(false);
    
        cameraEffects.SwitchToGameView();

        StartCoroutine(UpdateBackgroundColorCo(upcomingData.groundMaterial.color, 1.5f));
        
        yield return tileAnimator.GetActiveCoroutine();
    
        tileAnimator.EnableMainSceneObjects(false);
        
        yield return cameraEffects.GetActiveCameraCo(); 
        
        LoadScene(levelName);
    }

    /// <summary>
    /// Coroutine for loading the main menu from a level.
    /// </summary>
    private IEnumerator LoadMainMenuCo()
    {
        CleanUpScene();
        ui.EnableInGameUI(false);
    
        cameraEffects.SwitchToMenuView();

        UpdateBackgroundColor(defaultColor);
        
        yield return tileAnimator.GetActiveCoroutine();
        
        yield return UnloadCurrentScene();
        
        if (tileAnimator == null) tileAnimator = FindFirstObjectByType<TileAnimator>();
    
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

    /// <summary>
    /// Loads a new scene additively and sets it as the current level.
    /// </summary>
    private void LoadScene(string sceneNameToLoad)
    {
        currentLevelName = sceneNameToLoad;
        SceneManager.LoadSceneAsync(sceneNameToLoad, LoadSceneMode.Additive);
    }

    /// <summary>
    /// Unloads the currently active level scene.
    /// </summary>
    private AsyncOperation UnloadCurrentScene() => SceneManager.UnloadSceneAsync(currentLevelName);

    /// <summary>
    /// Cleans up all dynamic objects from the current level before transitioning.
    /// </summary>
    private void CleanUpScene()
    {
        GameManager.instance.StopMakingEnemies();
        GameManager.instance.CleanUpVFX();
        EliminateAllEnemies();
        EliminateAllTowers();
        EliminateAllPreviews();
        
        if (currentActiveGrid != null) tileAnimator.ShowGrid(currentActiveGrid, false);
    }

    /// <summary>
    /// Destroys all active tower preview objects.
    /// </summary>
    private void EliminateAllPreviews()
    {
        TowerPreview[] previews = FindObjectsByType<TowerPreview>(FindObjectsSortMode.None);
        foreach (var preview in previews)
        {
            Destroy(preview.gameObject);
        }
    }

    /// <summary>
    /// Removes all active enemies from the scene.
    /// </summary>
    private void EliminateAllEnemies()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            enemy.RemoveEnemy();
        }
    }

    /// <summary>
    /// Destroys all built towers.
    /// </summary>
    private void EliminateAllTowers()
    {
        Tower[] towers = FindObjectsByType<Tower>(FindObjectsSortMode.None);

        foreach (Tower tower in towers)
        {
            Destroy(tower.gameObject);
        }
    }

    /// <summary>
    /// Public wrapper to start the background color fade.
    /// </summary>
    public void UpdateBackgroundColor(Color targetColor)
    {
        StartCoroutine(UpdateBackgroundColorCo(targetColor, 1.5f));
    }

    /// <summary>
    /// Coroutine to smoothly lerp the ground material color.
    /// </summary>
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
    
    /// <summary>
    /// Caches the active level grid so it can be animated out during scene transitions.
    /// </summary>
    public void UpdateCurrentGrid(GridBuilder newGrid) => currentActiveGrid = newGrid;

    /// <summary>
    /// Gets the build index of the next level.
    /// </summary>
    public int GetNextLevelIndex() => SceneUtility.GetBuildIndexByScenePath(currentLevelName) + 1;
    
    /// <summary>
    /// Gets the scene name of the next level (e.g., "Level_3").
    /// </summary>
    public string GetNextLevelName() => "Level_" + GetNextLevelIndex();
    
    /// <summary>
    /// Checks if the current level is the last one in the build settings.
    /// </summary>
    public bool HasNoMoreLevels() => GetNextLevelIndex() >= SceneManager.sceneCountInBuildSettings;
}