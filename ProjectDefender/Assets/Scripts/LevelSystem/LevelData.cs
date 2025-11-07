using UnityEngine;
using System.Collections.Generic;

// This "CreateAssetMenu" line lets you create these in the Project folder
[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Details")]
    public Material groundMaterial;
    public int levelCurrency = 1000;

    [Header("Towers")]
    public List<TowerUnlockData> towersUnlocked;
}