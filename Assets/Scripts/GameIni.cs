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
            Debug.LogError("GameInitializer: FirebaseInitializer n�o encontrado na cena! Certifique-se de que ele est� na cena 0.");
        }

        // Verifica se o DatabaseManager j� existe
        if (FindObjectOfType<DatabaseManager>() == null)
        {
            if (databaseManagerPrefab != null)
            {
                Instantiate(databaseManagerPrefab);
                Debug.Log("GameInitializer: DatabaseManager instanciado com sucesso na cena 0.");
            }
            else
            {
                Debug.LogError("GameInitializer: DatabaseManager Prefab n�o atribu�do no Inspector! Atribua o prefab do DatabaseManager.");
            }
        }
        else
        {
            Debug.Log("GameInitializer: DatabaseManager j� existe na cena.");
        }

        // Verifica se o FireBaseAuth j� existe
        if (FindObjectOfType<FireBaseAuth>() == null)
        {
            if (fireBaseAuthPrefab != null)
            {
                Instantiate(fireBaseAuthPrefab);
                Debug.Log("GameInitializer: FireBaseAuth instanciado com sucesso na cena 0.");
            }
            else
            {
                Debug.LogError("GameInitializer: FireBaseAuth Prefab n�o atribu�do no Inspector! Atribua o prefab do FireBaseAuth.");
            }
        }
        else
        {
            Debug.Log("GameInitializer: FireBaseAuth j� existe na cena.");
        }
    }

    void Start()
    {
        Debug.Log("GameInitializer: Start chamado. Aguardando inicializa��o do Firebase...");
        StartCoroutine(WaitForFirebaseAndLoadScene());
    }

    private IEnumerator WaitForFirebaseAndLoadScene()
    {
        if (firebaseInitializer == null)
        {
            Debug.LogError("GameInitializer: FirebaseInitializer n�o encontrado. N�o � poss�vel prosseguir.");
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