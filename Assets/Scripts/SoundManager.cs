using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [Header("UI Settings")]
    public Slider volumeSlider; // El Slider que controlar� el volumen
    public AudioListener audioListener; // El componente AudioListener en la escena

    private void Start()
    {
        // Aseg�rate de que el AudioListener est� asignado, si no lo est�, toma el primero que encuentre
        if (audioListener == null)
        {
            audioListener = FindFirstObjectByType<AudioListener>();
        }

        // Si el slider est� asignado, configuramos su valor inicial al volumen actual
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("GameVolume", 1f); // Cargar el volumen guardado (valor por defecto 1)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged); // A�adir un listener para cambios en el Slider
        }
    }

    // M�todo que se llama cada vez que se cambia el valor del Slider
    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value; // Cambiar el volumen general del juego
        PlayerPrefs.SetFloat("GameVolume", value); // Guardar el valor del volumen en PlayerPrefs
    }

    // M�todo para ajustar el volumen manualmente sin el Slider (opcional)
    public void SetVolume(float value)
    {
        if (audioListener != null)
        {
            AudioListener.volume = value; // Ajustar el volumen
        }
    }
}
