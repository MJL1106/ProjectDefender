using System;
using UnityEngine;

/// <summary>
/// A marker script used by EnemyPortal to identify path waypoints.
/// Disables its own renderer on Awake for cleanup.
/// </summary>
public class Waypoint : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}