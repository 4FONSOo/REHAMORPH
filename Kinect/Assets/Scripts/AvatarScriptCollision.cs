using UnityEngine;

public class AvatarScriptCollision : MonoBehaviour
{
    public LevelManager levelManager;
    public Level0Manager level0Manager; // Referência para o gerente do nível 0

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entrei em contato com: " + other.gameObject.name);

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
                Debug.LogError("LevelManager não está atribuído no Inspector!");
            }
        }

        // Se colidir com um cubo invisível do Nível 0
        if (other.CompareTag("TriggerCube"))
        {
            Debug.Log("Cubo Invisível Detectado - Movimento reconhecido");
            if (level0Manager != null)
            {
                level0Manager.MovementRecognized();
            }
            else
            {
                Debug.LogError("Level0Manager não está atribuído no Inspector!");
            }
            other.gameObject.SetActive(false); // Desativa o cubo após a colisão
        }
    }
}
