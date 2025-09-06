using System;
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

    private void Awake()
    {
        uiAnim = GetComponentInParent<UIAnimator>();
        buildButtonsEffects = GetComponentsInChildren<UIBuildButtonOnHoverEffect>();
        buildButtons = GetComponentsInChildren<UIBuildButton>();
    }

    public UIBuildButton[] GetBuildButtons() => buildButtons;

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
