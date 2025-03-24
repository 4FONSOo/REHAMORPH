using UnityEngine;
using UnityEngine.UI;

public class ClickToDestroy : MonoBehaviour
{
    public Text scoreText;      // Referência ao texto do score na UI
    public GameObject cubePrefab;  // Prefab do cubo
    public Transform spawnArea; // Objeto que define a área onde os cubos nascem
    public int maxCubes = 5;     // Máximo de cubos antes de terminar

    private int score = 0;
    private int cubesSpawned = 0;

    void Start()
    {
        UpdateScoreUI();
        SpawnCube(); // Gera o primeiro cubo
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // Se clicar com o botão esquerdo do rato
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Cube"))
                {
                    Destroy(hit.collider.gameObject); // Destroi o cubo clicado
                    score++; // Aumenta o score
                    UpdateScoreUI();
                    SpawnCube(); // Gera um novo cubo
                }
            }
        }
    }

    void SpawnCube()
    {
        if (cubesSpawned >= maxCubes) return; // Limite de cubos atingido

        // Define uma posição aleatória dentro da área de spawn
        Vector3 spawnPosition = new Vector3(
            spawnArea.position.x + Random.Range(-2f, 2f),
            spawnArea.position.y,
            spawnArea.position.z
        );

        Instantiate(cubePrefab, spawnPosition, Quaternion.identity);
        cubesSpawned++;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }
}
