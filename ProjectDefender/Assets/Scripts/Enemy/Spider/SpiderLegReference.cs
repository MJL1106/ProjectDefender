using System;
using UnityEngine;

/// <summary>
/// Provides ground contact point for spider leg IK system.
/// Raycasts down to find exact ground position for leg placement.
/// </summary>
public class SpiderLegReference : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float contactPointRadius = .05f;

    /// <summary>
    /// Returns ground contact point directly below this reference.
    /// Falls back to current position if no ground detected.
    /// </summary>
    public Vector3 ContactPoint()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, whatIsGround))
            return hitInfo.point;

        return transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, ContactPoint());
        Gizmos.DrawWireSphere(ContactPoint(), contactPointRadius);
    }
}