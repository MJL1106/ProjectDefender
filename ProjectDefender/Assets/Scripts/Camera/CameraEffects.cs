using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Manages camera state transitions and visual effects.
/// Handles switching between menu, level select, and gameplay views.
/// Provides screen shake and cinematic camera focus effects.
/// </summary>
public class CameraEffects : MonoBehaviour
{
    private CameraController camController;
    private Coroutine cameraCo;

    [Header("Transition details")] 
    [SerializeField] private float transitionDuration = 3;
    
    [Space]
    [SerializeField] private Vector3 inMenuPosition;
    [SerializeField] private Quaternion inMenuRotation;
    [Space]
    [SerializeField] private Vector3 inGamePosition;
    [SerializeField] private Quaternion inGameRotation;
    [Space] 
    [SerializeField] private Vector3 levelSelectPosition;
    [SerializeField] private Quaternion levelSelectRotation;

    [Header("ScreenShake Details")] 
    [Range(0.01f, .5f)]
    [SerializeField] private float shakeMagnitude;
    [Range(0.1f, 3f)]
    [SerializeField] private float shakeDuration;

    [Header("Castle Focus Details")] 
    [SerializeField] private float focusOnCastleDuration = 2;
    [SerializeField] private float heightOffset = 3; // Height above castle for camera position
    [SerializeField] private float distanceToCastle = 7; // Distance from castle for camera position

    private void Awake()
    {
        camController = GetComponent<CameraController>();
    }

    private void Start()
    {
        // Skip camera setup if testing level in editor
        if (GameManager.instance.IsTestingLevel())
        {
            camController.EnableCameraControlls(true);
            EnableAllTiles(true);
            return;
        }
        
        EnableAllTiles(false);
        EnableLevelButtonTiles(false);
        SwitchToMenuView();
    }

    /// <summary>
    /// Triggers screen shake effect. Used for tower build/sell feedback.
    /// </summary>
    public void ScreenShake(float newDuration, float newMagnitude)
    {
        StartCoroutine(ScreenShakeVFX(newDuration, newMagnitude));
    }

    /// <summary>
    /// Cinematically focuses camera on castle.
    /// Used for victory/defeat sequences to draw attention to castle state.
    /// </summary>
    public void FocusOnCastle()
    {
        Transform castle = FindFirstObjectByType<Castle>().transform;

        if (castle == null)
        {
            Debug.Log("No Castle to focus on!");
            return;
        }

        // Calculate position in front of castle at elevated angle
        Vector3 directionToCastle = (castle.position - transform.position).normalized;
        Vector3 targetPosition = castle.position - directionToCastle * distanceToCastle;
        targetPosition.y = castle.position.y + heightOffset;

        Quaternion targetRotation = Quaternion.LookRotation(castle.position - targetPosition);
        
        if (cameraCo != null) StopCoroutine(cameraCo);

        cameraCo = StartCoroutine(ChangePositionAndRotation(targetPosition, targetRotation, focusOnCastleDuration));
        StartCoroutine(EnableCameraControlsAfter(focusOnCastleDuration + .1f));
    }

    /// <summary>
    /// Transitions camera to main menu view and disables interactive tiles.
    /// </summary>
    public void SwitchToMenuView()
    {
        if (cameraCo != null) StopCoroutine(cameraCo);
    
        cameraCo = StartCoroutine(ChangePositionAndRotation(inMenuPosition, inMenuRotation, transitionDuration));
        camController.AdjustPitchValue(inMenuRotation.eulerAngles.x);
    
        EnableAllTiles(false);
        EnableLevelButtonTiles(false);
        UnselectAllTiles();
    }
    
    /// <summary>
    /// Removes outline highlighting from all level selection tiles.
    /// </summary>
    public void UnselectAllTiles()
    {
        LevelButtonTile[] levelButtons = FindObjectsByType<LevelButtonTile>(FindObjectsSortMode.None);
        foreach (var levelButton in levelButtons)
        {
            if (levelButton != null && levelButton.outline != null)
            {
                levelButton.outline.enabled = false;
            }
        }
    }

    /// <summary>
    /// Transitions camera to gameplay view and enables player controls after delay.
    /// Delay allows for cinematic reveal before gameplay starts.
    /// </summary>
    public void SwitchToGameView()
    {
        if (cameraCo != null) StopCoroutine(cameraCo);
        
        cameraCo = StartCoroutine(ChangePositionAndRotation(inGamePosition, inGameRotation, transitionDuration));
        camController.AdjustPitchValue(inGameRotation.eulerAngles.x);

        StartCoroutine(EnableCameraControlsAfter(transitionDuration + 1.8f));
        StartCoroutine(EnableTilesAfter(transitionDuration + 1.8f));
    }

    /// <summary>
    /// Transitions camera to level selection view with clickable level buttons.
    /// </summary>
    public void SwitchToLevelSelectView()
    {
        if (cameraCo != null) StopCoroutine(cameraCo);
    
        cameraCo = StartCoroutine(ChangePositionAndRotation(levelSelectPosition, levelSelectRotation, transitionDuration));
        camController.AdjustPitchValue(levelSelectRotation.eulerAngles.x);
    
        EnableAllTiles(false);
        EnableLevelButtonTiles(true);
    }

    /// <summary>
    /// Toggles collider interaction for level selection button tiles.
    /// </summary>
    public void EnableLevelButtonTiles(bool enable)
    {
        LevelButtonTile[] levelButtons = FindObjectsByType<LevelButtonTile>(FindObjectsSortMode.None);
        foreach (var levelButton in levelButtons)
        {
            Collider collider = levelButton.GetComponent<Collider>();
            if (collider != null) collider.enabled = enable;
        }
    }

    /// <summary>
    /// Toggles collider interaction for all build slot tiles.
    /// </summary>
    public void EnableAllTiles(bool enable)
    {
        BuildSlot[] tiles = FindObjectsByType<BuildSlot>(FindObjectsSortMode.None);
        foreach (var tile in tiles)
        {
            Collider collider = tile.GetComponent<Collider>();
            if (collider != null) collider.enabled = enable;
        }
    }

    private IEnumerator EnableTilesAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnableAllTiles(true);
    }

    /// <summary>
    /// Smoothly interpolates camera position and rotation over specified duration.
    /// Disables player controls during transition for cinematic effect.
    /// </summary>
    private IEnumerator ChangePositionAndRotation(Vector3 targetPosition, Quaternion targetRotation, float duration = 3,
        float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        camController.EnableCameraControlls(false);

        float time = 0;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    private IEnumerator EnableCameraControlsAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        camController.EnableCameraControlls(true);
    }

    /// <summary>
    /// Applies random camera shake by offsetting position.
    /// Returns to original position when complete.
    /// </summary>
    private IEnumerator ScreenShakeVFX(float duration, float magnitude)
    {
        Vector3 originalPosition = camController.transform.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float x = Random.Range(-1, 1) * magnitude;
            float y = Random.Range(-1, 1) * magnitude;

            camController.transform.position = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        camController.transform.position = originalPosition;
    }

    public Coroutine GetActiveCameraCo() => cameraCo;
}