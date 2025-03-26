using UnityEngine;
using UnityEngine.UI;

public class Level0Manager : MonoBehaviour
{
    [Header("Referência ao Avatar")]
    public Transform avatar;  // O Avatar está nas coordenadas (2,0,0)

    [Header("Cubos Invisíveis")]
    public GameObject cubePrefab;
    private GameObject armLeftCube, armRightCube, legLeftCube, legRightCube;

    [Header("UI")]
    public Text messageText;

    private int currentStep = 0;
    private string[] movementSteps = { "Levante o braço esquerdo", "Levante o braço direito", "Mova-se para a esquerda", "Mova-se para a direita" };

    void Start()
    {
        if (messageText != null)
        {
            messageText.text = movementSteps[currentStep];
        }

        // Criar os 4 cubos invisíveis nas novas posições corretas
        CreateTriggerCubes();
    }

    void CreateTriggerCubes()
    {
        // Definir posições fixas conforme especificado
        Vector3 armLeftPos = new Vector3(1.042f, 1.82f, 0f);  // Braço esquerdo (acima)
        Vector3 armRightPos = new Vector3(2.88f, 1.962f, 0f); // Braço direito (acima)
        Vector3 legLeftPos = new Vector3(0.901f, 0.4f, 0f);   // Perna esquerda (lateral)
        Vector3 legRightPos = new Vector3(3.08f, 0.4f, 0f);  // Perna direita (lateral)

        // Criar cubos invisíveis nas novas posições
        armLeftCube = Instantiate(cubePrefab, armLeftPos, Quaternion.identity);
        armRightCube = Instantiate(cubePrefab, armRightPos, Quaternion.identity);
        legLeftCube = Instantiate(cubePrefab, legLeftPos, Quaternion.identity);
        legRightCube = Instantiate(cubePrefab, legRightPos, Quaternion.identity);

        // Configurar os cubos
        SetupTriggerCube(armLeftCube, "TriggerCube_ArmLeft");
        SetupTriggerCube(armRightCube, "TriggerCube_ArmRight");
        SetupTriggerCube(legLeftCube, "TriggerCube_LegLeft");
        SetupTriggerCube(legRightCube, "TriggerCube_LegRight");

        // Apenas o primeiro cubo fica ativo no início
        armLeftCube.SetActive(true);
        armRightCube.SetActive(false);
        legLeftCube.SetActive(false);
        legRightCube.SetActive(false);
    }

    void SetupTriggerCube(GameObject cube, string name)
    {
        cube.name = name;
        cube.tag = "TriggerCube";

        // Tornar invisível
        /*Renderer cubeRenderer = cube.GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            cubeRenderer.enabled = false; // Oculta o visual do cubo
        }*/

        // Garantir que não caia nem se mova
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = cube.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true; // Mantém o cubo fixo no espaço

        // Adicionar um BoxCollider se não houver
        BoxCollider collider = cube.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = cube.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true;
    }

public void MovementRecognized(string movement)
{
    Debug.Log(movement + " reconhecido!");

    if (messageText != null)
    {
        messageText.text = "Muito bem!";
    }

    // Avança para o próximo passo
    currentStep++;

    // Ativar o próximo cubo e desativar o atual
    if (currentStep == 1)
    {
        armLeftCube.SetActive(false);
        armRightCube.SetActive(true);
        messageText.text = movementSteps[currentStep]; // Atualiza a mensagem
    }
    else if (currentStep == 2)
    {
        armRightCube.SetActive(false);
        legLeftCube.SetActive(true);
        messageText.text = movementSteps[currentStep];
    }
    else if (currentStep == 3)
    {
        legLeftCube.SetActive(false);
        legRightCube.SetActive(true);
        messageText.text = movementSteps[currentStep];
    }
    else if (currentStep >= 4) // Quando termina todos os passos
    {
        legRightCube.SetActive(false);
        messageText.text = "Treino Concluído!";
    }
}

}
