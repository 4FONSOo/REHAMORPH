using UnityEngine;

public class AvatarScriptCollision : MonoBehaviour
{
    public LevelManager levelManager;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entrou no Trigger com: " + other.gameObject.name);

        if (other.CompareTag("Cube"))
        {
            Debug.Log("Colisão com Cube detectada!");

            if (levelManager != null)
            {
                levelManager.CubeTouched();
            }

            Destroy(other.gameObject);
            Debug.Log("Cubo destruído!");
        }
    }
}
