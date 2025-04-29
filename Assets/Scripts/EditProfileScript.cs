using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Extensions;

public class EditProfile : MonoBehaviour
{
    public TMP_InputField editNomeInput;
    public TMP_InputField editPasswordInput;
    public TMP_InputField editEmailInput;
    public TMP_Text feedbackText;
    public Button saveButton;
    public Button cancelButton;
    public Button logoutButton;

    private string loggedInUserId;
    private string loggedInEmail;
    private DatabaseManager dbManager;
    private FireBaseAuth authManager;

    void Start()
    {
        dbManager = FindObjectOfType<DatabaseManager>();
        if (dbManager == null)
        {
            Debug.LogError("DatabaseManager n�o encontrado na cena! Certifique-se de que est� anexado a um GameObject.");
            UpdateFeedback("Erro: DatabaseManager n�o encontrado!", false);
            return;
        }

        authManager = FindObjectOfType<FireBaseAuth>();
        if (authManager == null)
        {
            Debug.LogError("AuthManager n�o encontrado na cena! Certifique-se de que est� anexado a um GameObject.");
            UpdateFeedback("Erro: AuthManager n�o encontrado!", false);
            return;
        }

        if (editNomeInput == null || editEmailInput == null || editPasswordInput == null)
        {
            Debug.LogError("Um ou mais campos de entrada (TMP_InputField) n�o foram atribu�dos no Inspector!");
            UpdateFeedback("Erro: Campos de entrada n�o atribu�dos!", false);
            return;
        }

        if (feedbackText == null)
        {
            Debug.LogError("FeedbackText n�o atribu�do no EditProfile! Certifique-se de que est� anexado a um GameObject na cena.");
            UpdateFeedback("Erro: FeedbackText n�o atribu�do!", false);
            return;
        }

        if (saveButton == null || cancelButton == null || logoutButton == null)
        {
            Debug.LogError("Um ou mais bot�es (saveButton, cancelButton, logoutButton) n�o foram atribu�dos no Inspector!");
            UpdateFeedback("Erro: Bot�es n�o atribu�dos!", false);
            return;
        }

        ConfigureInputFields();

        saveButton.onClick.AddListener(SaveProfileChanges);
        cancelButton.onClick.AddListener(CancelProfileEdit);
        logoutButton.onClick.AddListener(Logout);

        if (PlayerPrefs.HasKey("loggedInUser"))
        {
            loggedInEmail = PlayerPrefs.GetString("loggedInUser");
            Debug.Log("Usu�rio logado: " + loggedInEmail);
            UpdateFeedback("Bem-vindo, " + loggedInEmail + "!", true);
        }
        else
        {
            Debug.LogWarning("Nenhum utilizador est� logado no PlayerPrefs! Verificando Firebase Authentication...");
            UpdateFeedback("Verificando usu�rio logado...", true);
            var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
            if (currentUser != null)
            {
                loggedInEmail = currentUser.Email;
                loggedInUserId = currentUser.UserId;
                PlayerPrefs.SetString("loggedInUser", loggedInEmail);
                PlayerPrefs.SetString("loggedInUserId", loggedInUserId);
                PlayerPrefs.Save();
                Debug.Log($"Usu�rio recuperado do Firebase Authentication: Email={loggedInEmail}, UserId={loggedInUserId}");
                UpdateFeedback("Usu�rio recuperado com sucesso: " + loggedInEmail, true);
            }
            else
            {
                Debug.LogWarning("Nenhum utilizador est� logado! Redirecionando para a tela de login.");
                UpdateFeedback("Nenhum usu�rio logado. Redirecionando para o login...", false);
                SceneManager.LoadScene(2);
                return;
            }
        }

        if (PlayerPrefs.HasKey("loggedInUserId"))
        {
            loggedInUserId = PlayerPrefs.GetString("loggedInUserId");
            Debug.Log("UserId do usu�rio logado: " + loggedInUserId);
        }
        else
        {
            Debug.LogWarning("UserId do usu�rio logado n�o encontrado no PlayerPrefs! Verificando Firebase Authentication...");
            UpdateFeedback("Verificando ID do usu�rio...", true);
            var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
            if (currentUser != null)
            {
                loggedInUserId = currentUser.UserId;
                PlayerPrefs.SetString("loggedInUserId", loggedInUserId);
                PlayerPrefs.Save();
                Debug.Log($"UserId recuperado do Firebase Authentication: {loggedInUserId}");
                UpdateFeedback("ID do usu�rio recuperado com sucesso.", true);
            }
            else
            {
                Debug.LogWarning("UserId do usu�rio logado n�o encontrado! Redirecionando para a tela de login.");
                UpdateFeedback("ID do usu�rio n�o encontrado. Redirecionando para o login...", false);
                SceneManager.LoadScene(2);
                return;
            }
        }

        LoadUserProfile();
    }

    private void ConfigureInputFields()
    {
        editNomeInput.contentType = TMP_InputField.ContentType.Standard;
        editEmailInput.contentType = TMP_InputField.ContentType.EmailAddress;
        editPasswordInput.contentType = TMP_InputField.ContentType.Password;

        editNomeInput.ForceLabelUpdate();
        editEmailInput.ForceLabelUpdate();
        editPasswordInput.ForceLabelUpdate();
    }

