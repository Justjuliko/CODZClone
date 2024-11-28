using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [Header("UI Settings")]
    public Slider volumeSlider; // El Slider que controlará el volumen
    public AudioListener audioListener; // El componente AudioListener en la escena

    private void Start()
    {
        // Asegúrate de que el AudioListener esté asignado, si no lo está, toma el primero que encuentre
        if (audioListener == null)
        {
            audioListener = FindFirstObjectByType<AudioListener>();
        }

        // Si el slider está asignado, configuramos su valor inicial al volumen actual
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("GameVolume", 1f); // Cargar el volumen guardado (valor por defecto 1)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged); // Añadir un listener para cambios en el Slider
        }
    }

    // Método que se llama cada vez que se cambia el valor del Slider
    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value; // Cambiar el volumen general del juego
        PlayerPrefs.SetFloat("GameVolume", value); // Guardar el valor del volumen en PlayerPrefs
    }

    // Método para ajustar el volumen manualmente sin el Slider (opcional)
    public void SetVolume(float value)
    {
        if (audioListener != null)
        {
            AudioListener.volume = value; // Ajustar el volumen
        }
    }
}
