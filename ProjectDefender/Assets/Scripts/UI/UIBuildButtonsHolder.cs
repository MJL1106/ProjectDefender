using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIBuildButtonsHolder : MonoBehaviour
{
    private UIAnimator uiAnim;
    private bool isBuildMenuActive;
    
    [SerializeField] private float openAnimationDuration = 0.1f;

    [SerializeField]
    private float yPositionOffset;

    private UIBuildButtonOnHoverEffect[] buildButtonsEffects;
    private UIBuildButton[] buildButtons;
    
    private List<UIBuildButton> unlockedBuildButtons;
    private UIBuildButton lastSelectedButton;

    private void Awake()
    {
        uiAnim = GetComponentInParent<UIAnimator>();
        buildButtonsEffects = GetComponentsInChildren<UIBuildButtonOnHoverEffect>();
        buildButtons = GetComponentsInChildren<UIBuildButton>();
    }

    private void Update()
    {
        CheckBuildButtonsHotKeys();
    }

    private void CheckBuildButtonsHotKeys()
    {
        for (int i = 0; i < unlockedBuildButtons.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectNewButton(i);
                break;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && lastSelectedButton != null)
        {
            lastSelectedButton.BuildTower();
        }
    }

    public void SelectNewButton(int buttonIndex)
    {
        if (buttonIndex >= unlockedBuildButtons.Count)
        {
            return;
        }

        foreach (var button in unlockedBuildButtons)
        {
            button.SelectButton(false);
        }

        UIBuildButton selectedButton = unlockedBuildButtons[buttonIndex];

        selectedButton.SelectButton(true);
    }

    public UIBuildButton[] GetBuildButtons() => buildButtons;
    
    public List<UIBuildButton> GetUnlockedBuildButtons() => unlockedBuildButtons;
    
    public UIBuildButton GetLastSelected() => lastSelectedButton;
    
    public void SetLastSelected(UIBuildButton newLastSelected) => lastSelectedButton = newLastSelected;

    public void UpdateUnlockedBuildButtons()
    {
        unlockedBuildButtons = new List<UIBuildButton>();
        
        foreach (var button in buildButtons)
        {
            if (button.buttonUnlocked) unlockedBuildButtons.Add(button);
        }
    }

    public void ShowBuildButtons(bool showButtons)
    {
        isBuildMenuActive = showButtons;

        float yOffset = isBuildMenuActive ? yPositionOffset : -yPositionOffset;
        float methodDelay = isBuildMenuActive ? openAnimationDuration : 0;
        
        uiAnim.ChangePosition(transform, new Vector3(0,yOffset), openAnimationDuration);
        
        Invoke(nameof(ToggleButtonMovement), methodDelay);
    }

    private void ToggleButtonMovement()
    {
        foreach (var button in buildButtonsEffects)
        {
            button.ToggleMovement(isBuildMenuActive);
        }
    }
}
