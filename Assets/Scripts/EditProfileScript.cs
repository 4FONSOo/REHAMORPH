using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;

public class EditProfile : MonoBehaviour
{
    public TMP_InputField editNomeInput;
    public TMP_InputField editIdadeInput;
    public TMP_InputField editAlturaInput;
    public TMP_InputField editPesoInput;
    public TMP_InputField editEmailInput;
    public TMP_InputField editPasswordInput;
    public TMP_Text feedbackText;
    public Button saveButton;
    public Button cancelButton;
    public Button logoutButton;
    public Button deleteAccountButton;

    private string loggedInUserId;
    private string loggedInEmail;
    private DatabaseManager dbManager;
    private FireBaseAuth authManager;
    private DeleteAccount deleteAccountManager;
    private bool isActive = true;

    void Start()
    {
        Debug.Log("EditProfile: Inicializando...");

        // Tenta encontrar os managers
        dbManager = FindObjectOfType<DatabaseManager>();
        authManager = FindObjectOfType<FireBaseAuth>();
        deleteAccountManager = FindObjectOfType<DeleteAccount>();

        // Verificações de inicialização
        if (dbManager == null)
        {
            Debug.LogError("EditProfile: DatabaseManager não encontrado na cena!");
            StartCoroutine(UpdateFeedback("Erro: DatabaseManager não encontrado!", false));
            DisableButtons();
            return;
        }

        if (authManager == null)
        {
            Debug.LogError("EditProfile: AuthManager não encontrado na cena!");
            StartCoroutine(UpdateFeedback("Erro: AuthManager não encontrado!", false));
            DisableButtons();
            return;
        }

        if (deleteAccountManager == null)
        {
            Debug.LogError("EditProfile: DeleteAccount não encontrado na cena!");
            StartCoroutine(UpdateFeedback("Erro: DeleteAccount não encontrado!", false));
            DisableButtons();
            return;
        }

        // Verifica se os campos de entrada estão atribuídos
        if (editNomeInput == null || editIdadeInput == null || editAlturaInput == null || editPesoInput == null || editEmailInput == null || editPasswordInput == null)
        {
            Debug.LogError("EditProfile: Um ou mais campos de entrada não foram atribuídos no Inspector!");
            StartCoroutine(UpdateFeedback("Erro: Campos de entrada não atribuídos!", false));
            DisableButtons();
            return;
        }

        if (feedbackText == null)
        {
            Debug.LogError("EditProfile: FeedbackText não atribuído!");
            StartCoroutine(UpdateFeedback("Erro: FeedbackText não atribuído!", false));
            DisableButtons();
            return;
        }

        if (saveButton == null || cancelButton == null || logoutButton == null || deleteAccountButton == null)
        {
            Debug.LogError("EditProfile: Um ou mais botões não foram atribuídos no Inspector!");
            StartCoroutine(UpdateFeedback("Erro: Botões não atribuídos!", false));
            DisableButtons();
            return;
        }

        Debug.Log("EditProfile: Todos os componentes foram inicializados corretamente.");

        // Configura os tipos de entrada dos campos
        ConfigureInputFields();

        // Configura os listeners dos botões
        saveButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        logoutButton.onClick.RemoveAllListeners();
        deleteAccountButton.onClick.RemoveAllListeners();

        saveButton.onClick.AddListener(SaveProfileChanges);
        cancelButton.onClick.AddListener(CancelProfileEdit);
        logoutButton.onClick.AddListener(Logout);
        deleteAccountButton.onClick.AddListener(DeleteAccount);

        Debug.Log("EditProfile: Listeners dos botões configurados com sucesso.");

        // Verifica se há um usuário logado
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        if (currentUser != null)
        {
            loggedInEmail = currentUser.Email;
            loggedInUserId = currentUser.UserId;
            PlayerPrefs.SetString("loggedInUser", loggedInEmail);
            PlayerPrefs.SetString("loggedInUserId", loggedInUserId);
            PlayerPrefs.Save();
            Debug.Log($"EditProfile: Usuário logado: Email={loggedInEmail}, UserId={loggedInUserId}");
            StartCoroutine(UpdateFeedback("Bem-vindo, " + loggedInEmail + "!", true));
        }
        else if (PlayerPrefs.HasKey("loggedInUser") && PlayerPrefs.HasKey("loggedInUserId"))
        {
            loggedInEmail = PlayerPrefs.GetString("loggedInUser");
            loggedInUserId = PlayerPrefs.GetString("loggedInUserId");
            Debug.Log($"EditProfile: Usuário recuperado do PlayerPrefs: Email={loggedInEmail}, UserId={loggedInUserId}");
            StartCoroutine(UpdateFeedback("Bem-vindo, " + loggedInEmail + "!", true));
        }
        else
        {
            Debug.LogWarning("EditProfile: Nenhum usuário logado! Redirecionando para a tela de login.");
            StartCoroutine(UpdateFeedback("Nenhum usuário logado. Redirecionando para o login...", false));
            SceneManager.LoadScene(2);
            return;
        }

        // Carrega os dados do perfil
        LoadUserProfile();
    }

