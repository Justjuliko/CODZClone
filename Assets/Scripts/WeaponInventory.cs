using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInventory : MonoBehaviour
{
    [Header("=== INVENTORY SETTINGS ===")]
    [Tooltip("Current state: True = Using primary weapon, False = Using secondary weapon")]
    public bool slot1 = true;

    [Header("=== DEBUG INFO ===")]
    [Tooltip("ID of the primary weapon in the inventory.")]
    [ReadOnly] public int primaryWeaponID = -1; // -1 indicates no weapon assigned

    [Tooltip("ID of the secondary weapon in the inventory.")]
    [ReadOnly] public int secondaryWeaponID = -1; // -1 indicates no weapon assigned

    private GameObject[] weaponObjects; // Store references to child weapon objects

    [Header("=== INPUT SYSTEM ===")]
    [Tooltip("Input Action Asset for handling input.")]
    public InputActionAsset inputActions;

    private InputAction nextWeaponAction; // Reference to the "next" action

    private void Start()
    {
        // Cache all child weapons
        weaponObjects = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            weaponObjects[i] = transform.GetChild(i).gameObject;
            weaponObjects[i].SetActive(false); // Ensure all weapons start as inactive
        }

        // Initialize inventory with the first available weapon if it exists
        if (transform.childCount > 0)
        {
            primaryWeaponID = 0; // Default to the first child
            ActivateWeapon(primaryWeaponID); // Activate the primary weapon
        }

        // Setup Input Action
        if (inputActions != null)
        {
            nextWeaponAction = inputActions.FindActionMap("Player").FindAction("next");
            if (nextWeaponAction != null)
            {
                nextWeaponAction.performed += _ => ToggleWeaponSlot();
                nextWeaponAction.Enable();
            }
            else
            {
                Debug.LogWarning("Action 'next' not found in Action Map 'Player'.");
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the action
        if (nextWeaponAction != null)
        {
            nextWeaponAction.performed -= _ => ToggleWeaponSlot();
        }
    }

    private void Update()
    {
        // Manage active weapon based on the slot state
        UpdateActiveWeapon();
    }

    /// <summary>
    /// Activates a weapon and deactivates the other based on the current slot.
    /// </summary>
    private void UpdateActiveWeapon()
    {
        if (primaryWeaponID != -1) // Check if a primary weapon exists
            weaponObjects[primaryWeaponID].SetActive(slot1);

        if (secondaryWeaponID != -1) // Check if a secondary weapon exists
            weaponObjects[secondaryWeaponID].SetActive(!slot1);
    }

    /// <summary>
    /// Overwrite the current weapon in the active slot.
    /// </summary>
    /// <param name="newWeaponID">The ID of the new weapon to assign.</param>
    public void OverwriteCurrentWeapon(int newWeaponID)
    {
        if (newWeaponID < 0 || newWeaponID >= weaponObjects.Length)
        {
            Debug.LogError("Invalid weapon ID: " + newWeaponID);
            return;
        }

        if (slot1) // Replace the primary weapon
        {
            if (primaryWeaponID != -1)
                weaponObjects[primaryWeaponID].SetActive(false); // Deactivate the old primary weapon

            primaryWeaponID = newWeaponID;
        }
        else // Replace the secondary weapon
        {
            if (secondaryWeaponID != -1)
                weaponObjects[secondaryWeaponID].SetActive(false); // Deactivate the old secondary weapon

            secondaryWeaponID = newWeaponID;
        }

        UpdateActiveWeapon(); // Ensure the correct weapon is active
    }

    /// <summary>
    /// Toggle the active weapon slot.
    /// </summary>
    private void ToggleWeaponSlot()
    {
        slot1 = !slot1; // Switch between true and false
        UpdateActiveWeapon(); // Update active weapons immediately
    }

    /// <summary>
    /// Activate a specific weapon by ID, ensuring no conflicts.
    /// </summary>
    /// <param name="weaponID">ID of the weapon to activate.</param>
    private void ActivateWeapon(int weaponID)
    {
        for (int i = 0; i < weaponObjects.Length; i++)
        {
            weaponObjects[i].SetActive(i == weaponID); // Only activate the specified weapon
        }
    }

    /// <summary>
    /// Get the current weapon ID based on the active slot.
    /// </summary>
    /// <returns>The ID of the currently active weapon.</returns>
    public int GetCurrentWeaponID()
    {
        return slot1 ? primaryWeaponID : secondaryWeaponID;
    }
}
