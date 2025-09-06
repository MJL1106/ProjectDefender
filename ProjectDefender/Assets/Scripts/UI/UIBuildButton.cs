using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class UIBuildButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UI ui;
    private BuildManager buildManager;
    private CameraEffects cameraEffects;
    private GameManager gameManager;
    private TowerAttackRadiusDisplay towerAttackRadiusDisplay;

    [SerializeField] private string towerName;
    [FormerlySerializedAs("price")] [SerializeField] private int towerPrice = 50;
    [Space]
    [SerializeField] private GameObject towerToBuild;
    [SerializeField] private float towerCentreY = .5f;
    
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerPriceText;

    [Header("Tower Details")] [SerializeField]
    private float towerAttackRadius = 3;
    

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        buildManager = FindFirstObjectByType<BuildManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        gameManager = FindFirstObjectByType<GameManager>();

        towerAttackRadiusDisplay = FindFirstObjectByType<TowerAttackRadiusDisplay>(FindObjectsInactive.Include);

        if (towerToBuild != null) towerAttackRadius = towerToBuild.GetComponent<Tower>().GetAttackRadius();
    }

    public void UnlockTowerIfNeeded(string towerNameToCheck, bool unlockStatus)
    {
        if (towerNameToCheck != towerName) return;
        
        gameObject.SetActive(unlockStatus);
    }

    public void BuildTower()
    {
        if (gameManager.HasEnoughCurrency(towerPrice) == false)
        {
            ui.inGameUI.ShakeCurrencyUI();
            return;
        }
        
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        BuildSlot slotToUse = buildManager.GetSelectedSlot();
        
        towerAttackRadiusDisplay.ShowAttackRadius(true, towerAttackRadius,slotToUse.GetBuildPosition(.5f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        towerAttackRadiusDisplay.ShowAttackRadius(false, towerAttackRadius,Vector3.zero);
    }
}
