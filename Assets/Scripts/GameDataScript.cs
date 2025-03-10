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
using System.Net;
using System.Net.Mail;

public class GameDataScript : MonoBehaviour
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
    private const string salt = "s3gur@!";

    // Configurações do SMTP – ajuste de acordo com seu provedor
    private const string smtpServer = "smtp.yourserver.com"; // Ex: smtp.gmail.com
    private const int smtpPort = 587; // Porta comum para TLS
    private const string smtpUser = "yourEmail@domain.com"; // Seu e-mail usado no SMTP
    private const string smtpPass = "yourPassword"; // Senha do e-mail
    // URL base do seu endpoint web de confirmação (não será utilizado diretamente, mas pode ser exibido no e-mail)
    private const string confirmationUrlBase = "http://yourserver.com/confirm?token=";

    void Start()
    {
        // Define o caminho para o arquivo de banco de dados na pasta persistente
        dbPath = Path.Combine(Application.persistentDataPath, "game_data.db");
        CopyDatabaseIfNeeded();

        Debug.Log("Database path: " + dbPath);
    }

    // Copia a base de dados se não existir no persistentDataPath
    void CopyDatabaseIfNeeded()
    {
        // Ajuste o caminho de origem conforme sua configuração local
        string sourcePath = "C://Users//Utilizador//REHAMORPH - MENUS//REHAMORPH---MENUS-Work/game_data.db";
        if (!File.Exists(dbPath))
        {
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, dbPath);
                Debug.Log("Database copied to: " + dbPath);
            }
            else
            {
                Debug.LogError("Source database not found at: " + sourcePath);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Se os campos de registro estiverem ativos
            if (nomeInput != null && nomeInput.gameObject.activeInHierarchy)
            {
                RegisterUser();
            }
            // Se os campos de login estiverem ativos
            else if (loginEmailInput != null && loginEmailInput.gameObject.activeInHierarchy)
            {
                LoginUser();
            }
        }
    }

    // Método de registro com geração de token de confirmação e envio de e-mail
    public void RegisterUser()
    {
        Debug.Log("RegisterUser() called");

        if (string.IsNullOrWhiteSpace(nomeInput.text) ||
            string.IsNullOrWhiteSpace(idadeInput.text) ||
            string.IsNullOrWhiteSpace(pesoInput.text) ||
            string.IsNullOrWhiteSpace(alturaInput.text) ||
            string.IsNullOrWhiteSpace(emailInput.text) ||
            string.IsNullOrWhiteSpace(passwordInput.text))
        {
            ShowFeedback("All fields are required!", false);
            return;
        }

        // Armazena o email antes de limpar os campos
        string userEmail = emailInput.text;

        string passwordHash = HashPassword(passwordInput.text);
        // Gera o token de confirmação usando Guid
        string confirmationToken = System.Guid.NewGuid().ToString();

        string dbName = "URI=file:" + dbPath;
        bool registrationSuccess = false;

        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                Debug.Log("Connected to DB for registration");

                // Verifica se o e-mail já está registrado
                using (var checkCommand = connection.CreateCommand())
                {
                    checkCommand.CommandText = "SELECT COUNT(*) FROM player WHERE email = @email";
                    checkCommand.Parameters.AddWithValue("@email", userEmail);
                    int count = System.Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (count > 0)
                    {
                        ShowFeedback("This email is already registered!", false);
                        return;
                    }
                }

                // Insere os dados do novo usuário com o token e is_confirmed = 0
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT INTO player (nome, idade, peso, altura, email, password_hash, confirmation_token, is_confirmed)
                                          VALUES (@nome, @idade, @peso, @altura, @email, @password_hash, @token, 0);";
                    command.Parameters.AddWithValue("@nome", nomeInput.text);
                    command.Parameters.AddWithValue("@idade", idadeInput.text);
                    command.Parameters.AddWithValue("@peso", pesoInput.text);
                    command.Parameters.AddWithValue("@altura", alturaInput.text);
                    command.Parameters.AddWithValue("@email", userEmail);
                    command.Parameters.AddWithValue("@password_hash", passwordHash);
                    command.Parameters.AddWithValue("@token", confirmationToken);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        registrationSuccess = true;
                        ShowFeedback("Account created! Check your email to confirm your account.", true);
                        ClearRegistrationFields();
                    }
                    else
                    {
                        ShowFeedback("Error inserting data!", false);
                        return;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error inserting data: " + e.Message);
                ShowFeedback("Error inserting data!", false);
                return;
            }
            finally
            {
                connection.Close();
                Debug.Log("DB connection closed after registration");
            }
        }

        if (registrationSuccess)
        {
            // Utiliza a variável userEmail, que contém o endereço informado pelo usuário,
            // mesmo após os campos serem limpos.
            StartCoroutine(SendConfirmationEmail(userEmail, confirmationToken));
            // Redireciona para a cena de confirmação (build index 12)
            SceneManager.LoadScene(12);
        }
    }


    // Envia o e-mail de confirmação utilizando o .NET SMTP (System.Net.Mail)
    IEnumerator SendConfirmationEmail(string userEmail, string confirmationToken)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            Debug.LogError("Email do usuário está vazio. Não é possível enviar o e-mail de confirmação.");
            yield break;
        }

        string confirmationLink = confirmationUrlBase + confirmationToken;

        SmtpClient client = new SmtpClient(smtpServer, smtpPort);
        client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);
        client.EnableSsl = true;

        MailMessage mailMessage = new MailMessage();
        mailMessage.From = new System.Net.Mail.MailAddress(smtpUser);
        mailMessage.To.Add(userEmail);
        mailMessage.Subject = "Confirm your account";
        mailMessage.Body = "Please confirm your account by clicking the following link: " + confirmationLink;

        try
        {
            client.Send(mailMessage);
            Debug.Log("Confirmation email sent to: " + userEmail);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error sending confirmation email: " + ex.Message);
        }

        yield return null;
    }

    // Método de login que só permite acesso se a conta estiver confirmada (is_confirmed = 1)
    public void LoginUser()
    {
        Debug.Log("LoginUser() called");

        if (string.IsNullOrWhiteSpace(loginEmailInput.text) || string.IsNullOrWhiteSpace(loginPasswordInput.text))
        {
            ShowFeedback("Fill in email and password!", false);
            return;
        }

        string dbName = "URI=file:" + dbPath;

        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                Debug.Log("DB connection opened for login");

                // Seleciona somente contas confirmadas
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM player WHERE email = @email AND is_confirmed = 1;";
                    command.Parameters.AddWithValue("@email", loginEmailInput.text);

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader["password_hash"].ToString();
                            string enteredHash = HashPassword(loginPasswordInput.text);

                            if (storedHash == enteredHash)
                            {
                                Debug.Log("Login successful for: " + loginEmailInput.text);
                                ShowFeedback("Login successful!", true);
                                SceneManager.LoadScene(2);
                            }
                            else
                            {
                                Debug.LogWarning("Incorrect password for: " + loginEmailInput.text);
                                ShowFeedback("Incorrect password!", false);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Account not found or not confirmed for email: " + loginEmailInput.text);
                            ShowFeedback("Account not found or not confirmed!", false);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error during login: " + e.Message);
                ShowFeedback("Error during login!", false);
            }
            finally
            {
                connection.Close();
                Debug.Log("DB connection closed after login attempt");
            }
        }
    }

    void ShowFeedback(string message, bool isSuccess)
    {
        feedbackText.text = message;
        feedbackText.color = isSuccess ? Color.blue : Color.red;
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

    void ClearRegistrationFields()
    {
        nomeInput.text = "";
        idadeInput.text = "";
        pesoInput.text = "";
        alturaInput.text = "";
        emailInput.text = "";
        passwordInput.text = "";
    }
}
