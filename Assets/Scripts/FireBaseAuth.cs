using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using TMPro;
using System;

public class FireBaseAuth : MonoBehaviour
{
    private FirebaseAuth auth;
    public UserScreenManager screenManager;
    public EmailSender emailSender;
    private TMP_Text feedbackText;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        Debug.Log("FireBaseAuth inicializado com sucesso. FirebaseAuth configurado.");

        feedbackText = GameObject.Find("FeedbackText")?.GetComponent<TMP_Text>();
        if (feedbackText == null)
        {
            Debug.LogWarning("FeedbackText n�o encontrado na cena! Certifique-se de que existe um GameObject chamado 'FeedbackText' com um componente TMP_Text.");
        }

        if (emailSender == null)
        {
            emailSender = FindObjectOfType<EmailSender>();
            if (emailSender == null)
            {
                Debug.LogError("EmailSender n�o encontrado na cena! Certifique-se de que est� anexado a um GameObject.");
            }
            else
            {
                Debug.Log("EmailSender encontrado com sucesso.");
            }
        }

        if (auth.CurrentUser != null)
        {
            Debug.Log($"Usu�rio j� est� logado ao iniciar a cena! UserId: {auth.CurrentUser.UserId}, Email: {auth.CurrentUser.Email}");
            PlayerPrefs.SetString("loggedInUser", auth.CurrentUser.Email);
            PlayerPrefs.SetString("loggedInUserId", auth.CurrentUser.UserId);
            PlayerPrefs.Save();
            Debug.Log("Dados do usu�rio logado salvos no PlayerPrefs.");
            UpdateFeedback("Usu�rio j� est� logado: " + auth.CurrentUser.Email, true);
        }
    }

    public void Register(string email, string password, string nome, Action onSuccess = null, Action<string> onError = null)
    {
        Debug.Log($"Iniciando registro do usu�rio: email={email}, nome={nome}");
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Erro ao registrar usu�rio: {task.Exception}");
                UpdateFeedback($"Erro ao registrar: {task.Exception.Message}", false);
                onError?.Invoke(task.Exception.Message);
                return;
            }

            if (task.IsCompleted)
            {
                FirebaseUser user = task.Result.User;
                Debug.Log($"Usu�rio registrado com sucesso no Firebase Authentication. UserId: {user.UserId}");
                UpdateFeedback("Registro efetuado com sucesso!", true);

                UserProfile profile = new UserProfile { DisplayName = nome };
                user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(profileTask =>
                {
                    if (profileTask.IsFaulted || profileTask.IsCanceled)
                    {
                        Debug.LogError($"Erro ao atualizar perfil do usu�rio: {profileTask.Exception}");
                        UpdateFeedback($"Erro ao atualizar perfil: {profileTask.Exception.Message}", false);
                        onError?.Invoke(profileTask.Exception.Message);
                        return;
                    }

                    if (profileTask.IsCompleted)
                    {
                        Debug.Log($"Perfil do usu�rio {nome} atualizado com sucesso.");
                        SaveUserData(user.UserId, nome, email);

                        string token = GenerateToken();
                        SaveConfirmationToken(token, user.UserId, email);

                        if (emailSender != null)
                        {
                            Debug.Log($"Enviando e-mail de confirma��o para {email} com token {token}...");
                            UpdateFeedback("A enviar e-mail de confirma��o...", true);
                            emailSender.SendConfirmationEmail(email, token);
                            UpdateFeedback("E-mail de confirma��o enviado com sucesso! Verifique a sua caixa de correio.", true);
                        }
                        else
                        {
                            Debug.LogError("EmailSender n�o est� atribu�do! N�o foi poss�vel enviar o e-mail de confirma��o.");
                            UpdateFeedback("Erro: N�o foi poss�vel enviar o e-mail de confirma��o.", false);
                            onError?.Invoke("N�o foi poss�vel enviar o e-mail de confirma��o.");
                            return;
                        }

                        onSuccess?.Invoke();
                    }
                });
            }
        });
    }

    public void Login(string email, string password, Action<string> onSuccess = null, Action<string> onError = null)
    {
        Debug.Log($"Iniciando login do usu�rio: email={email}");
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Erro ao logar: {task.Exception}");
                UpdateFeedback($"Erro ao fazer login: {task.Exception.Message}", false);
                onError?.Invoke(task.Exception.Message);
                return;
            }

            if (task.IsCompleted)
            {
                FirebaseUser user = task.Result.User;
                Debug.Log($"Usu�rio {user.DisplayName} logado com sucesso! UserId: {user.UserId}");
                UpdateFeedback("Login efetuado com sucesso!", true);
                PlayerPrefs.SetString("loggedInUser", email);
                PlayerPrefs.SetString("loggedInUserId", user.UserId);
                PlayerPrefs.Save();
                Debug.Log($"Dados salvos no PlayerPrefs: loggedInUser={email}, loggedInUserId={user.UserId}");
                onSuccess?.Invoke(user.UserId);
            }
        });
    }

    private void SaveUserData(string userId, string nome, string email)
    {
        Debug.Log($"Salvando dados do usu�rio no DatabaseManager: userId={userId}");
        DatabaseManager.SaveUser(userId, nome, email);
    }

    private string GenerateToken()
    {
        string token = System.Guid.NewGuid().ToString();
        Debug.Log($"Token gerado: {token}");
        return token;
    }

    private void SaveConfirmationToken(string token, string userId, string email)
    {
        Debug.Log($"Salvando token de confirma��o no DatabaseManager: token={token}, userId={userId}, email={email}");
        DatabaseManager.SaveConfirmationToken(token, userId, email);
    }

    private void UpdateFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.green : Color.red;
        }
        Debug.Log(message);
    }
}