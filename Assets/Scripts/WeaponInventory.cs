using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInventory : MonoBehaviour
{
    [Header("=== INVENTORY SETTINGS ===")]
    [Tooltip("Current state: True = Using primary weapon, False = Using secondary weapon")]
    public bool slot1 = true; // Indicates which weapon slot is currently active (true = primary, false = secondary)

    [Header("=== DEBUG INFO ===")]
    [Tooltip("ID of the primary weapon in the inventory.")]
    public int primaryWeaponID = -1; // -1 indicates no weapon assigned to primary slot

    [Tooltip("ID of the secondary weapon in the inventory.")]
    public int secondaryWeaponID = -1; // -1 indicates no weapon assigned to secondary slot

    private GameObject[] weaponObjects; // Array to store references to child weapon objects

    [Header("=== INPUT SYSTEM ===")]
    [Tooltip("Input Action Asset for handling input.")]
    public InputActionAsset inputActions; // Reference to the InputActionAsset for handling inputs

    private InputAction nextWeaponAction; // Reference to the "next weapon" action input

    PlayerShooting playerShooting; // Reference to the PlayerShooting script
    WeaponStatsClass weaponStatsClass; // Reference to the WeaponStatsClass script

    private void Start()
    {
        weaponStatsClass = GetComponent<WeaponStatsClass>(); // Get the WeaponStatsClass component
        playerShooting = GetComponent<PlayerShooting>(); // Get the PlayerShooting component

        // Cache all child weapon objects
        weaponObjects = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            weaponObjects[i] = transform.GetChild(i).gameObject;
            weaponObjects[i].SetActive(false); // Ensure all weapons start as inactive
        }

        // Initialize inventory with the first available weapon, if there are any
        if (transform.childCount > 0)
        {
            primaryWeaponID = 0; // Default to the first child as the primary weapon
            ActivateWeapon(primaryWeaponID); // Activate the primary weapon
        }

        // Setup Input Action for "next weapon" action
        if (inputActions != null)
        {
            nextWeaponAction = inputActions.FindActionMap("Player").FindAction("next");
            if (nextWeaponAction != null)
            {
                nextWeaponAction.performed += _ => ToggleWeaponSlot(); // Add listener to switch weapons when action is performed
                nextWeaponAction.Enable(); // Enable the action
            }
            else
            {
                Debug.LogWarning("Action 'next' not found in Action Map 'Player'.");
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the "next weapon" action when the object is destroyed
        if (nextWeaponAction != null)
        {
            nextWeaponAction.performed -= _ => ToggleWeaponSlot();
        }
    }

    private void Update()
    {
        // Update the active weapon based on the current slot state
        UpdateActiveWeapon();
    }

    /// <summary>
    /// Activates a weapon and deactivates the other based on the current slot.
    /// </summary>
    private void UpdateActiveWeapon()
    {
        // If both the primary and secondary weapons have the same ID, keep both weapons active
        if (primaryWeaponID == secondaryWeaponID && primaryWeaponID != -1)
        {
            // Both should be active if they have the same ID
            weaponObjects[primaryWeaponID].SetActive(true);
        }
        else
        {
            // If the IDs are different, handle activation normally
            if (primaryWeaponID != -1) // Check if there is a primary weapon
                weaponObjects[primaryWeaponID].SetActive(slot1); // Activate primary if it's the active slot

            if (secondaryWeaponID != -1) // Check if there is a secondary weapon
                weaponObjects[secondaryWeaponID].SetActive(!slot1); // Activate secondary if it's the active slot
        }
    }

    /// <summary>
    /// Overwrite the current weapon in the active slot with a new weapon.
    /// </summary>
    /// <param name="newWeaponID">The ID of the new weapon to assign.</param>
    public void OverwriteCurrentWeapon(int newWeaponID)
    {
        if (newWeaponID < 0 || newWeaponID >= weaponObjects.Length)
        {
            Debug.LogError("Invalid weapon ID: " + newWeaponID);
            return;
        }

        // If the new weapon is the same as the secondary weapon, attempt to overwrite primary if it's not already assigned
        if (newWeaponID == secondaryWeaponID)
        {
            if (newWeaponID == primaryWeaponID)
            {
                Debug.LogWarning("Cannot overwrite: New weapon ID is the same as the primary weapon.");
                return; // Prevent overwriting if the ID is the same as the primary
            }
        }

        // If only one weapon exists (secondary weapon ID is -1)
        if (secondaryWeaponID == -1)
        {
            if (primaryWeaponID != -1)
                weaponObjects[primaryWeaponID].SetActive(false); // Deactivate the primary weapon if it exists

            secondaryWeaponID = newWeaponID; // Assign the new weapon to the secondary slot
            ToggleWeaponSlot(); // Switch to the new weapon slot
            weaponStatsClass.currentIdUpdate(); // Update the weapon stats
            playerShooting.RefillAmmo(); // Refill ammo for the new weapon
            Debug.Log("Overwrite");
        }
        else
        {
            // If both weapon slots have assigned weapons, replace the active one (primary or secondary)
            if (slot1) // If the primary weapon is active
            {
                if (primaryWeaponID != -1)
                    weaponObjects[primaryWeaponID].SetActive(false); // Deactivate the previous primary weapon

                primaryWeaponID = newWeaponID; // Assign the new weapon to the primary slot
                weaponStatsClass.currentIdUpdate();
                playerShooting.RefillAmmo();
                Debug.Log("Overwrite");
            }
            else // If the secondary weapon is active
            {
                if (secondaryWeaponID != -1)
                    weaponObjects[secondaryWeaponID].SetActive(false); // Deactivate the previous secondary weapon

                secondaryWeaponID = newWeaponID; // Assign the new weapon to the secondary slot
                weaponStatsClass.currentIdUpdate();
                playerShooting.RefillAmmo();
                Debug.Log("Overwrite");
            }
        }

        UpdateActiveWeapon(); // Ensure the active weapon is updated immediately
    }

    /// <summary>
    /// Toggle between the primary and secondary weapon slots only if both slots are occupied.
    /// </summary>
    private void ToggleWeaponSlot()
    {
        // Only toggle if both weapon slots have assigned weapons
        if (primaryWeaponID != -1 && secondaryWeaponID != -1)
        {
            slot1 = !slot1; // Switch between primary and secondary weapons
            playerShooting.saveOtherWeaponMaxAmmo(); // Save the ammo for the other weapon
            weaponStatsClass.currentIdUpdate(); // Update the weapon stats
            playerShooting.SwapAmmo(); // Swap ammo between weapons            
            UpdateActiveWeapon(); // Update active weapons immediately
            playerShooting.ApplyRotationShake(); // Apply shake effect for weapon switch
        }
        else
        {
            Debug.Log("Cannot change weapon: Both slots must have weapons assigned.");
        }
    }

    /// <summary>
    /// Activate a specific weapon by ID, ensuring no conflicts.
    /// </summary>
    /// <param name="weaponID">ID of the weapon to activate.</param>
    private void ActivateWeapon(int weaponID)
    {
        for (int i = 0; i < weaponObjects.Length; i++)
        {
            weaponObjects[i].SetActive(i == weaponID); // Only activate the weapon with the specified ID
        }
    }

    /// <summary>
    /// Get the current weapon ID based on the active slot.
    /// </summary>
    /// <returns>The ID of the currently active weapon.</returns>
    public int GetCurrentWeaponID()
    {
        return slot1 ? primaryWeaponID : secondaryWeaponID; // Return the ID of the active weapon
    }
}
