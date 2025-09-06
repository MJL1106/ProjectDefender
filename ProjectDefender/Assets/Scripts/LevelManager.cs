using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public List<TowerUnlockData> towersUnlocked;


    private void Start()
    {
        UnlockAvailableTowers();
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
    }

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
