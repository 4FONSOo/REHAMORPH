using UnityEngine;
using UnityEngine.SceneManagement;

public class UserScreenManager : MonoBehaviour
{
    public string userSceneName = "UserScene"; // Nome da cena padrão para todos os usuários
    public DatabaseManager dbManager;

    private string currentUserId;

    // Carregar a cena padrão para o usuário
    public void LoadUserScreen(string userId)
    {
        currentUserId = userId;
        dbManager.GetUser(userId, (user) =>
        {
            if (user != null)
            {
                SceneManager.LoadScene(userSceneName);
                Debug.Log($"Carregando cena do usuário: {userSceneName}");
            }
            else
            {
                Debug.LogError("Usuário não encontrado!");
            }
        });
    }

    // Getter para o ID do usuário atual (pode ser útil para outros scripts)
    public string GetCurrentUserId()
    {
        return currentUserId;
    }
}