using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Level0Manager : MonoBehaviour
{
    [Header("Configurações dos Cubos Invisíveis")]
    public GameObject[] triggerCubes; // Cubos invisíveis para cada membro
    public string[] instructionTexts = {
        "Levante o braço esquerdo",
        "Levante o braço direito",
        "Mexa a perna esquerda",
        "Mexa a perna direita"
    };

    [Header("Referências do Avatar")]
    public Transform avatar;

    [Header("UI")]
    public Text instructionText;
    public Text feedbackText;

    private int currentStep = 0;

    void Start()
    {
        feedbackText.gameObject.SetActive(false);
        UpdateInstruction();
        ActivateTriggerCube();
    }

    void UpdateInstruction()
    {
        if (instructionText != null && currentStep < instructionTexts.Length)
        {
            instructionText.text = instructionTexts[currentStep];
        }
    }

    void ActivateTriggerCube()
    {
        // Desativa todos os cubos primeiro
        foreach (var cube in triggerCubes)
        {
            cube.SetActive(false);
        }

        // Ativa apenas o cubo do movimento atual
        if (currentStep < triggerCubes.Length)
        {
            triggerCubes[currentStep].SetActive(true);
        }
    }

    public void MovementRecognized()
    {
        StartCoroutine(ShowFeedbackAndNextStep());
    }

    IEnumerator ShowFeedbackAndNextStep()
    {
        feedbackText.text = "Muito bem!";
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f); // Mostra a mensagem por 1 segundo
        feedbackText.gameObject.SetActive(false);

        currentStep++;
        if (currentStep < instructionTexts.Length)
        {
            UpdateInstruction();
            ActivateTriggerCube();
        }
        else
        {
            instructionText.text = "Nível concluído!";
        }
    }
}
