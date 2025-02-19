using UnityEngine;
using UnityEngine.Audio;

public class BackgroundMusic : MonoBehaviour
{
    // Referência ao AudioMixer para controle do volume
    public AudioMixer audioMixer;

    // Instância estática para garantir que apenas uma instância do objeto exista
    private static BackgroundMusic instance;

    void Awake()
    {
        // Verifica se já existe uma instância deste objeto
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Mantém a música ao mudar de cena
            LoadVolume(); // 🔹 Aplica o volume salvo assim que o jogo inicia
        }
        else
        {
            Destroy(gameObject); // Evita duplicação de música
        }
    }

    // Método para carregar o volume salvo nas configurações
    private void LoadVolume()
    {
        // Verifica se há um volume salvo no PlayerPrefs
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("musicVolume"); // Obtém o volume salvo

            // Aplica o volume ao AudioMixer, garantindo que nunca seja menor que um valor mínimo
            audioMixer.SetFloat("music", Mathf.Log10(Mathf.Max(savedVolume, 0.0001f)) * 20);

            Debug.Log("Volume carregado: " + savedVolume); // Exibe o volume carregado no console
        }
    }
}