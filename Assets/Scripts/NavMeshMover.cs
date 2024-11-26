using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NavMeshMover : MonoBehaviour
{
    [Header("=== NAVIGATION SETTINGS ===")]
    [Tooltip("Target that the NavMeshAgent will move towards.")]
    private Transform target;  // Now private to find automatically

    private NavMeshAgent navMeshAgent;

    [Header("=== ANIMATION SETTINGS ===")]
    [Tooltip("Animator variable to update based on agent speed.")]
    public string animatorSpeedVariable = "Speed";

    private Animator animator;

    private void Awake()
    {
        // Get references to required components
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Automatically find the player with the "Player" tag
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            target = player.transform; // Assign the player as the target
        }
        else
        {
            Debug.LogError("Player with 'Player' tag not found in the scene.");
        }
    }

    private void Update()
    {
        // Check if a target has been assigned
        if (target != null)
        {
            navMeshAgent.SetDestination(target.position); // Move towards the target (player)
        }

        // Update the Animator variable with the current agent speed
        UpdateAnimator();
    }

    /// <summary>
    /// Updates the animator's speed variable based on the NavMeshAgent's velocity.
    /// </summary>
    private void UpdateAnimator()
    {
        if (animator != null && !string.IsNullOrEmpty(animatorSpeedVariable))
        {
            // Calculate the current speed of the NavMeshAgent
            float speed = navMeshAgent.velocity.magnitude;

            // Update the Animator variable
            animator.SetFloat(animatorSpeedVariable, speed);
        }
    }

    /// <summary>
    /// Sets a new target for the NavMeshAgent to move towards.
    /// </summary>
    /// <param name="newTarget">The new Transform target.</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Get the current target of the NavMeshAgent.
    /// </summary>
    /// <returns>The current target's Transform.</returns>
    public Transform GetCurrentTarget()
    {
        return target;
    }
}
