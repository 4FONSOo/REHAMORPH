using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class ScriptMenu : MonoBehaviour
{
    public AudioMixer audioMixer; // Mixer de áudio
    public TMP_Dropdown ResolutionDropdown;
    public TMP_Dropdown QualityDropdown; // Dropdown para qualidade
    public Slider VolumeSlider; // Slider para volume
    public TMP_Text muteButtonText; // Referência ao texto do botão de mute

    private bool isMuted = false; // Estado do mute
    private Resolution[] resolutions;

    void Start()
    {
        // Configurar slider de volume
        if (VolumeSlider != null)
        {
            float currentVolume;
            if (audioMixer.GetFloat("Volume", out currentVolume))
            {
                VolumeSlider.value = currentVolume;
            }
        }
        else
        {
            Debug.LogError("Slider de volume não foi atribuído!");
        }

        // Configurar resoluções
        if (ResolutionDropdown == null)
        {
            ResolutionDropdown = FindObjectOfType<TMP_Dropdown>();
            if (ResolutionDropdown == null)
            {
                Debug.LogError("Nenhum Dropdown de resolução encontrado!");
                return;
            }
        }

        resolutions = Screen.resolutions;
        ResolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        ResolutionDropdown.AddOptions(options);
        ResolutionDropdown.value = currentResolutionIndex;
        ResolutionDropdown.RefreshShownValue();

        // Configurar Qualidade
        if (QualityDropdown != null)
        {
            QualityDropdown.ClearOptions();
            List<string> qualityOptions = new List<string> { "Baixa", "Média", "Alta" };
            QualityDropdown.AddOptions(qualityOptions);
            QualityDropdown.value = QualitySettings.GetQualityLevel();
            QualityDropdown.RefreshShownValue();
        }
        else
        {
            Debug.LogError("Dropdown de qualidade não foi atribuído!");
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("Volume", volume);
        }
        else
        {
            Debug.LogError("AudioMixer não foi atribuído!");
        }
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    // Função para ativar/desativar mute
    public void ToggleMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            audioMixer.SetFloat("Volume", -80f); // Volume mínimo (silêncio)
            if (muteButtonText != null)
                muteButtonText.text = "Unmute"; // Atualiza o texto do botão
        }
        else
        {
            float volume = (VolumeSlider != null) ? VolumeSlider.value : 0; // Obtém o volume do slider
            audioMixer.SetFloat("Volume", volume);
            if (muteButtonText != null)
                muteButtonText.text = "Mute"; // Atualiza o texto do botão
        }

        Debug.Log("Mute: " + isMuted);
    }
}
