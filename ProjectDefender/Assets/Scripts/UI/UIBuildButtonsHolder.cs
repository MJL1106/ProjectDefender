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

    private Transform previewTower;
    
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
        if (isBuildMenuActive == false) return;
        
        for (int i = 0; i < unlockedBuildButtons.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectNewButton(i);
                break;
            }
        }

        if (lastSelectedButton != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                lastSelectedButton.ConfirmTowerBuild();
                previewTower = null;
            }
          
          if (Input.GetKeyDown(KeyCode.Q)) RotateTarget(previewTower, -90);
          if (Input.GetKeyDown(KeyCode.E)) RotateTarget(previewTower, 90);
        }
    }

    private void RotateTarget(Transform target, float angle)
    {
        if (target == null) return;
        
        target.Rotate(0, angle,0);
        target.GetComponent<ForwardAttackDisplay>()?.UpdateLines();
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

    public void SetLastSelected(UIBuildButton newLastSelected, Transform newPreview)
    {
        lastSelectedButton = newLastSelected;
        previewTower = newPreview;
    }

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
    
    public void OnTileSelectionChanged(BuildSlot newSelectedSlot)
    {
        // If we have a selected button with a preview, update its position
        if (lastSelectedButton != null && previewTower != null && newSelectedSlot != null)
        {
            Vector3 previewPosition = newSelectedSlot.GetBuildPosition(1f); // Use same offset as in SelectButton
            previewTower.position = previewPosition;
        
            // Update the preview component if it exists
            TowerPreview towerPreview = previewTower.GetComponent<TowerPreview>();
            if (towerPreview != null)
            {
                towerPreview.ShowPreview(true, previewPosition);
            }
        }
    }
}
