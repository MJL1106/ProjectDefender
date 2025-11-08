using System;
using UnityEngine;
using UnityEngine.Rendering;
using Screen = UnityEngine.Device.Screen;

/// <summary>
/// Handles player camera controls including WASD movement, mouse rotation, zoom, and edge panning.
/// Uses smooth damping for all movements and enforces boundary limits around level center.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private bool canControll;
    [SerializeField] private Vector3 levelCentrePoint;
    [SerializeField] private float maxDistanceFromCentre;
    
    [Header("Movement Details")] 
    [SerializeField] private float movementSpeed = 200;
    [SerializeField] private float mouseMovementSpeed = 200;

    [Header("Edge Movement Details")] 
    [SerializeField] private float edgeThreshold = 10; // Pixel distance from screen edge to trigger movement
    [SerializeField] private float edgeMovementSpeed = 10;
    private float screenWidth;
    private float screenHeight;
    
    [Header("Rotation details")] 
    [SerializeField] private Transform focusPoint;
    [SerializeField] private float maxFocusPointDistance = 15;
    [SerializeField] private float rotationSpeed = 200;
    
    [Space] 
    private float pitch;
    [SerializeField] private float minPitch = 46f;
    [SerializeField] private float maxPitch = 85f;

    [Header("Zoom Details")]
    [SerializeField] private float zoomSpeed = 200;
    [SerializeField] private float minZoom = 3;
    [SerializeField] private float maxZoom = 15;
    
    private float smoothTime = 0.1f; // Smoothing duration for all camera movements
    private Vector3 movementVelocity = Vector3.zero;
    private Vector3 mouseMovementVelocity = Vector3.zero;
    private Vector3 edgeMovementVelocity = Vector3.zero;
    private Vector3 zoomVelocity = Vector3.zero;
    private Vector3 lastMousePosition;

    private void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        
        pitch = transform.eulerAngles.x;
    }

    private void Update()
    {
        if (!canControll) return;
        
        HandleRotation();
        HandleZoom();
        HandleMouseMovement();
        HandleMovement();
        //HandleEdgeMovement();

        // Update focus point for camera rotation pivot
        focusPoint.position = transform.position + (transform.forward * GetFocusPointDistance());
    }

    public void EnableCameraControlls(bool enable) => canControll = enable;
    
    public float AdjustPitchValue(float value) => pitch = value;

    /// <summary>
    /// Adjusts WASD movement speed. Called from settings menu.
    /// </summary>
    public float AdjustKeyboardSensitivity(float value) => movementSpeed = value;

    /// <summary>
    /// Adjusts middle mouse drag speed. Called from settings menu.
    /// </summary>
    public float AdjustMouseSensitivity(float value) => mouseMovementSpeed = value;
    
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoomDirection = transform.forward * scroll * zoomSpeed;
        Vector3 targetPosition = transform.position + zoomDirection;

        // Prevent zooming beyond min/max bounds
        if (transform.position.y < minZoom && scroll > 0) return;
        if (transform.position.y > maxZoom && scroll < 0) return;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref zoomVelocity, smoothTime);
    }

    /// <summary>
    /// Calculates distance to ground for focus point positioning.
    /// Uses raycast to detect actual ground distance or returns max distance.
    /// </summary>
    private float GetFocusPointDistance()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxFocusPointDistance))
        {
            return hit.distance;
        }

        return maxFocusPointDistance;
    }

    /// <summary>
    /// Handles right-click camera rotation around focus point.
    /// Clamps vertical rotation (pitch) to prevent camera flipping.
    /// </summary>
    private void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float horizontalRotation = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float verticalRotation = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            pitch -= verticalRotation;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            
            transform.RotateAround(focusPoint.position, Vector3.up, horizontalRotation);
            
            // Calculate the angle difference and apply it
            float currentPitch = transform.eulerAngles.x;
            // Handle angle wrapping (Unity uses 0-360)
            if (currentPitch > 180) currentPitch -= 360;
            float pitchDifference = pitch - currentPitch;
            
            transform.RotateAround(focusPoint.position, transform.right, pitchDifference);
            
            transform.LookAt(focusPoint);
        }
    }

    /// <summary>
    /// Handles WASD keyboard movement with boundary enforcement.
    /// Projects movement onto horizontal plane to prevent altitude changes.
    /// </summary>
    private void HandleMovement()
    {
        Vector3 targetPosition = transform.position;
        
        float vInput = Input.GetAxisRaw("Vertical");
        float hInput = Input.GetAxisRaw("Horizontal");

        if (vInput == 0 && hInput == 0) return;

        // Flatten forward vector to prevent Y-axis movement
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        
        if (vInput != 0) targetPosition += flatForward * (vInput * movementSpeed * Time.deltaTime);
        if (hInput != 0) targetPosition += transform.right * (hInput * movementSpeed * Time.deltaTime);
        
        // Enforce circular boundary around level center
        if (Vector3.Distance(levelCentrePoint, targetPosition) > maxDistanceFromCentre)
        {
            targetPosition = levelCentrePoint +
                             (targetPosition - levelCentrePoint).normalized * maxDistanceFromCentre;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref movementVelocity, smoothTime);
    }

    /// <summary>
    /// Handles middle mouse button drag movement.
    /// Inverts mouse delta for intuitive "grab and drag" feel.
    /// </summary>
    private void HandleMouseMovement()
    {
        if (Input.GetMouseButtonDown(2)) lastMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(2))
        {
            Vector3 positionDifference = Input.mousePosition - lastMousePosition;
            Vector3 moveRight = transform.right * (-positionDifference.x) * mouseMovementSpeed * Time.deltaTime;
            Vector3 moveForward = transform.forward * (-positionDifference.y) * mouseMovementSpeed * Time.deltaTime;

            // Prevent Y-axis movement
            moveRight.y = 0;
            moveForward.y = 0;

            Vector3 movement = moveRight + moveForward;
            Vector3 targetPosition = transform.position + movement;

            // Enforce boundary limits
            if (Vector3.Distance(levelCentrePoint, targetPosition) > maxDistanceFromCentre)
            {
                targetPosition = levelCentrePoint +
                                 (targetPosition - levelCentrePoint).normalized * maxDistanceFromCentre;
            }

            transform.position =
                Vector3.SmoothDamp(transform.position, targetPosition, ref mouseMovementVelocity, smoothTime);
            
            lastMousePosition = Input.mousePosition;
        }
    }

    /// <summary>
    /// Handles edge scrolling when mouse near screen borders.
    /// Currently disabled in Update() but implementation preserved.
    /// </summary>
    private void HandleEdgeMovement()
    {
        Vector3 targetPosition = transform.position;
        Vector3 mousePosition = Input.mousePosition;
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        if (mousePosition.x > screenWidth - edgeThreshold) 
            targetPosition += transform.right * edgeMovementSpeed * Time.deltaTime;

        if (mousePosition.x < edgeThreshold) 
            targetPosition -= transform.right * edgeMovementSpeed * Time.deltaTime;

        if (mousePosition.y > screenHeight - edgeThreshold)
            targetPosition += flatForward * edgeMovementSpeed * Time.deltaTime;
        
        if (mousePosition.y < screenHeight - edgeThreshold)
            targetPosition -= flatForward * edgeMovementSpeed * Time.deltaTime;
        
        if (Vector3.Distance(levelCentrePoint, targetPosition) > maxDistanceFromCentre)
        {
            targetPosition = levelCentrePoint +
                             (targetPosition - levelCentrePoint).normalized * maxDistanceFromCentre;
        }

        transform.position =
            Vector3.SmoothDamp(transform.position, targetPosition, ref edgeMovementVelocity, smoothTime);
    }
}