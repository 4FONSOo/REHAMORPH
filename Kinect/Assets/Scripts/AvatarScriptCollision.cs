using UnityEngine;

public class AvatarScriptCollision : MonoBehaviour
{
    public LevelManager levelManager;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entrei");

        // Verifica se o objeto colidido tem a tag "Cube"
        if (other.CompareTag("Cube"))
        {
            Debug.Log("Cubo");
            // Chama o método CubeTouched no LevelManager para destruir o cubo e atualizar o score
            if (levelManager != null)
            {
                Debug.Log("touch");
                levelManager.CubeTouched();
            }
            else
            {
                Debug.LogError("LevelManager não está atribuído no Inspector!");
            }
        }
    }
}