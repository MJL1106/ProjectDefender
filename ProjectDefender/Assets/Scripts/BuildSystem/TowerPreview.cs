using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Manages the visual preview of towers before placement.
/// Creates a transparent ghost version of the tower with attack range visualization.
/// Strips gameplay components to create a lightweight preview-only object.
/// </summary>
public class TowerPreview : MonoBehaviour
{
    private List<System.Type> compToKeep = new List<System.Type>();
    
    private MeshRenderer[] meshRenderers;
    private RadiusDisplay attackRadiusDisplay;
    private ForwardAttackDisplay forwardDisplay;

    private float attackRange;
    private bool towerAttacksForward;

    /// <summary>
    /// Initializes the preview by copying visuals from the actual tower prefab.
    /// Removes all gameplay scripts and makes meshes transparent.
    /// </summary>
    /// <param name="towerToBuild">The tower prefab to create a preview for</param>
    public void SetupTowerPreview(GameObject towerToBuild)
    {
        Tower tower = towerToBuild.GetComponent<Tower>();
        
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        attackRadiusDisplay = transform.AddComponent<RadiusDisplay>();
        forwardDisplay = tower.GetComponent<ForwardAttackDisplay>();
        attackRange = tower.GetAttackRange();
        towerAttacksForward = tower.towerAttacksForward;
        
        SecureComponents();
        MakeAllMeshTransparent();
        DestroyExtraComponents();
        
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows or hides the tower preview at the specified position.
    /// Displays appropriate attack range visualization (circle or forward lines).
    /// </summary>
    /// <param name="showPreview">Whether to show the preview</param>
    /// <param name="previewPosition">World position to display preview</param>
    public void ShowPreview(bool showPreview, Vector3 previewPosition)
    {
        transform.position = previewPosition;
        
        if (!towerAttacksForward) attackRadiusDisplay.CreateCircle(showPreview, attackRange);
        else forwardDisplay.CreateLines(showPreview, attackRange);
    }

    /// <summary>
    /// Marks components that should not be destroyed during cleanup.
    /// Keeps only visual and preview-related components.
    /// </summary>
    private void SecureComponents()
    {
        compToKeep.Add(typeof(Transform));
        compToKeep.Add(typeof(TowerPreview));
        compToKeep.Add(typeof(RadiusDisplay));
        compToKeep.Add(typeof(LineRenderer));
        compToKeep.Add(typeof(ForwardAttackDisplay));
    }

    private bool ComponentSecured(Component compToCheck)
    {
        return compToKeep.Contains(compToCheck.GetType());
    }

    /// <summary>
    /// Removes all gameplay components from the preview object.
    /// Prevents preview from running tower logic or consuming resources.
    /// </summary>
    private void DestroyExtraComponents()
    {
        Component[] components = GetComponents<Component>();

        foreach (var componentToCheck in components)
        {
            if (!ComponentSecured(componentToCheck)) Destroy(componentToCheck);
        }
    }

    private void MakeAllMeshTransparent()
    {
        Material previewMat = FindFirstObjectByType<BuildManager>().GetBuildPreviewMat();

        foreach (var mesh in meshRenderers)
        {
            mesh.material = previewMat;
        }
    }
}