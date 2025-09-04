using System;
using UnityEngine;
using UnityEngine.Rendering;
using Screen = UnityEngine.Device.Screen;

public class CameraController : MonoBehaviour
{
    [SerializeField] private bool canControll;
    [SerializeField] private Vector3 levelCentrePoint;
    [SerializeField] private float maxDistanceFromCentre;
    
    
    [Header("Movement Details")] 
    [SerializeField] private float movementSpeed = 200;
    [SerializeField] private float mouseMovementSpeed = 200;

    [Header("Edge Movement Details")] 
    [SerializeField] private float edgeThreshold = 10;
    [SerializeField] private float edgeMovementSpeed = 10;
    private float screenWidth;
    private float screenHeight;
    
    [Header("Rotation details")] 
    [SerializeField] private Transform focusPoint;
    [SerializeField] private float maxFocusPointDistance = 15;
    [SerializeField] private float rotationSpeed = 200;
    
    [Space] 
    private float pitch;
    [SerializeField] private float minPitch = 5f;
    [SerializeField] private float maxPitch = 85f;

    [Header("Zoom Details")]
    [SerializeField] private float zoomSpeed = 200;
    [SerializeField] private float minZoom = 3;
    [SerializeField] private float maxZoom = 15;
    
    
    private float smoothTime = 0.1f;
    private Vector3 movementVelocity = Vector3.zero;
    private Vector3 mouseMovementVelocity = Vector3.zero;
    private Vector3 edgeMovementVelocity = Vector3.zero;
    private Vector3 zoomVelocity = Vector3.zero;
    private Vector3 lastMousePosition;

    private void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }

    private void Update()
    {
        if (!canControll) return;
        
        HandleRotation();
        HandleZoom();
        HandleMouseMovement();
        HandleMovement();
        //HandleEdgeMovement();

        focusPoint.position = transform.position + (transform.forward * GetFocusPointDistance());
    }

    public void EnableCameraControlls(bool enable) => canControll = enable;
    
    public float AdjustPitchValue(float value) => pitch = value;

    public float AdjustKeyboardSensitivity(float value) => movementSpeed = value;

    public float AdjustMouseSensitivity(float value) => mouseMovementSpeed = value;
    
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoomDirection = transform.forward * scroll * zoomSpeed;
        Vector3 targetPosition = transform.position + zoomDirection;

        if (transform.position.y < minZoom && scroll > 0) return;
        if (transform.position.y > maxZoom && scroll < 0) return;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref zoomVelocity, smoothTime);
    }

    private float GetFocusPointDistance()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxFocusPointDistance))
        {
            return hit.distance;
        }

        return maxFocusPointDistance;
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float horizontalRotation = Input.GetAxis("Mouse X") * rotationSpeed *Time.deltaTime;
            float verticalRotation = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            pitch = Mathf.Clamp(pitch - verticalRotation, minPitch, maxPitch);
            
            transform.RotateAround(focusPoint.position, Vector3.up, horizontalRotation);
            transform.RotateAround(focusPoint.position, transform.right, pitch - transform.eulerAngles.x);
            
            transform.LookAt(focusPoint);
        }
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = transform.position;
        
        float vInput = Input.GetAxisRaw("Vertical");
        float hInput = Input.GetAxisRaw("Horizontal");

        if (vInput == 0 && hInput == 0) return;

        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        
        if (vInput != 0) targetPosition += flatForward * (vInput * movementSpeed * Time.deltaTime);

        if (hInput != 0) targetPosition += transform.right * (hInput * movementSpeed * Time.deltaTime);
        
        if (Vector3.Distance(levelCentrePoint, targetPosition) > maxDistanceFromCentre)
        {
            targetPosition = levelCentrePoint +
                             (targetPosition - levelCentrePoint).normalized * maxDistanceFromCentre;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref movementVelocity, smoothTime);
    }

    private void HandleMouseMovement()
    {
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 positionDifference = Input.mousePosition - lastMousePosition;
            Vector3 moveRight = transform.right * (-positionDifference.x) * mouseMovementSpeed * Time.deltaTime;
            Vector3 moveForward = transform.forward * (-positionDifference.y) * mouseMovementSpeed * Time.deltaTime;

            moveRight.y = 0;
            moveForward.y = 0;

            Vector3 movement = moveRight + moveForward;
            Vector3 targetPosition = transform.position + movement;

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
