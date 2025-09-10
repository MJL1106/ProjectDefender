using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelButtonTile : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private LevelManager levelManager;
    private TileAnimator tileAnimator;
    private TextMeshPro myText => GetComponentInChildren<TextMeshPro>();
    
    [SerializeField] private int levelIndex;

    private Vector3 defaultPosition;
    private Coroutine currentMoveCo;
    private Coroutine moveToDefaultCo;

    private bool canClick;
    private bool canMove;
    private bool unlocked;

    private void Awake()
    {
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        levelManager = FindAnyObjectByType<LevelManager>();
        defaultPosition = transform.position;
        CheckIfLevelUnlocked();
    }

    public void CheckIfLevelUnlocked()
    {
        if (levelIndex == 1) PlayerPrefs.SetInt("Level_1 unlocked", 1);

        unlocked = PlayerPrefs.GetInt("Level_" + levelIndex + " unlocked", 0) == 1;

        if (unlocked == false) myText.text = "Locked";
            else myText.text = "Level " + levelIndex;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (canClick == false) return;

        if (unlocked == false)
        {
            Debug.Log("Level locked!!!!");
            return;
        }

        canMove = true;
        transform.position = defaultPosition;
        levelManager.LoadLevelFromMenu("Level_" + levelIndex);
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

    private void OnValidate()
    {
        levelIndex = transform.GetSiblingIndex() + 1;

        if (myText != null) myText.text = "Level " + levelIndex;

    }

    private void OnEnable()
    {
        canMove = true;
    }
}
