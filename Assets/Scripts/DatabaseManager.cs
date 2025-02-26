using System.IO;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static string dbPath; // Caminho global para acesso ao banco de dados
    private const string fileName = "game_data.db";

    void Awake()
    {
        // Caminho de origem: arquivo distribu�do com o jogo (pasta StreamingAssets)
        string sourcePath = Path.Combine(Application.streamingAssetsPath, fileName);
        // Caminho de destino: onde o arquivo ser� copiado para permitir escrita
        string destinationPath = Path.Combine(Application.persistentDataPath, fileName);

        // Se o arquivo n�o existir no destino, copia-o da pasta StreamingAssets
        if (!File.Exists(destinationPath))
        {
            try
            {
                File.Copy(sourcePath, destinationPath);
                Debug.Log("Base de dados copiada para: " + destinationPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Erro ao copiar a base de dados: " + e.Message);
            }
        }
        else
        {
            Debug.Log("Base de dados j� existe em: " + destinationPath);
        }

        // Define o caminho global que ser� utilizado por todos os scripts
        dbPath = destinationPath;
    }
}
