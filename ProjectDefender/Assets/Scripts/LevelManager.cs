using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private TileAnimator tileAnimator;
    private UI ui;
    
    private GridBuilder currentActiveGrid;
    private string currentLevelName;
    
    private void Awake()
    {
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
    public void LoadLevelFromMenu(string levelName) => StartCoroutine(LoadLevelFromMenuCo(levelName));

    public void LoadMainMenu() => StartCoroutine(LoadMainMenuCo());

    private IEnumerator LoadLevelCo(string levelName)
    {
        CleanUpScene();
        ui.EnableInGameUI(false);
        
        yield return tileAnimator.GetActiveCoroutine();
        
        UnloadCurrentScene();
        LoadScene(levelName);
    }

    private IEnumerator LoadLevelFromMenuCo(string levelName)
    {
        tileAnimator.ShowMainGrid(false);
        ui.EnableMainMenuUI(false);

        yield return tileAnimator.GetActiveCoroutine();
        
        tileAnimator.EnableMainSceneObjects(false);
        
        LoadScene(levelName);
    }

    private IEnumerator LoadMainMenuCo()
    {
        CleanUpScene();
        
        ui.EnableInGameUI(false);

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
        EliminateAllEnemies();
        EliminateAllTowers();
        
        if (currentActiveGrid != null) tileAnimator.ShowGrid(currentActiveGrid, false);
    }

    private void EliminateAllEnemies()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            enemy.DestroyEnemy();
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
}
