using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manages the panel containing all UIBuildButtons.
/// Handles showing/hiding, hotkeys (1, 2, 3), and selection state.
/// </summary>
public class UIBuildButtonsHolder : MonoBehaviour
{
    private UIAnimator uiAnim;
    private bool isBuildMenuActive;
    
    [SerializeField] private float openAnimationDuration = 0.1f;

    [SerializeField]
    private float yPositionOffset; // The vertical distance the panel animates

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

    /// <summary>
    /// Checks for numeric hotkeys (1, 2, 3...) to select build buttons.
    /// Also checks for Q/E to rotate the preview.
    /// </summary>
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

    /// <summary>
    /// Rotates a transform (the preview) around the Y-axis.
    /// </summary>
    /// <param name="target">The transform to rotate.</param>
    /// <param name="angle">The angle in degrees to rotate.</param>
    private void RotateTarget(Transform target, float angle)
    {
        if (target == null) return;
        
        target.Rotate(0, angle,0);
        target.GetComponent<ForwardAttackDisplay>()?.UpdateLines();
    }

    /// <summary>
    /// Selects a new build button based on its index in the unlocked list (for hotkeys).
    /// </summary>
    /// <param name="buttonIndex">The index of the button to select (0 = '1', 1 = '2', etc.).</param>
    public void SelectNewButton(int buttonIndex)
    {
        if (buttonIndex >= unlockedBuildButtons.Count) return;

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

    /// <summary>
    /// Caches the last selected button and its preview transform.
    /// </summary>
    /// <param name="newLastSelected">The build button that was selected.</param>
    /// <param name="newPreview">The preview object associated with the button.</param>
    public void SetLastSelected(UIBuildButton newLastSelected, Transform newPreview)
    {
        lastSelectedButton = newLastSelected;
        previewTower = newPreview;
    }

    /// <summary>
    /// Filters the complete list of build buttons to create a list of only the unlocked ones.
    /// </summary>
    public void UpdateUnlockedBuildButtons()
    {
        unlockedBuildButtons = new List<UIBuildButton>();
        
        foreach (var button in buildButtons)
        {
            if (button.buttonUnlocked) unlockedBuildButtons.Add(button);
        }
    }

    /// <summary>
    /// Animates the build button panel in or out.
    /// </summary>
    /// <param name="showButtons">True to animate in, false to animate out.</param>
    public void ShowBuildButtons(bool showButtons)
    {
        isBuildMenuActive = showButtons;

        float yOffset = isBuildMenuActive ? yPositionOffset : -yPositionOffset;
        float methodDelay = isBuildMenuActive ? openAnimationDuration : 0;
        
        uiAnim.ChangePosition(transform, new Vector3(0,yOffset), openAnimationDuration);

        Invoke(nameof(ToggleButtonMovement), methodDelay);
    }

    /// <summary>
    /// Toggles the hover effect movement for all child buttons.
    /// </summary>
    private void ToggleButtonMovement()
    {
        foreach (var button in buildButtonsEffects)
        {
            button.ToggleMovement(isBuildMenuActive);
        }
    }
    
    /// <summary>
    /// Called by BuildManager when a new tile is clicked.
    /// Moves the active preview to the new slot.
    /// </summary>
    /// <param name="newSelectedSlot">The newly selected build slot.</param>
    public void OnTileSelectionChanged(BuildSlot newSelectedSlot)
    {
        if (lastSelectedButton != null && previewTower != null && newSelectedSlot != null)
        {
            Vector3 previewPosition = newSelectedSlot.GetBuildPosition(1f);

            previewTower.position = previewPosition;
        
            TowerPreview towerPreview = previewTower.GetComponent<TowerPreview>();
            if (towerPreview != null)
            {
                towerPreview.ShowPreview(true, previewPosition);
            }
        }
    }
}