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
    private UIBuildButtonsHolder buildButtonsHolder;
    private UIBuildButtonOnHoverEffect onHoverEffect;

    [SerializeField] private string towerName;
    [FormerlySerializedAs("price")] [SerializeField] private int towerPrice = 50;
    [Space]
    [SerializeField] private GameObject towerToBuild;
    
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerPriceText;
    
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
        towerPreview.SetupTowerPreview(newPreview);
        towerPreview.transform.parent = buildManager.transform;
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

        Vector3 previewPosition = slotToUse.GetBuildPosition(1f);
        
        towerPreview.gameObject.SetActive(select);
        towerPreview.ShowPreview(select, previewPosition);
        onHoverEffect.ShowCaseButton(select);
        buildButtonsHolder.SetLastSelected(this, towerPreview.transform);
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

    public void ConfirmTowerBuild()
    {
        buildManager.BuildTower(towerToBuild, towerPrice, towerPreview.transform);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        buildManager.MouseOverUI(true);
    
        foreach (var button in buildButtonsHolder.GetUnlockedBuildButtons())
        {
            button.SelectButton(false);
        }
    
        SelectButton(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        buildManager.MouseOverUI(false);
    }

    private void OnValidate()
    {
        towerNameText.text = towerName;
        towerPriceText.text = towerPrice + "";
        gameObject.name = "BuildButton_UI - " + towerName;
    }

}
