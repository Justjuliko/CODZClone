using UnityEngine;

public class SpawnPointActivator : MonoBehaviour
{
    // Method called when another object enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered has the "PlayerPresence" tag
        if (other.CompareTag("PlayerPresence"))
        {
            // Access the first child of the object that entered the trigger
            Transform firstChild = other.transform.GetChild(0);

            // Activate the first child if it exists
            if (firstChild != null)
            {
                firstChild.gameObject.SetActive(true);
            }
        }
    }

    // Method called when another object exits the trigger
    private void OnTriggerExit(Collider other)
    {
        // Check if the object that exited has the "PlayerPresence" tag
        if (other.CompareTag("PlayerPresence"))
        {
            // Access the first child of the object that exited the trigger
            Transform firstChild = other.transform.GetChild(0);

            // Deactivate the first child if it exists
            if (firstChild != null)
            {
                firstChild.gameObject.SetActive(false);
            }
        }
    }
}
