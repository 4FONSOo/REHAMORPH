using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class TokenValidation : MonoBehaviour
{
    public TMP_InputField tokenInput;
    public Button validateButton;
    public TMP_Text feedbackText;
    private DatabaseManager dbManager;
    private FirebaseInitializer firebaseInitializer;
    private bool isActive = true;
    private bool isFirebaseReady = false;

    void Awake()
    {
        Debug.Log("TokenValidation: Awake chamado.");
    }

    void Start()
    {
        Debug.Log("TokenValidation: Inicializando...");

        // Tenta encontrar o FirebaseInitializer
        firebaseInitializer = FindObjectOfType<FirebaseInitializer>();
        if (firebaseInitializer == null)
        {
            Debug.LogError("TokenValidation: FirebaseInitializer não encontrado na cena! Certifique-se de que ele está na cena 0 e usa DontDestroyOnLoad.");
            StartCoroutine(UpdateFeedback("Erro: FirebaseInitializer não encontrado!", false));
            return;
        }

        // Tenta encontrar o DatabaseManager
        dbManager = FindObjectOfType<DatabaseManager>();

        // Verificações de inicialização
        if (validateButton == null)
        {
            Debug.LogError("TokenValidation: validateButton não foi atribuído no Inspector! Atribua o botão 'Validar' no Inspector.");
            return;
        }
        if (tokenInput == null)
        {
            Debug.LogError("TokenValidation: tokenInput não foi atribuído no Inspector! Atribua o campo de entrada de token no Inspector.");
            return;
        }
        if (feedbackText == null)
        {
            Debug.LogError("TokenValidation: feedbackText não foi atribuído no Inspector! Atribua o texto de feedback no Inspector.");
            return;
        }
        if (dbManager == null)
        {
            Debug.LogError("TokenValidation: DatabaseManager não encontrado na cena! Certifique-se de que foi instanciado na cena 0 e usa DontDestroyOnLoad.");
            StartCoroutine(UpdateFeedback("Erro: DatabaseManager não encontrado!", false));
            validateButton.interactable = false;
            return;
        }

        Debug.Log("TokenValidation: Todos os componentes foram inicializados corretamente.");

        // Configurar o listener do botão
        validateButton.onClick.RemoveAllListeners();
        validateButton.onClick.AddListener(OnValidateButtonClicked);
        validateButton.interactable = true;
        Debug.Log("TokenValidation: Listener do botão 'Validar' configurado com sucesso.");

        // Aguardar a inicialização do Firebase
        StartCoroutine(WaitForFirebaseInitialization());
    }

    private IEnumerator WaitForFirebaseInitialization()
    {
        Debug.Log("TokenValidation: Aguardando inicialização do Firebase...");
        while (!firebaseInitializer.IsFirebaseInitialized)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("TokenValidation: Firebase inicializado com sucesso.");
        isFirebaseReady = true;
        StartCoroutine(UpdateFeedback("Insira o token e clique em 'Validar'.", true));
    }

    void OnEnable()
    {
        Debug.Log("TokenValidation: OnEnable chamado.");
        if (validateButton != null)
        {
            validateButton.onClick.RemoveAllListeners();
            validateButton.onClick.AddListener(OnValidateButtonClicked);
            validateButton.interactable = true;
            Debug.Log("TokenValidation: Listener do botão 'Validar' reconfigurado no OnEnable.");
        }
    }

    void OnDestroy()
    {
        isActive = false;
        Debug.Log("TokenValidation: Destruído.");
    }

    public void OnValidateButtonClicked()
    {
        Debug.Log("TokenValidation: Botão 'Validar' clicado.");

        if (!isFirebaseReady)
        {
            Debug.LogError("TokenValidation: Firebase ainda não está inicializado! Tente novamente em alguns segundos.");
            StartCoroutine(UpdateFeedback("Erro: Firebase ainda não inicializado!", false));
            return;
        }

        if (tokenInput == null)
        {
            Debug.LogError("TokenValidation: tokenInput não foi atribuído durante o clique!");
            StartCoroutine(UpdateFeedback("Erro: Campo de token não atribuído!", false));
            return;
        }

        string token = tokenInput.text.Trim();
        Debug.Log($"TokenValidation: Token inserido pelo usuário: {token}");

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("TokenValidation: Token está vazio.");
            StartCoroutine(UpdateFeedback("Por favor, insira um token.", false));
            return;
        }

        Debug.Log("TokenValidation: Iniciando verificação do token no Firebase...");
        StartCoroutine(UpdateFeedback("Verificando token...", true));
        ConfirmEmail(token);
    }

    private void ConfirmEmail(string token)
    {
        Debug.Log($"TokenValidation: Chamando VerifyConfirmationToken com token: {token}");

        if (dbManager == null)
        {
            Debug.LogError("TokenValidation: DatabaseManager é null durante a verificação do token!");
            StartCoroutine(UpdateFeedback("Erro: DatabaseManager não encontrado!", false));
            return;
        }

        dbManager.VerifyConfirmationToken(token, (userId) =>
        {
            Debug.Log($"TokenValidation: Callback de sucesso chamado. Resultado do VerifyConfirmationToken: userId = {userId}");

            if (userId != null)
            {
                Debug.Log($"TokenValidation: Token válido. Atualizando confirmação para o usuário {userId}...");
                dbManager.UpdateUserConfirmation(userId, true, () =>
                {
                    Debug.Log("TokenValidation: Usuário marcado como confirmado com sucesso.");
                    dbManager.RemoveConfirmationToken(token, () =>
                    {
                        Debug.Log("TokenValidation: Token removido com sucesso.");
                    }, (error) =>
                    {
                        Debug.LogWarning($"TokenValidation: Erro ao remover o token: {error}");
                        StartCoroutine(UpdateFeedback($"Erro ao remover o token: {error}", false));
                    });

                    StartCoroutine(UpdateFeedback("E-mail confirmado com sucesso! Você pode fazer login.", true));
                    Debug.Log("TokenValidation: Redirecionando para a tela de login (cena 2)...");
                    SceneManager.LoadScene(2);
                }, (error) =>
                {
                    Debug.LogError($"TokenValidation: Erro ao atualizar a confirmação: {error}");
                    StartCoroutine(UpdateFeedback($"Erro ao atualizar a confirmação: {error}", false));
                });
            }
            else
            {
                Debug.LogWarning("TokenValidation: Token inválido ou não encontrado.");
                StartCoroutine(UpdateFeedback("Token inválido ou expirado.", false));
            }
        }, (error) =>
        {
            Debug.LogError($"TokenValidation: Erro retornado pelo VerifyConfirmationToken: {error}");
            StartCoroutine(UpdateFeedback($"Erro ao verificar o token: {error}", false));
        });
    }

    private IEnumerator UpdateFeedback(string message, bool isSuccess)
    {
        if (feedbackText == null || !isActive)
        {
            Debug.LogWarning("TokenValidation: FeedbackText é null ou o script não está ativo. Cancelando UpdateFeedback.");
            yield break;
        }

        Debug.Log($"TokenValidation: Exibindo feedback: {message} (isSuccess: {isSuccess})");
        feedbackText.text = message;
        feedbackText.color = isSuccess ? new Color(0f, 0.5f, 0f) : Color.red;
        yield return new WaitForSeconds(3f);

        if (feedbackText != null && isActive)
        {
            feedbackText.text = "";
            Debug.Log("TokenValidation: Feedback limpo após 3 segundos.");
        }
    }
}