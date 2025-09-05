using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private TileAnimator tileAnim;
    private Vector3 defaultPosition;
    private BuildManager buildManager;

    private bool tileCanBeMoved = true;
    private bool buildSlotAvailable = true;

    private Coroutine currentMovementUpCo;
    private Coroutine moveToDefaultCo;

    private void Awake()
    {
        tileAnim = FindFirstObjectByType<TileAnimator>();
        buildManager = FindFirstObjectByType<BuildManager>();
        defaultPosition = transform.position;
    }

    private void Start()
    {
        if (buildSlotAvailable == false)
        {
            transform.position += new Vector3(0, .1f);
        }
    }

    public void SetSlotAvailableTo(bool value) => buildSlotAvailable = value;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buildSlotAvailable == false) return;
        
        if (tileCanBeMoved == false) return;
        
        MoveTileUp();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buildSlotAvailable == false) return;
        
        if (tileCanBeMoved == false) return;

        if (currentMovementUpCo != null) Invoke(nameof(MoveToDefaultPosition), tileAnim.GetTravelDuration());
        else MoveToDefaultPosition();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (buildSlotAvailable == false) return;
        
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (buildManager.GetSelectedSlot() == this) return;
        
        buildManager.EnableBuildMenu();
        buildManager.SelectBuildSlot(this);
        MoveTileUp();

        tileCanBeMoved = false;
    }

    public void UnselectTile()
    {
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

    public Vector3 GetBuildPosition(float yOffset) => defaultPosition + new Vector3(0,yOffset);
}
