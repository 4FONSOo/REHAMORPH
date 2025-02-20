using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle muteToggle; // Toggle do UI para mute

    private float _lastVolume = 0.5f; // Volume salvo antes de mutar

    private void Start()
    {
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            _lastVolume = musicSlider.value;
            SetVolume();
        }

        muteToggle.onValueChanged.AddListener(delegate { MuteToggle(); }); // Adiciona evento ao botão de mute
    }

    // Método para alterar o volume com base no slider
    public void SetVolume()
    {
        float volume = musicSlider.value;
        _lastVolume = volume; // Salva o volume antes de mutar
        myMixer.SetFloat("music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
        PlayerPrefs.Save();
    }

    // Método para mutar e desmutar
    public void MuteToggle()
    {
        if (muteToggle.isOn)
        {
            myMixer.SetFloat("music", -80f); // Volume mínimo no AudioMixer (silêncio total)
        }
        else
        {
            myMixer.SetFloat("music", Mathf.Log10(_lastVolume) * 20); // Restaura o último volume salvo
        }
    }

    private void LoadVolume()
    {
        _lastVolume = PlayerPrefs.GetFloat("musicVolume");
        musicSlider.value = _lastVolume;
        SetVolume();
    }
}
