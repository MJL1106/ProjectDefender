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

    public UIBuildButton[] GetBuildButtons() => buildButtons;
    public List<UIBuildButton> GetUnlockedBuildButtons() => unlockedBuildButtons;
    public void SetLastSelected(UIBuildButton newLastSelected) => lastSelectedButton = newLastSelected;
    public UIBuildButton GetLastSelected() => lastSelectedButton;

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
