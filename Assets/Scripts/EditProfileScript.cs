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

        // Verifica��es de inicializa��o
        if (dbManager == null)
        {
            Debug.LogError("EditProfile: DatabaseManager n�o encontrado na cena!");
            StartCoroutine(UpdateFeedback("Erro: DatabaseManager n�o encontrado!", false));
            DisableButtons();
            return;
        }

        if (authManager == null)
        {
            Debug.LogError("EditProfile: AuthManager n�o encontrado na cena!");
            StartCoroutine(UpdateFeedback("Erro: AuthManager n�o encontrado!", false));
            DisableButtons();
            return;
        }

        if (deleteAccountManager == null)
        {
            Debug.LogError("EditProfile: DeleteAccount n�o encontrado na cena!");
            StartCoroutine(UpdateFeedback("Erro: DeleteAccount n�o encontrado!", false));
            DisableButtons();
            return;
        }

        // Verifica se os campos de entrada est�o atribu�dos
        if (editNomeInput == null || editIdadeInput == null || editAlturaInput == null || editPesoInput == null || editEmailInput == null || editPasswordInput == null)
        {
            Debug.LogError("EditProfile: Um ou mais campos de entrada n�o foram atribu�dos no Inspector!");
            StartCoroutine(UpdateFeedback("Erro: Campos de entrada n�o atribu�dos!", false));
            DisableButtons();
            return;
        }

        if (feedbackText == null)
        {
            Debug.LogError("EditProfile: FeedbackText n�o atribu�do!");
            StartCoroutine(UpdateFeedback("Erro: FeedbackText n�o atribu�do!", false));
            DisableButtons();
            return;
        }

        if (saveButton == null || cancelButton == null || logoutButton == null || deleteAccountButton == null)
        {
            Debug.LogError("EditProfile: Um ou mais bot�es n�o foram atribu�dos no Inspector!");
            StartCoroutine(UpdateFeedback("Erro: Bot�es n�o atribu�dos!", false));
            DisableButtons();
            return;
        }

        Debug.Log("EditProfile: Todos os componentes foram inicializados corretamente.");

        // Configura os tipos de entrada dos campos
        ConfigureInputFields();

        // Configura os listeners dos bot�es
        saveButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        logoutButton.onClick.RemoveAllListeners();
        deleteAccountButton.onClick.RemoveAllListeners();

        saveButton.onClick.AddListener(SaveProfileChanges);
        cancelButton.onClick.AddListener(CancelProfileEdit);
        logoutButton.onClick.AddListener(Logout);
        deleteAccountButton.onClick.AddListener(DeleteAccount);

        Debug.Log("EditProfile: Listeners dos bot�es configurados com sucesso.");

        // Verifica se h� um usu�rio logado
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        if (currentUser != null)
        {
            loggedInEmail = currentUser.Email;
            loggedInUserId = currentUser.UserId;
            PlayerPrefs.SetString("loggedInUser", loggedInEmail);
            PlayerPrefs.SetString("loggedInUserId", loggedInUserId);
            PlayerPrefs.Save();
            Debug.Log($"EditProfile: Usu�rio logado: Email={loggedInEmail}, UserId={loggedInUserId}");
            StartCoroutine(UpdateFeedback("Bem-vindo, " + loggedInEmail + "!", true));
        }
        else if (PlayerPrefs.HasKey("loggedInUser") && PlayerPrefs.HasKey("loggedInUserId"))
        {
            loggedInEmail = PlayerPrefs.GetString("loggedInUser");
            loggedInUserId = PlayerPrefs.GetString("loggedInUserId");
            Debug.Log($"EditProfile: Usu�rio recuperado do PlayerPrefs: Email={loggedInEmail}, UserId={loggedInUserId}");
            StartCoroutine(UpdateFeedback("Bem-vindo, " + loggedInEmail + "!", true));
        }
        else
        {
            Debug.LogWarning("EditProfile: Nenhum usu�rio logado! Redirecionando para a tela de login.");
            StartCoroutine(UpdateFeedback("Nenhum usu�rio logado. Redirecionando para o login...", false));
            SceneManager.LoadScene(2);
            return;
        }

        // Carrega os dados do perfil
        LoadUserProfile();
    }

    void OnDestroy()
    {
        isActive = false;
        Debug.Log("EditProfile: Destru�do.");
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
        Debug.Log("EditProfile: Bot�es desativados devido a erro de inicializa��o.");
    }

    public void LoadUserProfile()
    {
        if (string.IsNullOrEmpty(loggedInUserId) || string.IsNullOrEmpty(loggedInEmail))
        {
            Debug.LogWarning("EditProfile: Erro ao carregar perfil: Dados do usu�rio logado ausentes!");
            StartCoroutine(UpdateFeedback("Erro: Dados do usu�rio logado ausentes!", false));
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
                Debug.Log($"EditProfile: Dados do usu�rio carregados: Nome={user.nome}, Idade={user.idade}, Altura={user.altura}, Peso={user.peso}, Email={user.email}, IsConfirmed={user.is_confirmed}");

                // Preenche os campos com os dados do usu�rio
                editNomeInput.text = user.nome ?? "";
                editIdadeInput.text = user.idade.ToString();
                editAlturaInput.text = user.altura.ToString("F2");
                editPesoInput.text = user.peso.ToString("F2");
                editEmailInput.text = user.email ?? loggedInEmail;
                editPasswordInput.text = "********";

                // For�a a atualiza��o visual dos campos
                editNomeInput.ForceLabelUpdate();
                editIdadeInput.ForceLabelUpdate();
                editAlturaInput.ForceLabelUpdate();
                editPesoInput.ForceLabelUpdate();
                editEmailInput.ForceLabelUpdate();
                editPasswordInput.ForceLabelUpdate();

                Debug.Log("EditProfile: Dados do usu�rio preenchidos nos campos com sucesso!");
                StartCoroutine(UpdateFeedback("Perfil carregado com sucesso!", true));
            }
            else
            {
                Debug.LogWarning($"EditProfile: Nenhum dado encontrado para o userId: {loggedInUserId}. Preenchendo com valores padr�o.");
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

                StartCoroutine(UpdateFeedback("Nenhum dado encontrado para o usu�rio. Preenchendo com dados padr�o.", false));
            }
        });
    }

    public void SaveProfileChanges()
    {
        Debug.Log("EditProfile: Bot�o 'Salvar' clicado.");

        if (string.IsNullOrEmpty(loggedInUserId) || string.IsNullOrEmpty(loggedInEmail))
        {
            Debug.LogWarning("EditProfile: Erro ao salvar perfil: Dados do usu�rio logado ausentes!");
            StartCoroutine(UpdateFeedback("Erro: Dados do usu�rio logado ausentes!", false));
            return;
        }

        // Valida��o dos campos
        if (string.IsNullOrWhiteSpace(editNomeInput.text) || string.IsNullOrWhiteSpace(editEmailInput.text))
        {
            StartCoroutine(UpdateFeedback("Nome e e-mail s�o obrigat�rios!", false));
            return;
        }

        if (!int.TryParse(editIdadeInput.text, out int idade) || idade < 0)
        {
            StartCoroutine(UpdateFeedback("Idade deve ser um n�mero inteiro n�o negativo!", false));
            return;
        }

        if (!float.TryParse(editAlturaInput.text, out float altura) || altura < 0)
        {
            StartCoroutine(UpdateFeedback("Altura deve ser um n�mero n�o negativo!", false));
            return;
        }

        if (!float.TryParse(editPesoInput.text, out float peso) || peso < 0)
        {
            StartCoroutine(UpdateFeedback("Peso deve ser um n�mero n�o negativo!", false));
            return;
        }

        StartCoroutine(UpdateFeedback("Salvando altera��es...", true));

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
                    Debug.LogError("EditProfile: Nenhum usu�rio autenticado encontrado!");
                    StartCoroutine(UpdateFeedback("Erro: Nenhum usu�rio autenticado encontrado!", false));
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != loggedInEmail)
                {
                    firebaseUser.SendEmailVerificationBeforeUpdatingEmailAsync(newEmail).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Debug.LogError($"EditProfile: Erro ao enviar e-mail de verifica��o: {task.Exception}");
                            StartCoroutine(UpdateFeedback($"Erro ao atualizar e-mail: {task.Exception.Message}", false));
                            return;
                        }

                        if (task.IsCompleted)
                        {
                            Debug.Log("EditProfile: E-mail de verifica��o enviado com sucesso!");
                            StartCoroutine(UpdateFeedback("Um e-mail de verifica��o foi enviado para o novo endere�o. Por favor, confirme antes de continuar.", true));
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
                Debug.LogError("EditProfile: Usu�rio n�o encontrado no banco de dados!");
                StartCoroutine(UpdateFeedback("Erro: Usu�rio n�o encontrado no banco de dados!", false));
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
        StartCoroutine(UpdateFeedback("Cancelando edi��o de perfil...", true));
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
            Debug.LogWarning("EditProfile: FeedbackText � null ou o script n�o est� ativo. Cancelando UpdateFeedback.");
            yield break;
        }

        Debug.Log($"EditProfile: Exibindo feedback: {message} (isSuccess: {isSuccess})");
        feedbackText.text = message;
        feedbackText.color = isSuccess ? new Color(0f, 0.5f, 0f) : Color.red;
        yield return new WaitForSeconds(4f);

        if (feedbackText != null && isActive)
        {
            feedbackText.text = "";
            Debug.Log("EditProfile: Feedback limpo ap�s 4 segundos.");
        }
    }
}