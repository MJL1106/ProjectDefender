using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages enemy visual effects, including death VFX and transparency.
/// Also handles aligning the visual model to ground slope.
/// </summary>
public class EnemyVisuals : MonoBehaviour
{
    private ObjectPoolManager objectPool;
    
    [SerializeField] private GameObject onDeathVfx; // Prefab to spawn when the enemy dies
    [SerializeField] private float onDeathVfcScale = .5f; // Scale multiplier for the death VFX
    
    [Space]
    [SerializeField] protected Transform visuals; // The child transform containing the enemy's mesh/model
    [SerializeField] private LayerMask whatIsGround; // LayerMask used for slope alignment raycast
    [SerializeField] private float verticalRotationSpeed; // How quickly the model rotates to match the slope

    [Header("Transparency Details")] 
    [SerializeField] private Material transparentMat; // Material used when enemy is hidden (stealthed)
    private List<Material> originalMat;
    private MeshRenderer[] myRenderers;

    protected virtual void Awake()
    {
        CollectDefaultMaterials();
    }

    protected virtual void Start()
    {
        objectPool = ObjectPoolManager.instance;
    }
    
    protected virtual void Update()
    {
        AlignWithSlope();
    }

    /// <summary>
    /// Toggles the enemy's material between normal and transparent.
    /// Used by stealth mechanics.
    /// </summary>
    public void MakeTransparent(bool transparent)
    {
        for (int i = 0; i < myRenderers.Length; i++)
        {
            Material materialToApply = transparent ? transparentMat : originalMat[i];
            myRenderers[i].material = materialToApply;
        }
    }

    /// <summary>
    /// Spawns and scales the death VFX prefab from the object pool.
    /// </summary>
    public void CreateOnDeathVfx()
    {
        if (onDeathVfx == null) return;
        
        GameObject newDeathVfx =
            objectPool.Get(onDeathVfx, transform.position + new Vector3(0, .15f, 0), Quaternion.identity);
        newDeathVfx.transform.localScale = new Vector3(onDeathVfcScale, onDeathVfcScale, onDeathVfcScale);
    }

    /// <summary>
    /// Caches all MeshRenderers and their original materials on awake.
    /// Used to revert from the transparent material.
    /// </summary>
    protected void CollectDefaultMaterials()
    {
        myRenderers = GetComponentsInChildren<MeshRenderer>();
        originalMat = new List<Material>();

        foreach (var renderer in myRenderers)
        {
            originalMat.Add(renderer.material);
        }
    }

    /// <summary>
    /// Adjusts the visual model's rotation to match the ground normal.
    /// Keeps the enemy "planted" on slopes.
    /// </summary>
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