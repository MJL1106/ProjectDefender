using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private TileAnimator tileAnim;
    private Outline outline;
    private Vector3 defaultPosition;
    private BuildManager buildManager;

    private bool tileCanBeMoved = true;
    private bool buildSlotAvailable = true;
    
    private GameObject builtTower;
    private int towerOriginalPrice;

    private Coroutine currentMovementUpCo;
    private Coroutine moveToDefaultCo;

    // Colors for different states
    private Color buildableColor = Color.white;
    private Color unbuildableColor = new Color(1f, 0.3f, 0.3f);

    private void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        tileAnim = FindFirstObjectByType<TileAnimator>();
        buildManager = FindFirstObjectByType<BuildManager>();
        defaultPosition = transform.position;
        
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.OutlineMode = Outline.Mode.VisibleEdgesOnly; 
        
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = 2.0f;
        outline.enabled = false;
    }

    public void SetSlotAvailableTo(bool value) => buildSlotAvailable = value;

    public void SetBuiltTower(GameObject tower, int price)
    {
        builtTower = tower;
        towerOriginalPrice = price;
    }

    public bool HasTower() => builtTower != null;

    public int GetTowerSellValue() => Mathf.RoundToInt(towerOriginalPrice * 0.5f);

    public void RemoveTower()
    {
        if (builtTower != null)
        {
            Destroy(builtTower);
            builtTower = null;
            towerOriginalPrice = 0;
            buildSlotAvailable = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tileAnim.IsGridMoving()) return;
        
        if (outline != null)
        {
            outline.OutlineColor = buildSlotAvailable ? buildableColor : unbuildableColor;
            outline.enabled = true;
        }
        
        if (tileCanBeMoved)
        {
            MoveTileUp();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tileAnim.IsGridMoving()) return;
        
        if (outline != null)
        {
            outline.enabled = false;
        }
        
        if (tileCanBeMoved == false) return;

        if (currentMovementUpCo != null) Invoke(nameof(MoveToDefaultPosition), tileAnim.GetTravelDuration());
        else MoveToDefaultPosition();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (tileAnim.IsGridMoving()) return;
    
        if (eventData.button != PointerEventData.InputButton.Left) return;

        Debug.Log($"BuildSlot clicked! buildSlotAvailable: {buildSlotAvailable}");
    
        // If clicking a different slot, just deselect the previous one visually
        if (buildManager.GetSelectedSlot() != null && buildManager.GetSelectedSlot() != this)
        {
            buildManager.GetSelectedSlot().UnselectTile();
        }
    
        SnapToBeforeBuildPosition();
        buildManager.SelectBuildSlot(this);
        MoveTileUp();

        // Check if tower exists on this slot, show appropriate menu
        bool hasTower = buildManager.HasTowerOnSlot(this);
        Debug.Log($"Has tower on slot: {hasTower}");
    
        if (hasTower)
        {
            Debug.Log("Enabling sell menu");
            buildManager.DisableBuildMenu(); // Hide build menu if it was showing
            buildManager.EnableSellMenu();
        }
        else if (buildSlotAvailable)
        {
            Debug.Log("Enabling build menu");
            buildManager.DisableSellMenu(); // Hide sell menu if it was showing
            buildManager.EnableBuildMenu();
            ui.BuildButtonsHolderUI.GetLastSelected()?.SelectButton(true);
        }
        else
        {
            Debug.Log("Slot not available and no tower");
        }

        tileCanBeMoved = false;
    }

    public void UnselectTile()
    {
        if (outline != null) outline.enabled = false;
        
        MoveToDefaultPosition();
        tileCanBeMoved = true;
    }

    private void MoveTileUp()
    {
        Vector3 targetPosition = transform.position + new Vector3(0, tileAnim.GetBuildOffset(), 0);
        currentMovementUpCo = StartCoroutine(tileAnim.MoveTileCo(transform,targetPosition));
    }

    private void MoveToDefaultPosition()
    {
        moveToDefaultCo = StartCoroutine(tileAnim.MoveTileCo(transform,defaultPosition));
    }

    public void SnapToDefaultPosition()
    {
        if (moveToDefaultCo != null) StopCoroutine(moveToDefaultCo);

        transform.position = defaultPosition;
    }

    public void SnapToBeforeBuildPosition()
    {
        Vector3 targetPosition = defaultPosition + new Vector3(0, tileAnim.GetBuildOffset(), 0);
        transform.position = targetPosition;
    }

    public Vector3 GetBuildPosition(float yOffset) => defaultPosition + new Vector3(0,yOffset);
}