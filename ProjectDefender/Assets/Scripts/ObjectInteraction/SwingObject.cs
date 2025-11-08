using System;
using UnityEngine;

/// <summary>
/// Continuously swings an object back and forth along a specified axis.
/// Uses a sine wave for smooth oscillation.
/// </summary>
public class SwingObject : MonoBehaviour
{
    [Header("Swing Settings")] 
    [SerializeField] private Vector3 swingAxis; // The axis around which to swing
    [SerializeField] private float swingDegree = 10; // Maximum angle of the swing in degrees
    [SerializeField] private float swingSpeed = 1; // Speed of the swing oscillation

    private Quaternion startRotation;

    private void Start()
    {
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        // Calculate the current angle using a sine wave
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingDegree;

        // Apply the rotation relative to the initial starting rotation
        transform.localRotation = startRotation * Quaternion.AngleAxis(angle, swingAxis.normalized);
    }
}