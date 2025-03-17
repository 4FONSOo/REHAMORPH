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
        // Caminho do banco de dados na pasta persistente
        dbPath = Path.Combine(Application.persistentDataPath, "game_data.db");
        CopyDatabaseIfNeeded();
    }

    void CopyDatabaseIfNeeded()
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "game_data.db");

        if (!File.Exists(dbPath))
        {
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, dbPath);
                Debug.Log("Banco de dados copiado para: " + dbPath);
            }
            else
            {
                Debug.LogError("ERRO: O banco de dados de origem não existe! Caminho: " + sourcePath);
            }
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

        if (!File.Exists(dbPath))
        {
            Debug.LogError("Erro: O banco de dados não existe!");
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
                        PlayerPrefs.Save();
                        SceneManager.LoadScene(2); // Redireciona para a tela de login
                    }
                    else
                    {
                        Debug.LogWarning("Conta não encontrada ou já eliminada.");
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
                Debug.Log("Conexão com a BD fechada após tentativa de eliminação.");
            }
        }
    }
}
