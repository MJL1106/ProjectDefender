using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelButtonTile : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private LevelManager levelManager;
    private TileAnimator tileAnimator;
    public Outline outline { get; private set; }
    private TextMeshPro myText => GetComponentInChildren<TextMeshPro>();
    
    [SerializeField] private int levelIndex;

    private Vector3 defaultPosition;
    private Coroutine currentMoveCo;
    private Coroutine moveToDefaultCo;

    private bool canClick;
    private bool unlocked;

    private void Awake()
    {
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        levelManager = FindAnyObjectByType<LevelManager>();
        defaultPosition = transform.position;
        
        // Add outline component
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.OutlineMode = Outline.Mode.VisibleEdgesOnly; 
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = 2.0f;
        outline.enabled = false;
        
        CheckIfLevelUnlocked();
    }

    public void CheckIfLevelUnlocked()
    {
        if (levelIndex == 1) PlayerPrefs.SetInt("Level_1 unlocked", 1);

        unlocked = PlayerPrefs.GetInt("Level_" + levelIndex + " unlocked", 0) == 1;

        UpdateLevelButtonText();
    }

    private void UpdateLevelButtonText()
    {
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
        
        transform.position = defaultPosition;
        levelManager.LoadLevelFromMenu("Level_" + levelIndex);
    }

    public void EnableCLickOnButton(bool enable) => canClick = enable;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tileAnimator.IsGridMoving()) return;
        
        // Enable outline
        if (outline != null) outline.enabled = true;
        
        MoveTileUp();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tileAnimator.IsGridMoving()) return;
        
        // Disable outline
        if (outline != null) outline.enabled = false;

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
}