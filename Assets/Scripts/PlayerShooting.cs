using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

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
    int maxOtherMagazineAmmo;
    int maxOtherReserveAmmo;

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
    public Vector3 originalPosition;

    [Header("=== DEBUG SETTINGS ===")]
    public float damageRange = 20f;
    private float nextShootTime = 0f;

    private bool isReloading = false;
    private bool isRecoiling = false;
    private InputAction attackAction;
    private InputAction reloadAction;
    private InputAction aimAction;

    [Header("=== OTHER COMPONENTS ===")]
    private FirstPersonController firstPersonController; // Referencia al controlador de primera persona
    private Player playerScript; // Referencia al script del jugador

    [Header("=== AIM RETICLE ===")]
    public GameObject aimReticle; // Objeto que act�a como ret�cula

    [Header("=== SHOOTING SOUND SETTINGS ===")]
    public AudioClip shootingSound; // Clip de sonido del disparo
    public int audioSourcePoolSize = 5; // Tama�o de la pool de AudioSources

    private AudioSource[] audioSources; // Pool de AudioSources
    private int currentAudioSourceIndex = 0; // �ndice del AudioSource actual

    [Header("=== RELOADING SOUND SETTINGS ===")]
    public AudioClip reloadSound; // Clip de sonido para la recarga
    private AudioSource reloadAudioSource; // AudioSource dedicado al sonido de recarga

    [Header("=== ZOMBIE HIT SOUND SETTINGS ===")]
    public AudioClip zombieHitSound; // Clip de sonido al golpear al zombi
    public float zombieHitSoundVolume = 1.0f; // Volumen configurable
    private AudioSource zombieHitAudioSource; // AudioSource para el sonido de impacto

    [Header("=== LIGHT SETTINGS ===")]
    public List<GameObject> muzzleflashLights;


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

        playerScript = GameObject.Find("FirstPersonController").GetComponent<Player>();

        // Asegurar que la ret�cula est� encendida al inicio
        if (aimReticle != null)
        {
            aimReticle.SetActive(true);
        }
        // Crear un pool de AudioSources para evitar superposiciones
        audioSources = new AudioSource[audioSourcePoolSize];
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
            newAudioSource.clip = shootingSound;
            newAudioSource.playOnAwake = false;
            audioSources[i] = newAudioSource;
        }
        // Configurar el AudioSource para el sonido de recarga
        reloadAudioSource = gameObject.AddComponent<AudioSource>();
        reloadAudioSource.clip = reloadSound;
        reloadAudioSource.loop = true; // Activar el bucle
        reloadAudioSource.playOnAwake = false;

        // Configurar AudioSource para el sonido de impacto
        zombieHitAudioSource = gameObject.AddComponent<AudioSource>();
        zombieHitAudioSource.clip = zombieHitSound;
        zombieHitAudioSource.volume = zombieHitSoundVolume;
        zombieHitAudioSource.loop = false;
        zombieHitAudioSource.playOnAwake = false;
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

    private void Shoot()
    {
        nextShootTime = Time.time + fireRate;

        // Reproducir sonido de disparo usando el siguiente AudioSource en la pool
        if (shootingSound != null)
        {
            AudioSource audioSourceToUse = audioSources[currentAudioSourceIndex];
            audioSourceToUse.Play();
            currentAudioSourceIndex = (currentAudioSourceIndex + 1) % audioSourcePoolSize; // Ciclar al siguiente AudioSource
        }

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
                    // Reproducir sonido de impacto
                    if (zombieHitAudioSource != null && zombieHitSound != null)
                    {
                        zombieHitAudioSource.Play();
                    }

                    // Obtener el efecto de sangre del pool y posicionarlo en el punto de impacto
                    ObjectPool bloodPool = FindFirstObjectByType<ObjectPool>();
                    if (bloodPool != null)
                    {
                        GameObject bloodEffect = bloodPool.GetObject();
                        bloodEffect.transform.position = hit.point;
                        bloodEffect.transform.rotation = Quaternion.LookRotation(hit.normal);
                    }
                }
            }
        }

            ApplyRecoil();

        if (muzzleflashLights != null && muzzleflashLights.Count > 0)
        {
            StartCoroutine(turnOnLights());
        }

        currentMagazineAmmo--;
        UpdateAmmoUI();
    }

    private IEnumerator turnOnLights()
    {
        foreach (var light in muzzleflashLights)
        {
            light.SetActive(true);
        }
        yield return new WaitForSeconds(0.15f);

        foreach (var light in muzzleflashLights)
        {
            light.SetActive(false);
        }
    }

    private void StartAiming()
    {
        // Cambiar la posici�n de las armas para apuntar
        foreach (var obj in animableObjects)
        {
            obj.transform.localPosition = aimPosition;
        }

        // Desactivar la ret�cula de apuntado
        if (aimReticle != null)
        {
            aimReticle.SetActive(false);
        }
    }

    private void StopAiming()
    {
        // Restaurar la posici�n original de las armas
        foreach (var obj in animableObjects)
        {
            obj.transform.localPosition = originalPosition;
        }

        // Activar la ret�cula de apuntado
        if (aimReticle != null)
        {
            aimReticle.SetActive(true);
        }
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

        // Iniciar el sonido de recarga en bucle
        if (reloadSound != null)
        {
            reloadAudioSource.Play();
        }

        StartCoroutine(ReloadAnimation());

        yield return new WaitForSeconds(reloadTime);

        // Detener el sonido de recarga
        if (reloadAudioSource.isPlaying)
        {
            reloadAudioSource.Stop();
        }

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
    public void ApplyRotationShake()
    {
        // Desencadenamos la rotaci�n de todos los objetos animables
        foreach (var obj in animableObjects)
        {
            StartCoroutine(RotateBackToOriginal(obj));
        }
    }

    private IEnumerator RotateBackToOriginal(GameObject obj)
    {
        // Obtener la rotaci�n original del objeto
        Quaternion originalRotation = obj.transform.localRotation;

        // Realizar la rotaci�n r�pida en Y de 25 grados
        obj.transform.Rotate(0f, 25f, 0f, Space.Self);

        // Esperamos un breve periodo antes de devolver la rotaci�n a la original
        yield return new WaitForSeconds(0.1f);

        // Regresamos la rotaci�n a la original
        obj.transform.localRotation = originalRotation;
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
                                  Vector3 newAimPosition, float newDamageRange, Vector3 newOriginalPosition)
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
        originalPosition = newOriginalPosition;

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
    public void saveOtherWeaponMaxAmmo()
    {
        maxOtherMagazineAmmo = maxMagazineAmmo;
        maxOtherReserveAmmo = maxReserveAmmo;
    }
    public void RefillSecondaryAmmo()
    {
        otherWeaponMagazineAmmo = maxOtherMagazineAmmo;
        otherWeaponReserveAmmo = maxOtherReserveAmmo;

        UpdateAmmoUI();
    }
}
