using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TileAnimator : MonoBehaviour
{
    [SerializeField] private float defaultMoveDuration = .1f;

    [Header("Build Slot Movement")] [SerializeField]
    private float buildSlotYOffset = 0.25f;

    [Header("Grid Animation Details")] [SerializeField]
    private float tileMoveDuration = .1f;
    [SerializeField] private float tileDelay = .1f;
    [SerializeField] private float yOffset = 5;


    [Space]
    [SerializeField] private List<GameObject> mainMenuObjects = new List<GameObject>();
    [SerializeField] private GridBuilder mainSceneGrid;
    private Coroutine currentActiveCoroutine;
    private bool isGridMoving;

    [Header("Grid Dissolve Details")] 
    [SerializeField] private Material dissolveMat;
    [SerializeField] private float dissolveDuration = 1.2f;
    private List<Transform> dissolvingObjects = new List<Transform>();


    private void Start()
    {
        if (GameManager.instance.IsTestingLevel()) return;
        
        CollectMainSceneObjects();
        ShowGrid(mainSceneGrid,true);
    }

    public void ShowMainGrid(bool showMainGrid)
    {
        ShowGrid(mainSceneGrid, showMainGrid);
    }

    public void ShowGrid(GridBuilder gridToMove, bool showGrid)
    {
        List<GameObject> objectsToMove = GetObjectsToMove(gridToMove, showGrid);
        
        if (gridToMove.IsOnFirstLoad()) ApplyOffset(objectsToMove, new Vector3(0, -yOffset, 0));

        float offset = showGrid ? yOffset : -yOffset;

        gridToMove.MakeTilesNonInteractable(true);
        currentActiveCoroutine = StartCoroutine(MoveGridCo(objectsToMove, offset, showGrid));
    }

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
    
    public void MoveTile(Transform objectToMove, Vector3 targetPosition, bool showGrid, float? newDuration = null)
    {
        float moveDelay = showGrid ? 0 : .8f;
        float duration = newDuration ?? defaultMoveDuration;
        StartCoroutine(MoveTileCo(objectToMove, targetPosition, moveDelay, duration));
    }

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

    private void ApplyOffset(List<GameObject> objectsToMove, Vector3 offset)
    {
        foreach (var obj in objectsToMove)
        {
            obj.transform.position += offset;
        }
    }

    public void EnableMainSceneObjects(bool enable)
    {
        foreach (var obj in mainMenuObjects)
        {
            obj.SetActive(enable);
        }
    }
    
    private void CollectMainSceneObjects()
    {
        mainMenuObjects.AddRange(mainSceneGrid.GetTileSetup());
        mainMenuObjects.AddRange(GetExtraObjects());
    }

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


    private List<GameObject> GetExtraObjects()
    {
        List<GameObject> extraObjects = new List<GameObject>();

        extraObjects.AddRange(FindObjectsByType<EnemyPortal>(FindObjectsSortMode.None).Select(component => component.gameObject));
        extraObjects.AddRange(FindObjectsByType<Castle>(FindObjectsSortMode.None).Select(component => component.gameObject));

        return extraObjects;
    }

    public Coroutine GetActiveCoroutine() => currentActiveCoroutine;
    
    public float GetBuildOffset() => buildSlotYOffset;
    public float GetTravelDuration() => defaultMoveDuration;

    public bool IsGridMoving() => isGridMoving;
}
