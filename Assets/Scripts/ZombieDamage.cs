using UnityEngine;

public class ZombieDamage : MonoBehaviour
{
    [Header("=== DAMAGE SETTINGS ===")]
    [Tooltip("Damage that the zombie deals when it touches the player.")]
    public float damageAmount = 10f;

    [Tooltip("Detection range to trigger damage.")]
    public float damageRange = 2f;  // Distance at which the zombie deals damage

    [Tooltip("Time in seconds before the zombie attacks once in range.")]
    public float attackDelay = 2f;  // Delay in seconds before attacking, adjustable from the Inspector

    private Transform player;  // Reference to the player
    private float attackTimer = 0f;  // Timer for how long to wait before attacking
    private bool playerInRange = false;  // Flag to check if player is in range

    private void Start()
    {
        // Find the player object by its tag
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player not found! Ensure the player has the 'Player' tag.");
        }
    }

    private void Update()
    {
        if (player != null)
        {
            // Calculate distance between the zombie and the player
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Check if the player is within the damage range
            if (distanceToPlayer <= damageRange)
            {
                // If player is in range, start the attack timer
                if (!playerInRange)
                {
                    playerInRange = true;
                    attackTimer = attackDelay; // Reset attack timer when player enters range
                }

                // Countdown the attack timer
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }

                // If timer reaches 0, attack the player
                if (attackTimer <= 0)
                {
                    player.GetComponent<Player>().TakeDamage(damageAmount);
                    attackTimer = attackDelay; // Reset attack timer
                }
            }
            else
            {
                // If player is out of range, reset the attack timer
                if (playerInRange)
                {
                    playerInRange = false;
                    attackTimer = attackDelay;  // Reset attack timer
                }
            }
        }
    }
}
