using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages individual build slots where towers can be placed.
/// Handles hover effects, selection states, and visual feedback for buildable/unbuildable slots.
/// Integrates with BuildManager to coordinate tower placement across the grid.
/// </summary>
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

    // Visual feedback colors
    private Color buildableColor = Color.white;
    private Color unbuildableColor = new Color(1f, 0.3f, 0.3f);

    private void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        tileAnim = FindFirstObjectByType<TileAnimator>();
        buildManager = FindFirstObjectByType<BuildManager>();
        defaultPosition = transform.position;
        
        // Setup outline component for hover/select visual feedback
        if (outline == null) outline = gameObject.AddComponent<Outline>();
        
        outline.OutlineMode = Outline.Mode.VisibleEdgesOnly; 
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = 2.0f;
        outline.enabled = false;
    }

    /// <summary>
    /// Sets whether this slot can have towers built on it.
    /// Used to mark slots as restricted (e.g., path tiles, spawn points).
    /// </summary>
    public void SetSlotAvailableTo(bool value) => buildSlotAvailable = value;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tileAnim.IsGridMoving()) return;
        if (isSelected) return;
        
        bool hasTower = buildManager.HasTowerOnSlot(this);
        
        // Show appropriate outline color based on slot state
        if (outline != null)
        {
            if (hasTower) outline.OutlineColor = buildableColor;
            else outline.OutlineColor = buildSlotAvailable ? buildableColor : unbuildableColor;
            
            outline.enabled = true;
        }
        
        // Lift tile on hover if no tower is present
        if (tileCanBeMoved && !hasTower) MoveTileUp();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tileAnim.IsGridMoving()) return;
        if (isSelected) return;
        if (outline != null) outline.enabled = false;
        if (tileCanBeMoved == false) return;

        // Delay return to default if tile is still animating up
        if (currentMovementUpCo != null) Invoke(nameof(MoveToDefaultPosition), tileAnim.GetTravelDuration());
        else MoveToDefaultPosition();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (tileAnim.IsGridMoving()) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (buildManager.GetSelectedSlot() == this) return;
        
        // Deselect previously selected slot
        if (buildManager.GetSelectedSlot() != null && buildManager.GetSelectedSlot() != this)
        {
            buildManager.GetSelectedSlot().UnselectTile();
        }
        
        bool hasTower = buildManager.HasTowerOnSlot(this);
        
        if (!hasTower) SnapToBeforeBuildPosition();
        
        buildManager.SelectBuildSlot(this);
        
        isSelected = true;
        if (outline != null)
        {
            outline.OutlineColor = hasTower ? buildableColor : (buildSlotAvailable ? buildableColor : unbuildableColor);
            outline.enabled = true;
        }
        
        if (!hasTower) MoveTileUp();
        
        // Show appropriate UI menu based on slot state
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

    /// <summary>
    /// Deselects this tile and returns it to default state.
    /// Called when another slot is selected or build action is cancelled.
    /// </summary>
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
        currentMovementUpCo = StartCoroutine(tileAnim.MoveTileCo(transform, targetPosition));
    }

    private void MoveToDefaultPosition()
    {
        moveToDefaultCo = StartCoroutine(tileAnim.MoveTileCo(transform, defaultPosition));
    }

    /// <summary>
    /// Instantly moves tile to default position without animation.
    /// Used when grid is being moved or reset.
    /// </summary>
    public void SnapToDefaultPosition()
    {
        if (moveToDefaultCo != null) StopCoroutine(moveToDefaultCo);
        transform.position = defaultPosition;
    }

    /// <summary>
    /// Instantly moves tile to pre-build raised position.
    /// Used when selecting an empty slot for building.
    /// </summary>
    public void SnapToBeforeBuildPosition()
    {
        Vector3 targetPosition = defaultPosition + new Vector3(0, tileAnim.GetBuildOffset(), 0);
        transform.position = targetPosition;
    }

    /// <summary>
    /// Gets the world position where a tower should be spawned on this slot.
    /// </summary>
    /// <param name="yOffset">Additional height offset for tower placement</param>
    /// <returns>World position for tower spawn</returns>
    public Vector3 GetBuildPosition(float yOffset) => defaultPosition + new Vector3(0, yOffset);
}