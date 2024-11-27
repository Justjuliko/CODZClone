using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("=== WEAPON SETTINGS ===")]
    [Tooltip("Damage dealt by the weapon.")]
    public float weaponDamage = 25f;

    [Tooltip("Time in seconds between each shot.")]
    public float fireRate = 0.5f;  // Cooldown for shooting, simulating rate of fire.

    [Tooltip("Maximum amount of ammo the weapon can hold.")]
    public int maxAmmo = 30;

    [Tooltip("Current ammo in the weapon.")]
    private int currentAmmo;

    [Tooltip("Ammo reserve. Total ammo available.")]
    public int ammoReserve = 90;

    [Tooltip("Time to reload the weapon.")]
    public float reloadTime = 2f;

    [Header("=== UI SETTINGS ===")]
    [Tooltip("Text UI element to show ammo count.")]
    public TMP_Text ammoText;

    [Tooltip("Text UI element to show ammo reserve count.")]
    public TMP_Text reserveAmmoText;

    [Tooltip("Text UI element to show reload or no ammo warnings.")]
    public TMP_Text warningText; // Text UI to show reload or no ammo warning

    [Header("=== SHOOTING BEHAVIOR ===")]
    [Tooltip("Set true for automatic fire, false for semi-automatic.")]
    public bool auto = false;  // Whether the weapon is automatic or semi-automatic

    [Header("=== DEBUG SETTINGS ===")]
    [Tooltip("Damage range for the attack.")]
    public float damageRange = 20f;

    [Tooltip("Cooldown timer between shots.")]
    private float shootCooldown = 0f;

    private bool isReloading = false;  // To check if the player is in reload state
    private InputAction attackAction;
    private InputAction reloadAction;

    private Player playerScript;  // Reference to the Player script

    private void Awake()
    {
        // Get references to Input Actions from the InputSystem
        var playerInputActions = new InputSystem_Actions(); // Replace with your actual Input Actions class
        attackAction = playerInputActions.Player.Attack;
        reloadAction = playerInputActions.Player.Reload;

        // Enable the actions
        attackAction.Enable();
        reloadAction.Enable();
    }

    private void Start()
    {
        // Initialize ammo
        currentAmmo = maxAmmo;

        // Initialize ammo UI
        UpdateAmmoUI();

        // Initially hide the warning text
        warningText.gameObject.SetActive(false);

        // Find the Player script on the player object (using the tag "Player")
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    private void Update()
    {
        // Update the cooldown timer
        if (shootCooldown > 0)
        {
            shootCooldown -= Time.deltaTime;
        }

        // Check for shooting input
        if (auto)
        {
            // If automatic fire, shoot continuously while the key is held
            if (attackAction.ReadValue<float>() > 0 && shootCooldown <= 0 && !isReloading && currentAmmo > 0)
            {
                Shoot();
            }
        }
        else
        {
            // If semi-automatic, shoot once per press
            if (attackAction.triggered && shootCooldown <= 0 && !isReloading && currentAmmo > 0)
            {
                Shoot();
            }
        }

        // Check for reload input
        if (reloadAction.triggered && !isReloading)
        {
            StartCoroutine(Reload());
        }

        // Display appropriate message if no ammo
        if (currentAmmo == 0 && ammoReserve == 0)
        {
            ShowWarning("No ammo");
        }
        else if (currentAmmo == 0)
        {
            ShowWarning("Reload");
        }
        else
        {
            HideWarning();
        }
    }

    private void Shoot()
    {
        // Play shooting logic (damage to zombies, sound, etc.)
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, damageRange))
        {
            if (hit.collider.CompareTag("Zombie"))
            {
                hit.collider.GetComponent<Zombie>().TakeDamage(weaponDamage);

                // Add points to the player when zombie takes damage
                if (playerScript != null)
                {
                    playerScript.AddPoints(10);  // Adding 10 points to the player
                }
            }
        }

        // Decrease ammo count
        currentAmmo--;
        shootCooldown = fireRate; // Reset cooldown

        // Update ammo UI
        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        // Update UI with current ammo and max ammo
        ammoText.text = currentAmmo.ToString();
        reserveAmmoText.text = ammoReserve.ToString();
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        // Display reloading message or animation (optional)
        Debug.Log("Reloading...");

        // Wait for the reload time
        yield return new WaitForSeconds(reloadTime);

        // Reload ammo
        int ammoNeeded = maxAmmo - currentAmmo;

        if (ammoReserve >= ammoNeeded)
        {
            currentAmmo = maxAmmo;
            ammoReserve -= ammoNeeded;
        }
        else
        {
            currentAmmo += ammoReserve;
            ammoReserve = 0;
        }

        // Update ammo UI
        UpdateAmmoUI();

        isReloading = false;
    }

    private void ShowWarning(string message)
    {
        warningText.gameObject.SetActive(true);  // Show warning text
        warningText.text = message;  // Set the warning message
    }

    private void HideWarning()
    {
        warningText.gameObject.SetActive(false);  // Hide warning text when ammo is available
    }
}
