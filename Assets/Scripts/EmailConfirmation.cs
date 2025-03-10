using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class ConfirmEmail : MonoBehaviour
{
    // Associe esse campo a um TMP_InputField na sua cena onde o usuário digita o token
    public TMP_InputField tokenInput;

    private string dbPath;

    void Start()
    {
        if (tokenInput == null)
        {
            Debug.LogError("O campo tokenInput não foi atribuído no Inspector!");
            return;
        }

        // Define o caminho para o banco de dados na pasta persistente
        dbPath = Path.Combine(Application.persistentDataPath, "game_data.db");
        CopyDatabaseIfNeeded();
    }

    // Copia a base de dados para a pasta persistente, se ainda não existir
    void CopyDatabaseIfNeeded()
    {
        string sourcePath = "C://Users//Utilizador//REHAMORPH - MENUS//REHAMORPH---MENUS-Work/game_data.db";
        if (!File.Exists(dbPath))
        {
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, dbPath);
                Debug.Log("Base de dados copiada para: " + dbPath);
            }
            else
            {
                Debug.LogError("ERRO: A base de dados de origem não existe! Caminho: " + sourcePath);
            }
        }
    }

    // Método chamado ao clicar no botão de confirmar o token
    public void ConfirmUserAccount()
    {
        if (tokenInput == null)
        {
            Debug.LogError("O campo tokenInput não foi atribuído!");
            return;
        }

        string token = tokenInput.text.Trim();
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("Token está vazio!");
            return;
        }

        string connectionString = "URI=file:" + dbPath;
        using (var connection = new SqliteConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    // Verifica se existe um usuário com o token informado e que ainda não foi confirmado
                    command.CommandText = "SELECT COUNT(*) FROM player WHERE confirmation_token = @token AND is_confirmed = 0;";
                    command.Parameters.AddWithValue("@token", token);
                    int count = System.Convert.ToInt32(command.ExecuteScalar());

                    if (count > 0)
                    {
                        // Atualiza o status para confirmado
                        using (var updateCommand = connection.CreateCommand())
                        {
                            updateCommand.CommandText = "UPDATE player SET is_confirmed = 1 WHERE confirmation_token = @token;";
                            updateCommand.Parameters.AddWithValue("@token", token);
                            updateCommand.ExecuteNonQuery();
                        }
                        Debug.Log("Conta confirmada com sucesso!");
                        // Após confirmação, redireciona para a cena de login (build index 2)
                        SceneManager.LoadScene(2);
                    }
                    else
                    {
                        Debug.LogWarning("Token inválido ou conta já confirmada.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Erro ao confirmar conta: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
