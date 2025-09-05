using System;
using UnityEngine;

public class UIBuildButton : MonoBehaviour
{
    private BuildManager buildManager;
    private CameraEffects cameraEffects;
    private GameManager gameManager;
    
    [SerializeField] private int price = 50;
    [SerializeField] private GameObject towerToBuild;
    [SerializeField] private float towerCentreY = .5f;

    private void Awake()
    {
        buildManager = FindFirstObjectByType<BuildManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void BuildTower()
    {
        if (gameManager.HasEnoughCurrency(price) == false) return;
        
        if (towerToBuild == null)
        {
            Debug.LogWarning("YOu did not assign a tower to this button!");
            return;
        }
        
        BuildSlot slotToUse = buildManager.GetSelectedSlot();
        buildManager.CancelBuildAction();
        slotToUse.SnapToDefaultPosition();
        cameraEffects.ScreenShake(.15f, .02f);

        GameObject newTower = Instantiate(towerToBuild,slotToUse.GetBuildPosition(towerCentreY), Quaternion.identity);
    }
}
