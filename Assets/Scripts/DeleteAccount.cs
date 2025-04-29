using UnityEngine;
using UnityEngine.SceneManagement;

public class DeleteAccount : MonoBehaviour
{
    private DatabaseManager dbManager;
    private FireBaseAuth authManager;

    void Start()
    {
        // Encontrar os componentes DatabaseManager e AuthManager na cena
        dbManager = FindObjectOfType<DatabaseManager>();
        if (dbManager == null)
        {
            Debug.LogError("DatabaseManager não encontrado na cena! Certifique-se de que está anexado a um GameObject.");
            return;
        }

        authManager = FindObjectOfType<FireBaseAuth>();
        if (authManager == null)
        {
            Debug.LogError("AuthManager não encontrado na cena! Certifique-se de que está anexado a um GameObject.");
            return;
        }
    }

    public void DeleteUserAccount()
    {
        Debug.Log("Tentativa de eliminar conta...");

        if (!PlayerPrefs.HasKey("loggedInUser"))
        {
            Debug.LogWarning("Nenhum utilizador está logado!");
            return;
        }

        string loggedInEmail = PlayerPrefs.GetString("loggedInUser");
        string loggedInUserId = PlayerPrefs.GetString("loggedInUserId");

        if (string.IsNullOrEmpty(loggedInUserId))
        {
            Debug.LogWarning("UserId do usuário logado não encontrado!");
            return;
        }

        // Remover os dados do usuário do Firebase Realtime Database
        Debug.Log($"Removendo dados do usuário no caminho 'users/{loggedInUserId}'...");
        Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference
            .Child("users")
            .Child(loggedInUserId)
            .RemoveValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Erro ao remover dados do usuário: {task.Exception}");
                    return;
                }

                if (task.IsCompleted)
                {
                    Debug.Log("Dados do usuário removidos com sucesso do Firebase Realtime Database!");
                }
            });

        // Excluir a conta do usuário no Firebase Authentication
        var firebaseUser = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (firebaseUser == null)
        {
            Debug.LogError("Nenhum usuário autenticado encontrado!");
            return;
        }

        firebaseUser.DeleteAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Erro ao excluir conta do Firebase Authentication: {task.Exception}");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Conta excluída com sucesso do Firebase Authentication!");
                PlayerPrefs.DeleteKey("loggedInUser");
                PlayerPrefs.DeleteKey("loggedInUserId");
                PlayerPrefs.Save();
                SceneManager.LoadScene(2); // Redirecionar para a tela de login
            }
        });
    }
}