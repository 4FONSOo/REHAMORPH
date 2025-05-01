using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class FireBaseAuth : MonoBehaviour
{
    private static FireBaseAuth instance;

    private FirebaseAuth auth;
    public UserScreenManager screenManager;
    public EmailSender emailSender;
    private TMP_Text feedbackText;
    private FirebaseInitializer firebaseInitializer;
    private bool isFirebaseInitialized = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("FireBaseAuth: Instância criada e marcada como DontDestroyOnLoad.");
        }
        else
        {
            Debug.Log("FireBaseAuth: Instância já existe. Destruindo duplicata.");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        firebaseInitializer = FindObjectOfType<FirebaseInitializer>();
        if (firebaseInitializer == null)
        {
            Debug.LogError("FireBaseAuth: FirebaseInitializer não encontrado na cena! Certifique-se de que ele está na cena 0.");
            return;
        }

        StartCoroutine(WaitForFirebaseInitialization());
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        Debug.Log("FireBaseAuth: Aguardando inicialização do Firebase...");
        while (!firebaseInitializer.IsFirebaseInitialized)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("FireBaseAuth: Firebase inicializado. Configurando FirebaseAuth...");
        auth = FirebaseAuth.DefaultInstance;
        Debug.Log("FireBaseAuth: FirebaseAuth configurado com sucesso.");
        isFirebaseInitialized = true;

        feedbackText = GameObject.Find("FeedbackText")?.GetComponent<TMP_Text>();
        if (feedbackText == null)
        {
            Debug.LogWarning("FireBaseAuth: FeedbackText não encontrado! Certifique-se que existe na cena atual.");
        }

        if (emailSender == null)
        {
            emailSender = FindObjectOfType<EmailSender>();
            if (emailSender != null)
                Debug.Log("FireBaseAuth: EmailSender encontrado com sucesso.");
            else
                Debug.LogError("FireBaseAuth: EmailSender não encontrado na cena!");
        }

        if (auth.CurrentUser != null)
        {
            Debug.Log($"FireBaseAuth: Usuário já está logado: {auth.CurrentUser.Email}");
            PlayerPrefs.SetString("loggedInUser", auth.CurrentUser.Email);
            PlayerPrefs.SetString("loggedInUserId", auth.CurrentUser.UserId);
            PlayerPrefs.Save();
        }
    }

    public void Register(string email, string password, string nome, int idade, float altura, float peso, string tipo, Action onSuccess = null, Action<string> onError = null)
    {
        if (!isFirebaseInitialized)
        {
            Debug.LogError("FireBaseAuth: Firebase não está inicializado! Não é possível registrar.");
            UpdateFeedback("Erro: Firebase não inicializado!", false);
            onError?.Invoke("Firebase não inicializado.");
            return;
        }

        Debug.Log($"FireBaseAuth: [Register] Iniciando: {email}, {nome}, {idade}, {altura}, {peso}, {tipo}");

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                string error = task.Exception?.InnerException?.Message ?? "Erro desconhecido.";
                Debug.LogError($"FireBaseAuth: [Register] Erro: {error}");
                UpdateFeedback("Erro ao registrar: " + error, false);
                onError?.Invoke(error);
                return;
            }

            FirebaseUser user = task.Result.User;
            Debug.Log($"FireBaseAuth: Usuário registrado com sucesso: {user.Email}");

            user.UpdateUserProfileAsync(new UserProfile { DisplayName = nome }).ContinueWithOnMainThread(profileTask =>
            {
                if (profileTask.IsFaulted || profileTask.IsCanceled)
                {
                    string error = profileTask.Exception?.InnerException?.Message ?? "Erro ao atualizar perfil.";
                    Debug.LogError($"FireBaseAuth: {error}");
                    UpdateFeedback(error, false);
                    onError?.Invoke(error);
                    return;
                }

                SaveUserData(user.UserId, nome, email, idade, altura, peso, tipo);

                string token = GenerateToken();
                SaveConfirmationToken(token, user.UserId, email);

                if (emailSender != null)
                {
                    Debug.Log($"FireBaseAuth: A enviar e-mail para {email} com token: {token}");
                    UpdateFeedback("A enviar e-mail de confirmação...", true);
                    emailSender.SendConfirmationEmail(email, token);
                    UpdateFeedback("E-mail de confirmação enviado. Verifique a sua caixa de correio.", true);
                }
                else
                {
                    Debug.LogError("FireBaseAuth: EmailSender é null. Não foi possível enviar o e-mail.");
                    UpdateFeedback("Erro ao enviar e-mail de confirmação.", false);
                    onError?.Invoke("Erro ao enviar e-mail.");
                    return;
                }

                onSuccess?.Invoke();
            });
        });
    }

    public void Login(string email, string password, Action<string> onSuccess = null, Action<string> onError = null)
    {
        if (!isFirebaseInitialized)
        {
            Debug.LogError("FireBaseAuth: Firebase não está inicializado! Não é possível fazer login.");
            UpdateFeedback("Erro: Firebase não inicializado!", false);
            onError?.Invoke("Firebase não inicializado.");
            return;
        }

        Debug.Log($"FireBaseAuth: Tentando login com: {email}");
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                string error = task.Exception?.InnerException?.Message ?? "Erro desconhecido.";
                Debug.LogError($"FireBaseAuth: Erro ao logar: {error}");
                UpdateFeedback("Erro ao fazer login: " + error, false);
                onError?.Invoke(error);
                return;
            }

            FirebaseUser user = task.Result.User;
            Debug.Log($"FireBaseAuth: Login bem-sucedido: {user.DisplayName} ({user.UserId})");
            PlayerPrefs.SetString("loggedInUser", email);
            PlayerPrefs.SetString("loggedInUserId", user.UserId);
            PlayerPrefs.Save();
            UpdateFeedback("Login efetuado com sucesso!", true);
            onSuccess?.Invoke(user.UserId);
        });
    }

    private void SaveUserData(string userId, string nome, string email, int idade, float altura, float peso, string tipo)
    {
        Debug.Log($"FireBaseAuth: A guardar dados do utilizador: {userId}");
        DatabaseManager.SaveUser(userId, nome, email, idade, altura, peso, tipo);
    }

    private string GenerateToken()
    {
        string token = Guid.NewGuid().ToString();
        Debug.Log($"FireBaseAuth: Token gerado: {token}");
        return token;
    }

    private void SaveConfirmationToken(string token, string userId, string email)
    {
        Debug.Log("FireBaseAuth: A guardar token de confirmação...");
        DatabaseManager.SaveConfirmationToken(token, userId, email);
    }

    private void UpdateFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.green : Color.red;
        }
        Debug.Log($"FireBaseAuth: [Feedback] {message}");
    }
}