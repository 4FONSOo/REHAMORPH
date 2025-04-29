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
            Debug.LogWarning("FeedbackText não encontrado na cena! Certifique-se de que existe um GameObject chamado 'FeedbackText' com um componente TMP_Text.");
        }

        if (emailSender == null)
        {
            emailSender = FindObjectOfType<EmailSender>();
            if (emailSender == null)
            {
                Debug.LogError("EmailSender não encontrado na cena! Certifique-se de que está anexado a um GameObject.");
            }
            else
            {
                Debug.Log("EmailSender encontrado com sucesso.");
            }
        }

        if (auth.CurrentUser != null)
        {
            Debug.Log($"Usuário já está logado ao iniciar a cena! UserId: {auth.CurrentUser.UserId}, Email: {auth.CurrentUser.Email}");
            PlayerPrefs.SetString("loggedInUser", auth.CurrentUser.Email);
            PlayerPrefs.SetString("loggedInUserId", auth.CurrentUser.UserId);
            PlayerPrefs.Save();
            Debug.Log("Dados do usuário logado salvos no PlayerPrefs.");
            UpdateFeedback("Usuário já está logado: " + auth.CurrentUser.Email, true);
        }
    }

    public void Register(string email, string password, string nome, Action onSuccess = null, Action<string> onError = null)
    {
        Debug.Log($"Iniciando registro do usuário: email={email}, nome={nome}");
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Erro ao registrar usuário: {task.Exception}");
                UpdateFeedback($"Erro ao registrar: {task.Exception.Message}", false);
                onError?.Invoke(task.Exception.Message);
                return;
            }

            if (task.IsCompleted)
            {
                FirebaseUser user = task.Result.User;
                Debug.Log($"Usuário registrado com sucesso no Firebase Authentication. UserId: {user.UserId}");
                UpdateFeedback("Registro efetuado com sucesso!", true);

                UserProfile profile = new UserProfile { DisplayName = nome };
                user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(profileTask =>
                {
                    if (profileTask.IsFaulted || profileTask.IsCanceled)
                    {
                        Debug.LogError($"Erro ao atualizar perfil do usuário: {profileTask.Exception}");
                        UpdateFeedback($"Erro ao atualizar perfil: {profileTask.Exception.Message}", false);
                        onError?.Invoke(profileTask.Exception.Message);
                        return;
                    }

                    if (profileTask.IsCompleted)
                    {
                        Debug.Log($"Perfil do usuário {nome} atualizado com sucesso.");
                        SaveUserData(user.UserId, nome, email);

                        string token = GenerateToken();
                        SaveConfirmationToken(token, user.UserId, email);

                        if (emailSender != null)
                        {
                            Debug.Log($"Enviando e-mail de confirmação para {email} com token {token}...");
                            UpdateFeedback("A enviar e-mail de confirmação...", true);
                            emailSender.SendConfirmationEmail(email, token);
                            UpdateFeedback("E-mail de confirmação enviado com sucesso! Verifique a sua caixa de correio.", true);
                        }
                        else
                        {
                            Debug.LogError("EmailSender não está atribuído! Não foi possível enviar o e-mail de confirmação.");
                            UpdateFeedback("Erro: Não foi possível enviar o e-mail de confirmação.", false);
                            onError?.Invoke("Não foi possível enviar o e-mail de confirmação.");
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
        Debug.Log($"Iniciando login do usuário: email={email}");
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
                Debug.Log($"Usuário {user.DisplayName} logado com sucesso! UserId: {user.UserId}");
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
        Debug.Log($"Salvando dados do usuário no DatabaseManager: userId={userId}");
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
        Debug.Log($"Salvando token de confirmação no DatabaseManager: token={token}, userId={userId}, email={email}");
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