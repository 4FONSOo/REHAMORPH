using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using TMPro;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;
using System.IO;

public class EditProfile : MonoBehaviour
{
    public TMP_InputField editNomeInput;
    public TMP_InputField editIdadeInput;
    public TMP_InputField editPesoInput;
    public TMP_InputField editAlturaInput;
    public TMP_InputField editPasswordInput;
    public TMP_InputField editEmailInput;

    private string dbPath;
    private string loggedInEmail;

    void Start()
    {
        dbPath = Path.Combine(Application.persistentDataPath, "game_data.db");
        Debug.Log("Caminho da base de dados: " + dbPath);

        // Verifica se há um usuário logado
        if (PlayerPrefs.HasKey("loggedInUser"))
        {
            loggedInEmail = PlayerPrefs.GetString("loggedInUser");
            Debug.Log("Usuário logado: " + loggedInEmail);
        }
        else
        {
            Debug.LogWarning("Nenhum utilizador está logado! Redirecionando para a tela de login.");
            SceneManager.LoadScene(2); 
            return;
        }

        CopyDatabaseIfNeeded();
        LoadUserProfile(); // Agora só chama se houver um utilziador logado
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
                Debug.LogError("ERRO: O banco de dados não existe no caminho: " + sourcePath);
            }
        }
    }

    public void LoadUserProfile()
    {
        if (string.IsNullOrEmpty(loggedInEmail))
        {
            Debug.LogWarning("Erro ao carregar perfil: Nenhum utilizador está logado!");
            return;
        }

        editEmailInput.text = loggedInEmail;

        if (!File.Exists(dbPath))
        {
            Debug.LogError("Erro: O banco de dados não existe!");
            return;
        }

        string dbName = "URI=file:" + dbPath;
        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT nome, idade, peso, altura FROM player WHERE email = @email;";
                    command.Parameters.AddWithValue("@email", loggedInEmail);

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            editNomeInput.text = reader["nome"].ToString();
                            editIdadeInput.text = reader["idade"].ToString();
                            editPesoInput.text = reader["peso"].ToString();
                            editAlturaInput.text = reader["altura"].ToString();
                            Debug.Log("Dados carregados com sucesso!");
                        }
                        else
                        {
                            Debug.LogWarning("Nenhum dado encontrado para o email: " + loggedInEmail);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Erro ao carregar dados do utilizador: " + e.Message);
            }
            finally
            {
                connection.Close();
            }
        }
    }

    public void SaveProfileChanges()
    {
        if (string.IsNullOrEmpty(loggedInEmail))
        {
            Debug.LogWarning("Erro ao salvar perfil: Nenhum utilizador está logado!");
            return;
        }

        if (!File.Exists(dbPath))
        {
            Debug.LogError("Erro: O banco de dados não existe!");
            return;
        }

        string dbName = "URI=file:" + dbPath;
        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE player 
                        SET nome = @nome, idade = @idade, peso = @peso, altura = @altura 
                        WHERE email = @loggedInEmail;";

                    command.Parameters.AddWithValue("@nome", editNomeInput.text);
                    command.Parameters.AddWithValue("@idade", editIdadeInput.text);
                    command.Parameters.AddWithValue("@peso", editPesoInput.text);
                    command.Parameters.AddWithValue("@altura", editAlturaInput.text);
                    command.Parameters.AddWithValue("@loggedInEmail", loggedInEmail);
                    command.ExecuteNonQuery();
                }

                if (!string.IsNullOrWhiteSpace(editPasswordInput.text))
                {
                    string newPasswordHash = HashPassword(editPasswordInput.text);
                    using (var passwordCommand = connection.CreateCommand())
                    {
                        passwordCommand.CommandText = "UPDATE player SET password_hash = @passwordHash WHERE email = @loggedInEmail;";
                        passwordCommand.Parameters.AddWithValue("@passwordHash", newPasswordHash);
                        passwordCommand.Parameters.AddWithValue("@loggedInEmail", loggedInEmail);
                        passwordCommand.ExecuteNonQuery();
                    }
                }

                Debug.Log("Perfil atualizado com sucesso!");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Erro ao atualizar o perfil: " + e.Message);
            }
            finally
            {
                connection.Close();
            }
        }
    }

    private string HashPassword(string password)
    {
        string salt = "s3gur@!";
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password + salt);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public void CancelProfileEdit()
    {
        SceneManager.LoadScene(10);
    }
}
