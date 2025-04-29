using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RegisterUI : MonoBehaviour
{
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_InputField nomeField;
    public Button registerButton;
    public Button goToLoginButton;
    public TMP_Text feedbackText;
    private FireBaseAuth fireBaseAuth; // Substitu�do de AuthManager para FireBaseAuth

    void Start()
    {
        // Localizar o componente FireBaseAuth na cena
        fireBaseAuth = FindObjectOfType<FireBaseAuth>();
        if (fireBaseAuth == null)
        {
            Debug.LogError("FireBaseAuth n�o encontrado na cena! Certifique-se de que est� anexado a um GameObject.");
        }

        // Valida��o dos campos atribu�dos no Inspector
        if (emailField == null)
        {
            Debug.LogError("Campo 'Email Field' n�o atribu�do no Inspector! Certifique-se de arrastar um TMP_InputField para este campo.");
        }
        if (passwordField == null)
        {
            Debug.LogError("Campo 'Password Field' n�o atribu�do no Inspector! Certifique-se de arrastar um TMP_InputField para este campo.");
        }
        if (nomeField == null)
        {
            Debug.LogError("Campo 'Nome Field' n�o atribu�do no Inspector! Certifique-se de arrastar um TMP_InputField para este campo.");
        }
        if (registerButton == null)
        {
            Debug.LogError("Campo 'Register Button' n�o atribu�do no Inspector! Certifique-se de arrastar um Button para este campo.");
        }
        if (goToLoginButton == null)
        {
            Debug.LogError("Campo 'Go To Login Button' n�o atribu�do no Inspector! Certifique-se de arrastar um Button para este campo.");
        }
        if (feedbackText == null)
        {
            Debug.LogError("Campo 'Feedback Text' n�o atribu�do no Inspector! Certifique-se de arrastar um TMP_Text para este campo.");
        }

        if (emailField == null || passwordField == null || nomeField == null || registerButton == null ||
            goToLoginButton == null || feedbackText == null)
        {
            Debug.LogError("Um ou mais campos n�o est�o atribu�dos no Inspector! Inicializa��o interrompida.");
            // Desativar os bot�es para evitar cliques
            if (registerButton != null)
            {
                registerButton.interactable = false;
            }
            if (goToLoginButton != null)
            {
                goToLoginButton.interactable = false;
            }
            return;
        }

        ConfigureInputFields();

        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        goToLoginButton.onClick.AddListener(OnGoToLoginButtonClicked);
    }

    private void ConfigureInputFields()
    {
        emailField.contentType = TMP_InputField.ContentType.EmailAddress;
        passwordField.contentType = TMP_InputField.ContentType.Password;
        nomeField.contentType = TMP_InputField.ContentType.Standard;

        emailField.ForceLabelUpdate();
        passwordField.ForceLabelUpdate();
        nomeField.ForceLabelUpdate();
    }

    void OnRegisterButtonClicked()
    {
        // Verificar se todos os campos est�o preenchidos
        if (string.IsNullOrWhiteSpace(emailField.text) ||
            string.IsNullOrWhiteSpace(passwordField.text) ||
            string.IsNullOrWhiteSpace(nomeField.text))
        {
            UpdateFeedback("Campos obrigat�rios n�o preenchidos!", false);
            return;
        }

        string email = emailField.text;
        string password = passwordField.text;
        string nome = nomeField.text;

        // Valida��o pr�via do formato do e-mail
        if (!IsEmailFormatValid(email))
        {
            UpdateFeedback("O e-mail est� mal formatado! Por favor, corrija.", false);
            return;
        }

        if (fireBaseAuth == null)
        {
            UpdateFeedback("Erro: FireBaseAuth n�o encontrado na cena!", false);
            Debug.LogError("FireBaseAuth n�o encontrado! N�o � poss�vel realizar o registro.");
            return;
        }

        UpdateFeedback("Iniciando registro...", true);

        // Chamar o m�todo de registro com callbacks
        fireBaseAuth.Register(email, password, nome,
            onSuccess: () =>
            {
                Debug.Log("Registro bem-sucedido! Redirecionando para a cena de valida��o de token.");
                UpdateFeedback("Registro realizado com sucesso!", true);
                SceneManager.LoadScene(12);
            },
            onError: (error) =>
            {
                Debug.LogWarning($"Erro ao registrar: {error}");
                if (error.Contains("The email address is badly formatted"))
                {
                    UpdateFeedback("O e-mail est� mal formatado! Por favor, corrija.", false);
                }
                else if (error.Contains("The email address is already in use"))
                {
                    UpdateFeedback("Este e-mail j� est� registrado! Use outro e-mail.", false);
                }
                else
                {
                    UpdateFeedback($"Erro ao registrar: {error}", false);
                }
            });
    }

    void OnGoToLoginButtonClicked()
    {
        UpdateFeedback("A ir para a tela de login...", true);
        SceneManager.LoadScene(2);
    }

    private bool IsEmailFormatValid(string email)
    {
        if (!email.Contains("@") || !email.Contains("."))
        {
            return false;
        }

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