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

    [Header("=== OTHER WEAPON AMMO ===")]
    private int otherWeaponMagazineAmmo;
    private int otherWeaponReserveAmmo;

    [Header("=== UI SETTINGS ===")]
    public TMP_Text ammoText;
    public TMP_Text reserveAmmoText;
    public TMP_Text warningText;

    [Header("=== SHOOTING BEHAVIOR ===")]
    public bool auto = false;

    [Header("=== ANIMATION SETTINGS ===")]
    public List<GameObject> animableObjects;
    public float recoilAngle = -5f;
    public Vector3 reloadAngle = new Vector3(-10f, 5f, 0f);

    public Vector3 aimPosition = new Vector3(0.1f, -0.2f, 0f);
    private Vector3 originalPosition;

    [Header("=== DEBUG SETTINGS ===")]
    public float damageRange = 20f;
    private float nextShootTime = 0f;

    private bool isReloading = false;
    private bool isRecoiling = false;
    private InputAction attackAction;
    private InputAction reloadAction;
    private InputAction aimAction;

    private Player playerScript;

    private void Awake()
    {
        var playerInputActions = new InputSystem_Actions();
        attackAction = playerInputActions.Player.Attack;
        reloadAction = playerInputActions.Player.Reload;
        aimAction = playerInputActions.Player.Aim;

        attackAction.Enable();
        reloadAction.Enable();
        aimAction.Enable();
    }

    private void Start()
    {
        currentMagazineAmmo = maxMagazineAmmo;
        currentReserveAmmo = maxReserveAmmo;

        // Inicializar munici�n para la otra arma
        otherWeaponMagazineAmmo = 0;
        otherWeaponReserveAmmo = 0;

        UpdateAmmoUI();
        warningText.gameObject.SetActive(false);

        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        originalPosition = animableObjects[0].transform.localPosition;
    }

    private void Update()
    {
        if (aimAction.ReadValue<float>() > 0)
        {
            StartAiming();
        }
        else
        {
            StopAiming();
        }

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
        foreach (var obj in animableObjects)
        {
            obj.transform.localPosition = aimPosition;
        }
    }

    private void StopAiming()
    {
        foreach (var obj in animableObjects)
        {
            obj.transform.localPosition = originalPosition;
        }
    }

    private void Shoot()
    {
        nextShootTime = Time.time + fireRate;

        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, damageRange);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Zombie"))
            {
                Zombie zombie = hit.collider.GetComponent<Zombie>();
                if (zombie != null)
                {
                    zombie.TakeDamage(weaponDamage);

                    if (playerScript != null)
                    {
                        playerScript.AddPoints(10);
                    }
                }
            }
        }

        ApplyRecoil();

        currentMagazineAmmo--;
        UpdateAmmoUI();
    }

    private void ApplyRecoil()
    {
        if (!isReloading && !isRecoiling)
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
        obj.transform.Rotate(angleChange);

        yield return new WaitForSeconds(duration / 2);

        obj.transform.localRotation = Quaternion.Euler(Vector3.zero);

        isRecoiling = false;
    }

    public IEnumerator Reload()
    {
        if (currentMagazineAmmo == maxMagazineAmmo || currentReserveAmmo <= 0)
        {
            yield break;
        }

        isReloading = true;

        StartCoroutine(ReloadAnimation());

        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = maxMagazineAmmo - currentMagazineAmmo;

        if (currentReserveAmmo >= ammoNeeded)
        {
            currentMagazineAmmo = maxMagazineAmmo;
            currentReserveAmmo -= ammoNeeded;
        }
        else
        {
            currentMagazineAmmo += currentReserveAmmo;
            currentReserveAmmo = 0;
        }

        UpdateAmmoUI();
        isReloading = false;
    }

    private IEnumerator ReloadAnimation()
    {
        foreach (var obj in animableObjects)
        {
            obj.transform.Rotate(reloadAngle);
        }

        yield return new WaitForSeconds(reloadTime);

        foreach (var obj in animableObjects)
        {
            obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
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

        UpdateAmmoUI();
    }

    public void SwapAmmo()
    {
        int tempMagazine = currentMagazineAmmo;
        int tempReserve = currentReserveAmmo;

        currentMagazineAmmo = otherWeaponMagazineAmmo;
        currentReserveAmmo = otherWeaponReserveAmmo;

        otherWeaponMagazineAmmo = tempMagazine;
        otherWeaponReserveAmmo = tempReserve;

        UpdateAmmoUI();
    }

    public void RefillAmmo()
    {
        currentMagazineAmmo = maxMagazineAmmo;
        currentReserveAmmo = maxReserveAmmo;

        UpdateAmmoUI();
    }
}
