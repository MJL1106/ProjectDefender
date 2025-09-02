using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 120;

    private float smoothTime = 0.1f;
    private Vector3 movementVelocity = Vector3.zero;
    
    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = transform.position;
        
        float vInput = Input.GetAxisRaw("Vertical");
        float hInput = Input.GetAxisRaw("Horizontal");

        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        
        if (vInput != 0) targetPosition += flatForward * (vInput * movementSpeed * Time.deltaTime);

        if (hInput != 0) targetPosition += transform.right * (hInput * movementSpeed * Time.deltaTime);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref movementVelocity, smoothTime);
    }
}
