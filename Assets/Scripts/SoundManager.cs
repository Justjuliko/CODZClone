using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [Header("UI Settings")]
    public Slider volumeSlider; // The Slider that will control the volume
    public AudioListener audioListener; // The AudioListener component in the scene

    private void Start()
    {
        // Make sure the AudioListener is assigned, if not, get the first one found
        if (audioListener == null)
        {
            audioListener = FindFirstObjectByType<AudioListener>();
        }

        // If the slider is assigned, set its initial value to the current volume
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("GameVolume", 1f); // Load the saved volume (default value is 1)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged); // Add a listener for changes in the Slider
        }
    }

    // Method called every time the Slider value changes
    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value; // Change the overall volume of the game
        PlayerPrefs.SetFloat("GameVolume", value); // Save the volume value in PlayerPrefs
    }

    // Method to manually adjust the volume without the Slider (optional)
    public void SetVolume(float value)
    {
        if (audioListener != null)
        {
            AudioListener.volume = value; // Adjust the volume
        }
    }
}
