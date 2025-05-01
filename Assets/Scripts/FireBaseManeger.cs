using Firebase.Database;
using UnityEngine;
using System;
using Firebase.Extensions;

public class DatabaseManager : MonoBehaviour
{
    private DatabaseReference dbReference;
    private static DatabaseManager instance;
    private FirebaseInitializer firebaseInitializer;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("DatabaseManager: Inst�ncia criada e marcada como DontDestroyOnLoad.");
        }
        else
        {
            Debug.Log("DatabaseManager: Inst�ncia j� existe. Destruindo duplicata.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        firebaseInitializer = FindObjectOfType<FirebaseInitializer>();
        if (firebaseInitializer == null)
        {
            Debug.LogError("DatabaseManager: FirebaseInitializer n�o encontrado na cena! Certifique-se de que ele est� na cena 0.");
            return;
        }

        // Espera at� que o Firebase esteja inicializado
        StartCoroutine(WaitForFirebaseInitialization());
    }

    private System.Collections.IEnumerator WaitForFirebaseInitialization()
    {
        Debug.Log("DatabaseManager: Aguardando inicializa��o do Firebase...");
        while (!firebaseInitializer.IsFirebaseInitialized)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("DatabaseManager: Firebase inicializado. Inicializando DatabaseReference...");
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("DatabaseManager: DatabaseReference inicializado com sucesso: " + (dbReference != null));
    }

    public static void SaveUser(string userId, string nome, string email, int idade, float altura, float peso, string tipo)
    {
        User user = new User(nome, email, idade, altura, peso, tipo);
        string json = JsonUtility.ToJson(user);
        Debug.Log($"DatabaseManager: Salvando usu�rio no caminho 'users/{userId}': {json}");
        FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"DatabaseManager: Erro ao salvar usu�rio {userId}: {task.Exception}");
            }
            else if (task.IsCompleted)
            {
                Debug.Log($"DatabaseManager: Usu�rio {userId} salvo com sucesso.");
            }
        });
    }

    public void GetUser(string userId, System.Action<User> callback)
    {
        if (dbReference == null)
        {
            Debug.LogWarning("DatabaseManager: DatabaseReference n�o inicializado. Tentando inicializar...");
            InitializeDatabase();
            if (dbReference == null)
            {
                Debug.LogError("DatabaseManager: Falha ao inicializar DatabaseReference!");
                callback?.Invoke(null);
                return;
            }
        }

        Debug.Log($"DatabaseManager: Buscando usu�rio no caminho 'users/{userId}'...");
        dbReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"DatabaseManager: Erro ao buscar usu�rio {userId}: {task.Exception}");
                callback?.Invoke(null);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    User user = JsonUtility.FromJson<User>(snapshot.GetRawJsonValue());
                    Debug.Log($"DatabaseManager: Usu�rio {userId} encontrado: {snapshot.GetRawJsonValue()}");
                    callback?.Invoke(user);
                }
                else
                {
                    Debug.LogWarning($"DatabaseManager: Usu�rio {userId} n�o encontrado.");
                    callback?.Invoke(null);
                }
            }
        });
    }

    public void UpdateUserConfirmation(string userId, bool isConfirmed, System.Action onSuccess, System.Action<string> onError)
    {
        if (dbReference == null)
        {
            Debug.LogWarning("DatabaseManager: DatabaseReference n�o inicializado. Tentando inicializar...");
            InitializeDatabase();
            if (dbReference == null)
            {
                Debug.LogError("DatabaseManager: Falha ao inicializar DatabaseReference!");
                onError?.Invoke("DatabaseReference n�o inicializado.");
                return;
            }
        }

        Debug.Log($"DatabaseManager: Atualizando confirma��o para o usu�rio {userId}: isConfirmed = {isConfirmed}");
        dbReference.Child("users").Child(userId).Child("is_confirmed").SetValueAsync(isConfirmed).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"DatabaseManager: Erro ao atualizar confirma��o do usu�rio {userId}: {task.Exception}");
                onError?.Invoke(task.Exception.Message);
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log($"DatabaseManager: Confirma��o do usu�rio {userId} atualizada com sucesso.");
                onSuccess?.Invoke();
            }
        });
    }

    public static void SaveConfirmationToken(string token, string userId, string email)
    {
        ConfirmationToken confirmationToken = new ConfirmationToken(userId, email, DateTime.UtcNow.ToString("o"));
        string json = JsonUtility.ToJson(confirmationToken);
        Debug.Log($"DatabaseManager: Salvando token de confirma��o no caminho 'tokens/{token}': {json}");
        FirebaseDatabase.DefaultInstance.RootReference.Child("tokens").Child(token).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"DatabaseManager: Erro ao salvar token de confirma��o {token}: {task.Exception}");
            }
            else if (task.IsCompleted)
            {
                Debug.Log($"DatabaseManager: Token de confirma��o {token} salvo com sucesso.");
            }
        });
    }

    public void VerifyConfirmationToken(string token, System.Action<string> onSuccess, System.Action<string> onError)
    {
        if (dbReference == null)
        {
            Debug.LogWarning("DatabaseManager: DatabaseReference n�o inicializado. Tentando inicializar...");
            InitializeDatabase();
            if (dbReference == null)
            {
                Debug.LogError("DatabaseManager: Falha ao inicializar DatabaseReference!");
                onError?.Invoke("DatabaseReference n�o inicializado.");
                return;
            }
        }

        Debug.Log($"DatabaseManager: Verificando token no caminho 'tokens/{token}'...");
        dbReference.Child("tokens").Child(token).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"DatabaseManager: Erro ao verificar o token {token}: {task.Exception}");
                if (task.Exception != null)
                {
                    Debug.LogError($"DatabaseManager: Detalhes do erro: {task.Exception.Message}");
                    foreach (var innerException in task.Exception.InnerExceptions)
                    {
                        Debug.LogError($"DatabaseManager: Inner Exception: {innerException.Message}");
                    }
                }
                onError?.Invoke(task.Exception?.Message ?? "Erro desconhecido ao verificar o token.");
                return;
            }

            if (task.IsCanceled)
            {
                Debug.LogError($"DatabaseManager: Verifica��o do token {token} foi cancelada.");
                onError?.Invoke("Verifica��o cancelada.");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log($"DatabaseManager: Tarefa de verifica��o do token {token} conclu�da com sucesso.");
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string rawJson = snapshot.GetRawJsonValue();
                    Debug.Log($"DatabaseManager: Dados brutos encontrados para o token {token}: {rawJson}");
                    ConfirmationToken confirmationToken = JsonUtility.FromJson<ConfirmationToken>(rawJson);
                    if (confirmationToken != null)
                    {
                        Debug.Log($"DatabaseManager: Token {token} encontrado. user_id associado: {confirmationToken.user_id}");
                        onSuccess?.Invoke(confirmationToken.user_id);
                    }
                    else
                    {
                        Debug.LogError($"DatabaseManager: Erro ao desserializar o token {token}. Dados brutos: {rawJson}");
                        onError?.Invoke("Erro ao processar os dados do token.");
                    }
                }
                else
                {
                    Debug.LogWarning($"DatabaseManager: Token {token} n�o encontrado na base de dados.");
                    onSuccess?.Invoke(null);
                }
            }
            else
            {
                Debug.LogWarning($"DatabaseManager: Tarefa de verifica��o do token {token} n�o foi conclu�da corretamente.");
                onError?.Invoke("Falha ao concluir a verifica��o do token.");
            }
        });
    }

    public void RemoveConfirmationToken(string token, System.Action onSuccess, System.Action<string> onError)
    {
        if (dbReference == null)
        {
            Debug.LogWarning("DatabaseManager: DatabaseReference n�o inicializado. Tentando inicializar...");
            InitializeDatabase();
            if (dbReference == null)
            {
                Debug.LogError("DatabaseManager: Falha ao inicializar DatabaseReference!");
                onError?.Invoke("DatabaseReference n�o inicializado.");
                return;
            }
        }

        Debug.Log($"DatabaseManager: Removendo token no caminho 'tokens/{token}'...");
        dbReference.Child("tokens").Child(token).RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"DatabaseManager: Erro ao remover o token {token}: {task.Exception}");
                onError?.Invoke(task.Exception.Message);
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log($"DatabaseManager: Token {token} removido com sucesso.");
                onSuccess?.Invoke();
            }
        });
    }
}

[System.Serializable]
public class User
{
    public string nome;
    public string email;
    public int idade;
    public float altura;
    public float peso;
    public string tipo;
    public bool is_confirmed;

    public User(string nome, string email, int idade, float altura, float peso, string tipo)
    {
        this.nome = nome;
        this.email = email;
        this.idade = idade;
        this.altura = altura;
        this.peso = peso;
        this.tipo = tipo;
        this.is_confirmed = false;
    }
}

[System.Serializable]
public class ConfirmationToken
{
    public string user_id;
    public string email;
    public string created_at;

    public ConfirmationToken(string userId, string email, string createdAt)
    {
        this.user_id = userId;
        this.email = email;
        this.created_at = createdAt;
    }
}