    void OnDestroy()
    {
        isActive = false;
        Debug.Log("EditProfile: Destruído.");
    }

    private void ConfigureInputFields()
    {
        editNomeInput.contentType = TMP_InputField.ContentType.Standard;
        editIdadeInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        editAlturaInput.contentType = TMP_InputField.ContentType.DecimalNumber;
        editPesoInput.contentType = TMP_InputField.ContentType.DecimalNumber;
        editEmailInput.contentType = TMP_InputField.ContentType.EmailAddress;
        editPasswordInput.contentType = TMP_InputField.ContentType.Password;

        editNomeInput.ForceLabelUpdate();
        editIdadeInput.ForceLabelUpdate();
        editAlturaInput.ForceLabelUpdate();
        editPesoInput.ForceLabelUpdate();
        editEmailInput.ForceLabelUpdate();
        editPasswordInput.ForceLabelUpdate();
        Debug.Log("EditProfile: Campos de entrada configurados.");
    }

    private void DisableButtons()
    {
        if (saveButton != null) saveButton.interactable = false;
        if (cancelButton != null) cancelButton.interactable = false;
        if (logoutButton != null) logoutButton.interactable = false;
        if (deleteAccountButton != null) deleteAccountButton.interactable = false;
        Debug.Log("EditProfile: Botões desativados devido a erro de inicialização.");
    }

    public void LoadUserProfile()
    {
        if (string.IsNullOrEmpty(loggedInUserId) || string.IsNullOrEmpty(loggedInEmail))
        {
            Debug.LogWarning("EditProfile: Erro ao carregar perfil: Dados do usuário logado ausentes!");
            StartCoroutine(UpdateFeedback("Erro: Dados do usuário logado ausentes!", false));
            return;
        }

        // Limpa os campos antes de carregar os dados
        editNomeInput.text = "";
        editIdadeInput.text = "";
        editAlturaInput.text = "";
        editPesoInput.text = "";
        editEmailInput.text = "";
        editPasswordInput.text = "";
        Debug.Log("EditProfile: Campos de entrada limpos antes de carregar os dados do perfil.");

        Debug.Log($"EditProfile: Carregando perfil para userId: {loggedInUserId}");
        dbManager.GetUser(loggedInUserId, (user) =>
        {
            if (user != null)
            {
                Debug.Log($"EditProfile: Dados do usuário carregados: Nome={user.nome}, Idade={user.idade}, Altura={user.altura}, Peso={user.peso}, Email={user.email}, IsConfirmed={user.is_confirmed}");

                // Preenche os campos com os dados do usuário
                editNomeInput.text = user.nome ?? "";
                editIdadeInput.text = user.idade.ToString();
                editAlturaInput.text = user.altura.ToString("F2");
                editPesoInput.text = user.peso.ToString("F2");
                editEmailInput.text = user.email ?? loggedInEmail;
                editPasswordInput.text = "********";

                // Força a atualização visual dos campos
                editNomeInput.ForceLabelUpdate();
                editIdadeInput.ForceLabelUpdate();
                editAlturaInput.ForceLabelUpdate();
                editPesoInput.ForceLabelUpdate();
                editEmailInput.ForceLabelUpdate();
                editPasswordInput.ForceLabelUpdate();

                Debug.Log("EditProfile: Dados do usuário preenchidos nos campos com sucesso!");
                StartCoroutine(UpdateFeedback("Perfil carregado com sucesso!", true));
            }
            else
            {
                Debug.LogWarning($"EditProfile: Nenhum dado encontrado para o userId: {loggedInUserId}. Preenchendo com valores padrão.");
                editEmailInput.text = loggedInEmail;
                editPasswordInput.text = "********";
                editNomeInput.text = "";
                editIdadeInput.text = "0";
                editAlturaInput.text = "0.00";
                editPesoInput.text = "0.00";

                editNomeInput.ForceLabelUpdate();
                editIdadeInput.ForceLabelUpdate();
                editAlturaInput.ForceLabelUpdate();
                editPesoInput.ForceLabelUpdate();
                editEmailInput.ForceLabelUpdate();
                editPasswordInput.ForceLabelUpdate();

                StartCoroutine(UpdateFeedback("Nenhum dado encontrado para o usuário. Preenchendo com dados padrão.", false));
            }
        });
    }

