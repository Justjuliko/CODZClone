using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerShooting : MonoBehaviour
{
    [Header("=== WEAPON SETTINGS ===")]
    public float weaponDamage = 25f;
    public float fireRate = 0.5f;  
    public int maxMagazineAmmo;    
    public int maxReserveAmmo;
    public float reloadTime = 2f;

    [Header("=== CURRENT AMMO ===")]
    public int currentMagazineAmmo;
    public int currentReserveAmmo = 90;

    [Header("=== UI SETTINGS ===")]
    public TMP_Text ammoText;
    public TMP_Text reserveAmmoText;
    public TMP_Text warningText;

    [Header("=== SHOOTING BEHAVIOR ===")]
    public bool auto = false;

    [Header("=== ANIMATION SETTINGS ===")]
    public List<GameObject> animableObjects; // Objects affected by recoil and reload animations
    public float recoilAngle = -5f; // Angle for recoil animation
    public Vector3 reloadAngle = new Vector3(-10f, 5f, 0f); // Angle change during reload

    // Position for aiming
    public Vector3 aimPosition = new Vector3(0.1f, -0.2f, 0f);  // Editable in inspector
    private Vector3 originalPosition; // Original position of the weapon

    [Header("=== DEBUG SETTINGS ===")]
    public float damageRange = 20f;
    private float nextShootTime = 0f; // Enforces fire rate cooldown

    private bool isReloading = false;
    private bool isRecoiling = false; // New flag to track if recoil is active
    private InputAction attackAction;
    private InputAction reloadAction;
    private InputAction aimAction; // Reference to Aim action

    private Player playerScript;

    private void Awake()
    {
        var playerInputActions = new InputSystem_Actions();
        attackAction = playerInputActions.Player.Attack;
        reloadAction = playerInputActions.Player.Reload;
        aimAction = playerInputActions.Player.Aim; // Reference to Aim action

        attackAction.Enable();
        reloadAction.Enable();
        aimAction.Enable(); // Enable Aim action
    }

    private void Start()
    {
        currentMagazineAmmo = maxMagazineAmmo;
        currentReserveAmmo = maxReserveAmmo;
        UpdateAmmoUI();
        warningText.gameObject.SetActive(false);

        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        // Store the original positions of all the weapons
        originalPosition = animableObjects[0].transform.localPosition;
    }

    private void Update()
    {
        // Comprobamos la entrada del botón "Aim" (presionado o suelto)
        if (aimAction.ReadValue<float>() > 0) // Si el botón está presionado
        {
            StartAiming();
        }
        else // Si el botón está suelto
        {
            StopAiming();
        }

        // Check for firing logic
        if (auto)
        {
            if (attackAction.ReadValue<float>() > 0 && Time.time >= nextShootTime && !isReloading && currentMagazineAmmo > 0)
            {
                Shoot();
            }
        }
        else
        {
            if (attackAction.triggered && Time.time >= nextShootTime && !isReloading && currentMagazineAmmo > 0)
            {
                Shoot();
            }
        }

        // Start reloading only if not in recoil or reloading already
        if (reloadAction.triggered && !isReloading && !isRecoiling)
        {
            StartCoroutine(Reload());
        }

        if (currentMagazineAmmo == 0 && currentReserveAmmo == 0)
        {
            ShowWarning("No ammo");
        }
        else if (currentMagazineAmmo == 0)
        {
            ShowWarning("Reload");
        }
        else
        {
            HideWarning();
        }
    }

    private void StartAiming()
    {
        // Cambiar la posición de los objetos a la posición de apuntado
        foreach (var obj in animableObjects)
        {
            obj.transform.localPosition = aimPosition; // 'aimPosition' es el Vector3 que defines en el Inspector
        }
    }

    private void StopAiming()
    {
        // Restablecer la posición original de los objetos
        foreach (var obj in animableObjects)
        {
            obj.transform.localPosition = originalPosition; // 'originalPosition' es el Vector3 original
        }
    }



    private void Shoot()
    {
        // Actualizamos el tiempo para el próximo disparo permitido
        nextShootTime = Time.time + fireRate;

        // Usamos RaycastAll para detectar todos los objetos en la trayectoria del disparo
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, damageRange);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Zombie"))
            {
                // Dañamos cada zombi impactado
                Zombie zombie = hit.collider.GetComponent<Zombie>();
                if (zombie != null)
                {
                    zombie.TakeDamage(weaponDamage);

                    // Agregamos puntos al jugador
                    if (playerScript != null)
                    {
                        playerScript.AddPoints(10);
                    }
                }
            }
        }

        // Aplicamos el recoil después de disparar
        ApplyRecoil();

        // Reducimos el conteo de munición
        currentMagazineAmmo--;
        UpdateAmmoUI();
    }

    private void ApplyRecoil()
    {
        // Solo aplicar recoil si no está en recarga
        if (!isReloading && !isRecoiling) // No permitir recoil si estamos recargando
        {
            isRecoiling = true;
            foreach (var obj in animableObjects)
            {
                StartCoroutine(RecoilAnimation(obj, Vector3.right * recoilAngle, fireRate));
            }
        }
    }

    private IEnumerator RecoilAnimation(GameObject obj, Vector3 angleChange, float duration)
    {
        // Apply recoil
        obj.transform.Rotate(angleChange);

        // Wait for the duration
        yield return new WaitForSeconds(duration / 2); // Half the time for returning to original position

        // Return to original rotation
        obj.transform.localRotation = Quaternion.Euler(Vector3.zero);

        isRecoiling = false; // Reset recoil state
    }

    public IEnumerator Reload()
    {
        // Verificar si la recarga es válida
        if (currentMagazineAmmo == maxMagazineAmmo || currentReserveAmmo <= 0)
        {
            // No se recarga si el cargador está lleno o si no hay balas en la reserva
            yield break;
        }

        isReloading = true;

        // Iniciar animación de recarga
        StartCoroutine(ReloadAnimation());

        // Esperar el tiempo de recarga
        yield return new WaitForSeconds(reloadTime);

        // Calcular cuántas balas se necesitan para llenar el cargador
        int ammoNeeded = maxMagazineAmmo - currentMagazineAmmo;

        if (currentReserveAmmo >= ammoNeeded)
        {
            // Recargar completamente si hay suficiente munición en la reserva
            currentMagazineAmmo = maxMagazineAmmo;
            currentReserveAmmo -= ammoNeeded;
        }
        else
        {
            // Recargar solo lo que queda en la reserva
            currentMagazineAmmo += currentReserveAmmo;
            currentReserveAmmo = 0;
        }

        UpdateAmmoUI();
        isReloading = false;
    }

    private IEnumerator ReloadAnimation()
    {
        // Realizamos la rotación de recarga para todos los objetos al mismo tiempo
        foreach (var obj in animableObjects)
        {
            obj.transform.Rotate(reloadAngle); // Aplicamos el ángulo de recarga
        }

        // Esperamos el tiempo total de recarga para que todas las animaciones se completen
        yield return new WaitForSeconds(reloadTime);

        // Restablecemos la rotación de los objetos al final de la animación
        foreach (var obj in animableObjects)
        {
            obj.transform.localRotation = Quaternion.Euler(Vector3.zero); // Reseteamos la rotación
        }
    }

    private void UpdateAmmoUI()
    {
        ammoText.text = currentMagazineAmmo.ToString();
        reserveAmmoText.text = currentReserveAmmo.ToString();
    }

    private void ShowWarning(string message)
    {
        warningText.gameObject.SetActive(true);
        warningText.text = message;
    }

    private void HideWarning()
    {
        warningText.gameObject.SetActive(false);
    }
    public void UpdateWeaponSettings(float newWeaponDamage, float newFireRate, int newMaxAmmo, int newAmmoReserve,
                                  float newReloadTime, bool newAuto, float newRecoilAngle, Vector3 newReloadAngle,
                                  Vector3 newAimPosition, float newDamageRange)
    {
        weaponDamage = newWeaponDamage;
        fireRate = newFireRate;
        maxMagazineAmmo = newMaxAmmo;
        maxReserveAmmo = newAmmoReserve;
        reloadTime = newReloadTime;
        auto = newAuto;
        recoilAngle = newRecoilAngle;
        reloadAngle = newReloadAngle;
        aimPosition = newAimPosition;
        damageRange = newDamageRange;

        // Si se actualizan los valores en tiempo de ejecución, podrías querer actualizar la UI
        UpdateAmmoUI();
    }
}
