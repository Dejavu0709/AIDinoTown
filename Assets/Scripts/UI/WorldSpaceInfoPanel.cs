using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Component to attach to a 3D object to display information above it
/// </summary>
public class WorldSpaceInfoPanel : MonoBehaviour
{
    [Tooltip("Reference to the UI panel prefab to instantiate")]
    public GameObject uiPanelPrefab;

    [Tooltip("Canvas to parent the UI panel to")]
    public Canvas targetCanvas;

    [Tooltip("Offset from the object's position")]
    public Vector3 offset = new Vector3(0, 2.0f, 0);

    [Tooltip("Whether to always face the camera")]
    public bool faceCamera = true;

    [Tooltip("Whether to smoothly follow the target")]
    public bool smoothFollow = true;

    [Tooltip("Follow speed when smooth follow is enabled")]
    public float followSpeed = 10f;

    private GameObject instantiatedPanel;
    private UIWorldSpacePanel worldSpacePanel;

    private void Start()
    {
        // Find canvas if not assigned
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas == null)
            {
                Debug.LogError("No Canvas found in the scene! Please assign a target Canvas.");
                return;
            }
        }

        // Create the panel
        CreatePanel();
    }

    /// <summary>
    /// Creates and initializes the UI panel
    /// </summary>
    private void CreatePanel()
    {
        if (uiPanelPrefab == null)
        {
            Debug.LogError("UI Panel Prefab not assigned!");
            return;
        }

        // Instantiate the panel as a child of the canvas
        instantiatedPanel = Instantiate(uiPanelPrefab, targetCanvas.transform);
        
        // Get or add the UIWorldSpacePanel component
        worldSpacePanel = instantiatedPanel.GetComponent<UIWorldSpacePanel>();
        if (worldSpacePanel == null)
        {
            worldSpacePanel = instantiatedPanel.AddComponent<UIWorldSpacePanel>();
        }

        // Configure the panel
        worldSpacePanel.target = this.transform;
        worldSpacePanel.offset = this.offset;
        worldSpacePanel.faceCamera = this.faceCamera;
        worldSpacePanel.smoothFollow = this.smoothFollow;
        worldSpacePanel.followSpeed = this.followSpeed;

        // Initially hide the panel
        HidePanel();
    }

    /// <summary>
    /// Shows the panel
    /// </summary>
    public void ShowPanel()
    {
        if (instantiatedPanel != null)
        {
            instantiatedPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the panel
    /// </summary>
    public void HidePanel()
    {
        if (instantiatedPanel != null)
        {
            instantiatedPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Sets text content if the panel has a Text or TextMeshProUGUI component
    /// </summary>
    /// <param name="content">The text content to display</param>
    public void SetTextContent(string content)
    {
        if (instantiatedPanel == null)
            return;

        // Try to find Text component
        Text textComponent = instantiatedPanel.GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = content;
            return;
        }

        // Try to find TextMeshProUGUI component
        TextMeshProUGUI tmpComponent = instantiatedPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpComponent != null)
        {
            tmpComponent.text = content;
        }
    }

    private void OnDestroy()
    {
        // Clean up the instantiated panel when this object is destroyed
        if (instantiatedPanel != null)
        {
            Destroy(instantiatedPanel);
        }
    }
}
