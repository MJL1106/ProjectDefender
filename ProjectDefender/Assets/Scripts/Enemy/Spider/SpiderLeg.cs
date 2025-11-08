using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Individual spider leg with procedural IK-style movement.
/// Moves to ground contact point when distance threshold exceeded.
/// Coordinates with opposite leg to maintain stability (one moves at a time).
/// </summary>
public class SpiderLeg : MonoBehaviour
{
    private EnemySpiderVisuals spiderVisuals;
    private ObjectPoolManager objectPool;
    
    [SerializeField] private float legSpeed = 2.5f;
    [SerializeField] private float moveThreshold = .45f; // Distance before leg needs to move
    private bool shouldMove;
    private bool canMove = true;
    private Coroutine moveCo;

    [Header("Leg setup")]
    [SerializeField] private SpiderLeg oppositeLeg; // Paired leg for coordination
    [SerializeField] private SpiderLegReference legRef; // Ground contact reference point
    [SerializeField] private Transform actualTarget; // IK target
    [SerializeField] private Transform bottomLeg;
    [SerializeField] private Vector3 placementOffset;
    [SerializeField] private Transform worldTargetReference; // Persistent world-space target

    private void Awake()
    {
        objectPool = ObjectPoolManager.instance;
        spiderVisuals = GetComponentInParent<EnemySpiderVisuals>();
        
        // Create persistent world target to avoid parenting issues
        worldTargetReference = Instantiate(worldTargetReference, actualTarget.position, Quaternion.identity).transform;
        worldTargetReference.gameObject.name = legRef.gameObject.name + "_world";
        
        legSpeed = spiderVisuals.legSpeed;
    }

    /// <summary>
    /// Updates leg IK target and initiates movement when threshold exceeded.
    /// </summary>
    public void UpdateLeg()
    {
        actualTarget.position = worldTargetReference.position;
        shouldMove = Vector3.Distance(worldTargetReference.position, legRef.ContactPoint()) > moveThreshold;

        if (bottomLeg != null) bottomLeg.forward = Vector3.down;

        if (shouldMove && canMove)
        {
            if (moveCo != null) StopCoroutine(moveCo);
            StartCoroutine(LegMoveCo());
        }
    }

    /// <summary>
    /// Moves leg to new contact point while locking opposite leg for stability.
    /// </summary>
    private IEnumerator LegMoveCo()
    {
        oppositeLeg.CanMove(false);
        
        while (Vector3.Distance(worldTargetReference.position, legRef.ContactPoint()) > .01f)
        {
            worldTargetReference.position = Vector3.MoveTowards(worldTargetReference.position, legRef.ContactPoint(),
                legSpeed * Time.deltaTime);

            yield return null;
        }
        
        oppositeLeg.CanMove(true);
    }

    public void SpeedUpLeg() => StartCoroutine(SpeedUpLegCo());
    
    /// <summary>
    /// Temporarily increases leg speed for visual emphasis.
    /// </summary>
    private IEnumerator SpeedUpLegCo()
    {
        legSpeed = spiderVisuals.increasedLegSpeed;
        yield return new WaitForSeconds(1);
        legSpeed = spiderVisuals.legSpeed;
    }

    private void OnEnable()
    {
        ParentLegReference(false);
    }

    private void OnDisable()
    {
        ParentLegReference(true);
    }

    public void CanMove(bool enableMovement) => canMove = enableMovement;

    /// <summary>
    /// Manages world target parenting for object pooling.
    /// Unparented during use, parented to pool when disabled.
    /// </summary>
    private void ParentLegReference(bool parent)
    {
        if (worldTargetReference == null) return;
        worldTargetReference.transform.parent = parent ? objectPool.transform : null;
    }
}