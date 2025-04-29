using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public Button goToSettingsButton; // Bot�o para ir para a cena de defini��es

    void Start()
    {
        // Verificar se h� um usu�rio logado
        if (!PlayerPrefs.HasKey("loggedInUserId") || string.IsNullOrEmpty(PlayerPrefs.GetString("loggedInUserId")))
        {
            Debug.LogWarning("Nenhum usu�rio logado! Redirecionando para a tela de login...");
            SceneManager.LoadScene(2); // Redireciona para a cena de login
            return;
        }

        // Exibir uma mensagem de boas-vindas (opcional)
        string email = PlayerPrefs.GetString("loggedInUser");
        Debug.Log($"Bem-vindo ao jogo, {email}!");

        // Configurar o bot�o para ir �s defini��es
        if (goToSettingsButton == null)
        {
            Debug.LogError("Campo 'Go To Settings Button' n�o atribu�do no Inspector!");
            return;
        }

        goToSettingsButton.onClick.AddListener(OnGoToSettingsButtonClicked);
    }

    void OnGoToSettingsButtonClicked()
    {
        Debug.Log("Redirecionando para a cena de defini��es...");
        SceneManager.LoadScene(11); // Substitua pelo �ndice da sua cena de defini��es
    }
}