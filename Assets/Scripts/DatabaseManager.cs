using System.IO;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static string dbPath; // Caminho global para acesso ao banco de dados
    private const string fileName = "game_data.db";

    void Awake()
    {
        // Caminho de origem: arquivo distribuído com o jogo (pasta StreamingAssets)
        string sourcePath = Path.Combine(Application.streamingAssetsPath, fileName);
        // Caminho de destino: onde o arquivo será copiado para permitir escrita
        string destinationPath = Path.Combine(Application.persistentDataPath, fileName);

        // Se o arquivo não existir no destino, copia-o da pasta StreamingAssets
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
            Debug.Log("Base de dados já existe em: " + destinationPath);
        }

        // Define o caminho global que será utilizado por todos os scripts
        dbPath = destinationPath;
    }
}
