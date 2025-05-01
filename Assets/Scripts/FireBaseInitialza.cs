using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    private static FirebaseInitializer instance;
    public bool IsFirebaseInitialized { get; private set; } = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("FirebaseInitializer: Instância criada e marcada como DontDestroyOnLoad.");
        }
        else
        {
            Debug.Log("FirebaseInitializer: Instância já existe. Destruindo duplicata.");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log("FirebaseInitializer: Iniciando a inicialização do Firebase...");

        FirebaseApp.Create(new AppOptions
        {
            DatabaseUrl = new System.Uri("https://rehamorph-default-rtdb.europe-west1.firebasedatabase.app/"),
        });

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"FirebaseInitializer: Erro ao verificar dependências do Firebase: {task.Exception}");
                IsFirebaseInitialized = false;
                return;
            }

            if (task.IsCanceled)
            {
                Debug.LogError("FirebaseInitializer: Verificação de dependências do Firebase foi cancelada.");
                IsFirebaseInitialized = false;
                return;
            }

            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                Debug.Log("FirebaseInitializer: Firebase inicializado com sucesso!");
                IsFirebaseInitialized = true;
            }
            else
            {
                Debug.LogError($"FirebaseInitializer: Erro ao inicializar Firebase: {dependencyStatus}");
                IsFirebaseInitialized = false;
            }
        });
    }
}