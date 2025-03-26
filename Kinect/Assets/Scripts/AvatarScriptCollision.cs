using UnityEngine;

public class AvatarScriptCollision : MonoBehaviour
{
    public LevelManager levelManager;
    public Level0Manager level0Manager; // Referência para o nível 0

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Contato com: " + other.gameObject.name);

        // Se colidir com um cubo normal do jogo
        if (other.CompareTag("Cube"))
        {
            Debug.Log("Cubo Normal Detectado");
            if (levelManager != null)
            {
                levelManager.CubeTouched();
                Debug.Log("Pontuação atualizada e cubo destruído.");
            }
            else
            {
                Debug.LogError("LevelManager não atribuído no Inspector!");
            }
        }

        // Se colidir com cubos invisíveis do Nível 0
        if (other.CompareTag("TriggerCube"))
        {
            Debug.Log("Cubo Invisível Detectado - Movimento reconhecido");
            if (level0Manager != null)
            {
                if (other.name.Contains("ArmLeft"))
                {
                    Debug.Log("Braço Esquerdo reconhecido!");
                    level0Manager.MovementRecognized("Braço Esquerdo");
                }
                else if (other.name.Contains("ArmRight"))
                {
                    Debug.Log("Braço Direito reconhecido!");
                    level0Manager.MovementRecognized("Braço Direito");
                }
                else if (other.name.Contains("LegLeft"))
                {
                    Debug.Log("Perna Esquerda reconhecida!");
                    level0Manager.MovementRecognized("Perna Esquerda");
                }
                else if (other.name.Contains("LegRight"))
                {
                    Debug.Log("Perna Direita reconhecida!");
                    level0Manager.MovementRecognized("Perna Direita");
                }
            }
            else
            {
                Debug.LogError("Level0Manager não atribuído no Inspector!");
            }
            other.gameObject.SetActive(false); // Desativa o cubo após a colisão
        }
    }
}
