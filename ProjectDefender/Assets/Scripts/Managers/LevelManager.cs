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
    
    private void Awake()
    {
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        ui = FindFirstObjectByType<UI>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) LoadLevelFromMenu("Level_1");
        if (Input.GetKeyDown(KeyCode.K)) LoadMainMenu();
        if (Input.GetKeyDown(KeyCode.R)) RestartCurrentLevel();
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
        CleanUpScene();
        ui.EnableInGameUI(false);
        
        cameraEffects.SwitchToGameView();
        yield return tileAnimator.GetActiveCoroutine();
        
        UnloadCurrentScene();
        LoadScene(levelName);
    }

    private IEnumerator LoadLevelFromMenuCo(string levelName)
    {
        tileAnimator.ShowMainGrid(false);
        ui.EnableMainMenuUI(false);
        
        cameraEffects.SwitchToGameView();

        yield return tileAnimator.GetActiveCoroutine();
        
        tileAnimator.EnableMainSceneObjects(false);
        
        LoadScene(levelName);
    }

    private IEnumerator LoadMainMenuCo()
    {
        CleanUpScene();
        
        ui.EnableInGameUI(false);
        
        cameraEffects.SwitchToMenuView();

        yield return tileAnimator.GetActiveCoroutine();
        UnloadCurrentScene();

        tileAnimator.EnableMainSceneObjects(true);
        tileAnimator.ShowMainGrid(true);

        yield return tileAnimator.GetActiveCoroutine();

        ui.EnableMainMenuUI(true);
    }

    private void LoadScene(string sceneNameToLoad)
    {
        currentLevelName = sceneNameToLoad;
        SceneManager.LoadSceneAsync(sceneNameToLoad, LoadSceneMode.Additive);
    }

    private void UnloadCurrentScene() => SceneManager.UnloadSceneAsync(currentLevelName);

    private void CleanUpScene()
    {
       // WaveManager waveManager = FindFirstObjectByType<WaveManager>();
       // if (waveManager != null)
        //{
        //    waveManager.DeactivateWaveManager();
       // }
        
        EliminateAllEnemies();
        EliminateAllTowers();
        
        if (currentActiveGrid != null) tileAnimator.ShowGrid(currentActiveGrid, false);
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

    public void UpdateCurrentGrid(GridBuilder newGrid) => currentActiveGrid = newGrid;

    public int GetNextLevelIndex() => SceneUtility.GetBuildIndexByScenePath(currentLevelName) + 1;
    public string GetNextLevelName() => "Level_" + GetNextLevelIndex();
    public bool HasNoMoreLevels() => GetNextLevelIndex() >= SceneManager.sceneCountInBuildSettings;
}
