using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private TileAnimator tileAnimator;
    private UI ui;
    
    private GridBuilder currentActiveGrid;
    private string currentSceneName;
    
    private void Awake()
    {
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        ui = FindFirstObjectByType<UI>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) StartCoroutine(LoadLevelCo());
        if (Input.GetKeyDown(KeyCode.K)) StartCoroutine(LoadMainMenuCo());
    }

    private IEnumerator LoadLevelCo()
    {
        tileAnimator.ShowMainGrid(false);
        ui.EnableMainMenuUI(false);

        yield return tileAnimator.GetActiveCoroutine();
        
        tileAnimator.EnableMainSceneObjects(false);

        currentSceneName = "Level_1";
        LoadScene("Level_1");
    }

    private IEnumerator LoadMainMenuCo()
    {
        EliminateAllEnemies();
        EliminateAllTowers();
        
        tileAnimator.ShowGrid(currentActiveGrid, false);
        ui.EnableInGameUI(false);

        yield return tileAnimator.GetActiveCoroutine();
        UnloadCurrentScene();

        tileAnimator.EnableMainSceneObjects(true);
        tileAnimator.ShowMainGrid(true);

        yield return tileAnimator.GetActiveCoroutine();

        ui.EnableMainMenuUI(true);
    }

    private void LoadScene(string sceneNameToLoad) =>
        SceneManager.LoadSceneAsync(sceneNameToLoad, LoadSceneMode.Additive);

    private void UnloadCurrentScene() => SceneManager.UnloadSceneAsync(currentSceneName);

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
