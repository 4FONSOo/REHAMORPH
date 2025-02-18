using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Mant�m o objeto ao mudar de cena
        }
        else
        {
            Destroy(gameObject); // Evita duplica��o de m�sica
        }
    }
}
