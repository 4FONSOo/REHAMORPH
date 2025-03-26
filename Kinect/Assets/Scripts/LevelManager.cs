using UnityEngine;
using UnityEngine.UI;
using System;

public class LevelManager : MonoBehaviour
{
    [Header("Configurações do Cubo")]
    public GameObject cubePrefab;
    public Vector3[] spawnOffsets;  // Posições relativas ao avatar

    [Header("Referências do Avatar")]
    public Transform avatar;

    [Header("UI")]
    public Text scoreText;
    public Text timerText;
    public Text levelCompleteText;

    private int score = 0;
    private float timer = 0f;
    private GameObject currentCube;
    private int currentCubeIndex = 0;

    void Start()
    {
        if (levelCompleteText != null)
            levelCompleteText.gameObject.SetActive(false);
        UpdateScoreUI();
        SpawnCube();
    }

    void Update()
    {
        if (score < 5)
        {
            timer += Time.deltaTime;
            if (timerText != null)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(timer);
                timerText.text = string.Format("Tempo: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }
        }
    }

    public void CubeTouched()
    {
        if (currentCube != null)
        {
            Destroy(currentCube);
            currentCube = null;
        }

        score++;
        UpdateScoreUI();

        if (score >= 5)
        {
            LevelComplete();
        }
        else
        {
            SpawnCube();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Pontuação: " + score;
    }

    void SpawnCube()
    {
        if (avatar == null || spawnOffsets.Length == 0)
        {
            Debug.LogError("Avatar ou posições de spawn não configurados corretamente.");
            return;
        }

        if (currentCubeIndex >= spawnOffsets.Length)
        {
            currentCubeIndex = 0;
        }

        Vector3 spawnPosition = avatar.position + spawnOffsets[currentCubeIndex];
        currentCubeIndex++;

        currentCube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);
        currentCube.tag = "Cube";

        Collider cubeCollider = currentCube.GetComponent<Collider>();
        if (cubeCollider == null)
        {
            cubeCollider = currentCube.AddComponent<BoxCollider>();
        }
        cubeCollider.isTrigger = true;  // Garante que a colisão funcione corretamente

        Rigidbody rb = currentCube.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = currentCube.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    void LevelComplete()
    {
        if (levelCompleteText != null)
        {
            levelCompleteText.text = "Nível Concluído!";
            levelCompleteText.gameObject.SetActive(true);
        }
    }
}