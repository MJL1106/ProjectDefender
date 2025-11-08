using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages a single level selection button in the main menu.
/// Handles pointer interactions (hover, click), level locking, and animation.
/// </summary>
public class LevelButtonTile : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private LevelManager levelManager;
    private TileAnimator tileAnimator;
    public Outline outline { get; private set; }
    private TextMeshPro myText => GetComponentInChildren<TextMeshPro>();
    
    [SerializeField] private int levelIndex; // The build index of the level this tile loads

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

    /// <summary>
    /// Checks PlayerPrefs to see if this level is unlocked.
    /// Level 1 is always unlocked by default.
    /// </summary>
    public void CheckIfLevelUnlocked()
    {
        if (levelIndex == 1) PlayerPrefs.SetInt("Level_1 unlocked", 1);

        unlocked = PlayerPrefs.GetInt("Level_" + levelIndex + " unlocked", 0) == 1;

        UpdateLevelButtonText();
    }

    /// <summary>
    /// Updates the text on the tile to "Locked" or "Level X".
    /// </summary>
    private void UpdateLevelButtonText()
    {
        if (unlocked == false) myText.text = "Locked";
        else myText.text = "Level " + levelIndex;
    }

    /// <summary>
    /// Called when the tile is clicked.
    /// Loads the level if it's unlocked and clicking is enabled.
    /// </summary>
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

    /// <summary>
    /// Allows or blocks the tile from being clicked.
    /// Used by LevelManager to prevent clicks during scene transitions.
    /// </summary>
    public void EnableCLickOnButton(bool enable) => canClick = enable;
    
    /// <summary>
    /// Called when the pointer hovers over the tile.
    /// Animates the tile up and enables the outline.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tileAnimator.IsGridMoving()) return;
        
        // Enable outline
        if (outline != null) outline.enabled = true;
        
        MoveTileUp();
    }

    /// <summary>
    /// Called when the pointer leaves the tile.
    /// Animates the tile back to default and disables the outline.
    /// </summary>
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

    /// <summary>
    /// Triggers the animation to move the tile up on hover.
    /// </summary>
    private void MoveTileUp()
    {
        Vector3 targetPosition = transform.position + new Vector3(0, tileAnimator.GetBuildOffset(), 0);
        currentMoveCo = StartCoroutine(tileAnimator.MoveTileCo(transform, targetPosition));
    }

    /// <summary>
    /// Triggers the animation to move the tile to its original position.
    /// </summary>
    private void MoveToDefault()
    {
        moveToDefaultCo = StartCoroutine(tileAnimator.MoveTileCo(transform, defaultPosition));
    }

    /// <summary>
    /// Editor-only function to auto-assign level index and text.
    /// Ensures data is correct in the editor.
    /// </summary>
    private void OnValidate()
    {
        levelIndex = transform.GetSiblingIndex() + 1;

        if (myText != null) myText.text = "Level " + levelIndex;
    }
}