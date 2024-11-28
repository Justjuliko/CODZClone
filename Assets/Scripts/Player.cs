using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necesario para TextMeshPro
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor; // Necesario para CustomPropertyDrawer
#endif

public class Player : MonoBehaviour
{
    [Header("=== PLAYER STATS ===")]
    [Tooltip("Maximum health of the player.")]
    public float maxHealth = 100f;

    [Tooltip("Current health of the player.")]
    [CustomReadOnly] public float currentHealth;

    [Tooltip("Health regeneration rate per second.")]
    public float healthRegenRate = 5f;

    [Tooltip("Time in seconds without damage for health to start regenerating.")]
    public float regenDelay = 5f;

    [Tooltip("Time passed since last damage received.")]
    [CustomReadOnly] public float timeSinceLastDamage = 0f;

    [Header("=== UI ===")]
    [Tooltip("Player's health bar UI (Slider).")]
    public Slider healthBar;

    [Tooltip("TextMeshPro element to display points.")]
    public TMP_Text pointsText;

    [Header("=== POINT SYSTEM ===")]
    [Tooltip("Current points of the player.")]
    [CustomReadOnly] public int currentPoints = 0;

    [Tooltip("Initial points of the player.")]
    public int startingPoints = 0;

    private void Start()
    {
        // Initialize health and points
        currentHealth = maxHealth;
        currentPoints = startingPoints;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        UpdatePointsUI(); // Update points display at the start
    }

    private void Update()
    {
        // Update the time since the last damage and check for health regeneration
        if (timeSinceLastDamage < regenDelay)
        {
            timeSinceLastDamage += Time.deltaTime;
        }

        RegenerateHealth();
    }

    // Function to call when the player takes damage
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        timeSinceLastDamage = 0f; // Reset timer for health regen when damage is taken

        if (currentHealth <= 0f)
        {
            SceneManager.LoadScene("DeathMenu");
            currentHealth = 0f; // Prevent health from going below zero
        }

        UpdateHealthBar();
    }

    // Regenerates health if the player has not taken damage in the last 'regenDelay' seconds
    private void RegenerateHealth()
    {
        if (timeSinceLastDamage >= regenDelay && currentHealth < maxHealth)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth; // Prevent health from exceeding max health
            }
            UpdateHealthBar();
        }
    }

    // Updates the health bar UI
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }
    // Public method to add points to the player
    public void AddPoints(int points)
    {
        currentPoints += points;
        UpdatePointsUI(); // Update the points display
    }
    public void RemovePoints(int points)
    {
        currentPoints -= points;
        UpdatePointsUI(); // Update the points display
    }

    // Updates the points display in the TMP_Text element
    private void UpdatePointsUI()
    {
        if (pointsText != null)
        {
            pointsText.text = $"{currentPoints}";
        }
    }

    // Function to heal the player (can be used by other scripts to heal)
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth; // Prevent exceeding max health
        }
        UpdateHealthBar();
    }
}

// Attribute to make a field read-only in the Inspector
public class CustomReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
// Custom drawer for CustomReadOnly fields
[CustomPropertyDrawer(typeof(CustomReadOnlyAttribute))]
public class CustomReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}
#endif
