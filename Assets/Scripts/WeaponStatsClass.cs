using UnityEngine;

public class WeaponStatsClass : MonoBehaviour
{
    int currentWeaponID = -1; // -1 significa que no hay arma

    // Variables de munición
    private int[] storedAmmo; // Para almacenar las balas actuales de cada arma
    private int[] storedReserveAmmo; // Para almacenar las reservas de cada arma

    // Variables de estadísticas de las armas
    public float newWeaponDamage;
    public float newFireRate;
    public int newMaxMagazineAmmo;
    public int newReserveAmmo;
    public float newReloadTime;
    public bool newAuto;
    public float newRecoilAngle;
    public Vector3 newReloadAngle;
    public Vector3 newAimPosition;
    public int newDamageRange;

    private PlayerShooting playerShootingScript;
    private WeaponInventory inventory;
    private Vector3 newOriginalPosition;

    private void Start()
    {
        // Obtener las referencias a otros componentes
        playerShootingScript = GetComponent<PlayerShooting>();
        inventory = GetComponent<WeaponInventory>();

        // Inicializar las variables de munición si no están ya inicializadas
        if (storedAmmo == null || storedAmmo.Length == 0)
        {
            storedAmmo = new int[4];  // Cambia el tamaño según la cantidad de armas que tengas
            storedReserveAmmo = new int[4];  // Cambia el tamaño según la cantidad de armas que tengas

            // Inicializar la munición para cada arma (si no se hace de otro modo)
            for (int i = 0; i < storedAmmo.Length; i++)
            {
                storedAmmo[i] = 0;  // Inicia la munición actual en 0
                storedReserveAmmo[i] = 0;  // Inicia las reservas en 0
            }
        }

        if (inventory == null || playerShootingScript == null)
        {
            Debug.LogError("WeaponStatsClass: Missing WeaponInventory or PlayerShooting component.");
        }
    }

    public void currentIdUpdate()
    {
        // Asegúrate de que las referencias no sean null antes de usarlas
        if (inventory != null && playerShootingScript != null)
        {
            // Asegurarse de que currentWeaponID se obtiene correctamente
            currentWeaponID = inventory.GetCurrentWeaponID();

            if (currentWeaponID >= 0) // Asegurarse de que el ID es válido
            {
                changeValues(); // Actualizamos los valores del arma

                // Comprobar que la munición y las reservas están configuradas correctamente
                if (storedReserveAmmo != null && currentWeaponID < storedReserveAmmo.Length)
                {
                    // Actualizar la configuración del arma en el script de PlayerShooting
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
        // Se actualizan los valores según el ID del arma
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
                newDamageRange = 20;
                break;

            case 1:
                newWeaponDamage = 15;
                newFireRate = 0.2f;
                newMaxMagazineAmmo = 30;
                newReserveAmmo = 150;
                newReloadTime = 3f;
                newAuto = true;
                newRecoilAngle = -0.5f;
                newReloadAngle = new Vector3(30f, 0f, 0f);
                newAimPosition = new Vector3(0f, -0.38f, 0.33f);
                newOriginalPosition = new Vector3(0.28f, -0.52f, 0.33f);
                newDamageRange = 30;
                break;

            case 2:
                newWeaponDamage = 80;
                newFireRate = 0.7f;
                newMaxMagazineAmmo = 8;
                newReserveAmmo = 48;
                newReloadTime = 5f;
                newAuto = false;
                newRecoilAngle = -8f;
                newReloadAngle = new Vector3(30f, 0f, 0f);
                newAimPosition = new Vector3(0f, -0.26f, 0.2f);
                newOriginalPosition = new Vector3(0.276f, -0.406f, 0.203f);
                newDamageRange = 10;
                break;

            case 3:
                newWeaponDamage = 160;
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