    public void SaveProfileChanges()
    {
        Debug.Log("EditProfile: Botão 'Salvar' clicado.");

        if (string.IsNullOrEmpty(loggedInUserId) || string.IsNullOrEmpty(loggedInEmail))
        {
            Debug.LogWarning("EditProfile: Erro ao salvar perfil: Dados do usuário logado ausentes!");
            StartCoroutine(UpdateFeedback("Erro: Dados do usuário logado ausentes!", false));
            return;
        }

        // Validação dos campos
        if (string.IsNullOrWhiteSpace(editNomeInput.text) || string.IsNullOrWhiteSpace(editEmailInput.text))
        {
            StartCoroutine(UpdateFeedback("Nome e e-mail são obrigatórios!", false));
            return;
        }

        if (!int.TryParse(editIdadeInput.text, out int idade) || idade < 0)
        {
            StartCoroutine(UpdateFeedback("Idade deve ser um número inteiro não negativo!", false));
            return;
        }

        if (!float.TryParse(editAlturaInput.text, out float altura) || altura < 0)
        {
            StartCoroutine(UpdateFeedback("Altura deve ser um número não negativo!", false));
            return;
        }

        if (!float.TryParse(editPesoInput.text, out float peso) || peso < 0)
        {
            StartCoroutine(UpdateFeedback("Peso deve ser um número não negativo!", false));
            return;
        }

        StartCoroutine(UpdateFeedback("Salvando alterações...", true));

        string nome = editNomeInput.text;
        string newEmail = editEmailInput.text;
        string newPassword = editPasswordInput.text;

        dbManager.GetUser(loggedInUserId, (user) =>
        {
            if (user != null)
            {
                // Cria um novo objeto User sem o atributo 'tipo'
                User updatedUser = new User(nome, newEmail, idade, altura, peso, "");
                updatedUser.is_confirmed = user.is_confirmed;
                DatabaseManager.SaveUser(loggedInUserId, updatedUser.nome, updatedUser.email, updatedUser.idade, updatedUser.altura, updatedUser.peso, updatedUser.tipo);

                StartCoroutine(UpdateFeedback("Dados do perfil salvos no banco de dados!", true));

                var firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
                if (firebaseUser == null)
                {
                    Debug.LogError("EditProfile: Nenhum usuário autenticado encontrado!");
                    StartCoroutine(UpdateFeedback("Erro: Nenhum usuário autenticado encontrado!", false));
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != loggedInEmail)
                {
                    firebaseUser.SendEmailVerificationBeforeUpdatingEmailAsync(newEmail).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Debug.LogError($"EditProfile: Erro ao enviar e-mail de verificação: {task.Exception}");
                            StartCoroutine(UpdateFeedback($"Erro ao atualizar e-mail: {task.Exception.Message}", false));
                            return;
                        }

                        if (task.IsCompleted)
                        {
                            Debug.Log("EditProfile: E-mail de verificação enviado com sucesso!");
                            StartCoroutine(UpdateFeedback("Um e-mail de verificação foi enviado para o novo endereço. Por favor, confirme antes de continuar.", true));
                            PlayerPrefs.SetString("loggedInUser", newEmail);
                            PlayerPrefs.Save();
                            loggedInEmail = newEmail;
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(newPassword) && newPassword != "********")
                {
                    firebaseUser.UpdatePasswordAsync(newPassword).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Debug.LogError($"EditProfile: Erro ao atualizar senha: {task.Exception}");
                            StartCoroutine(UpdateFeedback($"Erro ao atualizar senha: {task.Exception.Message}", false));
                            return;
                        }

                        if (task.IsCompleted)
                        {
                            Debug.Log("EditProfile: Senha atualizada com sucesso!");
                            StartCoroutine(UpdateFeedback("Senha atualizada com sucesso!", true));
                        }
                    });
                }
                else
                {
                    StartCoroutine(UpdateFeedback("Dados do perfil atualizados com sucesso!", true));
                }
            }
            else
            {
                Debug.LogError("EditProfile: Usuário não encontrado no banco de dados!");
                StartCoroutine(UpdateFeedback("Erro: Usuário não encontrado no banco de dados!", false));
            }
        });
    }

    public void Logout()
    {
        StartCoroutine(UpdateFeedback("Efetuando logout...", true));
        FirebaseAuth.DefaultInstance.SignOut();
        PlayerPrefs.DeleteKey("loggedInUser");
        PlayerPrefs.DeleteKey("loggedInUserId");
        PlayerPrefs.Save();
        SceneManager.LoadScene(2);
    }

    public void CancelProfileEdit()
    {
        StartCoroutine(UpdateFeedback("Cancelando edição de perfil...", true));
        SceneManager.LoadScene(10);
    }

    public void DeleteAccount()
    {
        StartCoroutine(UpdateFeedback("Apagando conta...", true));
        deleteAccountManager.DeleteUserAccount();
    }

    private IEnumerator UpdateFeedback(string message, bool isSuccess)
    {
        if (feedbackText == null || !isActive)
        {
            Debug.LogWarning("EditProfile: FeedbackText é null ou o script não está ativo. Cancelando UpdateFeedback.");
            yield break;
        }

        Debug.Log($"EditProfile: Exibindo feedback: {message} (isSuccess: {isSuccess})");
        feedbackText.text = message;
        feedbackText.color = isSuccess ? new Color(0f, 0.5f, 0f) : Color.red;
        yield return new WaitForSeconds(4f);

        if (feedbackText != null && isActive)
        {
            feedbackText.text = "";
            Debug.Log("EditProfile: Feedback limpo após 4 segundos.");
        }
    }
}