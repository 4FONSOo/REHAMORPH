using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ScriptMenu : MonoBehaviour
{
    public TMP_Dropdown ResolutionDropdown;
    public TMP_Dropdown QualityDropdown; // Dropdown para qualidade
    public Button editProfileButton; // Bot�o para ir para a cena de edi��o de perfil
    public Button backButton;       // Bot�o para voltar ao menu principal
    private Resolution[] resolutions;

    void OnEnable()
    {
        // Configurar resolu��es
        if (ResolutionDropdown == null)
        {
            ResolutionDropdown = FindObjectOfType<TMP_Dropdown>();
            if (ResolutionDropdown == null)
            {
                Debug.LogError("Nenhum Dropdown de resolu��o encontrado!");
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
            List<string> qualityOptions = new List<string> { "Baixa", "M�dia", "Alta" };
            QualityDropdown.AddOptions(qualityOptions);
            QualityDropdown.value = QualitySettings.GetQualityLevel();
            QualityDropdown.RefreshShownValue();
        }
        else
        {
            Debug.LogError("Dropdown de qualidade n�o foi atribu�do!");
        }

        // Configurar os bot�es
        if (editProfileButton == null)
        {
            Debug.LogError("Bot�o 'Edit Profile Button' n�o atribu�do no Inspector!");
        }
        else
        {
            editProfileButton.onClick.AddListener(GoToEditProfile);
        }

        if (backButton == null)
        {
            Debug.LogError("Bot�o 'Back Button' n�o atribu�do no Inspector!");

        }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    void GoToEditProfile()
    {
        Debug.Log("Redirecionando para a cena de Edi��o de Perfil (cena 11).");
        SceneManager.LoadScene(10); // Cena de Edi��o de Perfil
    }
}