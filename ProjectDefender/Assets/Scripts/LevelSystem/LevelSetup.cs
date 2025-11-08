using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runs at the start of a level scene to configure it.
/// Loads LevelData, sets up the grid, deletes menu objects, and prepares managers.
/// </summary>
public class LevelSetup : MonoBehaviour
{
    private UI ui;
    private TileAnimator tileAnimator;
    private LevelManager levelManager;
    private GameManager gameManager;
    private BuildManager buildManager;
    
    [Header("Level Setup")] 
    [SerializeField] private GridBuilder myMainGrid; // The grid/map for this specific level
    [SerializeField] private List<GameObject> extraObjectsToDelete; // Objects from menu/other scenes to remove
    [SerializeField] private WaveManager myWaveManager; // The wave manager for this level

    private LevelData myLevelData;
    
    /// <summary>
    /// Initializes the level by loading data, setting up managers, and animating the grid.
    /// </summary>
    private IEnumerator Start()
    {
        if (LevelWasLoadedToMainScene())
        {
            string sceneName = gameObject.scene.name;
            myLevelData = Resources.Load<LevelData>("LevelData/" + sceneName);
            
            if (myLevelData == null)
            {
                 Debug.LogError("FATAL: Could not find LevelData for this scene: " + sceneName);
                 yield break;
            }

            DeleteExtraObjects();

            buildManager = FindFirstObjectByType<BuildManager>();
            buildManager.UpdateBuildManager(myWaveManager, myMainGrid);
            
            levelManager.UpdateCurrentGrid(myMainGrid);

            tileAnimator = FindFirstObjectByType<TileAnimator>();
            tileAnimator.ShowGrid(myMainGrid, true);

            yield return tileAnimator.GetActiveCoroutine();

            ui = FindFirstObjectByType<UI>();
            ui.EnableInGameUI(true);

            gameManager = FindFirstObjectByType<GameManager>();
            gameManager.PrepareLevel(myLevelData.levelCurrency, myWaveManager);
        } 
        
        UnlockAvailableTowers();
    }

    /// <summary>
    /// Checks if the LevelManager exists, indicating this is a level scene
    /// loaded from the main scene, not a standalone test.
    /// </summary>
    private bool LevelWasLoadedToMainScene()
    {
        levelManager = FindFirstObjectByType<LevelManager>();

        return levelManager != null;
    }

    /// <summary>
    /// Removes specified GameObjects that are not part of the level.
    /// e.g., Menu UI, level selection grid, etc.
    /// </summary>
    private void DeleteExtraObjects()
    {
        foreach (var obj in extraObjectsToDelete)
        {
            Destroy(obj);
        }
    }

    /// <summary>
    /// Reads the LevelData to unlock the correct build buttons in the UI.
    /// </summary>
    private void UnlockAvailableTowers()
    {
        if (myLevelData == null)
        {
            string sceneName = gameObject.scene.name;
            myLevelData = Resources.Load<LevelData>("LevelData/" + sceneName);
            if (myLevelData == null) return; 
        }

        UI ui = FindFirstObjectByType<UI>();

        foreach (var unlockData in myLevelData.towersUnlocked)
        {
            foreach (var buildButton in ui.BuildButtonsHolderUI.GetBuildButtons())
            {
                buildButton.UnlockTowerIfNeeded(unlockData.towerName, unlockData.unlocked);
            }
        }
        
        ui.BuildButtonsHolderUI.UpdateUnlockedBuildButtons();
    }
}