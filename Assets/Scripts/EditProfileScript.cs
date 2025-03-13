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

    void Start()
    {
        dbPath = Path.Combine(Application.persistentDataPath, "C:/Users/afons/AppData/LocalLow/DefaultCompany/REHAMORPH - MENUS/game_data.db");
        Debug.Log("Caminho da base de dados utilizada: " + dbPath);
        CopyDatabaseIfNeeded();
        Debug.Log("Email guardado no PlayerPrefs após login: " + PlayerPrefs.GetString("loggedInUser"));
        OpenEditProfileMenu();
    }

    void CopyDatabaseIfNeeded()
    {
        string sourcePath = "C:/Users/afons/AppData/LocalLow/DefaultCompany/REHAMORPH - MENUS/game_data.db";
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

    public void OpenEditProfileMenu()
    {
        if (!PlayerPrefs.HasKey("loggedInUser"))
        {
            Debug.LogWarning("Nenhum utilizador está logado!");
            return;
        }

        string loggedInEmail = PlayerPrefs.GetString("loggedInUser");
        Debug.Log("Email armazenado no PlayerPrefs: " + loggedInEmail);
        editEmailInput.text = loggedInEmail;

        if (!File.Exists(dbPath))
        {
            Debug.LogError("Erro: A base de dados não existe no caminho: " + dbPath);
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
                    command.CommandText = "SELECT nome, idade, peso, altura FROM player WHERE email = @loggedInEmail;";
                    command.Parameters.AddWithValue("@loggedInEmail", loggedInEmail);
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            editNomeInput.text = reader["nome"].ToString();
                            editIdadeInput.text = reader["idade"].ToString();
                            editPesoInput.text = reader["peso"].ToString();
                            editAlturaInput.text = reader["altura"].ToString();
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
        if (!PlayerPrefs.HasKey("loggedInUser"))
        {
            Debug.LogWarning("Nenhum utilizador está logado!");
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
                    command.CommandText = "UPDATE player SET nome = @nome, idade = @idade, peso = @peso, altura = @altura WHERE email = @loggedInEmail;";
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

    public void ForceReloadProfile()
    {
        PlayerPrefs.DeleteKey("loggedInUser");
        PlayerPrefs.SetString("loggedInUser", "teste@teste.pt");
        PlayerPrefs.Save();
        Debug.Log("Forçando carregamento do perfil para: " + PlayerPrefs.GetString("loggedInUser"));
        OpenEditProfileMenu();
    }

    public void CancelProfileEdit()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
