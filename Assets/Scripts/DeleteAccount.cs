using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine.SceneManagement;

public class DeleteAccount : MonoBehaviour
{
    private string dbPath = "C://Users//Utilizador//REHAMORPH - MENUS//REHAMORPH - MENUS Work/game_data.db";

    public void DeleteUserAccount()
    {
        Debug.Log("Tentativa de eliminar conta...");

        if (!PlayerPrefs.HasKey("loggedInUser"))
        {
            Debug.LogWarning("Nenhum utilizador est� logado!");
            return;
        }

        string loggedInEmail = PlayerPrefs.GetString("loggedInUser");
        string dbName = "URI=file:" + dbPath;

        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                Debug.Log("Conex�o com a BD aberta para elimina��o de conta.");

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM player WHERE email = @loggedInEmail;";
                    command.Parameters.AddWithValue("@loggedInEmail", loggedInEmail);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Debug.Log("Conta eliminada com sucesso para o email: " + loggedInEmail);

                        // Remove os dados do utilizador e volta para a tela de login
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