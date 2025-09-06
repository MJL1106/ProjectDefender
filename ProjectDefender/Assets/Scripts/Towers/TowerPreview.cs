using System;
using Unity.VisualScripting;
using UnityEngine;

public class TowerPreview : MonoBehaviour
{
    private MeshRenderer[] meshRenderers;
    private Tower myTower;
    private TowerAttackRadiusDisplay attackRadiusDisplay;

    private float attackRange;

    private void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        myTower = GetComponent<Tower>();
        attackRadiusDisplay = transform.AddComponent<TowerAttackRadiusDisplay>();
        attackRange = myTower.GetAttackRange();

        MakeAllMeshTransparent();
        DestroyExtraComponents();
    }

    public void ShowPreview(bool showPreview, Vector3 previewPosition)
    {
        transform.position = previewPosition;
        attackRadiusDisplay.CreateCircle(showPreview, attackRange);
    }

    private void DestroyExtraComponents()
    {
        if (myTower != null)
        {
            CrossbowVisuals crossbowVisuals = GetComponent<CrossbowVisuals>();
            
            Destroy(crossbowVisuals);
            Destroy(myTower);
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
