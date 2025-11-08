using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

/// <summary>
/// Builds the level grid from a main prefab at edit time.
/// Manages the collection of tiles and provides access to the NavMeshSurface.
/// </summary>
public class GridBuilder : MonoBehaviour
{
    private NavMeshSurface myNavMesh => GetComponent<NavMeshSurface>();
    
    [SerializeField] private GameObject mainPrefab; // The single tile prefab used to build the entire grid.

    [SerializeField] private int gridLength = 10; // Dimensions of the grid.
    [SerializeField] private int gridWidth = 10;

    [SerializeField] private List<GameObject> createdTiles; // List of all instantiated tile GameObjects.

    private bool hadFirstLoad;

    /// <summary>
    /// Tells all created tiles to check and disable shadows if they are occluded.
    /// </summary>
    public void DisableShadowsIfNeeded()
    {
        foreach (var tile in createdTiles)
        {
            tile.GetComponent<TileSlot>().DisableShadowsIfNeeded();
        }
    }

    /// <summary>
    /// Checks if this is the first time the grid is being loaded/animated.
    /// Used by TileAnimator to know whether to apply an initial offset.
    /// </summary>
    public bool IsOnFirstLoad()
    {
        if (hadFirstLoad == false)
        {
            hadFirstLoad = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the list of all created tile GameObjects.
    /// </summary>
    public List<GameObject> GetTileSetup() => createdTiles;
    
    /// <summary>
    /// Rebuilds the NavMeshSurface for this grid.
    /// </summary>
    public void UpdateNavMesh() => myNavMesh.BuildNavMesh();

    /// <summary>
    /// Editor-only. Clears and rebuilds the grid in the editor.
    /// </summary>
    [ContextMenu("Build Grid")]
    private void BuildGrid()
    {
        ClearGrid();
        createdTiles = new List<GameObject>();
        
        for (int x = 0; x < gridLength; x++)
        {
            for (int z = 0; z < gridWidth; z++)
            {
                CreateTile(x,z);
            }
        }
    }

    /// <summary>
    /// Editor-only. Destroys all child tiles.
    /// </summary>
    [ContextMenu("Clear Grid")]
    private void ClearGrid()
    {
        foreach (GameObject tile in createdTiles)
        {
            DestroyImmediate(tile);
        }
        
        createdTiles.Clear();
    }
    
    /// <summary>
    /// Instantiates a single tile and adds it to the grid.
    /// </summary>
    private void CreateTile(float xPosition, float zPosition)
    {
        Vector3 newPosition = new Vector3(xPosition, 0, zPosition);
        GameObject newTile = Instantiate(mainPrefab, newPosition, Quaternion.identity, transform);
        
        createdTiles.Add(newTile);
        
        newTile.GetComponent<TileSlot>().TurnIntoBuildSlotIfNeeded(mainPrefab);
    }

    /// <summary>
    /// Temporarily sets all tiles to a non-interactable layer.
    /// Used during grid animations to prevent clicks.
    /// </summary>
    public void MakeTilesNonInteractable(bool makeNonInteractable)
    {
        foreach (var tile in createdTiles)
        {
            tile.GetComponent<TileSlot>().MakeNonInteractable(makeNonInteractable);
        }
    }
}