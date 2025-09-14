using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.Android;

public class EnemyVisuals : MonoBehaviour
{
    [SerializeField] private GameObject onDeathVfx;
    [SerializeField] private float onDeathVfcScale = .5f;
    
    [Space]
    [SerializeField] protected Transform visuals;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float verticalRotationSpeed;

    [Header("Transparency Details")] [SerializeField]
    private Material transparentMat;
    private List<Material> originalMat;
    private MeshRenderer[] myRenderers;

    protected virtual void Awake()
    {
        CollectDefaultMaterials();
    }

    protected virtual void Start()
    {
    }
    
    protected virtual void Update()
    {
        AlignWithSlope();

        if (Input.GetKeyDown(KeyCode.X)) MakeTransparent(true);
        if (Input.GetKeyDown(KeyCode.Z)) MakeTransparent(false);
    }

    public void MakeTransparent(bool transparent)
    {
        for (int i = 0; i < myRenderers.Length; i++)
        {
            Material materialToApply = transparent ? transparentMat : originalMat[i];
            myRenderers[i].material = materialToApply;
        }
    }

    public void CreateOnDeathVfx()
    {
        GameObject newDeathVfx =
            Instantiate(onDeathVfx, transform.position + new Vector3(0, .15f, 0), Quaternion.identity);
        newDeathVfx.transform.localScale = new Vector3(onDeathVfcScale, onDeathVfcScale, onDeathVfcScale);
    }

    protected void CollectDefaultMaterials()
    {
        myRenderers = GetComponentsInChildren<MeshRenderer>();
        originalMat = new List<Material>();

        foreach (var renderer in myRenderers)
        {
            originalMat.Add(renderer.material);
        }
    }

    private void AlignWithSlope()
    {
        if (visuals == null) return;

        if (Physics.Raycast(visuals.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, whatIsGround))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            visuals.rotation =
                Quaternion.Slerp(visuals.rotation, targetRotation, Time.deltaTime * verticalRotationSpeed);
        }
    }
}
