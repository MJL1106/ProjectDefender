using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

/// <summary>
/// Manages a single tower build button in the UI.
/// Handles hover, selection, preview spawning, and purchase confirmation.
/// </summary>
public class UIBuildButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
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

    /// <summary>
    /// Instantiates the tower's preview model and caches it.
    /// </summary>
    private void CreateTowerPreview()
    {
        GameObject newPreview = Instantiate(towerToBuild, Vector3.zero, Quaternion.identity);
        
        towerPreview = newPreview.AddComponent<TowerPreview>();
        towerPreview.SetupTowerPreview(newPreview);
        towerPreview.transform.parent = buildManager.transform;
    }

    /// <summary>
    /// Selects or deselects this button, showing/hiding the tower preview on the build slot.
    /// </summary>
    /// <param name="select">True to select and show preview, false to deselect.</param>
    public void SelectButton(bool select)
    {
        if (buildManager == null) 
        {
            Debug.LogWarning($"BuildManager is null in {gameObject.name}");
            return;
        }

        BuildSlot slotToUse = buildManager.GetSelectedSlot();

        if (slotToUse == null) return;
        
        if (towerPreview == null && towerToBuild != null) CreateTowerPreview();

        Vector3 previewPosition = slotToUse.GetBuildPosition(1f);
        
        towerPreview.gameObject.SetActive(select);
        towerPreview.ShowPreview(select, previewPosition);
        onHoverEffect.ShowCaseButton(select);
        buildButtonsHolder.SetLastSelected(this, towerPreview.transform);
    }

    /// <summary>
    /// Checks if this button matches the tower name and sets its unlocked status.
    /// </summary>
    /// <param name="towerNameToCheck">The name of the tower to check against.</param>
    /// <param name="unlockStatus">True to unlock and show, false to lock and hide.</param>
    public void UnlockTowerIfNeeded(string towerNameToCheck, bool unlockStatus)
    {
        if (towerNameToCheck != towerName) return;
    
        buttonUnlocked = unlockStatus;
        gameObject.SetActive(unlockStatus);
    
        // Create preview when unlocking if it doesn't exist
        if (unlockStatus && towerPreview == null && towerToBuild != null) CreateTowerPreview();
    }

    /// <summary>
    /// Finalizes the build action by calling the BuildManager.
    /// </summary>
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

    /// <summary>
    /// Editor-only. Updates text and GameObject name based on properties.
    /// </summary>
    private void OnValidate()
    {
        towerNameText.text = towerName;
        towerPriceText.text = towerPrice + "";
        gameObject.name = "BuildButton_UI - " + towerName;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ConfirmTowerBuild();
    }
}