using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public Button loginButton;
    public Button goToRegisterButton;
    public FireBaseAuth authManager;
    public TMP_Text feedbackText;

    void Start()
    {
        if (emailField == null)
        {
            Debug.LogError("Campo 'Email Field' n�o atribu�do no Inspector! Certifique-se de arrastar um TMP_InputField para este campo.");
        }
        if (passwordField == null)
        {
            Debug.LogError("Campo 'Password Field' n�o atribu�do no Inspector! Certifique-se de arrastar um TMP_InputField para este campo.");
        }
        if (loginButton == null)
        {
            Debug.LogError("Campo 'Login Button' n�o atribu�do no Inspector! Certifique-se de arrastar um Button para este campo.");
        }
        if (goToRegisterButton == null)
        {
            Debug.LogError("Campo 'Go To Register Button' n�o atribu�do no Inspector! Certifique-se de arrastar um Button para este campo.");
        }
        if (authManager == null)
        {
            Debug.LogError("Campo 'Auth Manager' n�o atribu�do no Inspector! Certifique-se de arrastar um GameObject com o componente AuthManager para este campo.");
        }
        if (feedbackText == null)
        {
            Debug.LogError("Campo 'Feedback Text' n�o atribu�do no Inspector! Certifique-se de arrastar um TMP_Text para este campo.");
        }

        if (emailField == null || passwordField == null || loginButton == null || goToRegisterButton == null || authManager == null || feedbackText == null)
        {
            Debug.LogError("Um ou mais campos n�o est�o atribu�dos no Inspector! Inicializa��o interrompida.");
            return;
        }

        ConfigureInputFields();

        loginButton.onClick.AddListener(OnLoginButtonClicked);
        goToRegisterButton.onClick.AddListener(OnGoToRegisterButtonClicked);
    }

    private void ConfigureInputFields()
    {
        emailField.contentType = TMP_InputField.ContentType.EmailAddress;
        passwordField.contentType = TMP_InputField.ContentType.Password;

        emailField.ForceLabelUpdate();
        passwordField.ForceLabelUpdate();
    }

    void OnLoginButtonClicked()
    {
        if (string.IsNullOrWhiteSpace(emailField.text) || string.IsNullOrWhiteSpace(passwordField.text))
        {
            UpdateFeedback("E-mail ou senha n�o preenchidos!", false);
            return;
        }

        string email = emailField.text;
        string password = passwordField.text;

        // Valida��o pr�via do formato do e-mail
        if (!IsEmailFormatValid(email))
        {
            UpdateFeedback("O e-mail est� mal formatado! Por favor, corrija.", false);
            return;
        }

        UpdateFeedback("Iniciando login...", true);

        authManager.Login(email, password,
            onSuccess: (userId) =>
            {
                Debug.Log($"Login bem-sucedido! Redirecionando para a pr�xima cena. UserId: {userId}");
                UpdateFeedback("Login realizado com sucesso!", true);
                SceneManager.LoadScene(4);
            },
            onError: (error) =>
            {
                Debug.LogWarning($"Erro ao fazer login: {error}");
                if (error.Contains("The email address is badly formatted"))
                {
                    UpdateFeedback("O e-mail est� mal formatado! Por favor, corrija.", false);
                }
                else
                {
                    UpdateFeedback("Essas credenciais n�o est�o corretas!!", false);
                }
            });
    }

    void OnGoToRegisterButtonClicked()
    {
        UpdateFeedback("A ir para a tela de registo...", true);
        SceneManager.LoadScene(3);
    }

    private bool IsEmailFormatValid(string email)
    {
        // Verifica��o simples: deve conter "@" e um dom�nio com ponto (ex.: ".com")
        if (!email.Contains("@") || !email.Contains("."))
        {
            return false;
        }

        // Verificar se h� pelo menos um caractere antes do "@", entre o "@" e o ".", e ap�s o "."
        int atIndex = email.IndexOf("@");
        int dotIndex = email.LastIndexOf(".");
        if (atIndex <= 0 || dotIndex <= atIndex + 1 || dotIndex >= email.Length - 1)
        {
            return false;
        }

        return true;
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