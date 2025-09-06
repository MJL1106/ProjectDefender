using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class UIBuildButton : MonoBehaviour, IPointerEnterHandler
{
    private UI ui;
    private BuildManager buildManager;
    private CameraEffects cameraEffects;
    private GameManager gameManager;
    private UIBuildButtonsHolder buildButtonsHolder;
    private UIBuildButtonOnHoverEffect onHoverEffect;

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
    
    private TowerPreview towerPreview;
    
    public bool buttonUnlocked { get; private set; }
    
    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        onHoverEffect = GetComponent<UIBuildButtonOnHoverEffect>();
        buildButtonsHolder = GetComponentInParent<UIBuildButtonsHolder>();
        
        buildManager = FindFirstObjectByType<BuildManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        CreateTowerPreview();
    }

    private void CreateTowerPreview()
    {
        GameObject newPreview = Instantiate(towerToBuild, Vector3.zero, Quaternion.identity);
        
        towerPreview = newPreview.AddComponent<TowerPreview>();
        towerPreview.gameObject.SetActive(false);
    }

    public void SelectButton(bool select)
    {
        if (buildManager == null) 
        {
            Debug.LogWarning($"BuildManager is null in {gameObject.name}");
            return;
        }

        BuildSlot slotToUse = buildManager.GetSelectedSlot();

        if (slotToUse == null) return;
        
        if (towerPreview == null && towerToBuild != null)
        {
            CreateTowerPreview();
        }

        Vector3 previewPosition = slotToUse.GetBuildPosition(1);
        towerPreview.gameObject.SetActive(select);
        towerPreview.ShowPreview(select, previewPosition);
        onHoverEffect.ShowCaseButton(select);
        buildButtonsHolder.SetLastSelected(this);
    }

    public void UnlockTowerIfNeeded(string towerNameToCheck, bool unlockStatus)
    {
        if (towerNameToCheck != towerName) return;
    
        buttonUnlocked = unlockStatus;
        gameObject.SetActive(unlockStatus);
    
        // Create preview when unlocking if it doesn't exist
        if (unlockStatus && towerPreview == null && towerToBuild != null)
        {
            CreateTowerPreview();
        }
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

        if (ui.BuildButtonsHolderUI.GetLastSelected() == null)
        {
            return;
        }
        
        BuildSlot slotToUse = buildManager.GetSelectedSlot();
        buildManager.CancelBuildAction();
        
        slotToUse.SnapToDefaultPosition();
        slotToUse.SetSlotAvailableTo(false);
        
        ui.BuildButtonsHolderUI.SetLastSelected(null);
        
        cameraEffects.ScreenShake(.15f, .02f);

        GameObject newTower = Instantiate(towerToBuild,slotToUse.GetBuildPosition(towerCentreY), Quaternion.identity);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buildButtonsHolder == null || buildManager == null) 
        {
            Debug.LogWarning("References not initialized yet in OnPointerEnter");
            return;
        }
    
        foreach (var button in buildButtonsHolder.GetBuildButtons())
        {
            button.SelectButton(false);
        }
    
        SelectButton(true);
    }

    private void OnValidate()
    {
        towerNameText.text = towerName;
        towerPriceText.text = towerPrice + "";
        gameObject.name = "BuildButton_UI - " + towerName;
    }

}
