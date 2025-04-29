using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TokenValidation : MonoBehaviour
{
    public TMP_InputField tokenInput;    // Campo onde o usuário insere o token
    public Button validateButton;        // Botão "Validar"
    public TMP_Text feedbackText;        // Texto para exibir mensagens na tela
    public DatabaseManager dbManager;    // Referência ao DatabaseManager

    void Start()
    {
        // Validações iniciais
        if (validateButton == null)
        {
            Debug.LogError("validateButton não foi atribuído no Inspector!");
            return;
        }
        if (tokenInput == null)
        {
            Debug.LogError("tokenInput não foi atribuído no Inspector!");
            return;
        }
        if (feedbackText == null)
        {
            Debug.LogError("feedbackText não foi atribuído no Inspector!");
            return;
        }
        if (dbManager == null)
        {
            Debug.LogError("dbManager não foi atribuído no Inspector! Certifique-se de que o DatabaseManager está anexado ao GameObject.");
            return;
        }

        feedbackText.text = "Insira o token e clique em 'Validar'.";
        validateButton.onClick.AddListener(OnValidateButtonClicked);
    }

    public void OnValidateButtonClicked()
    {
        if (tokenInput == null)
        {
            Debug.LogError("tokenInput não foi atribuído!");
            return;
        }

        string token = tokenInput.text.Trim(); // Remover espaços em branco
        Debug.Log("Token inserido: " + token);

        if (string.IsNullOrEmpty(token))
        {
            UpdateFeedback("Por favor, insira um token.", false);
            return;
        }

        Debug.Log("Verificando token no Firebase...");
        ConfirmEmail(token);
    }

    private void ConfirmEmail(string token)
    {
        Debug.Log($"Chamando VerifyConfirmationToken com token: {token}");
        dbManager.VerifyConfirmationToken(token, (userId) =>
        {
            Debug.Log($"Resultado do VerifyConfirmationToken: userId = {userId}");

            if (userId != null)
            {
                Debug.Log($"Token válido. Atualizando confirmação para o usuário {userId}...");
                // Token válido, marcar o usuário como confirmado
                dbManager.UpdateUserConfirmation(userId, true, () =>
                {
                    Debug.Log("Usuário marcado como confirmado com sucesso.");
                    // Opcional: remover o token após validação
                    dbManager.RemoveConfirmationToken(token, () =>
                    {
                        Debug.Log("Token removido com sucesso.");
                    }, (error) =>
                    {
                        Debug.LogWarning($"Erro ao remover o token: {error}");
                    });

                    UpdateFeedback("E-mail confirmado com sucesso! Você pode fazer login.", true);
                    Debug.Log("Redirecionando para a tela de login (cena 2)...");
                    SceneManager.LoadScene(2);
                }, (error) =>
                {
                    UpdateFeedback($"Erro ao atualizar a confirmação: {error}", false);
                });
            }
            else
            {
                UpdateFeedback("Token inválido ou expirado.", false);
            }
        }, (error) =>
        {
            UpdateFeedback($"Erro ao verificar o token: {error}", false);
        });
    }

    void UpdateFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.blue : Color.red;
        }
        Debug.Log(message);
    }
}