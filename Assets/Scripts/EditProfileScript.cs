using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using TMPro;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;

public class EditProfile : MonoBehaviour
{
    public TMP_InputField editNomeInput;
    public TMP_InputField editIdadeInput;
    public TMP_InputField editPesoInput;
    public TMP_InputField editAlturaInput;
    public TMP_InputField editPasswordInput;
    public TMP_InputField editEmailInput;

    private string dbPath = "C://Users//Utilizador//REHAMORPH - MENUS//REHAMORPH - MENUS Work/game_data.db";

    void Start()
    {
    }

    // Exibe o menu de edição e preenche os campos com os dados atuais do utilizador
    public void OpenEditProfileMenu()
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

    // Atualiza os dados do perfil na base de dados
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
                    command.Parameters.AddWithValue("@email", editEmailInput.text);
                    command.Parameters.AddWithValue("@loggedInEmail", loggedInEmail);

                    command.ExecuteNonQuery();
                }

                // Se o utilizador inseriu uma nova senha, atualiza na BD
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

    // Função para criptografar a senha
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
}