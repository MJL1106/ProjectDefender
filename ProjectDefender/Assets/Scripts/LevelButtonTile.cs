using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelButtonTile : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private LevelManager levelManager;
    private TileAnimator tileAnimator;
    
    [SerializeField] private int levelIndex;

    private Vector3 defaultPosition;
    private Coroutine currentMoveCo;
    private Coroutine moveToDefaultCo;

    private bool canClick;
    private bool canMove;

    private void Awake()
    {
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        levelManager = FindAnyObjectByType<LevelManager>();
        defaultPosition = transform.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (canClick == false) return;
        
        Debug.Log("Loading level! - Level_" + levelIndex);
    }

    public void EnableCLickOnButton(bool enable) => canClick = enable;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (canMove == false) return;
        
        MoveTileUp();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (canMove == false) return;

        if (currentMoveCo != null)
        {
            Invoke(nameof(MoveToDefault), tileAnimator.GetTravelDuration());
        }
        else
        {
            MoveToDefault();
        }
    }

    private void MoveTileUp()
    {
        Vector3 targetPosition = transform.position + new Vector3(0, tileAnimator.GetBuildOffset(), 0);
        currentMoveCo = StartCoroutine(tileAnimator.MoveTileCo(transform, targetPosition));
    }

    private void MoveToDefault()
    {
        moveToDefaultCo = StartCoroutine(tileAnimator.MoveTileCo(transform, defaultPosition));
    }

    private void OnEnable()
    {
        canMove = true;
    }
}
