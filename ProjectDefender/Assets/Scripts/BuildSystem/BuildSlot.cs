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
    
    private bool isSelected = false;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tileAnim.IsGridMoving()) return;
        
        if (isSelected) return;
        
        bool hasTower = buildManager.HasTowerOnSlot(this);
        
        if (outline != null)
        {
            if (hasTower)
            {
                outline.OutlineColor = buildableColor;
            }
            else
            {
                outline.OutlineColor = buildSlotAvailable ? buildableColor : unbuildableColor;
            }
            outline.enabled = true;
        }
        
        if (tileCanBeMoved && !hasTower)
        {
            MoveTileUp();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tileAnim.IsGridMoving()) return;
        
        if (isSelected) return;
        
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

        if (buildManager.GetSelectedSlot() == this) return;
        
        if (buildManager.GetSelectedSlot() != null && buildManager.GetSelectedSlot() != this)
        {
            buildManager.GetSelectedSlot().UnselectTile();
        }
        
        bool hasTower = buildManager.HasTowerOnSlot(this);
        
        if (!hasTower)
        {
            SnapToBeforeBuildPosition();
        }
        
        buildManager.SelectBuildSlot(this);
        
        isSelected = true;
        if (outline != null)
        {
            outline.OutlineColor = hasTower ? buildableColor : (buildSlotAvailable ? buildableColor : unbuildableColor);
            outline.enabled = true;
        }
        
        if (!hasTower)
        {
            MoveTileUp();
        }
        
        if (hasTower)
        {
            buildManager.DisableBuildMenu();
            ui.BuildButtonsHolderUI.GetLastSelected()?.SelectButton(false);
            buildManager.EnableSellMenu();
        }
        else if (buildSlotAvailable)
        {
            buildManager.DisableSellMenu();
            buildManager.EnableBuildMenu();
            ui.BuildButtonsHolderUI.GetLastSelected()?.SelectButton(true);
        }
        else
        {
            buildManager.CancelBuildAction();
            return;
        }

        tileCanBeMoved = false;
    }

    public void UnselectTile()
    {
        isSelected = false;
        
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