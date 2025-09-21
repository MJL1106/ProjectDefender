using System;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    // we need this script to detect waypoints

    private void Awake()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
