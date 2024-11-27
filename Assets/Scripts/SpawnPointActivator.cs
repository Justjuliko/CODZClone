using UnityEngine;

public class SpawnPointActivator : MonoBehaviour
{
    // Método llamado cuando otro objeto entra en el trigger
    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si el objeto que entra tiene el tag "PlayerPresence"
        if (other.CompareTag("PlayerPresence"))
        {
            // Accedemos al primer hijo del objeto que entra en el trigger
            Transform firstChild = other.transform.GetChild(0);

            // Activamos el primer hijo si existe
            if (firstChild != null)
            {
                firstChild.gameObject.SetActive(true);
            }
        }
    }

    // Método llamado cuando otro objeto sale del trigger
    private void OnTriggerExit(Collider other)
    {
        // Verificamos si el objeto que sale tiene el tag "PlayerPresence"
        if (other.CompareTag("PlayerPresence"))
        {
            // Accedemos al primer hijo del objeto que salió del trigger
            Transform firstChild = other.transform.GetChild(0);

            // Desactivamos el primer hijo si existe
            if (firstChild != null)
            {
                firstChild.gameObject.SetActive(false);
            }
        }
    }
}
