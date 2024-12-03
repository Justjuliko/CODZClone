using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public class Zombie : MonoBehaviour
{
    [Header("=== ZOMBIE SETTINGS ===")]
    public float maxHealth = 100f;  // Maximum health of the zombie
    public float health = 100f;  // Current health of the zombie
    private NavMeshAgent agent;  // The NavMeshAgent for movement
    private ZombieSpawner spawner;  // The spawner that controls the zombie's behavior
    private Animator animator;  // The animator for zombie animations
    private CapsuleCollider capsuleCollider;  // The collider for the zombie
    public float originalSpeed = 1;  // The original speed of the zombie

    private Player playerScript;  // Reference to the player script

    [Header("=== DAMAGE SETTINGS ===")]
    public float damageAmount = 10f;  // Amount of damage the zombie deals
    public float damageRange = 2f;  // Range at which the zombie can attack
    public float attackDelay = 2f;  // Time between attacks

    private Transform player;  // Reference to the player's transform
    private float attackTimer = 0f;  // Timer to manage attack delay
    private bool playerInRange = false;  // Whether the player is in range for an attack

    [Header("=== NAVIGATION SETTINGS ===")]
    private Transform target;  // Target the zombie is moving towards

    [Header("=== ANIMATION SETTINGS ===")]
    public string animatorSpeedVariable = "speed";  // Animator variable for speed
    public string animatorAttackVariable = "attack";  // Animator variable for attack

    [Header("=== AUDIO SETTINGS ===")]
    [Tooltip("Array of looping sounds to play randomly.")]
    public AudioClip[] loopSounds;  // Array to store multiple loop sounds
    private AudioSource loopAudioSource;  // AudioSource for looping sounds
    public float minLoopInterval = 5f;  // Minimum interval between loop sounds
    public float maxLoopInterval = 10f;  // Maximum interval between loop sounds

    [Tooltip("Sound to play when the zombie attacks.")]
    public AudioClip attackSound;  // Sound for when the zombie attacks
    private AudioSource attackAudioSource;  // AudioSource for the attack sound

    [Tooltip("Sound to play when the zombie spawns.")]
    public AudioClip spawnSound;  // Sound to play when the zombie spawns
    private AudioSource spawnAudioSource;  // AudioSource for the spawn sound
    public float spawnSoundVolume = 1.0f;  // Volume of the spawn sound


    private void Awake()
    {
        // Required components
        agent = GetComponent<NavMeshAgent>();  // Getting the NavMeshAgent component
        animator = GetComponent<Animator>();  // Getting the Animator component
        capsuleCollider = GetComponent<CapsuleCollider>();  // Getting the CapsuleCollider component
        spawner = Object.FindFirstObjectByType<ZombieSpawner>();  // Find the zombie spawner in the scene

        // Configure AudioSources
        loopAudioSource = gameObject.AddComponent<AudioSource>();  // Adding AudioSource for loop sounds
        attackAudioSource = gameObject.AddComponent<AudioSource>();  // Adding AudioSource for attack sound
        spawnAudioSource = gameObject.AddComponent<AudioSource>();  // Adding AudioSource for spawn sound

        // Configure loop AudioSource
        loopAudioSource.loop = false;  // Set loop to false for one-shot sounds
        loopAudioSource.playOnAwake = false;  // Do not play immediately on awake

        // Configure attack AudioSource
        attackAudioSource.clip = attackSound;  // Set the attack sound clip
        attackAudioSource.loop = false;  // Set loop to false for attack sound
        attackAudioSource.playOnAwake = false;  // Do not play immediately on awake

        // Configure spawn AudioSource
        spawnAudioSource.clip = spawnSound;  // Set the spawn sound clip
        spawnAudioSource.loop = false;  // Set loop to false for spawn sound
        spawnAudioSource.playOnAwake = false;  // Do not play immediately on awake
        spawnAudioSource.volume = spawnSoundVolume;  // Assign the spawn sound volume

        // Enable spatial audio
        ConfigureSpatialAudio(loopAudioSource);
        ConfigureSpatialAudio(attackAudioSource);
        ConfigureSpatialAudio(spawnAudioSource);
    }

    // Configure the spatial audio settings for the given AudioSource
    private void ConfigureSpatialAudio(AudioSource audioSource)
    {
        audioSource.spatialBlend = 1f;  // 100% spatial (3D sound)
        audioSource.minDistance = 2f;  // Max volume at this distance
        audioSource.maxDistance = 15f;  // Min volume beyond this distance
        audioSource.rolloffMode = AudioRolloffMode.Linear;  // Linear attenuation
    }

    private void Start()
    {
        health = maxHealth;  // Set health to max health at the start

        // Find the player object
        player = GameObject.Find("FirstPersonController")?.transform;

        if (player != null)
        {
            target = player;  // Set the player as the target
            playerScript = player.GetComponent<Player>();  // Get the player script
        }
        else
        {
            Debug.LogError("Player not found in the scene.");
        }

        // Start playing random loop sounds
        StartCoroutine(PlayRandomLoopSound());
    }

    private void Update()
    {
        if (target != null && !IsDead())
        {
            MoveTowardsTarget();  // Move towards the player
            HandlePlayerDamage();  // Handle damage to the player
        }

        UpdateAnimator();  // Update the animator with the current speed
    }

    // Take damage and decrease health
    public void TakeDamage(float damage)
    {
        health -= damage;  // Decrease health by damage amount

        if (health <= 0)
        {
            Die();  // If health reaches 0, the zombie dies
        }
    }

    private void Die()
    {
        agent.speed = 0;  // Stop movement when dying
        animator.SetBool("die", true);  // Trigger the "die" animation
        animator.SetBool(animatorAttackVariable, false);  // Stop attacking animation
        capsuleCollider.enabled = false;  // Disable the collider

        if (playerScript != null)
        {
            playerScript.AddPoints(60);  // Reward the player with points
        }

        StartCoroutine(WaitAndDeactivate(3f));  // Wait for a few seconds before deactivating the zombie
    }

    private IEnumerator WaitAndDeactivate(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);  // Wait for the specified time
        gameObject.SetActive(false);  // Deactivate the zombie object

        if (spawner != null)
        {
            spawner.ZombieKilled();  // Inform the spawner that the zombie was killed
        }

        agent.speed = originalSpeed;  // Reset the zombie's speed
        animator.SetBool("die", false);  // Stop the "die" animation
        health = maxHealth;  // Reset the health to max
        capsuleCollider.enabled = true;  // Re-enable the collider
    }

    // Handle damage to the player when in range
    private void HandlePlayerDamage()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);  // Get distance to player

            if (distanceToPlayer <= damageRange)
            {
                if (!playerInRange)
                {
                    playerInRange = true;  // Player is now in range
                    attackTimer = attackDelay;  // Reset attack timer
                    animator.SetBool(animatorAttackVariable, true);  // Trigger attack animation
                }

                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;  // Decrease attack timer
                }

                if (attackTimer <= 0)
                {
                    playerScript.TakeDamage(damageAmount);  // Deal damage to the player
                    attackAudioSource.Play();  // Play attack sound
                    attackTimer = attackDelay;  // Reset attack timer
                }
            }
            else
            {
                if (playerInRange)
                {
                    playerInRange = false;  // Player is out of range
                    attackTimer = attackDelay;  // Reset attack timer
                    animator.SetBool(animatorAttackVariable, false);  // Stop attack animation
                }
            }
        }
    }

    // Move towards the target
    private void MoveTowardsTarget()
    {
        agent.SetDestination(target.position);  // Set the target position for the NavMeshAgent
    }

    // Update the animator's speed parameter
    private void UpdateAnimator()
    {
        if (animator != null && !string.IsNullOrEmpty(animatorSpeedVariable))
        {
            float speed = agent.velocity.magnitude;  // Get the current speed of the zombie
            animator.SetFloat(animatorSpeedVariable, speed);  // Update the animator speed
        }
    }

    // Set a new target for the zombie to follow
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;  // Set the new target
    }

    // Get the current target the zombie is following
    public Transform GetCurrentTarget()
    {
        return target;  // Return the current target
    }

    // Check if the zombie is dead
    private bool IsDead()
    {
        return animator.GetBool("die") || health <= 0;  // Check if the zombie is dead
    }

    // Play a random looping sound at intervals
    private IEnumerator PlayRandomLoopSound()
    {
        while (true)
        {
            float randomDelay = Random.Range(minLoopInterval, maxLoopInterval);  // Randomize the delay
            yield return new WaitForSeconds(randomDelay);  // Wait for the randomized delay

            if (loopSounds.Length > 0)
            {
                // Select a random loop sound
                int randomIndex = Random.Range(0, loopSounds.Length);
                loopAudioSource.clip = loopSounds[randomIndex];
                loopAudioSource.Play();  // Play the selected loop sound

                // Wait until the sound finishes
                yield return new WaitForSeconds(loopAudioSource.clip.length);
            }
        }
    }

    // Handle the spawn animation and sound
    public IEnumerator HandleSpawn()
    {
        agent.speed = 0;  // Stop movement during spawn
        animator.SetBool("spawn", true);  // Trigger the "spawn" animation

        // Play the spawn sound if it's set
        if (spawnSound != null)
        {
            spawnAudioSource.gameObject.SetActive(true);  // Activate the spawn sound AudioSource
            spawnAudioSource.enabled = true;  // Enable the AudioSource
            spawnAudioSource.Play();  // Play the spawn sound
        }

        yield return new WaitForSeconds(1.4f);  // Wait for the spawn animation to finish
        animator.SetBool("spawn", false);  // Stop the "spawn" animation

        if (spawnAudioSource.isPlaying)
        {
            spawnAudioSource.Stop();  // Stop the spawn sound if it's still playing
        }

        agent.speed = originalSpeed;  // Reset the zombie's speed after spawning
    }
}
