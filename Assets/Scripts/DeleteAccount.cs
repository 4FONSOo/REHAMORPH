using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine.SceneManagement;
using System.IO;

public class DeleteAccount : MonoBehaviour
{
    private string dbPath;

    void Start()
    {
        // Define o caminho para o ficheiro da base de dados na pasta persistente
        dbPath = Path.Combine(Application.persistentDataPath, "game_data.db");
        CopyDatabaseIfNeeded();
    }

    // Copia a base de dados se n�o existir em persistentDataPath
    void CopyDatabaseIfNeeded()
    {
        string sourcePath = "C://Users//afons//REHAMORPH---MENUS-Work/game_data.db";

        if (!File.Exists(dbPath))
        {
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, dbPath);
                Debug.Log("Base de dados copiada para: " + dbPath);
            }
            else
            {
                Debug.LogError("ERRO: A base de dados de origem n�o existe! Caminho: " + sourcePath);
            }
        }
    }

    public void DeleteUserAccount()
    {
        Debug.Log("Tentativa de eliminar conta...");

        if (!PlayerPrefs.HasKey("loggedInUser"))
        {
            Debug.LogWarning("Nenhum utilizador est� logado!");
            return;
        }

        if (!File.Exists(dbPath))
        {
            Debug.LogError("Erro: A base de dados n�o existe!");
            return;
        }

        string loggedInEmail = PlayerPrefs.GetString("loggedInUser");
        string dbName = "URI=file:" + dbPath;

        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM player WHERE email = @loggedInEmail;";
                    command.Parameters.AddWithValue("@loggedInEmail", loggedInEmail);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Debug.Log("Conta eliminada com sucesso para o email: " + loggedInEmail);
                        PlayerPrefs.DeleteKey("loggedInUser");
                        SceneManager.LoadScene(2);
                    }
                    else
                    {
                        Debug.LogWarning("Conta n�o encontrada ou j� eliminada.");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Erro ao tentar eliminar conta: " + e.Message);
            }
            finally
            {
                connection.Close();
                Debug.Log("Conex�o com a BD fechada ap�s tentativa de elimina��o.");
            }
        }
    }
}
