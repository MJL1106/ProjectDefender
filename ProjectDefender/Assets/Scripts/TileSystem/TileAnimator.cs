using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class TileAnimator : MonoBehaviour
{
    [SerializeField] private float defaultMoveDuration = .1f;

    [Header("Build Slot Movement")] [SerializeField]
    private float buildSlotYOffset = 0.25f;

    [Header("Grid ANimation Details")] [SerializeField]
    private float tileMoveDuration = .1f;
    [SerializeField] private float tileDelay = .1f;
    [SerializeField] private float yOffset = 5;


    [Space]
    [SerializeField] private List<GameObject> mainMenuObjects = new List<GameObject>();
    [SerializeField] private GridBuilder mainSceneGrid;
    private Coroutine currentActiveCoroutine;
    private bool isGridMoving;


    private void Start()
    {
        CollectMainSceneObjects();
        ShowGrid(mainSceneGrid,true);
    }

    public void BringUpMainGrid(bool showMainGrid)
    {
        ShowGrid(mainSceneGrid, showMainGrid);
    }

    public void ShowGrid(GridBuilder gridToMove, bool showGrid)
    {
        List<GameObject> objectsToMove = GetObjectsToMove(gridToMove, showGrid);
        
        if (gridToMove.IsOnFirstLoad()) ApplyOffset(objectsToMove, new Vector3(0, -yOffset, 0));

        float offset = showGrid ? yOffset : -yOffset;

        currentActiveCoroutine = StartCoroutine(MoveGridCo(objectsToMove, offset));
    }

    private IEnumerator MoveGridCo(List<GameObject> objectsToMove, float yOffsetGrid)
    {
        isGridMoving = true;
        
        for (int i = 0; i < objectsToMove.Count; i++)
        {
            yield return new WaitForSeconds(tileDelay);

            Transform tile = objectsToMove[i].transform;
            Vector3 targetPosition = tile.position + new Vector3(0, yOffsetGrid, 0);
            
            MoveTile(tile,targetPosition, tileMoveDuration);
        }

        isGridMoving = false;
    }
    
    public void MoveTile(Transform objectToMove, Vector3 targetPosition, float? newDuration = null)
    {
        float duration = newDuration ?? defaultMoveDuration;
        StartCoroutine(MoveTileCo(objectToMove, targetPosition, duration));
    }

    public IEnumerator MoveTileCo(Transform objectToMove, Vector3 targetPosition, float? newDuration = null)
    {
        float time = 0;
        Vector3 startPosition = objectToMove.position;
        
        float duration = newDuration ?? defaultMoveDuration;

        while (time < duration)
        {
            objectToMove.position = Vector3.Lerp(startPosition, targetPosition, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        objectToMove.position = targetPosition;
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
