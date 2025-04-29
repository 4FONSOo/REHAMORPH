using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        FirebaseApp.Create(new AppOptions
        {
            DatabaseUrl = new System.Uri("https://rehamorph-default-rtdb.europe-west1.firebasedatabase.app/"),
        });

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Erro ao verificar dependências do Firebase: {task.Exception}");
                return;
            }

            if (task.IsCanceled)
            {
                Debug.LogError("Verificação de dependências do Firebase foi cancelada.");
                return;
            }

            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                Debug.Log("Firebase inicializado com sucesso!");
            }
            else
            {
                Debug.LogError($"Erro ao inicializar Firebase: {dependencyStatus}");
            }
        });
    }
}