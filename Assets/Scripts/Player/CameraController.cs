using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float movementSmoothness = 15f;
    [SerializeField] private float zoomSmoothness = 30f;

    [Header("Zooming")]
    [SerializeField] private float zoomSpeed = 20f;
    [SerializeField] private float minZoomHeight = 6f;
    [SerializeField] private float maxZoomHeight = 80f;

    [Header("Y Axis Rotation")]
    [SerializeField] private float yRoataionSnapAngle = 45f;

    [Header("X Axis Rotation")]
    [SerializeField] private float xRotationSnapAngle = 30f;
    [SerializeField] [Range(30f, 90f)] private float minRotateAngle = 30f;
    [SerializeField] [Range(30f, 90f)] private float maxRotateAngle = 90f;

    [Header("Key")]
    [SerializeField] private KeyCode rotationModifierKey = KeyCode.LeftControl;


    [Header("Common Settiongs")]
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float yDragThreshold = 100f;
    [SerializeField] private float xDragThreshold = 200f;

    private Vector3 targetPosition;
    private float targetYRotation = 0f;
    private float targetXRotation = 90f;

    private Vector3 dragStartPosition;
    private bool isDragging = false;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        targetXRotation = maxRotateAngle;
        transform.position = new Vector3(0f, 50f, 0f);
        targetPosition = transform.position;
    }

    private void Update()
    {
        HandlePanning();
        HandleRotation();
        HandleZoom();
    }

    private void LateUpdate()
    {
        float newX = Mathf.Lerp(transform.position.x, targetPosition.x, Time.deltaTime * movementSmoothness);
        float newZ = Mathf.Lerp(transform.position.z, targetPosition.z, Time.deltaTime * movementSmoothness);
        float newY = Mathf.Lerp(transform.position.y, targetPosition.y, Time.deltaTime * zoomSmoothness);

        transform.position = new Vector3(newX, newY, newZ);

        targetXRotation = Mathf.Clamp(targetXRotation, minRotateAngle, maxRotateAngle);
        Quaternion targetRotation = Quaternion.Euler(targetXRotation, targetYRotation, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void HandlePanning()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            verticalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            verticalInput = 1f;
        }

        Vector3 up = transform.up;
        Vector3 right = transform.right;
        up.y = 0;
        right.y = 0;

        Vector3 moveDirection = (up * verticalInput + right * horizontalInput);
        moveDirection.Normalize();
        targetPosition += moveSpeed * Time.deltaTime * moveDirection;
    }

    private void HandleRotation()
    {
        if (Input.GetKey(rotationModifierKey) && Input.GetMouseButtonDown(1))
        {
            isDragging = true;
            dragStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 dragDelta = Input.mousePosition - dragStartPosition;

            if (Mathf.Abs(dragDelta.x) > xDragThreshold)
            {
                targetYRotation += (dragDelta.x > 0 ? 1 : -1) * yRoataionSnapAngle;
                dragStartPosition = Input.mousePosition;
            }

            else if (Mathf.Abs(dragDelta.y) > yDragThreshold)
            {
                targetXRotation -= (dragDelta.y > 0 ? 1 : -1) * xRotationSnapAngle;
                dragStartPosition = Input.mousePosition;
            }
        }


    }

    private void HandleZoom()
    {
        float scrollValue = Input.GetAxis("Mouse ScrollWheel");
        if (scrollValue != 0)
        {
            Vector3 moveDirection = Vector3.down * scrollValue * zoomSpeed;
            Vector3 newTargetPosition = targetPosition + moveDirection;

            if (newTargetPosition.y >= minZoomHeight && newTargetPosition.y <= maxZoomHeight)
            {
                targetPosition = newTargetPosition;
            }
        }
    }
}
