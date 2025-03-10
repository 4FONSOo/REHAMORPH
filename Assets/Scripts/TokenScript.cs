using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mono.Data.Sqlite;
using System.Data;
using TMPro;
using System.IO;

public class TokenVerification : MonoBehaviour
{
    public TMP_InputField tokenInput;
    public TMP_Text feedbackText;
    private string dbPath;

    void Start()
    {
        dbPath = Path.Combine(Application.persistentDataPath, "game_data.db");
    }

    public void VerifyToken()
    {
        string enteredToken = tokenInput.text.Trim();
        if (string.IsNullOrWhiteSpace(enteredToken))
        {
            ShowFeedback("Please enter a valid token!", false);
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
                    command.CommandText = "SELECT email FROM player WHERE confirmation_token = @token AND is_confirmed = 0;";
                    command.Parameters.AddWithValue("@token", enteredToken);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        string userEmail = result.ToString();
                        ConfirmAccount(userEmail);
                    }
                    else
                    {
                        ShowFeedback("Invalid or expired token!", false);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error verifying token: " + e.Message);
                ShowFeedback("Error verifying token!", false);
            }
            finally
            {
                connection.Close();
            }
        }
    }

    private void ConfirmAccount(string email)
    {
        string dbName = "URI=file:" + dbPath;
        using (var connection = new SqliteConnection(dbName))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE player SET is_confirmed = 1 WHERE email = @email;";
                    command.Parameters.AddWithValue("@email", email);
                    command.ExecuteNonQuery();
                }

                ShowFeedback("Account confirmed successfully! Redirecting...", true);
                StartCoroutine(GoToLoginScene());
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error updating account status: " + e.Message);
                ShowFeedback("Error confirming account!", false);
            }
            finally
            {
                connection.Close();
            }
        }
    }

    IEnumerator GoToLoginScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(2); // Redireciona para a tela de login
    }

    void ShowFeedback(string message, bool isSuccess)
    {
        feedbackText.text = message;
        feedbackText.color = isSuccess ? Color.green : Color.red;
    }
}
