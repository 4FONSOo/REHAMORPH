using UnityEngine;
using UnityEngine.SceneManagement;

public class UserScreenManager : MonoBehaviour
{
    public string userSceneName = "UserScene"; // Nome da cena padr�o para todos os usu�rios
    public DatabaseManager dbManager;

    private string currentUserId;

    // Carregar a cena padr�o para o usu�rio
    public void LoadUserScreen(string userId)
    {
        currentUserId = userId;
        dbManager.GetUser(userId, (user) =>
        {
            if (user != null)
            {
                SceneManager.LoadScene(userSceneName);
                Debug.Log($"Carregando cena do usu�rio: {userSceneName}");
            }
            else
            {
                Debug.LogError("Usu�rio n�o encontrado!");
            }
        });
    }

    // Getter para o ID do usu�rio atual (pode ser �til para outros scripts)
    public string GetCurrentUserId()
    {
        return currentUserId;
    }
}