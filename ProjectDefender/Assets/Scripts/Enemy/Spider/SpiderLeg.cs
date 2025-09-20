using System;
using System.Collections;
using UnityEngine;

public class SpiderLeg : MonoBehaviour
{
    private EnemySpiderVisuals spiderVisuals;
    
    [SerializeField] private float legSpeed = 2.5f;
    [SerializeField] private float moveThreshold = .45f;
    private bool shouldMove;
    private bool canMove = true;
    private Coroutine moveCo;

    
    [Header("Leg setup")]
    [SerializeField] private SpiderLeg oppositeLeg;
    [SerializeField] private SpiderLegReference legRef;
    [SerializeField] private Transform actualTarget;
    [SerializeField] private Transform bottomLeg;
    [SerializeField] private Vector3 placementOffset;
    [SerializeField] private Transform worldTargetReference;

    private void Awake()
    {
        spiderVisuals = GetComponentInParent<EnemySpiderVisuals>();
        
        worldTargetReference = Instantiate(worldTargetReference, actualTarget.position, Quaternion.identity)
            .transform;
        worldTargetReference.gameObject.name = legRef.gameObject.name + "_world";
        
        legSpeed = spiderVisuals.legSpeed;
    }

    public void UpdateLeg()
    {
        actualTarget.position = worldTargetReference.position;// + placementOffset;
        shouldMove = Vector3.Distance(worldTargetReference.position, legRef.ContactPoint()) > moveThreshold;

        if (bottomLeg != null) bottomLeg.forward = Vector3.down;

        if (shouldMove && canMove)
        {
            if (moveCo != null) StopCoroutine(moveCo);
            
            StartCoroutine(LegMoveCo());
        }
    }

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
    
    private IEnumerator SpeedUpLegCo()
    {
        legSpeed = spiderVisuals.increasedLegSpeed;

        yield return new WaitForSeconds(1);

        legSpeed = spiderVisuals.legSpeed;
    }

    public void CanMove(bool enableMovement) => canMove = enableMovement;
}
