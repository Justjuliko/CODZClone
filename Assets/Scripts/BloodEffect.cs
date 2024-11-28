using UnityEngine;

public class BloodEffect : MonoBehaviour
{
    public float effectDuration = 1f; // Tiempo que el efecto estará activo

    private void OnEnable()
    {
        Invoke(nameof(DisableEffect), effectDuration);
    }

    private void DisableEffect()
    {
        // Retornar el objeto al pool
        ObjectPool pool = FindFirstObjectByType<ObjectPool>();
        if (pool != null)
        {
            pool.ReturnObject(gameObject);
        }
    }

    private void OnDisable()
    {
        CancelInvoke(); // Cancelar invocaciones pendientes
    }
}
