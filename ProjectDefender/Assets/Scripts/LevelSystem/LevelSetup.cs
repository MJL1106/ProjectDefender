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
    
    [Header("Level Details")]
    [SerializeField] private List<TowerUnlockData> towersUnlocked;
    [SerializeField] private int levelCurrency = 1000;

    [Header("Level Setup")] [SerializeField]
    private GridBuilder myMainGrid;
    [SerializeField] private List<GameObject> extraObjectsToDelete = new List<GameObject>();
    [SerializeField] private WaveManager myWaveManager;
    [SerializeField] private Material groundMaterial;
    
    private IEnumerator Start()
    {
        if (LevelWasLoadedToMainScene())
        {
            DeleteExtraObjects();

            buildManager = FindFirstObjectByType<BuildManager>();
            buildManager.UpdateBuildManager(myWaveManager, myMainGrid);
            
            levelManager.UpdateCurrentGrid(myMainGrid);
            levelManager.UpdateBackgroundColor(groundMaterial.color);

            tileAnimator = FindFirstObjectByType<TileAnimator>();
            tileAnimator.ShowGrid(myMainGrid, true);

            yield return tileAnimator.GetActiveCoroutine();

            ui = FindFirstObjectByType<UI>();
            ui.EnableInGameUI(true);

            gameManager = FindFirstObjectByType<GameManager>();
            gameManager.PrepareLevel(levelCurrency, myWaveManager);
        } 
        else
        {
            yield return new WaitForEndOfFrame();
            UnlockAvailableTowers();
        }
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
        UI ui = FindFirstObjectByType<UI>();

        foreach (var unlockData in towersUnlocked)
        {
            foreach (var buildButton in ui.BuildButtonsHolderUI.GetBuildButtons())
            {
                buildButton.UnlockTowerIfNeeded(unlockData.towerName, unlockData.unlocked);
            }
        }
        
        ui.BuildButtonsHolderUI.UpdateUnlockedBuildButtons();
    }

    public WaveManager GetWaveManager() => myWaveManager;

    [ContextMenu("Initialize Tower Data")]
    private void InitializeTowerData()
    {
        towersUnlocked.Clear();
        
        towersUnlocked.Add(new TowerUnlockData("Crossbow",false));
        towersUnlocked.Add(new TowerUnlockData("Cannon",false));
        towersUnlocked.Add(new TowerUnlockData("Rapid Fire Gun",false));
        towersUnlocked.Add(new TowerUnlockData("Hammer",false));
        towersUnlocked.Add(new TowerUnlockData("Spider Nest",false));
        towersUnlocked.Add(new TowerUnlockData("Anti-air Harpoon",false));
        towersUnlocked.Add(new TowerUnlockData("Just Fan",false));
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