    public void LoadUserProfile()
    {
        if (string.IsNullOrEmpty(loggedInUserId) || string.IsNullOrEmpty(loggedInEmail))
        {
            Debug.LogWarning("Erro ao carregar perfil: Dados do usu�rio logado ausentes!");
            UpdateFeedback("Erro: Dados do usu�rio logado ausentes!", false);
            return;
        }

        editEmailInput.text = loggedInEmail;
        Debug.Log($"Preenchendo e-mail: {loggedInEmail}");

        dbManager.GetUser(loggedInUserId, (user) =>
        {
            if (user != null)
            {
                Debug.Log($"Dados do usu�rio carregados: Nome={user.nome}, Email={user.email}");
                editNomeInput.text = user.nome ?? "";
                editNomeInput.ForceLabelUpdate();
                Debug.Log("Dados do usu�rio preenchidos nos campos com sucesso!");
                UpdateFeedback("Perfil carregado com sucesso!", true);
            }
            else
            {
                Debug.LogWarning("Nenhum dado encontrado para o userId: " + loggedInUserId);
                UpdateFeedback("Nenhum dado encontrado para o usu�rio.", false);
            }
        });
    }

    public void SaveProfileChanges()
    {
        if (string.IsNullOrEmpty(loggedInUserId) || string.IsNullOrEmpty(loggedInEmail))
        {
            Debug.LogWarning("Erro ao salvar perfil: Dados do usu�rio logado ausentes!");
            UpdateFeedback("Erro: Dados do usu�rio logado ausentes!", false);
            return;
        }

        if (string.IsNullOrWhiteSpace(editNomeInput.text) || string.IsNullOrWhiteSpace(editEmailInput.text))
        {
            UpdateFeedback("Nome e e-mail s�o obrigat�rios!", false);
            return;
        }

        UpdateFeedback("Salvando altera��es...", true);

        string nome = editNomeInput.text;
        string newEmail = editEmailInput.text;
        string newPassword = editPasswordInput.text;

        dbManager.GetUser(loggedInUserId, (user) =>
        {
            if (user != null)
            {
                User updatedUser = new User(nome, newEmail);
                updatedUser.is_confirmed = user.is_confirmed; // Preservar o estado de confirma��o
                DatabaseManager.SaveUser(loggedInUserId, updatedUser.nome, updatedUser.email);

                UpdateFeedback("Dados do perfil salvos no banco de dados!", true);

                var firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
                if (firebaseUser == null)
                {
                    Debug.LogError("Nenhum usu�rio autenticado encontrado!");
                    UpdateFeedback("Erro: Nenhum usu�rio autenticado encontrado!", false);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != loggedInEmail)
                {
                    firebaseUser.SendEmailVerificationBeforeUpdatingEmailAsync(newEmail).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Debug.LogError($"Erro ao enviar e-mail de verifica��o: {task.Exception}");
                            UpdateFeedback($"Erro ao atualizar e-mail: {task.Exception.Message}", false);
                            return;
                        }

                        if (task.IsCompleted)
                        {
                            Debug.Log("E-mail de verifica��o enviado com sucesso! O usu�rio precisa confirmar o novo e-mail.");
                            UpdateFeedback("Um e-mail de verifica��o foi enviado para o novo endere�o. Por favor, confirme antes de continuar.", true);
                            PlayerPrefs.SetString("loggedInUser", newEmail);
                            PlayerPrefs.Save();
                            loggedInEmail = newEmail;
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    firebaseUser.UpdatePasswordAsync(newPassword).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Debug.LogError($"Erro ao atualizar senha: {task.Exception}");
                            UpdateFeedback($"Erro ao atualizar senha: {task.Exception.Message}", false);
                            return;
                        }

                        if (task.IsCompleted)
                        {
                            Debug.Log("Senha atualizada com sucesso no Firebase Authentication!");
                            UpdateFeedback("Senha atualizada com sucesso!", true);
                        }
                    });
                }
                else
                {
                    UpdateFeedback("Dados do perfil atualizados com sucesso!", true);
                }
            }
            else
            {
                Debug.LogError("Usu�rio n�o encontrado no banco de dados!");
                UpdateFeedback("Erro: Usu�rio n�o encontrado no banco de dados!", false);
            }
        });
    }

    public void Logout()
    {
        Debug.Log("Iniciando logout do usu�rio...");
        UpdateFeedback("Efetuando logout...", true);

        FirebaseAuth.DefaultInstance.SignOut();
        Debug.Log("Usu�rio deslogado do Firebase Authentication com sucesso.");
        UpdateFeedback("Logout efetuado com sucesso!", true);

        PlayerPrefs.DeleteKey("loggedInUser");
        PlayerPrefs.DeleteKey("loggedInUserId");
        PlayerPrefs.Save();
        Debug.Log("Dados do PlayerPrefs limpos.");

        SceneManager.LoadScene(2);
        Debug.Log("Redirecionando para a cena de login (cena 2).");
    }

    public void CancelProfileEdit()
    {
        UpdateFeedback("Cancelando edi��o de perfil...", true);
        SceneManager.LoadScene(10);
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