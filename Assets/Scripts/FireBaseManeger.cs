using Firebase.Database;
using UnityEngine;
using System;

public class DatabaseManager : MonoBehaviour
{
    private DatabaseReference dbReference;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("DatabaseManager inicializado com sucesso. Refer�ncia ao Firebase Realtime Database estabelecida.");
    }

    public static void SaveUser(string userId, string nome, string email)
    {
        User user = new User(nome, email);
        string json = JsonUtility.ToJson(user);
        Debug.Log($"Salvando usu�rio no caminho 'users/{userId}': {json}");
        FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Erro ao salvar usu�rio {userId}: {task.Exception}");
            }
            else if (task.IsCompleted)
            {
                Debug.Log($"Usu�rio {userId} salvo com sucesso.");
            }
        });
    }

    public void GetUser(string userId, System.Action<User> callback)
    {
        Debug.Log($"Buscando usu�rio no caminho 'users/{userId}'...");
        dbReference.Child("users").Child(userId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Erro ao buscar usu�rio {userId}: {task.Exception}");
                callback(null);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    User user = JsonUtility.FromJson<User>(snapshot.GetRawJsonValue());
                    Debug.Log($"Usu�rio {userId} encontrado: {snapshot.GetRawJsonValue()}");
                    callback(user);
                }
                else
                {
                    Debug.LogWarning($"Usu�rio {userId} n�o encontrado.");
                    callback(null);
                }
            }
        });
    }

    public void UpdateUserConfirmation(string userId, bool isConfirmed, System.Action onSuccess, System.Action<string> onError)
    {
        Debug.Log($"Atualizando confirma��o para o usu�rio {userId}: isConfirmed = {isConfirmed}");
        dbReference.Child("users").Child(userId).Child("is_confirmed").SetValueAsync(isConfirmed).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Erro ao atualizar confirma��o do usu�rio {userId}: {task.Exception}");
                onError?.Invoke(task.Exception.Message);
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log($"Confirma��o do usu�rio {userId} atualizada com sucesso.");
                onSuccess?.Invoke();
            }
        });
    }

    public static void SaveConfirmationToken(string token, string userId, string email)
    {
        ConfirmationToken confirmationToken = new ConfirmationToken(userId, email, DateTime.UtcNow.ToString("o"));
        string json = JsonUtility.ToJson(confirmationToken);
        Debug.Log($"Salvando token de confirma��o no caminho 'tokens/{token}': {json}");
        FirebaseDatabase.DefaultInstance.RootReference.Child("tokens").Child(token).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Erro ao salvar token de confirma��o {token}: {task.Exception}");
            }
            else if (task.IsCompleted)
            {
                Debug.Log($"Token de confirma��o {token} salvo com sucesso.");
            }
        });
    }

    public void VerifyConfirmationToken(string token, System.Action<string> onSuccess, System.Action<string> onError)
    {
        Debug.Log($"Verificando token no caminho 'tokens/{token}'...");
        dbReference.Child("tokens").Child(token).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Erro ao verificar o token {token}: {task.Exception}");
                onError?.Invoke(task.Exception.Message);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    ConfirmationToken confirmationToken = JsonUtility.FromJson<ConfirmationToken>(snapshot.GetRawJsonValue());
                    Debug.Log($"Token {token} encontrado. user_id associado: {confirmationToken.user_id}");
                    onSuccess?.Invoke(confirmationToken.user_id);
                }
                else
                {
                    Debug.LogWarning($"Token {token} n�o encontrado na base de dados.");
                    onSuccess?.Invoke(null);
                }
            }
        });
    }

    public void RemoveConfirmationToken(string token, System.Action onSuccess, System.Action<string> onError)
    {
        Debug.Log($"Removendo token no caminho 'tokens/{token}'...");
        dbReference.Child("tokens").Child(token).RemoveValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Erro ao remover o token {token}: {task.Exception}");
                onError?.Invoke(task.Exception.Message);
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log($"Token {token} removido com sucesso.");
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
    public bool is_confirmed;

    public User(string nome, string email)
    {
        this.nome = nome;
        this.email = email;
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