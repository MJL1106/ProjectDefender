using UnityEngine;
using UnityEngine.EventSystems;

public class UIForceWaveButton : UIButton
{
    private UIGame uiGame;
    
    private void Start()
    {
        // Find UIGame in the scene since it's not a parent
        uiGame = FindFirstObjectByType<UIGame>();
        
        if (uiGame == null)
        {
            Debug.LogWarning("UIForceWaveButton: Could not find UIGame in the scene!");
        }
    }
    
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        
        if (uiGame != null)
        {
            uiGame.EnableNextWaveDetails(true);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        
        if (uiGame != null)
        {
            uiGame.EnableNextWaveDetails(false);
        }
    }
}
