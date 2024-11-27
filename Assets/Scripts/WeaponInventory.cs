using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInventory : MonoBehaviour
{
    [Header("=== INVENTORY SETTINGS ===")]
    [Tooltip("Current state: True = Using primary weapon, False = Using secondary weapon")]
    public bool slot1 = true;

    [Header("=== DEBUG INFO ===")]
    [Tooltip("ID of the primary weapon in the inventory.")]
    public int primaryWeaponID = -1; // -1 indicates no weapon assigned

    [Tooltip("ID of the secondary weapon in the inventory.")]
    public int secondaryWeaponID = -1; // -1 indicates no weapon assigned

    private GameObject[] weaponObjects; // Store references to child weapon objects

    [Header("=== INPUT SYSTEM ===")]
    [Tooltip("Input Action Asset for handling input.")]
    public InputActionAsset inputActions;

    private InputAction nextWeaponAction; // Reference to the "next" action

    PlayerShooting playerShooting;
    WeaponStatsClass weaponStatsClass;

    private void Start()
    {
        weaponStatsClass = GetComponent<WeaponStatsClass>();
        playerShooting = GetComponent<PlayerShooting>();

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
        // Si la primaria y la secundaria tienen el mismo ID, mantener ambas armas activas
        if (primaryWeaponID == secondaryWeaponID && primaryWeaponID != -1)
        {
            // Ambos deben estar activos si tienen el mismo ID
            weaponObjects[primaryWeaponID].SetActive(true);
        }
        else
        {
            // Si el ID es diferente, manejar la activación de forma normal
            if (primaryWeaponID != -1) // Verificar si existe un arma primaria
                weaponObjects[primaryWeaponID].SetActive(slot1);

            if (secondaryWeaponID != -1) // Verificar si existe un arma secundaria
                weaponObjects[secondaryWeaponID].SetActive(!slot1);
        }
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

        // Verificar si el nuevo ID es igual al ID del arma secundaria
        if (newWeaponID == secondaryWeaponID)
        {
            // Si el nuevo arma es igual a la secundaria, asignar al slot primario si no está ya asignada
            if (newWeaponID == primaryWeaponID)
            {
                Debug.LogWarning("Cannot overwrite: New weapon ID is the same as the primary weapon.");
                return; // No permitir la sobreescritura si el ID es igual al de la primaria también
            }
        }

        // Si solo hay una arma (ID secundaria es -1)
        if (secondaryWeaponID == -1)
        {
            // Reemplazar el arma secundaria
            if (primaryWeaponID != -1)
                weaponObjects[primaryWeaponID].SetActive(false); // Desactivar el arma primaria si existe

            secondaryWeaponID = newWeaponID;  // Asignar la nueva arma a la secundaria
            ToggleWeaponSlot();
            weaponStatsClass.currentIdUpdate();
            playerShooting.RefillAmmo();
            Debug.Log("Overwrite");
        }
        else
        {
            // Si ambos ID son diferentes de -1, reemplazar el arma activa (primaria o secundaria)
            if (slot1) // Si se está usando el arma primaria
            {
                if (primaryWeaponID != -1)
                    weaponObjects[primaryWeaponID].SetActive(false); // Desactivar el arma primaria anterior

                primaryWeaponID = newWeaponID;  // Asignar la nueva arma a la primaria
                weaponStatsClass.currentIdUpdate();
                playerShooting.RefillAmmo();
                Debug.Log("Overwrite");
            }
            else // Si se está usando el arma secundaria
            {
                if (secondaryWeaponID != -1)
                    weaponObjects[secondaryWeaponID].SetActive(false); // Desactivar el arma secundaria anterior

                secondaryWeaponID = newWeaponID;  // Asignar la nueva arma a la secundaria
                weaponStatsClass.currentIdUpdate();
                playerShooting.RefillAmmo();
                Debug.Log("Overwrite");
            }
        }

        UpdateActiveWeapon(); // Asegurarse de que el arma activa se actualice correctamente
    }


    /// <summary>
    /// Toggle the active weapon slot only if both slots are occupied.
    /// </summary>
    private void ToggleWeaponSlot()
    {
        // Solo hacer toggle si ambos slots están ocupados
        if (primaryWeaponID != -1 && secondaryWeaponID != -1)
        {
            slot1 = !slot1; // Cambiar entre primaria y secundaria
            playerShooting.saveOtherWeaponMaxAmmo();
            weaponStatsClass.currentIdUpdate();
            playerShooting.SwapAmmo();            
            UpdateActiveWeapon(); // Actualizar las armas activas de inmediato
            playerShooting.ApplyRotationShake();
        }
        else
        {
            Debug.Log("No se puede cambiar de arma: Ambos slots deben tener armas asignadas.");
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
            weaponObjects[i].SetActive(i == weaponID); // Solo activar el arma especificada
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
