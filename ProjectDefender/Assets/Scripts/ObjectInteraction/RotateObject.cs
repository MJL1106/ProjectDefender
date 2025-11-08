using System;
using UnityEngine;

/// <summary>
/// Continuously rotates an object around a specified vector.
/// Speed can be adjusted publicly.
/// </summary>
public class RotateObject : MonoBehaviour
{
    [SerializeField] private Vector3 rotationVector; // The axis and magnitude of rotation
    [SerializeField] private float rotationSpeed; // Multiplier for the rotation speed


    private void Update()
    {
        float newRotationSpeed = rotationSpeed * 100;
        transform.Rotate(rotationVector * newRotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Updates the rotation speed.
    /// </summary>
    public void AdjustRotationSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
    }
}