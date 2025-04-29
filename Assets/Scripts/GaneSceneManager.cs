using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public Button goToSettingsButton; // Botão para ir para a cena de definições

    void Start()
    {
        // Verificar se há um usuário logado
        if (!PlayerPrefs.HasKey("loggedInUserId") || string.IsNullOrEmpty(PlayerPrefs.GetString("loggedInUserId")))
        {
            Debug.LogWarning("Nenhum usuário logado! Redirecionando para a tela de login...");
            SceneManager.LoadScene(2); // Redireciona para a cena de login
            return;
        }

        // Exibir uma mensagem de boas-vindas (opcional)
        string email = PlayerPrefs.GetString("loggedInUser");
        Debug.Log($"Bem-vindo ao jogo, {email}!");

        // Configurar o botão para ir às definições
        if (goToSettingsButton == null)
        {
            Debug.LogError("Campo 'Go To Settings Button' não atribuído no Inspector!");
            return;
        }

        goToSettingsButton.onClick.AddListener(OnGoToSettingsButtonClicked);
    }

    void OnGoToSettingsButtonClicked()
    {
        Debug.Log("Redirecionando para a cena de definições...");
        SceneManager.LoadScene(11); // Substitua pelo índice da sua cena de definições
    }
}