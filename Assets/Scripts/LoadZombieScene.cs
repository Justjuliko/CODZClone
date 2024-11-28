using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadZombieScene : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    public void loadZombie()
    {
        SceneManager.LoadScene("Level");
    }
}
