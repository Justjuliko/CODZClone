using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ZombieSpawner : MonoBehaviour
{
    [Header("=== ZOMBIE SETTINGS ===")]
    [Tooltip("List of zombie prefabs to spawn.")]
    public List<GameObject> zombiePrefabs;

    [Tooltip("Maximum number of zombies alive at the same time.")]
    private int maxZombies = 6;  // Initial max zombies per round, starts at 6

    [Tooltip("Base spawn interval (time between spawns in seconds).")]
    public float baseSpawnInterval = 2f;

    [Header("=== SPAWN POINTS ===")]
    [Tooltip("List of spawn points where zombies can appear.")]
    public List<Transform> spawnPoints;

    [Header("=== ROUND SETTINGS ===")]
    [Tooltip("Initial number of kills required to advance to the next round.")]
    public int baseKillsToAdvance = 10;

    [Tooltip("Amount by which zombie max health increases each round.")]
    public float healthIncreasePerRound = 50f;

    [Tooltip("Maximum health zombies can reach.")]
    public float maxZombieHealth = 400f;

    [Tooltip("Amount by which zombie speed increases each round.")]
    public float speedIncreasePerRound = 0.5f;

    [Tooltip("Maximum speed zombies can reach.")]
    public float maxZombieSpeed = 5f;

    [Header("=== DEBUG INFO ===")]
    [Tooltip("Current number of zombies alive.")]
    [ReadOnly] public int zombiesAlive;

    [Tooltip("Total number of zombies killed by the player.")]
    [ReadOnly] public int zombiesKilled;

    [Tooltip("Current round number.")]
    [ReadOnly] public int currentRound = 1;

    [Tooltip("Current spawn interval between zombies (in seconds).")]
    [ReadOnly] public float currentSpawnInterval;  // Show the current spawn interval in the inspector

    [Tooltip("Current maximum zombies alive at once.")]
    [ReadOnly] public int maxZombiesCurrent;  // Display current max zombies

    [Tooltip("Number of zombies required to advance to the next round.")]
    [ReadOnly] public int killsToAdvanceCurrent;  // Display kills required for next round

    [Tooltip("Text for displaying the current round.")]
    public TMPro.TMP_Text roundText; // NEW: Text for displaying current round

    private List<GameObject> zombiePool = new List<GameObject>();
    private int killsThisRound = 0;
    private float spawnTimer;
    private bool isBetweenRounds = false;  // NEW: Flag to indicate if we're between rounds

    // Number of zombies the player needs to kill to advance each round
    private int killsToAdvanceThisRoundInternal;
    private float roundDelayTimer = 0f; // NEW: Timer for the delay between rounds

    private void Start()
    {
        currentSpawnInterval = baseSpawnInterval;
        InitializeZombiePool();
        killsToAdvanceThisRoundInternal = baseKillsToAdvance;  // Set initial kills to advance
        UpdateRoundText(); // Initialize round display
    }

    private void Update()
    {
        // Handle the delay between rounds
        if (isBetweenRounds)
        {
            roundDelayTimer -= Time.deltaTime;
            if (roundDelayTimer <= 0)
            {
                isBetweenRounds = false;
                spawnTimer = currentSpawnInterval; // Reset spawn timer to start spawning zombies
            }
            return;  // Don't spawn zombies until the delay is over
        }

        // Determine the number of remaining zombies needed to advance the round
        int remainingZombiesNeeded = killsToAdvanceThisRoundInternal - killsThisRound;
        int maxSpawnableZombies = Mathf.Min(maxZombies, remainingZombiesNeeded);

        // Generate zombies only if conditions allow
        if (zombiesAlive < maxSpawnableZombies && spawnTimer <= 0)
        {
            SpawnZombie();
            spawnTimer = currentSpawnInterval;
        }

        spawnTimer -= Time.deltaTime;
    }

    // Initialize the object pool for zombies
    private void InitializeZombiePool()
    {
        int initialPoolSize = 24; // Always have 24 zombies in the pool

        foreach (var prefab in zombiePrefabs)
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject zombie = Instantiate(prefab, transform);
                zombie.SetActive(false);
                zombiePool.Add(zombie);
            }
        }
    }

    // Spawn a zombie using the pooling system
    private void SpawnZombie()
    {
        foreach (var zombie in zombiePool)
        {
            if (!zombie.activeInHierarchy)
            {
                Transform spawnPoint = GetActiveSpawnPoint();
                if (spawnPoint != null)
                {
                    zombie.transform.position = spawnPoint.position;
                    zombie.transform.rotation = spawnPoint.rotation;
                    Zombie zombieScript = zombie.GetComponent<Zombie>();

                    if (zombieScript != null)
                    {
                        StartCoroutine(zombieScript.HandleSpawn());
                    }

                    zombie.SetActive(true);
                    zombiesAlive++;
                }
                break;
            }
        }
    }


    // Get a random active spawn point
    private Transform GetActiveSpawnPoint()
    {
        // Filter the spawn points to include only active ones
        List<Transform> activeSpawnPoints = new List<Transform>();
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.gameObject.activeInHierarchy)
            {
                activeSpawnPoints.Add(spawnPoint);
            }
        }

        // Return a random active spawn point, or null if none are active
        if (activeSpawnPoints.Count > 0)
        {
            return activeSpawnPoints[Random.Range(0, activeSpawnPoints.Count)];
        }

        return null;
    }

    // Call this function when a zombie is killed
    public void ZombieKilled()
    {
        zombiesKilled++;
        zombiesAlive--;
        killsThisRound++;

        if (killsThisRound >= killsToAdvanceThisRoundInternal)
        {
            AdvanceRound();
        }
    }

    // Advance to the next round and adjust settings
    private void AdvanceRound()
    {
        currentRound++;
        killsThisRound = 0;

        // Increase max zombies per round in steps of 3 until it reaches 24
        maxZombies = Mathf.Min(24, 6 + (currentRound - 1) * 3);  // Increment maxZombies by 3 per round, but cap at 24

        // Set the current max zombies for debugging
        maxZombiesCurrent = maxZombies;

        // Increase kills required to advance, but stop increasing after 90
        killsToAdvanceThisRoundInternal = Mathf.Min(90, Mathf.RoundToInt(baseKillsToAdvance + (currentRound - 1) * 6));  // Simple progression for kills

        // Set the current kills required to advance for debugging
        killsToAdvanceCurrent = killsToAdvanceThisRoundInternal;

        // Increase zombie stats
        IncreaseZombieStats();

        // Adjust the spawn interval, but stop decreasing after 0.7 seconds
        currentSpawnInterval = Mathf.Max(0.7f, baseSpawnInterval - 0.1f * (currentRound - 1)); // Decrease spawn interval as round increases

        UpdateRoundText(); // Update round display

        // Add a delay before starting to spawn zombies in the next round
        isBetweenRounds = true;
        roundDelayTimer = 1.5f; // 1.5 seconds delay
    }

    // Update the round text display (only show the round number)
    private void UpdateRoundText()
    {
        if (roundText != null)
        {
            roundText.text = $"{currentRound}"; // Show only the round number
        }
    }

    private void IncreaseZombieStats()
    {
        foreach (var zombie in zombiePool)
        {
            Zombie zombieScript = zombie.GetComponent<Zombie>();
            if (zombieScript != null)
            {
                // Increase max health within the allowed limit
                zombieScript.maxHealth = Mathf.Min(maxZombieHealth, zombieScript.maxHealth + healthIncreasePerRound);
                zombieScript.health = zombieScript.maxHealth; // Restore health to the updated max

                // Increase original speed within the allowed limit
                zombieScript.originalSpeed = Mathf.Min(maxZombieSpeed, zombieScript.originalSpeed + speedIncreasePerRound);
                zombieScript.GetComponent<NavMeshAgent>().speed = zombieScript.originalSpeed; // Update current speed
            }
        }
    }
}

// Attribute to make a field read-only in the Inspector
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
// Custom drawer for ReadOnly fields
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}
#endif
