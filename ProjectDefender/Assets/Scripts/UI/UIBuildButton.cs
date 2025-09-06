using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UIBuildButton : MonoBehaviour
{
    private BuildManager buildManager;
    private CameraEffects cameraEffects;
    private GameManager gameManager;

    [SerializeField] private string towerName;
    [FormerlySerializedAs("price")] [SerializeField] private int towerPrice = 50;
    [Space]
    [SerializeField] private GameObject towerToBuild;
    [SerializeField] private float towerCentreY = .5f;
    
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerPriceText;
    

    private void Awake()
    {
        buildManager = FindFirstObjectByType<BuildManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void UnlockTowerIfNeeded(string towerNameToCheck, bool unlockStatus)
    {
        if (towerNameToCheck != towerName) return;
        
        gameObject.SetActive(unlockStatus);
    }

    public void BuildTower()
    {
        if (gameManager.HasEnoughCurrency(towerPrice) == false) return;
        
        if (towerToBuild == null)
        {
            Debug.LogWarning("YOu did not assign a tower to this button!");
            return;
        }
        
        BuildSlot slotToUse = buildManager.GetSelectedSlot();
        buildManager.CancelBuildAction();
        
        slotToUse.SnapToDefaultPosition();
        slotToUse.SetSlotAvailableTo(false);
        
        cameraEffects.ScreenShake(.15f, .02f);

        GameObject newTower = Instantiate(towerToBuild,slotToUse.GetBuildPosition(towerCentreY), Quaternion.identity);
    }

    private void OnValidate()
    {
        towerNameText.text = towerName;
        towerPriceText.text = towerPrice + "";
        gameObject.name = "BuildButton_UI - " + towerName;
    }
}
