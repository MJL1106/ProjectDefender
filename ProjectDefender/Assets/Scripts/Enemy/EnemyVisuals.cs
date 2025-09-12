using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.Android;

public class EnemyVisuals : MonoBehaviour
{
    [SerializeField] protected Transform visuals;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float verticalRotationSpeed;

    protected virtual void Start()
    {
        
    }
    
    protected virtual void Update()
    {
        AlignWithSlope();
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
