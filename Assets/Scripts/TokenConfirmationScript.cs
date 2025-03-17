using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TokenValidation : MonoBehaviour
{
    public TMP_InputField tokenInput;    // Campo onde o usuário insere o token
    public Button validateButton;        // Botão "Validar"
    public TMP_Text feedbackText;        // Texto para exibir mensagens na tela

    void Start()
    {
        // Verifica se os componentes foram atribuídos
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
        }
        else
        {
            feedbackText.text = "Insira o token e pressione 'Validar'.";
        }

        // Configura o listener para o botão
        validateButton.onClick.AddListener(OnValidateButtonClicked);
    }

    public void OnValidateButtonClicked()
    {
        if (tokenInput == null)
        {
            Debug.LogError("tokenInput não foi atribuído!");
            return;
        }

        string token = tokenInput.text;
        Debug.Log("Token inserido: " + token);

        if (string.IsNullOrEmpty(token))
        {
            UpdateFeedback("Por favor, insira um token.", false);
            return;
        }

        // Procura pelo GameDataScript na cena
        GameDataScript gameData = FindObjectOfType<GameDataScript>();
        if (gameData == null)
        {
            Debug.LogError("GameDataScript não foi encontrado na cena!");
            UpdateFeedback("Erro interno: GameDataScript não encontrado!", false);
            return;
        }

        Debug.Log("Chamando ConfirmEmail do GameDataScript...");
        // Chama o método ConfirmEmail passando o token digitado
        gameData.ConfirmEmail(token);
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
