using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Panning")]
    [SerializeField] private float moveSpeed = 20f;

    [Header("Zooming")]
    [SerializeField] private float zoomSpeed = 100f;
    [SerializeField] private float minZoomHeight = 10f;
    [SerializeField] private float maxZoomHeight = 80f;

    [Header("Y Axis Rotation")]
    [SerializeField] private float yRoataionSnapAngle = 45f;

    [Header("X Axis Rotation")]
    [SerializeField] private float xRotationSnapAngle = 30f;
    [SerializeField] [Range(30f, 90f)] private float minRotateAngle = 30f;
    [SerializeField] [Range(30f, 90f)] private float maxRotateAngle = 90f;

    [Header("Key")]
    [SerializeField] private KeyCode rotationModifierKey = KeyCode.LeftControl;
    [SerializeField] private float rotationSensitivity = 0.5f;


    [Header("Common Settiongs")]
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float yDragThreshold = 100f;
    [SerializeField] private float xDragThreshold = 200f;

    private float targetYRotation = 0f;
    private float targetXRotation = 90f;
    private Vector3 dragStartPosition;
    private bool isDragging = false;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        targetXRotation = maxRotateAngle;
    }

    void Update()
    {
        HandlePanning();
        HandleRotation();
        HandleZoom();
    }

    private void HandlePanning()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 up = transform.up;
        Vector3 right = transform.right;
        up.y = 0;
        right.y = 0;
        up.Normalize();
        right.Normalize();

        Vector3 moveDirection = (up * verticalInput + right * horizontalInput);
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
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

        targetXRotation = Mathf.Clamp(targetXRotation, minRotateAngle, maxRotateAngle);
        Quaternion targetRotation = Quaternion.Euler(targetXRotation, targetYRotation, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void HandleZoom()
    {
        float scrollValue = Input.GetAxis("Mouse ScrollWheel");
        if (scrollValue != 0)
        {
            Vector3 moveDirection = transform.forward * scrollValue * zoomSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + moveDirection;

            if (newPosition.y >= minZoomHeight && newPosition.y <= maxZoomHeight)
            {
                transform.position = newPosition;
            }
        }
    }
}
