using System;
using UnityEngine;

public class UILevelSelection : MonoBehaviour
{
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
