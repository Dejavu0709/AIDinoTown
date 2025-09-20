using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages UI panels that follow 3D objects in world space
/// </summary>
public class UIWorldSpacePanel : MonoBehaviour
{
    [Tooltip("The target transform to follow")]
    public Transform target;

    [Tooltip("Offset from the target position")]
    public Vector3 offset = new Vector3(0, 2.0f, 0);

    [Tooltip("Whether to always face the camera")]
    public bool faceCamera = true;

    [Tooltip("Whether to smoothly follow the target")]
    public bool smoothFollow = true;

    [Tooltip("Follow speed when smooth follow is enabled")]
    public float followSpeed = 10f;

    private Camera mainCamera;
    private RectTransform rectTransform;
    private Canvas parentCanvas;

    private void Awake()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas == null)
        {
            Debug.LogError("UIWorldSpacePanel must be a child of a Canvas!");
        }
    }

    private void LateUpdate()
    {
        if (target == null || mainCamera == null || parentCanvas == null)
            return;

        // Get the world position with offset
        Vector3 targetPosition = target.position + offset;
        
        // Convert world position to screen position
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);

        // Check if the target is behind the camera
        if (screenPosition.z < 0)
        {
            // Hide the panel if the target is behind the camera
            //if (gameObject.activeSelf)
            //    gameObject.SetActive(false);
            return;
        }
        else if (!gameObject.activeSelf)
        {
            // Show the panel if it was hidden
            gameObject.SetActive(true);
        }

        // If using a screen space canvas
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // For screen space overlay, we can directly use screen position
            if (smoothFollow)
            {
                rectTransform.position = Vector3.Lerp(rectTransform.position, screenPosition, Time.deltaTime * followSpeed);
            }
            else
            {
                rectTransform.position = screenPosition;
            }
        }
        // If using a world space canvas
        else if (parentCanvas.renderMode == RenderMode.WorldSpace)
        {
            // For world space, position directly in world space
            if (smoothFollow)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
            }
            else
            {
                transform.position = targetPosition;
            }

            // Make the panel face the camera if needed
            if (faceCamera)
            {
                transform.rotation = mainCamera.transform.rotation;
            }
        }
        // If using a camera space canvas
        else if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            // Convert screen position to world position for the canvas camera
            Vector3 worldPos = parentCanvas.worldCamera.ScreenToWorldPoint(screenPosition);
            
            if (smoothFollow)
            {
                rectTransform.position = Vector3.Lerp(rectTransform.position, worldPos, Time.deltaTime * followSpeed);
            }
            else
            {
                rectTransform.position = worldPos;
            }
        }
    }

    /// <summary>
    /// Set a new target to follow
    /// </summary>
    /// <param name="newTarget">The transform to follow</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Set a new offset from the target
    /// </summary>
    /// <param name="newOffset">The offset vector</param>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
}
