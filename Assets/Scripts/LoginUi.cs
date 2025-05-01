using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoginUI : MonoBehaviour
{
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public Button loginButton;
    public Button goToRegisterButton;
    public TMP_Text feedbackText;
    private FireBaseAuth fireBaseAuth;
    private DatabaseManager dbManager;
    private bool isActive = true;

    void Start()
    {
        Debug.Log("Inicializando LoginUI...");

        fireBaseAuth = FindObjectOfType<FireBaseAuth>();
        dbManager = FindObjectOfType<DatabaseManager>();

        if (emailField == null)
        {
            Debug.LogError("Campo 'Email Field' não atribuído no Inspector!");
        }
        if (passwordField == null)
        {
            Debug.LogError("Campo 'Password Field' não atribuído no Inspector!");
        }
        if (loginButton == null)
        {
            Debug.LogError("Campo 'Login Button' não atribuído no Inspector!");
        }
        if (goToRegisterButton == null)
        {
            Debug.LogError("Campo 'Go To Register Button' não atribuído no Inspector!");
        }
        if (feedbackText == null)
        {
            Debug.LogError("Campo 'Feedback Text' não atribuído no Inspector!");
        }
        if (fireBaseAuth == null)
        {
            Debug.LogError("FireBaseAuth não encontrado na cena! Certifique-se de que foi instanciado na cena 0 e usa DontDestroyOnLoad.");
            StartCoroutine(UpdateFeedback("Erro: FireBaseAuth não encontrado!", false));
        }
        if (dbManager == null)
        {
            Debug.LogError("DatabaseManager não encontrado na cena! Certifique-se de que foi instanciado na cena 0 e usa DontDestroyOnLoad.");
            StartCoroutine(UpdateFeedback("Erro: DatabaseManager não encontrado!", false));
        }

        if (emailField == null || passwordField == null || loginButton == null ||
            goToRegisterButton == null || feedbackText == null || fireBaseAuth == null || dbManager == null)
        {
            Debug.LogError("Um ou mais campos não estão atribuídos! Inicialização interrompida.");
            if (loginButton != null)
            {
                loginButton.interactable = false;
            }
            if (goToRegisterButton != null)
            {
                goToRegisterButton.interactable = false;
            }
            return;
        }

        ConfigureInputFields();

        loginButton.onClick.RemoveAllListeners();
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        goToRegisterButton.onClick.RemoveAllListeners();
        goToRegisterButton.onClick.AddListener(OnGoToRegisterButtonClicked);

        Debug.Log("Listeners dos botões configurados: LoginButton e GoToRegisterButton.");
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
        Debug.Log("Botão 'Login' clicado.");

        if (string.IsNullOrWhiteSpace(emailField.text) || string.IsNullOrWhiteSpace(passwordField.text))
        {
            StartCoroutine(UpdateFeedback("Campos obrigatórios não preenchidos!", false));
            return;
        }

        string email = emailField.text;
        string password = passwordField.text;

        StartCoroutine(UpdateFeedback("Iniciando login...", true));

        fireBaseAuth.Login(email, password,
            onSuccess: (userId) =>
            {
                dbManager.GetUser(userId, (user) =>
                {
                    if (user != null)
                    {
                        if (user.is_confirmed)
                        {
                            Debug.Log("Conta validada. Redirecionando para a próxima cena...");
                            StartCoroutine(UpdateFeedback("Login bem-sucedido! Redirecionando...", true));
                            SceneManager.LoadScene(4);
                        }
                        else
                        {
                            Debug.LogWarning("Conta não validada. Redirecionando para a cena de validação de token...");
                            StartCoroutine(UpdateFeedback("Conta não validada. Por favor, valide sua conta.", false));
                            SceneManager.LoadScene(12);
                        }
                    }
                    else
                    {
                        Debug.LogError("Usuário não encontrado no banco de dados!");
                        StartCoroutine(UpdateFeedback("Erro: Usuário não encontrado no banco de dados!", false));
                    }
                });
            },
            onError: (error) =>
            {
                Debug.LogWarning($"Erro ao fazer login: {error}");
                StartCoroutine(UpdateFeedback($"Erro ao fazer login: {error}", false));
            });
    }

    void OnGoToRegisterButtonClicked()
    {
        Debug.Log("Botão 'Ir para Registo' clicado.");
        StartCoroutine(UpdateFeedback("A ir para a tela de registo...", true));
        SceneManager.LoadScene(3);
    }

    void OnDestroy()
    {
        isActive = false;
        Debug.Log("LoginUI destruído.");
    }

    private IEnumerator UpdateFeedback(string message, bool isSuccess)
    {
        if (feedbackText == null || !isActive)
        {
            Debug.LogWarning("FeedbackText é null ou o script não está ativo. Cancelando UpdateFeedback.");
            yield break;
        }

        feedbackText.text = message;
        feedbackText.color = isSuccess ? new Color(0f, 0.5f, 0f) : Color.red;
        yield return new WaitForSeconds(3f);

        if (feedbackText != null && isActive)
        {
            feedbackText.text = "";
        }
    }
}