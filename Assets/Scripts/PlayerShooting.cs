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
    public int maxAmmo = 30;
    private int currentAmmo;
    public int ammoReserve = 90;
    public float reloadTime = 2f;

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

    [Header("=== DEBUG SETTINGS ===")]
    public float damageRange = 20f;
    private float nextShootTime = 0f; // Enforces fire rate cooldown

    private bool isReloading = false;
    private InputAction attackAction;
    private InputAction reloadAction;

    private Player playerScript;

    private void Awake()
    {
        var playerInputActions = new InputSystem_Actions();
        attackAction = playerInputActions.Player.Attack;
        reloadAction = playerInputActions.Player.Reload;

        attackAction.Enable();
        reloadAction.Enable();
    }

    private void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        warningText.gameObject.SetActive(false);

        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    private void Update()
    {
        // Check for firing logic
        if (auto)
        {
            if (attackAction.ReadValue<float>() > 0 && Time.time >= nextShootTime && !isReloading && currentAmmo > 0)
            {
                Shoot();
            }
        }
        else
        {
            if (attackAction.triggered && Time.time >= nextShootTime && !isReloading && currentAmmo > 0)
            {
                Shoot();
            }
        }

        if (reloadAction.triggered && !isReloading)
        {
            StartCoroutine(Reload());
        }

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
        currentAmmo--;
        UpdateAmmoUI();
    }


    private void ApplyRecoil()
    {
        foreach (var obj in animableObjects)
        {
            StartCoroutine(RecoilAnimation(obj, Vector3.right * recoilAngle, fireRate));
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
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        StartCoroutine(ReloadAnimation());

        yield return new WaitForSeconds(reloadTime);

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

        UpdateAmmoUI();
        isReloading = false;
    }

    private IEnumerator ReloadAnimation()
    {
        float stepTime = reloadTime; // Divide reload time per object

        foreach (var obj in animableObjects)
        {
            obj.transform.Rotate(reloadAngle);

            yield return new WaitForSeconds(stepTime);

            obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    private void UpdateAmmoUI()
    {
        ammoText.text = currentAmmo.ToString();
        reserveAmmoText.text = ammoReserve.ToString();
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
}
