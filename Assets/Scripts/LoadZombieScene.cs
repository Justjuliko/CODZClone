using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadZombieScene : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void loadZombie()
    {
        SceneManager.LoadScene("Level");
    }
}
