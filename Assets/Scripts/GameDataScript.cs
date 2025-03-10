using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using MailKit.Security;
using MailKit.Net.Smtp; // Para MailKit.Net.Smtp.SmtpClient
using MimeKit;
using TMPro;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;
using System.IO;
using System;

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

    // Configurações do SMTP para Gmail – ajuste conforme sua conta
    private const string smtpServer = "smtp.gmail.com";
    private const int smtpPort = 587;
    // Use o seu e-mail completo do Gmail:
    private const string smtpUser = "afonsoogncalvesmarques@gmail.com";
    // Insira a senha de aplicativo gerada no Google (sem espaços extras)
    private const string smtpPass = "fqzbarsbgopnwphp";

    // URL base de confirmação (ajuste conforme sua necessidade)
    private const string confirmationUrlBase = "http://yourserver.com/confirm?token=";

    void Start()
    {
        // Define o caminho para o arquivo de banco de dados na pasta persistente
        dbPath = Path.Combine(Application.persistentDataPath, "game_data.db");
        CopyDatabaseIfNeeded();
        Debug.Log("Database path: " + dbPath);
    }

    // Copia o banco de dados se não existir no persistentDataPath
    void CopyDatabaseIfNeeded()
    {
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
            if (nomeInput != null && nomeInput.gameObject.activeInHierarchy)
            {
                RegisterUser();
            }
            else if (loginEmailInput != null && loginEmailInput.gameObject.activeInHierarchy)
            {
                LoginUser();
            }
        }
    }

    // Registra o usuário, insere os dados no banco e envia e-mail de confirmação
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
            ShowFeedback("Todos os campos são obrigatórios!!", false);
            return;
        }

        string userEmail = emailInput.text;
        string passwordHash = HashPassword(passwordInput.text);
        string confirmationToken = Guid.NewGuid().ToString();

        string dbName = "URI=file:" + dbPath;
        bool registrationSuccess = false;

        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                Debug.Log("Connected to DB for registration");

                using (var checkCommand = connection.CreateCommand())
                {
                    checkCommand.CommandText = "SELECT COUNT(*) FROM player WHERE email = @email";
                    checkCommand.Parameters.AddWithValue("@email", userEmail);
                    int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (count > 0)
                    {
                        ShowFeedback("Este e-mail já está a ser usado!!", false);
                        return;
                    }
                }

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
                        ShowFeedback("Conta criada!! Verifica o teu e-mail para confirmares a tua conta!!", true);
                        ClearRegistrationFields();
                    }
                    else
                    {
                        ShowFeedback("Erro ao inserir data!!", false);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error inserting data: " + e.Message);
                ShowFeedback("Erro ao inserir data!!", false);
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
            StartCoroutine(SendConfirmationEmail(userEmail, confirmationToken));
            SceneManager.LoadScene(12);
        }
    }

    // Envia o e-mail de confirmação usando MailKit
    IEnumerator SendConfirmationEmail(string userEmail, string confirmationToken)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            Debug.LogError("User email is empty. Cannot send confirmation email.");
            yield break;
        }

        string confirmationLink = confirmationUrlBase + confirmationToken;

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Seu Jogo", smtpUser));
            message.To.Add(new MailboxAddress("", userEmail));
            message.Subject = "Confirm your account";
            message.Body = new TextPart("plain")
            {
                Text = "Please confirm your account by clicking the following link: " + confirmationLink
            };

            using (var client = new SmtpClient())
            {
                // Define um domínio válido para o comando EHLO.
                // Se você possuir um domínio (FQDN) use-o no lugar de "localhost".
                client.LocalDomain = "localhost";
                client.Connect(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                client.Authenticate(smtpUser, smtpPass);
                client.Send(message);
                client.Disconnect(true);
            }
            Debug.Log("Confirmation email sent to: " + userEmail);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error sending confirmation email: " + ex.Message);
        }

        yield return null;
    }

    public void LoginUser()
    {
        Debug.Log("LoginUser() called");

        if (string.IsNullOrWhiteSpace(loginEmailInput.text) || string.IsNullOrWhiteSpace(loginPasswordInput.text))
        {
            ShowFeedback("Preenche o e-mail e a palavra-passe!!", false);
            return;
        }

        string dbName = "URI=file:" + dbPath;

        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                Debug.Log("DB connection opened for login");

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
                                ShowFeedback("Login com sucesso!!", true);
                                SceneManager.LoadScene(2);
                            }
                            else
                            {
                                Debug.LogWarning("Incorrect password for: " + loginEmailInput.text);
                                ShowFeedback("Palavra-passe incorreta!!", false);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Account not found or not confirmed for email: " + loginEmailInput.text);
                            ShowFeedback("Conta não encontrada ou não confirmada!!", false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error during login: " + e.Message);
                ShowFeedback("Erro durante o Login!!", false);
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
        StartCoroutine(HideFeedbackAfterTime(7f));
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
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
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
