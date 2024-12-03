using UnityEngine;

public class WeaponStatsClass : MonoBehaviour
{
    int currentWeaponID = -1; // -1 means no weapon is selected

    // Ammo variables
    private int[] storedAmmo; // Stores the current ammo for each weapon
    private int[] storedReserveAmmo; // Stores the reserve ammo for each weapon

    // Weapon stats variables
    public float newWeaponDamage; // Damage dealt by the weapon
    public float newFireRate; // Rate of fire for the weapon
    public int newMaxMagazineAmmo; // Maximum ammo capacity in the magazine
    public int newReserveAmmo; // Reserve ammo for the weapon
    public float newReloadTime; // Time it takes to reload the weapon
    public bool newAuto; // Whether the weapon is automatic or not
    public float newRecoilAngle; // Recoil angle when firing the weapon
    public Vector3 newReloadAngle; // Reload angle for animation
    public Vector3 newAimPosition; // Position of the weapon when aiming
    public int newDamageRange; // Range for the weapon's damage

    private PlayerShooting playerShootingScript; // Reference to the player shooting script
    private WeaponInventory inventory; // Reference to the weapon inventory
    private Vector3 newOriginalPosition; // The weapon's original position

    private void Start()
    {
        // Get references to other components
        playerShootingScript = GetComponent<PlayerShooting>();
        inventory = GetComponent<WeaponInventory>();

        // Initialize ammo arrays if they haven't been initialized
        if (storedAmmo == null || storedAmmo.Length == 0)
        {
            storedAmmo = new int[4];  // Adjust the size depending on how many weapons you have
            storedReserveAmmo = new int[4];  // Adjust the size depending on how many weapons you have

            // Initialize ammo for each weapon (if not set in another way)
            for (int i = 0; i < storedAmmo.Length; i++)
            {
                storedAmmo[i] = 0;  // Start with 0 ammo in the magazine
                storedReserveAmmo[i] = 0;  // Start with 0 reserve ammo
            }
        }

        // Check if inventory or shooting script is missing
        if (inventory == null || playerShootingScript == null)
        {
            Debug.LogError("WeaponStatsClass: Missing WeaponInventory or PlayerShooting component.");
        }
    }

    public void currentIdUpdate()
    {
        // Ensure references are not null before using them
        if (inventory != null && playerShootingScript != null)
        {
            // Get the current weapon ID
            currentWeaponID = inventory.GetCurrentWeaponID();

            if (currentWeaponID >= 0) // Ensure the weapon ID is valid
            {
                changeValues(); // Update the weapon stats

                // Check that ammo and reserves are correctly configured
                if (storedReserveAmmo != null && currentWeaponID < storedReserveAmmo.Length)
                {
                    // Update the weapon settings in the PlayerShooting script
                    playerShootingScript.UpdateWeaponSettings(newWeaponDamage, newFireRate, newMaxMagazineAmmo, newReserveAmmo, newReloadTime, newAuto, newRecoilAngle, newReloadAngle, newAimPosition, newDamageRange, newOriginalPosition);
                }
                else
                {
                    Debug.LogWarning("WeaponStatsClass: Ammo data not set properly.");
                }
            }
            else
            {
                Debug.LogError("WeaponStatsClass: Invalid currentWeaponID.");
            }
        }
        else
        {
            Debug.LogError("WeaponStatsClass: WeaponInventory or PlayerShooting is null.");
        }
    }

    public void changeValues()
    {
        // Update weapon stats based on the weapon ID
        switch (currentWeaponID)
        {
            case 0:
                newWeaponDamage = 20;
                newFireRate = 0.5f;
                newMaxMagazineAmmo = 7;
                newReserveAmmo = 49;
                newReloadTime = 2f;
                newAuto = false;
                newRecoilAngle = -5f;
                newReloadAngle = new Vector3(30f, 0f, 0f);
                newAimPosition = new Vector3(0f, -0.2f, 0.45f);
                newOriginalPosition = new Vector3(0.281f, -0.36f, 0.45f);
                newDamageRange = 30;
                break;

            case 1:
                newWeaponDamage = 20;
                newFireRate = 0.2f;
                newMaxMagazineAmmo = 30;
                newReserveAmmo = 150;
                newReloadTime = 3f;
                newAuto = true;
                newRecoilAngle = -1f;
                newReloadAngle = new Vector3(30f, 0f, 0f);
                newAimPosition = new Vector3(0f, -0.38f, 0.33f);
                newOriginalPosition = new Vector3(0.28f, -0.52f, 0.33f);
                newDamageRange = 40;
                break;

            case 2:
                newWeaponDamage = 160;
                newFireRate = 0.7f;
                newMaxMagazineAmmo = 8;
                newReserveAmmo = 48;
                newReloadTime = 5f;
                newAuto = false;
                newRecoilAngle = -8f;
                newReloadAngle = new Vector3(30f, 0f, 0f);
                newAimPosition = new Vector3(0f, -0.26f, 0.2f);
                newOriginalPosition = new Vector3(0.276f, -0.406f, 0.203f);
                newDamageRange = 7;
                break;

            case 3:
                newWeaponDamage = 200;
                newFireRate = 1f;
                newMaxMagazineAmmo = 5;
                newReserveAmmo = 40;
                newReloadTime = 3f;
                newAuto = false;
                newRecoilAngle = -5f;
                newReloadAngle = new Vector3(30f, 0f, 0f);
                newAimPosition = new Vector3(0f, -0.165f, 0.278f);
                newOriginalPosition = new Vector3(0.271f, -0.303f, 0.278f);
                newDamageRange = 50;
                break;

            default:
                Debug.LogError("WeaponStatsClass: Unknown weapon ID.");
                break;
        }
    }
}
