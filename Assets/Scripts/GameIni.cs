using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private GameObject databaseManagerPrefab;
    [SerializeField] private GameObject fireBaseAuthPrefab;
    private FirebaseInitializer firebaseInitializer;

    void Awake()
    {
        Debug.Log("GameInitializer: Awake chamado.");

        // Verifica se o FirebaseInitializer existe
        firebaseInitializer = FindObjectOfType<FirebaseInitializer>();
        if (firebaseInitializer == null)
        {
            Debug.LogError("GameInitializer: FirebaseInitializer não encontrado na cena! Certifique-se de que ele está na cena 0.");
        }

        // Verifica se o DatabaseManager já existe
        if (FindObjectOfType<DatabaseManager>() == null)
        {
            if (databaseManagerPrefab != null)
            {
                Instantiate(databaseManagerPrefab);
                Debug.Log("GameInitializer: DatabaseManager instanciado com sucesso na cena 0.");
            }
            else
            {
                Debug.LogError("GameInitializer: DatabaseManager Prefab não atribuído no Inspector! Atribua o prefab do DatabaseManager.");
            }
        }
        else
        {
            Debug.Log("GameInitializer: DatabaseManager já existe na cena.");
        }

        // Verifica se o FireBaseAuth já existe
        if (FindObjectOfType<FireBaseAuth>() == null)
        {
            if (fireBaseAuthPrefab != null)
            {
                Instantiate(fireBaseAuthPrefab);
                Debug.Log("GameInitializer: FireBaseAuth instanciado com sucesso na cena 0.");
            }
            else
            {
                Debug.LogError("GameInitializer: FireBaseAuth Prefab não atribuído no Inspector! Atribua o prefab do FireBaseAuth.");
            }
        }
        else
        {
            Debug.Log("GameInitializer: FireBaseAuth já existe na cena.");
        }
    }

    void Start()
    {
        Debug.Log("GameInitializer: Start chamado. Aguardando inicialização do Firebase...");
        StartCoroutine(WaitForFirebaseAndLoadScene());
    }

    private IEnumerator WaitForFirebaseAndLoadScene()
    {
        if (firebaseInitializer == null)
        {
            Debug.LogError("GameInitializer: FirebaseInitializer não encontrado. Não é possível prosseguir.");
            yield break;
        }

        while (!firebaseInitializer.IsFirebaseInitialized)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("GameInitializer: Firebase inicializado. Carregando a cena de registro (cena 1)...");
        SceneManager.LoadScene(1);
    }
}