using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using TMPro;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;
using System.IO;

public class game_dataScript : MonoBehaviour
{
    public TMP_InputField nomeInput;
    public TMP_InputField idadeInput;
    public TMP_InputField pesoInput;
    public TMP_InputField alturaInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public TMP_Text feedbackText;

    private string dbPath;
    private const string salt = "s3gur@!"; // Salt para segurança da senha

    void Start()
    {
        dbPath = Application.persistentDataPath + "/game_data.db";
        Debug.Log("📂 Caminho da base de dados: " + dbPath);

        if (!File.Exists(dbPath))
        {
            Debug.LogWarning("🚨 Base de dados não encontrada! Certifica-te de que estás a apontar para o ficheiro correto.");
        }
        else
        {
            Debug.Log("✅ Base de dados encontrada!");
        }

        // Verifica se há um usuário autenticado para login automático
        /*if (PlayerPrefs.HasKey("loggedInUser"))
        {
            string loggedInEmail = PlayerPrefs.GetString("loggedInUser");
            Debug.Log("🔐 Usuário autenticado detectado: " + loggedInEmail);
            SceneManager.LoadScene(4); // Avança automaticamente para a cena do jogo
        }*/
    }

    public void RegisterUser()
    {
        Debug.Log("🔵 Entrou no RegisterUser()");

        if (string.IsNullOrWhiteSpace(nomeInput.text) ||
            string.IsNullOrWhiteSpace(idadeInput.text) ||
            string.IsNullOrWhiteSpace(pesoInput.text) ||
            string.IsNullOrWhiteSpace(alturaInput.text) ||
            string.IsNullOrWhiteSpace(emailInput.text) ||
            string.IsNullOrWhiteSpace(passwordInput.text))
        {
            ShowFeedback("❌ Todos os campos são obrigatórios!", false);
            SceneManager.LoadScene(3);
            return;
        }

        if (!int.TryParse(idadeInput.text, out int idade) ||
            !float.TryParse(pesoInput.text, out float peso) ||
            !float.TryParse(alturaInput.text, out float altura))
        {
            ShowFeedback("❌ Idade, Peso e Altura devem ser números!", false);
            return;
        }

        string passwordHash = HashPassword(passwordInput.text);
        string dbName = "URI=file:" + dbPath;

        Debug.Log("🟢 Tentando conectar à base de dados...");
        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                Debug.Log("✅ Conexão estabelecida com sucesso!");

                // Verifica se o e-mail já existe
                using (var checkCommand = connection.CreateCommand())
                {
                    checkCommand.CommandText = "SELECT COUNT(*) FROM player WHERE email = @email";
                    checkCommand.Parameters.AddWithValue("@email", emailInput.text);
                    int count = System.Convert.ToInt32(checkCommand.ExecuteScalar());
                    Debug.Log("🔍 Usuários com este email: " + count);

                    if (count > 0)
                    {
                        ShowFeedback("❌ Este e-mail já está cadastrado!", false);
                        return;
                    }
                }

                // Insere os dados do novo usuário
                using (var command = connection.CreateCommand())
                {
                    Debug.Log("🚀 Executando o INSERT na base de dados...");
                    command.CommandText = @"INSERT INTO player (nome, idade, peso, altura, email, password_hash) 
                                            VALUES (@nome, @idade, @peso, @altura, @email, @password_hash);";
                    command.Parameters.AddWithValue("@nome", nomeInput.text);
                    command.Parameters.AddWithValue("@idade", idade);
                    command.Parameters.AddWithValue("@peso", peso);
                    command.Parameters.AddWithValue("@altura", altura);
                    command.Parameters.AddWithValue("@email", emailInput.text);
                    command.Parameters.AddWithValue("@password_hash", passwordHash);

                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.Log("🟢 Linhas afetadas pelo INSERT: " + rowsAffected);

                    if (rowsAffected > 0)
                    {
                        ShowFeedback("✅ Conta criada com sucesso!", true);
                    }
                    else
                    {
                        ShowFeedback("❌ Erro ao inserir dados!", false);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ ERRO AO INSERIR: " + e.Message);
                ShowFeedback("❌ Erro ao inserir dados!", false);
            }
            finally
            {
                connection.Close();
                Debug.Log("🔴 Conexão com a BD fechada.");
            }
        }
    }

    public void LoginUser()
    {
        Debug.Log("🔵 Entrou no LoginUser()");

        if (string.IsNullOrWhiteSpace(loginEmailInput.text) || string.IsNullOrWhiteSpace(loginPasswordInput.text))
        {
            ShowFeedback("❌ Preencha e-mail e senha!", false);
            return;
        }

        string dbName = "URI=file:" + dbPath;

        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                Debug.Log("🟢 Conexão com a BD aberta para login.");

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT password_hash FROM player WHERE email = @email;";
                    command.Parameters.AddWithValue("@email", loginEmailInput.text);

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader["password_hash"].ToString();
                            string enteredHash = HashPassword(loginPasswordInput.text);

                            if (storedHash == enteredHash) // ✅ Senha correta
                            {
                                Debug.Log("✅ Login bem-sucedido para: " + loginEmailInput.text);
                                ShowFeedback("✅ Login bem-sucedido!", true);

                                PlayerPrefs.SetString("loggedInUser", loginEmailInput.text);

                                // 🔥 AGORA SIM, AVANÇA PARA A PRÓXIMA CENA
                                SceneManager.LoadScene(4);
                            }
                            else // ❌ Senha incorreta
                            {
                                Debug.LogWarning("❌ Senha incorreta para o email: " + loginEmailInput.text);
                                ShowFeedback("❌ Senha incorreta!", false);
                                return; // 🔥 NÃO AVANÇA
                            }
                        }
                        else // ❌ Conta não encontrada
                        {
                            Debug.LogWarning("❌ Conta não encontrada para o email: " + loginEmailInput.text);
                            ShowFeedback("❌ Conta não encontrada!", false);
                            return; // 🔥 NÃO AVANÇA
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ Erro ao tentar login: " + e.Message);
                ShowFeedback("❌ Erro ao tentar login!", false);
                return; // 🔥 NÃO AVANÇA
            }
            finally
            {
                connection.Close();
                Debug.Log("🔴 Conexão com a BD fechada após tentativa de login.");
            }
        }
    }

    public void LogoutUser()
    {
        Debug.Log("🔴 Logout realizado. Redirecionando para a tela de login...");
        PlayerPrefs.DeleteKey("loggedInUser");
        SceneManager.LoadScene(2); // Retorna para a tela de login
    }

    void ShowFeedback(string message, bool isSuccess)
    {
        feedbackText.text = message;
        feedbackText.color = isSuccess ? Color.green : Color.red;
        StartCoroutine(HideFeedbackAfterTime(3f));  // Oculta a mensagem após 3 segundos
    }

    IEnumerator HideFeedbackAfterTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        feedbackText.text = "";
    }

    string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password + salt);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
