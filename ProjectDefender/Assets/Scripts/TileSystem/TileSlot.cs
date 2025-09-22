using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;

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

    public Material GetOriginalMaterial()
    {
        if (originalMaterial == null) originalMaterial = GetComponent<MeshRenderer>().sharedMaterial;

        return originalMaterial;
    }

    public Material GetMaterial() => meshRenderer.sharedMaterial;
    public Mesh GetMesh() => meshFilter.sharedMesh;
    public Collider GetCollider() => myCollider;
    public List<GameObject> GetAllChildren()
    {
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }

        return children;
    }

    public void TurnIntoBuildSlotIfNeeded(GameObject referenceTile)
    {
        BuildSlot buildSlot = GetComponent<BuildSlot>();

        if (referenceTile != tileSetHolder.tileField)
        {
            if (buildSlot != null) DestroyImmediate(buildSlot);
        }
        else
        {
            if (buildSlot == null) gameObject.AddComponent<BuildSlot>();
        }
    }
    private void UpdateNavMesh() => myNavMesh.BuildNavMesh();

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

    public void UpdateLayer(GameObject referenceObj)
    {
        gameObject.layer = referenceObj.layer;
        originalLayerIndex = gameObject.layer;
    }

    public void MakeNonInteractable(bool nonInteractable)
    {
        gameObject.layer = nonInteractable ? 15 : originalLayerIndex;
    }

    public void RotateTile(int dir)
    {
        transform.Rotate(0, 90 * dir, 0);
        UpdateNavMesh();
    }

    public void DisableShadowsIfNeeded()
    {
        UnityEngine.Rendering.ShadowCastingMode shadowMode = UnityEngine.Rendering.ShadowCastingMode.On;

        int blockedSides = 0;
        Vector3 point = transform.position + new Vector3(0, .49f, 0);
        Vector3[] direction = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        foreach (Vector3 dir in direction)
        {
            if (Physics.Raycast(point, dir, .6f))
                blockedSides++;
        }

        if (blockedSides == direction.Length) shadowMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        meshRenderer.shadowCastingMode = shadowMode;
    }

    public void AdjustY(int verticalDir)
    {
        transform.position += new Vector3(0, .1f * verticalDir, 0);
        UpdateNavMesh();
    }
    
    
}
