using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Component on an individual tile.
/// Manages its mesh, material, collider, and type (e.g., buildable).
/// Used by the level editor tools.
/// </summary>
public class TileSlot : MonoBehaviour
{
    private int originalLayerIndex;
    private Material originalMaterial;
    
     private MeshRenderer meshRenderer => GetComponent<MeshRenderer>();
    private MeshFilter meshFilter => GetComponent<MeshFilter>();
    private Collider myCollider => GetComponent<Collider>();
    private NavMeshSurface myNavMesh => GetComponentInParent<NavMeshSurface>(true);
    private TileSetHolder tileSetHolder => GetComponentInParent<TileSetHolder>(true);

    private void Awake()
    {
        originalLayerIndex = gameObject.layer;
        originalMaterial = GetComponent<MeshRenderer>().sharedMaterial;
    }
    
    /// <summary>
    /// Transforms this tile into a copy of a reference tile.
    /// Copies mesh, material, collider, children, and layer.
    /// </summary>
    public void SwitchTile(GameObject referenceTile)
    {
        gameObject.name = referenceTile.name;

        TileSlot newTile = referenceTile.GetComponent<TileSlot>();

        meshFilter.mesh = newTile.GetMesh();
        meshRenderer.material = newTile.GetMaterial();

        UpdateCollider(newTile.GetCollider());
        UpdateChildren(newTile);
        UpdateLayer(referenceTile);
        UpdateNavMesh();
        
        TurnIntoBuildSlotIfNeeded(referenceTile);
    }

    /// <summary>
    /// Gets the tile's original shared material assigned on Awake.
    /// </summary>
    public Material GetOriginalMaterial()
    {
        if (originalMaterial == null) originalMaterial = GetComponent<MeshRenderer>().sharedMaterial;

        return originalMaterial;
    }

    /// <summary>
    /// Gets the tile's current shared material.
    /// </summary>
    public Material GetMaterial() => meshRenderer.sharedMaterial;
    
    /// <summary>
    /// Gets the tile's current shared mesh.
    /// </summary>
    public Mesh GetMesh() => meshFilter.sharedMesh;
    
    /// <summary>
    /// Gets the tile's primary collider.
    /// </summary>
    public Collider GetCollider() => myCollider;
    
    /// <summary>
    /// Returns a list of all immediate child GameObjects.
    /// </summary>
    public List<GameObject> GetAllChildren()
    {
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }

        return children;
    }

    /// <summary>
    /// Adds or removes the BuildSlot component based on the reference tile type.
    /// Only "tileField" is buildable.
    /// </summary>
    public void TurnIntoBuildSlotIfNeeded(GameObject referenceTile)
    {
        BuildSlot buildSlot = GetComponent<BuildSlot>();

        if (referenceTile != tileSetHolder.tileField)
            if (buildSlot != null) DestroyImmediate(buildSlot);
        else
            if (buildSlot == null) gameObject.AddComponent<BuildSlot>();
        
    }
    
    /// <summary>
    /// Rebuilds the parent's NavMesh.
    /// </summary>
    private void UpdateNavMesh() => myNavMesh.BuildNavMesh();

    /// <summary>
    /// Replaces this tile's collider with a copy of the reference collider.
    /// </summary>
    private void UpdateCollider(Collider newCollider)
    {
        DestroyImmediate(myCollider);

        if (newCollider is BoxCollider)
        {
            BoxCollider original = newCollider.GetComponent<BoxCollider>();
            BoxCollider myNewCollider = transform.AddComponent<BoxCollider>();

            myNewCollider.center = original.center;
            myNewCollider.size = original.size;
        }

        if (newCollider is MeshCollider)
        {
            MeshCollider original = newCollider.GetComponent<MeshCollider>();
            MeshCollider myNewCollider = transform.AddComponent<MeshCollider>();

            myNewCollider.sharedMesh = original.sharedMesh;
            myNewCollider.convex = original.convex;
        }
    }

    /// <summary>
    /// Replaces this tile's children with copies of the reference tile's children.
    /// </summary>
    private void UpdateChildren(TileSlot newTile)
    {
        foreach (GameObject obj in GetAllChildren())
        {
            DestroyImmediate(obj);
        }

        foreach (GameObject obj in newTile.GetAllChildren())
        {
            Instantiate(obj, transform);
        }
    }

    /// <summary>
    /// Updates the tile's layer to match a reference object.
    /// </summary>
    public void UpdateLayer(GameObject referenceObj)
    {
        gameObject.layer = referenceObj.layer;
        originalLayerIndex = gameObject.layer;
    }

    /// <summary>
    /// Toggles the tile's layer to or from a non-interactable layer (15).
    /// </summary>
    public void MakeNonInteractable(bool nonInteractable)
    {
        gameObject.layer = nonInteractable ? 15 : originalLayerIndex;
    }

    /// <summary>
    /// Rotates the tile 90 degrees and updates the NavMesh.
    /// </summary>
    /// <param name="dir">The direction multiplier (e.g., 1 for 90°, -1 for -90°).</param>
    public void RotateTile(int dir)
    {
        transform.Rotate(0, 90 * dir, 0);
        UpdateNavMesh();
    }

    /// <summary>
    /// Checks if all 4 sides are blocked and disables shadows if so.
    /// Used for performance optimization on hidden tiles.
    /// </summary>
    public void DisableShadowsIfNeeded()
    {
        UnityEngine.Rendering.ShadowCastingMode shadowMode = UnityEngine.Rendering.ShadowCastingMode.On;

        int blockedSides = 0;
        Vector3 point = transform.position + new Vector3(0, .49f, 0);
        Vector3[] direction = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        foreach (Vector3 dir in direction)
        {
            if (Physics.Raycast(point, dir, .6f)) blockedSides++;
        }

        if (blockedSides == direction.Length) shadowMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        meshRenderer.shadowCastingMode = shadowMode;
    }

    /// <summary>
    /// Moves the tile up or down by a small increment and updates NavMesh.
    /// </summary>
    /// <param name="verticalDir">The direction multiplier (e.g., 1 for up, -1 for down).</param>
    public void AdjustY(int verticalDir)
    {
        transform.position += new Vector3(0, .1f * verticalDir, 0);
        UpdateNavMesh();
    }
}