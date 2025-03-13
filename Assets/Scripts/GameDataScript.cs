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
using System;

public class GameDataScript : MonoBehaviour
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
    private const string salt = "s3gur@!";
    private EmailSender emailSender;

    void Start()
    {
        dbPath = Path.Combine(Application.persistentDataPath, "game_data.db");
        CopyDatabaseIfNeeded();
        emailSender = FindObjectOfType<EmailSender>();
        Debug.Log("Database path: " + dbPath);
    }

    void CopyDatabaseIfNeeded()
    {
        string sourcePath = "C:/Users/afons/AppData/LocalLow/DefaultCompany/REHAMORPH - MENUS/game_data.db";
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

        if (!int.TryParse(idadeInput.text, out int idade))
        {
            ShowFeedback("Idade inválida!", false);
            return;
        }

        string userEmail = emailInput.text;
        string passwordHash = HashPassword(passwordInput.text);
        string confirmationToken = Guid.NewGuid().ToString();
        string dbName = "URI=file:" + dbPath;

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
                    command.CommandText = @"INSERT INTO player (nome, idade, peso, altura, email, password_hash, email_confirmed, confirmation_token)
                                          VALUES (@nome, @idade, @peso, @altura, @email, @password_hash, 0, @confirmation_token);";
                    command.Parameters.AddWithValue("@nome", nomeInput.text);
                    command.Parameters.AddWithValue("@idade", idade);
                    command.Parameters.AddWithValue("@peso", pesoInput.text);
                    command.Parameters.AddWithValue("@altura", alturaInput.text);
                    command.Parameters.AddWithValue("@email", userEmail);
                    command.Parameters.AddWithValue("@password_hash", passwordHash);
                    command.Parameters.AddWithValue("@confirmation_token", confirmationToken);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        ShowFeedback("Conta criada com sucesso! Confirma o teu e-mail.", true);

                        if (emailSender != null)
                        {
                            emailSender.SendConfirmationEmail(userEmail, confirmationToken);
                        }
                        else
                        {
                            Debug.LogError("EmailSender não encontrado na cena!");
                        }

                        ClearRegistrationFields();
                    }
                    else
                    {
                        ShowFeedback("Erro ao inserir dados!!", false);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error inserting data: " + e.Message);
                ShowFeedback("Erro ao inserir dados!!", false);
            }
            finally
            {
                connection.Close();
                Debug.Log("DB connection closed after registration");
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