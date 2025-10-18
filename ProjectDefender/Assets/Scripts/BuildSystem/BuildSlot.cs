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

    private Coroutine currentMovementUpCo;
    private Coroutine moveToDefaultCo;

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

    // Enable if want tiles to be raised to show upcoming grid to the player
    private void Start()
    {
       // if (buildSlotAvailable == false)
        //{
         //   transform.position += new Vector3(0, .1f);
      //  }
    }

    public void SetSlotAvailableTo(bool value) => buildSlotAvailable = value;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buildSlotAvailable == false|| tileAnim.IsGridMoving()) return;
        
        if (outline != null) outline.enabled = true;
        
        if (tileCanBeMoved == false) return;
        
        MoveTileUp();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buildSlotAvailable == false|| tileAnim.IsGridMoving()) return;
        
        if (tileCanBeMoved && outline != null)
        {
            outline.enabled = false;
        }
        
        if (tileCanBeMoved == false) return;

        if (currentMovementUpCo != null) Invoke(nameof(MoveToDefaultPosition), tileAnim.GetTravelDuration());
        else MoveToDefaultPosition();
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (buildSlotAvailable == false || tileAnim.IsGridMoving()) return;
        
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (buildManager.GetSelectedSlot() == this) return;
        
        SnapToBeforeBuildPosition();
        buildManager.EnableBuildMenu();
        buildManager.SelectBuildSlot(this);
        MoveTileUp();

        tileCanBeMoved = false;
        
        ui.BuildButtonsHolderUI.GetLastSelected()?.SelectButton(true);
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
