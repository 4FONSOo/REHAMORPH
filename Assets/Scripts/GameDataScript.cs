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
    // Campos de registro
    public TMP_InputField nomeInput;
    public TMP_InputField idadeInput;
    public TMP_InputField pesoInput;
    public TMP_InputField alturaInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    // Campos de login
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public TMP_Text feedbackText;

    private string dbPath;
    private const string salt = "s3gur@!"; // Salt para segurança da senha

    void Start()
    {
        // Usa o caminho definido no DatabaseManager
        dbPath = DatabaseManager.dbPath;
        Debug.Log("Caminho da base de dados: " + dbPath);

        if (!File.Exists(dbPath))
        {
            Debug.LogWarning("Base de dados não encontrada em: " + dbPath);
        }
        else
        {
            Debug.Log("Base de dados encontrada!");
        }
    }

    // Detecta a tecla "Enter" e chama a função adequada dependendo da página ativa
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Se o campo de registro estiver ativo, chama RegisterUser()
            if (nomeInput != null && nomeInput.gameObject.activeInHierarchy)
            {
                RegisterUser();
            }
            // Caso contrário, se o campo de login estiver ativo, chama LoginUser()
            else if (loginEmailInput != null && loginEmailInput.gameObject.activeInHierarchy)
            {
                LoginUser();
            }
        }
    }

    public void RegisterUser()
    {
        Debug.Log("Entrou no RegisterUser()");

        // Verifica se algum campo está vazio
        if (string.IsNullOrWhiteSpace(nomeInput.text) ||
            string.IsNullOrWhiteSpace(idadeInput.text) ||
            string.IsNullOrWhiteSpace(pesoInput.text) ||
            string.IsNullOrWhiteSpace(alturaInput.text) ||
            string.IsNullOrWhiteSpace(emailInput.text) ||
            string.IsNullOrWhiteSpace(passwordInput.text))
        {
            ShowFeedback("Todos os campos são obrigatórios!", false);
            return;
        }

        string passwordHash = HashPassword(passwordInput.text);
        string dbName = "URI=file:" + dbPath;
        bool registoSucesso = false;

        Debug.Log("Tentando conectar à base de dados...");
        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                Debug.Log("Conexão estabelecida com sucesso!");

                // Verifica se o e-mail já está registado
                using (var checkCommand = connection.CreateCommand())
                {
                    checkCommand.CommandText = "SELECT COUNT(*) FROM player WHERE email = @email";
                    checkCommand.Parameters.AddWithValue("@email", emailInput.text);
                    int count = System.Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (count > 0)
                    {
                        ShowFeedback("Este e-mail já está registado!", false);
                        return;
                    }
                }

                // Insere os dados na base de dados
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO player (nome, idade, peso, altura, email, password_hash) 
                                            VALUES (@nome, @idade, @peso, @altura, @email, @password_hash);";
                    command.Parameters.AddWithValue("@nome", nomeInput.text);
                    command.Parameters.AddWithValue("@idade", idadeInput.text);
                    command.Parameters.AddWithValue("@peso", pesoInput.text);
                    command.Parameters.AddWithValue("@altura", alturaInput.text);
                    command.Parameters.AddWithValue("@email", emailInput.text);
                    command.Parameters.AddWithValue("@password_hash", passwordHash);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        ShowFeedback("Conta criada com sucesso!", true);
                        registoSucesso = true;
                        // Limpa os campos após um registro bem-sucedido
                        nomeInput.text = "";
                        idadeInput.text = "";
                        pesoInput.text = "";
                        alturaInput.text = "";
                        emailInput.text = "";
                        passwordInput.text = "";
                    }
                    else
                    {
                        ShowFeedback("Erro ao inserir dados!", false);
                        return;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("ERRO AO INSERIR: " + e.Message);
                ShowFeedback("Erro ao inserir dados!", false);
                return;
            }
            finally
            {
                connection.Close();
                Debug.Log("Conexão com a BD fechada.");
            }
        }

        if (registoSucesso)
        {
            SceneManager.LoadScene(2);
        }
    }

    public void LoginUser()
    {
        Debug.Log("Entrou no LoginUser()");

        if (string.IsNullOrWhiteSpace(loginEmailInput.text) && string.IsNullOrWhiteSpace(loginPasswordInput.text))
        {
            ShowFeedback("Preencha o e-mail e a senha!", false);
            return;
        }

        if (string.IsNullOrWhiteSpace(loginEmailInput.text))
        {
            ShowFeedback("Preencha o e-mail!", false);
            return;
        }

        if (string.IsNullOrWhiteSpace(loginPasswordInput.text))
        {
            ShowFeedback("Preencha a senha!", false);
            return;
        }

        string dbName = "URI=file:" + dbPath;

        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                Debug.Log("Conexão com a BD aberta para login.");

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM player WHERE email = @email;";
                    command.Parameters.AddWithValue("@email", loginEmailInput.text);

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader["password_hash"].ToString();
                            string enteredHash = HashPassword(loginPasswordInput.text);

                            if (storedHash == enteredHash)
                            {
                                Debug.Log("Login bem-sucedido para: " + loginEmailInput.text);
                                ShowFeedback("Login bem-sucedido!", true);
                                PlayerPrefs.SetString("loggedInUser", loginEmailInput.text);
                                SceneManager.LoadScene(4);
                            }
                            else
                            {
                                Debug.LogWarning("Senha incorreta para o email: " + loginEmailInput.text);
                                ShowFeedback("Senha incorreta!", false);
                                return;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Conta não encontrada para o email: " + loginEmailInput.text);
                            ShowFeedback("Conta não encontrada!", false);
                            return;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Erro ao tentar login: " + e.Message);
                ShowFeedback("Erro ao tentar login!", false);
                return;
            }
            finally
            {
                connection.Close();
                Debug.Log("Conexão com a BD fechada após tentativa de login.");
            }
        }
    }

    public void LogoutUser()
    {
        Debug.Log("Logout realizado. Redirecionando para a tela de login...");
        PlayerPrefs.DeleteKey("loggedInUser");
        SceneManager.LoadScene(2);
    }

    void ShowFeedback(string message, bool isSuccess)
    {
        feedbackText.text = message;
        feedbackText.color = isSuccess ? Color.blue : Color.blue;
        StartCoroutine(HideFeedbackAfterTime(5f));
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
