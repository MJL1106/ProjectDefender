using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Manages all grid and tile animations.
/// Handles show/hide, dissolve, and hover movements for level transitions.
/// </summary>
public class TileAnimator : MonoBehaviour
{
    [SerializeField] private float defaultMoveDuration = .1f;

    [Header("Build Slot Movement")] [SerializeField]
    private float buildSlotYOffset = 0.25f; // Vertical distance a build slot moves on hover

    [Header("Grid Animation Details")] [SerializeField]
    private float tileMoveDuration = .1f; // Parameters for the grid show/hide animation
    [SerializeField] private float tileDelay = .1f;
    [SerializeField] private float yOffset = 5;


    [Space]
    [SerializeField] private List<GameObject> mainMenuObjects = new List<GameObject>(); // Objects part of the main menu scene (grid, castle, etc.)
    [SerializeField] private GridBuilder mainSceneGrid;
    private Coroutine currentActiveCoroutine;
    private bool isGridMoving;

    [Header("Grid Dissolve Details")] 
    [SerializeField] private Material dissolveMat; // Material used for the dissolve effect
    [SerializeField] private float dissolveDuration = 1.2f;
    private List<Transform> dissolvingObjects = new List<Transform>();
    private void Start()
    {
        if (GameManager.instance.IsTestingLevel()) return;
        
        CollectMainSceneObjects();
        ShowGrid(mainSceneGrid,true);
    }

    /// <summary>
    /// Animates the main menu grid in or out.
    /// </summary>
    public void ShowMainGrid(bool showMainGrid)
    {
        ShowGrid(mainSceneGrid, showMainGrid);
    }

    /// <summary>
    /// Animates a specified grid in or out (up or down) with a dissolve effect.
    /// </summary>
    /// <param name="gridToMove">The GridBuilder component whose tiles will be animated.</param>
    /// <param name="showGrid">True to animate the grid in, false to animate it out.</param>
    public void ShowGrid(GridBuilder gridToMove, bool showGrid)
    {
        List<GameObject> objectsToMove = GetObjectsToMove(gridToMove, showGrid);
        
        if (gridToMove.IsOnFirstLoad()) ApplyOffset(objectsToMove, new Vector3(0, -yOffset, 0));

        float offset = showGrid ? yOffset : -yOffset;

        gridToMove.MakeTilesNonInteractable(true);
        currentActiveCoroutine = StartCoroutine(MoveGridCo(objectsToMove, offset, showGrid));
    }

    /// <summary>
    /// Coroutine that moves all tiles in a grid sequentially with a delay.
    /// </summary>
    private IEnumerator MoveGridCo(List<GameObject> objectsToMove, float yOffsetGrid, bool showGrid)
    {
        isGridMoving = true;
        
        for (int i = 0; i < objectsToMove.Count; i++)
        {
            yield return new WaitForSeconds(tileDelay);
            
            if (objectsToMove[i] == null) continue;

            Transform tile = objectsToMove[i].transform;
            
            Vector3 targetPosition = tile.position + new Vector3(0, yOffsetGrid, 0);
            
            DissolveTile(showGrid, tile);
            MoveTile(tile,targetPosition, showGrid, tileMoveDuration);
        }

        while (dissolvingObjects.Count > 0)
        {
            yield return null;
        }

        foreach (var tile in objectsToMove)
        {
            tile.GetComponent<TileSlot>()?.MakeNonInteractable(false);
        }

        isGridMoving = false;
    }
    
    /// <summary>
    /// Wrapper to start the MoveTileCo coroutine with optional delay/duration.
    /// </summary>
    /// <param name="objectToMove">The transform of the tile to move.</param>
    /// <param name="targetPosition">The target world-space position.</param>
    /// <param name="showGrid">True if the tile is appearing, false if disappearing (affects delay).</param>
    /// <param name="newDuration">Optional: override the default move duration.</param>
    public void MoveTile(Transform objectToMove, Vector3 targetPosition, bool showGrid, float? newDuration = null)
    {
        float moveDelay = showGrid ? 0 : .8f;
        float duration = newDuration ?? defaultMoveDuration;
        StartCoroutine(MoveTileCo(objectToMove, targetPosition, moveDelay, duration));
    }

