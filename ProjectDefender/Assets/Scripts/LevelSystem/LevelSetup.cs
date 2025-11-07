using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSetup : MonoBehaviour
{
private UI ui;
    private TileAnimator tileAnimator;
    private LevelManager levelManager;
    private GameManager gameManager;
    private BuildManager buildManager;
    
    [Header("Level Setup")] [SerializeField]
    private GridBuilder myMainGrid;
    [SerializeField] private List<GameObject> extraObjectsToDelete = new List<GameObject>();
    [SerializeField] private WaveManager myWaveManager;

    private LevelData myLevelData;
    
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

            // The line for UpdateBackgroundColor is removed
            // as LevelManager now handles this *before* loading.

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

    private bool LevelWasLoadedToMainScene()
    {
        levelManager = FindFirstObjectByType<LevelManager>();

        return levelManager != null;
    }

    private void DeleteExtraObjects()
    {
        foreach (var obj in extraObjectsToDelete)
        {
            Destroy(obj);
        }
    }

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

    [ContextMenu("Initialize Tower Data")]
    private void InitializeTowerData()
    {
        // This is now handled by your LevelData assets
    }
}

[System.Serializable]
public class TowerUnlockData
{
    public string towerName;
    public bool unlocked;

    public TowerUnlockData(string newTowerName, bool newUnlockedStatus)
    {
        towerName = newTowerName;
        unlocked = newUnlockedStatus;
    }
}
