using System;
using UnityEngine;

/// <summary>
/// Manages the state of the level selection grid.
/// Enables or disables clickability on all level buttons.
/// </summary>
public class UILevelSelection : MonoBehaviour
{
    /// <summary>
    /// Finds all level buttons and enables/disables their clickability.
    /// </summary>
    /// <param name="canClick">True to make buttons clickable, false to disable them.</param>
    private void MakeButtonsClickable(bool canClick)
    {
        LevelButtonTile[] levelButtons = FindObjectsByType<LevelButtonTile>(FindObjectsSortMode.None);

        foreach (var btn in levelButtons)
        {
            btn.CheckIfLevelUnlocked();
            btn.EnableCLickOnButton(canClick);
        }
    }

    private void OnEnable()
    {
        MakeButtonsClickable(true);
    }

    private void OnDisable()
    {
        MakeButtonsClickable(false);
    }
}