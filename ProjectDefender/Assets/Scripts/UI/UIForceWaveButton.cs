using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A specialized UIButton that shows the next wave's details on hover.
/// </summary>
public class UIForceWaveButton : UIButton
{
    private UIGame uiGame;
    
    private void Start()
    {
        // Find UIGame in the scene since it's not a parent
        uiGame = FindFirstObjectByType<UIGame>();
        
        if (uiGame == null) Debug.LogWarning("UIForceWaveButton: Could not find UIGame in the scene!");
    }
    
    /// <summary>
    /// Shows the next wave details when the user hovers.
    /// </summary>
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        
        if (uiGame != null) uiGame.EnableNextWaveDetails(true);
    }

    /// <summary>
    /// Hides the next wave details when the user stops hovering.
    /// </summary>
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        
        if (uiGame != null) uiGame.EnableNextWaveDetails(false);
    }
}