    /// <summary>
    /// Coroutine to smoothly Lerp a tile's position.
    /// </summary>
    /// <param name="objectToMove">The transform of the tile to move.</param>
    /// <param name="targetPosition">The target world-space position.</param>
    /// <param name="delay">Optional: delay in seconds before the movement starts.</param>
    /// <param name="newDuration">Optional: override the default move duration.</param>
    public IEnumerator MoveTileCo(Transform objectToMove, Vector3 targetPosition, float delay = 0, float? newDuration = null)
    {
        yield return new WaitForSeconds(delay);
        
        float time = 0;
        Vector3 startPosition = objectToMove.position;
        
        float duration = newDuration ?? defaultMoveDuration;

        while (time < duration)
        {
            if (objectToMove == null) break;
            
            objectToMove.position = Vector3.Lerp(startPosition, targetPosition, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        if (objectToMove != null) objectToMove.position = targetPosition;
    }

    /// <summary>
    /// Applies the dissolve effect to all mesh renderers on a tile.
    /// </summary>
    /// <param name="showTile">True to dissolve in (appear), false to dissolve out (disappear).</param>
    /// <param name="tile">The parent transform of the tile to dissolve.</param>
    public void DissolveTile(bool showTile, Transform tile)
    {
        MeshRenderer[] meshRenderers = tile.GetComponentsInChildren<MeshRenderer>();

        if (tile.GetComponent<TileSlot>() != null)
        {
            foreach (MeshRenderer mesh in meshRenderers)
            {
                StartCoroutine(DissolveTileCo(mesh, dissolveDuration, showTile));
            }
        }
    }

    /// <summary>
    /// Coroutine to animate the dissolve shader's properties on a material instance.
    /// </summary>
    /// <param name="meshRenderer">The specific mesh renderer to apply the effect to.</param>
    /// <param name="duration">The length of the dissolve animation in seconds.</param>
    /// <param name="showTile">True to dissolve in (appear), false to dissolve out (disappear).</param>
    private IEnumerator DissolveTileCo(MeshRenderer meshRenderer, float duration, bool showTile)
    {
        TextMeshPro textMeshPro = meshRenderer.GetComponent<TextMeshPro>();

        if (textMeshPro != null)
        {
            textMeshPro.enabled = showTile;
            yield break;
        }
        
        dissolvingObjects.Add(meshRenderer.transform);

        float startValue = showTile ? 1 : 0;
        float targetValue = showTile ? 0 : 1;

        Material originalMat = meshRenderer.material;

        meshRenderer.material = new Material(dissolveMat);

        Material dissolveMatInstance = meshRenderer.material;
        
        dissolveMatInstance.SetColor("_BaseColor", originalMat.GetColor("_BaseColor"));
        dissolveMatInstance.SetFloat("_Metallic", originalMat.GetFloat("_Metallic"));
        dissolveMatInstance.SetFloat("_Smoothness", originalMat.GetFloat("_Smoothness"));
        dissolveMatInstance.SetFloat("_Dissolve", startValue);

        float time = 0;

        while (time < duration)
        {
            float currentDissolveValue = Mathf.Lerp(startValue, targetValue, time / duration);
            
            dissolveMatInstance.SetFloat("_Dissolve", currentDissolveValue);

            time += Time.deltaTime;
            yield return null;
        }

        meshRenderer.material = originalMat;

        if (meshRenderer != null) dissolvingObjects.Remove(meshRenderer.transform);
    }

    /// <summary>
    /// Applies an immediate position offset to a list of objects.
    /// </summary>
    private void ApplyOffset(List<GameObject> objectsToMove, Vector3 offset)
    {
        foreach (var obj in objectsToMove)
        {
            obj.transform.position += offset;
        }
    }

    /// <summary>
    /// Shows or hides all objects associated with the main menu.
    /// </summary>
    public void EnableMainSceneObjects(bool enable)
    {
        foreach (var obj in mainMenuObjects)
        {
            obj.SetActive(enable);
        }
    }
    
    /// <summary>
    /// Gathers all tiles and "extra objects" (portals, castle) into the main menu list.
    /// </summary>
    private void CollectMainSceneObjects()
    {
        mainMenuObjects.AddRange(mainSceneGrid.GetTileSetup());
        mainMenuObjects.AddRange(GetExtraObjects());
    }

    /// <summary>
    /// Creates a list of objects to be animated, combining grid tiles and extra objects.
    /// </summary>
    private List<GameObject> GetObjectsToMove(GridBuilder gridToMove, bool startWithTiles)
    {
        List<GameObject> objectsToMove = new List<GameObject>();
        List<GameObject> extraObjects = GetExtraObjects();

        if (startWithTiles)
        {
            objectsToMove.AddRange(gridToMove.GetTileSetup());
            objectsToMove.AddRange(extraObjects);
        }
        else
        {
            objectsToMove.AddRange(extraObjects);
            objectsToMove.AddRange(gridToMove.GetTileSetup());
        }

        return objectsToMove;
    }

    /// <summary>
    /// Finds all portals and castles in the scene to be included in animations.
    /// </summary>
    private List<GameObject> GetExtraObjects()
    {
        List<GameObject> extraObjects = new List<GameObject>();

        extraObjects.AddRange(FindObjectsByType<EnemyPortal>(FindObjectsSortMode.None).Select(component => component.gameObject));
        extraObjects.AddRange(FindObjectsByType<Castle>(FindObjectsSortMode.None).Select(component => component.gameObject));

        return extraObjects;
    }

    /// <summary>
    /// Gets the currently running main grid animation coroutine.
    /// </summary>
    public Coroutine GetActiveCoroutine() => currentActiveCoroutine;
    
    /// <summary>
    /// Returns the configured hover offset for build slots.
    /// </summary>
    public float GetBuildOffset() => buildSlotYOffset;
    
    /// <summary>
    /// Returns the default animation duration.
    /// </summary>
    public float GetTravelDuration() => defaultMoveDuration;

    /// <summary>
    /// Checks if a grid animation is currently in progress.
    /// </summary>
    public bool IsGridMoving() => isGridMoving;
}