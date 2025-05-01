using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class RegisterUI : MonoBehaviour
{
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_InputField nomeField;
    public TMP_InputField idadeField;
    public TMP_InputField alturaField;
    public TMP_InputField pesoField;
    public TMP_Dropdown tipoDropdown;
    public Button registerButton;
    public Button goToLoginButton;
    public TMP_Text feedbackText;
    private FireBaseAuth fireBaseAuth;

    void Start()
    {
        fireBaseAuth = FindObjectOfType<FireBaseAuth>();
        if (fireBaseAuth == null)
        {
            Debug.LogError("FireBaseAuth n�o encontrado na cena! Certifique-se de que est� anexado a um GameObject.");
        }

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
        if (idadeField == null)
        {
            Debug.LogError("Campo 'Idade Field' n�o atribu�do no Inspector! Certifique-se de arrastar um TMP_InputField para este campo.");
        }
        if (alturaField == null)
        {
            Debug.LogError("Campo 'Altura Field' n�o atribu�do no Inspector! Certifique-se de arrastar um TMP_InputField para este campo.");
        }
        if (pesoField == null)
        {
            Debug.LogError("Campo 'Peso Field' n�o atribu�do no Inspector! Certifique-se de arrastar um TMP_InputField para este campo.");
        }
        if (tipoDropdown == null)
        {
            Debug.LogError("Campo 'Tipo Dropdown' n�o atribu�do no Inspector! Certifique-se de arrastar um TMP_Dropdown para este campo.");
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

        if (emailField == null || passwordField == null || nomeField == null || idadeField == null || alturaField == null || pesoField == null ||
            tipoDropdown == null || registerButton == null || goToLoginButton == null || feedbackText == null)
        {
            Debug.LogError("Um ou mais campos n�o est�o atribu�dos no Inspector! Inicializa��o interrompida.");
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
        ConfigureDropdownOptions();

        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        goToLoginButton.onClick.AddListener(OnGoToLoginButtonClicked);
    }

    private void ConfigureInputFields()
    {
        emailField.contentType = TMP_InputField.ContentType.EmailAddress;
        passwordField.contentType = TMP_InputField.ContentType.Password;
        nomeField.contentType = TMP_InputField.ContentType.Standard;
        idadeField.contentType = TMP_InputField.ContentType.IntegerNumber;
        alturaField.contentType = TMP_InputField.ContentType.DecimalNumber;
        pesoField.contentType = TMP_InputField.ContentType.DecimalNumber;

        emailField.ForceLabelUpdate();
        passwordField.ForceLabelUpdate();
        nomeField.ForceLabelUpdate();
        idadeField.ForceLabelUpdate();
        alturaField.ForceLabelUpdate();
        pesoField.ForceLabelUpdate();
    }

    private void ConfigureDropdownOptions()
    {
        tipoDropdown.ClearOptions();
        tipoDropdown.options.Add(new TMP_Dropdown.OptionData("Paciente"));
        tipoDropdown.options.Add(new TMP_Dropdown.OptionData("Profissional"));
        tipoDropdown.value = 0;
        tipoDropdown.RefreshShownValue();
    }

    void OnRegisterButtonClicked()
    {
        if (string.IsNullOrWhiteSpace(emailField.text) ||
            string.IsNullOrWhiteSpace(passwordField.text) ||
            string.IsNullOrWhiteSpace(nomeField.text) ||
            string.IsNullOrWhiteSpace(idadeField.text) ||
            string.IsNullOrWhiteSpace(alturaField.text) ||
            string.IsNullOrWhiteSpace(pesoField.text))
        {
            StartCoroutine(UpdateFeedback("Campos obrigat�rios n�o preenchidos!", false));
            return;
        }

        string email = emailField.text;
        string password = passwordField.text;
        string nome = nomeField.text;

        if (!int.TryParse(idadeField.text, out int idade) || idade < 0)
        {
            StartCoroutine(UpdateFeedback("Idade deve ser um n�mero inteiro n�o negativo!", false));
            return;
        }

        if (!float.TryParse(alturaField.text, out float altura) || altura < 0)
        {
            StartCoroutine(UpdateFeedback("Altura deve ser um n�mero n�o negativo!", false));
            return;
        }

        if (!float.TryParse(pesoField.text, out float peso) || peso < 0)
        {
            StartCoroutine(UpdateFeedback("Peso deve ser um n�mero n�o negativo!", false));
            return;
        }

        string tipo = tipoDropdown.options[tipoDropdown.value].text.ToLower();

        if (!IsEmailFormatValid(email))
        {
            StartCoroutine(UpdateFeedback("O e-mail est� mal formatado! Por favor, corrija.", false));
            return;
        }

        if (fireBaseAuth == null)
        {
            StartCoroutine(UpdateFeedback("Erro: FireBaseAuth n�o encontrado na cena!", false));
            Debug.LogError("FireBaseAuth n�o encontrado! N�o � poss�vel realizar o registo.");
            return;
        }

        StartCoroutine(UpdateFeedback("Iniciando registo...", true));

        fireBaseAuth.Register(email, password, nome, idade, altura, peso, tipo,
            onSuccess: () =>
            {
                Debug.Log("Registo bem-sucedido! Redirecionando para a cena de valida��o de token.");
                StartCoroutine(UpdateFeedback("Registo realizado com sucesso!", true));
                SceneManager.LoadScene(12);
            },
            onError: (error) =>
            {
                Debug.LogWarning($"Erro ao registar: {error}");
                if (error.Contains("The email address is badly formatted"))
                {
                    StartCoroutine(UpdateFeedback("O e-mail est� mal formatado! Por favor, corrija.", false));
                }
                else if (error.Contains("The email address is already in use"))
                {
                    StartCoroutine(UpdateFeedback("Este e-mail j� est� registado! Use outro e-mail.", false));
                }
                else
                {
                    StartCoroutine(UpdateFeedback($"Erro ao registar: {error}", false));
                }
            });
    }

    void OnGoToLoginButtonClicked()
    {
        StartCoroutine(UpdateFeedback("A ir para a tela de login...", true));
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

    private IEnumerator UpdateFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? new Color(0f, 0.5f, 0f) : Color.red;
            yield return new WaitForSeconds(4f);
            feedbackText.text = "";
        }
        Debug.Log(message);
    }
}