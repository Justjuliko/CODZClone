using UnityEngine;

public class BloodEffect : MonoBehaviour
{
    public float effectDuration = 1f; // Duration for which the effect will be active

    private void OnEnable()
    {
        Invoke(nameof(DisableEffect), effectDuration); // Invoke the method to disable the effect after the specified duration
    }

    private void DisableEffect()
    {
        // Return the object to the pool
        ObjectPool pool = FindFirstObjectByType<ObjectPool>();
        if (pool != null)
        {
            pool.ReturnObject(gameObject); // Return the object to the pool
        }
    }

    private void OnDisable()
    {
        CancelInvoke(); // Cancel any pending invokes
    }
}
