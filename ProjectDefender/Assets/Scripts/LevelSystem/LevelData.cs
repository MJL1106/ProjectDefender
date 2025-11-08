using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject defining the settings for a specific level.
/// Stores initial currency, ground appearance, and available towers.
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Details")]
    public Material groundMaterial;
    public int levelCurrency = 1000;

    [Header("Towers")]
    public List<TowerUnlockData> towersUnlocked;
}

/// <summary>
/// A simple data class holding the unlock status for a tower by name.
/// </summary>
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