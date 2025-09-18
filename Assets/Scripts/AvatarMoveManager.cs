using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AvatarMoveManager : MonoBehaviour
{
    [Tooltip("List of transform points the avatar can move to")]
    public List<Transform> targetPoints = new List<Transform>();

    [Tooltip("Minimum time to wait at destination before moving to next point")]
    public float minWaitTime = 3f;

    [Tooltip("Maximum time to wait at destination before moving to next point")]
    public float maxWaitTime = 8f;

    [Tooltip("Distance threshold to consider destination reached")]
    public float destinationReachedThreshold = 0.5f;

    // Animation parameter names
    private const string ANIM_IS_WALKING = "isWalking";

    // Component references
    private AIDestinationSetter aiDestinationSetter;
    private IAstarAI aiController;
    private Animator animator;

    // State tracking
    private Transform currentTarget;
    private Transform dummyTarget;
    private bool isWaiting = false;

    private void Awake()
    {
        // Create a dummy target if needed
        dummyTarget = new GameObject("DummyTarget").transform;
        dummyTarget.SetParent(transform.parent);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get required components
        aiDestinationSetter = GetComponent<AIDestinationSetter>();
        aiController = GetComponent<IAstarAI>();
        animator = GetComponent<Animator>();

        if (aiDestinationSetter == null)
        {
            Debug.LogError("AIDestinationSetter component not found on " + gameObject.name);
            enabled = false;
            return;
        }

        if (aiController == null)
        {
            Debug.LogError("IAstarAI component not found on " + gameObject.name);
            enabled = false;
            return;
        }

        if (animator == null)
        {
            Debug.LogWarning("Animator component not found on " + gameObject.name + ". Animations will not play.");
        }

        if (targetPoints.Count == 0)
        {
            Debug.LogWarning("No target points assigned to " + gameObject.name);
            enabled = false;
            return;
        }

        // Start the movement cycle
        SelectNewDestination();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("isWaiting: " + isWaiting);
        // If we're waiting, don't do anything
        if (isWaiting)
        {
            //UpdateAnimationState(false);
            return;
        }

        // Check if we've reached the destination
        if (currentTarget != null && !aiController.pathPending &&
            (aiController.reachedEndOfPath || aiController.reachedDestination ||
             Vector3.Distance(transform.position, currentTarget.position) <= destinationReachedThreshold))
        {
            // We've reached the destination, start waiting
            StartCoroutine(WaitAtDestination());
            return;
        }

    }

    /// <summary>
    /// Selects a new random destination from the target points list
    /// Ensures the new target is different from the current one
    /// </summary>
    private void SelectNewDestination()
    {
        //Debug.Log("Selecting new destination");
        if (targetPoints.Count == 0) return;

        // If we only have one target point, we have no choice
        if (targetPoints.Count == 1)
        {
            currentTarget = targetPoints[0];
            aiDestinationSetter.target = currentTarget;
            isWaiting = false;
            return;
        }

        // Select a random target point that is different from the current one
        int randomIndex;
        int attempts = 0;
        Transform previousTarget = currentTarget; // Store current target to compare

        do
        {
            randomIndex = Random.Range(0, targetPoints.Count);
            currentTarget = targetPoints[randomIndex];
            attempts++;

            // Prevent infinite loop - if we've tried many times, just accept any target
            if (attempts >= 10) break;

        } while (currentTarget == previousTarget);

        // Set the destination for the AI
        if (aiDestinationSetter != null)
        {
            aiDestinationSetter.target = currentTarget;
        }
        //Debug.Log("Selected new destination: " + currentTarget.name);
        // Reset the waiting flag
        isWaiting = false;
                // Update animation state
        UpdateAnimationState(true);
    }

    /// <summary>
    /// Updates the animation state based on movement
    /// </summary>
    private void UpdateAnimationState(bool isMoving)
    {
        if (animator == null) return;

        // Check if we're moving
        //bool isMoving = aiController.velocity.sqrMagnitude > 0.1f;

        // Set the animation parameter
        //animator.SetBool(ANIM_IS_WALKING, isMoving);
        //Debug.Log("IsMoving: " + isMoving);
        if (isMoving)
            animator.SetInteger("State", 12);
        else
        {
            animator.SetInteger("State", 0);
            animator.SetTrigger("Reset");
        }
    }

    /// <summary>
    /// Coroutine to wait at the current destination before moving to the next one
    /// </summary>
    private IEnumerator WaitAtDestination()
    {
        // Set waiting flag
        isWaiting = true;

        // Stop movement by setting a dummy target at current position
        dummyTarget.position = transform.position;
        aiDestinationSetter.target = dummyTarget;

        // Update animation to idle
        UpdateAnimationState(false);

        // Wait for a random time
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);

        // Select a new destination
        SelectNewDestination();
    }

    private void OnDestroy()
    {
        // Clean up the dummy target
        if (dummyTarget != null)
        {
            Destroy(dummyTarget.gameObject);
        }
    }
}
