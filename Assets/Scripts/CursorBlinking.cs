using UnityEngine;
using TMPro;
using System.Collections;

public class BlinkingCursorMultiple : MonoBehaviour
{
    [System.Serializable]
    public class InputFieldData
    {
        public TMP_InputField inputField; // Referência ao campo de entrada
        public TextMeshProUGUI placeholder; // Placeholder que será usado como cursor
        public string defaultText; // Texto padrão do placeholder
        [HideInInspector] public bool isBlinking = false; // Controle do cursor piscante
    }

    public InputFieldData[] inputFields; // Lista de todos os campos

    void Start()
    {
        // Configura os placeholders iniciais e desativa os cursores padrões
        foreach (var field in inputFields)
        {
            field.placeholder.text = field.defaultText;
            field.inputField.caretColor = new Color(0, 0, 0, 0); // Torna o cursor nativo invisível
        }
    }

    void Update()
    {
        foreach (var field in inputFields)
        {
            if (field.inputField.isFocused) // Se o campo estiver selecionado (ativo)
            {
                if (field.inputField.text == "") // Se estiver vazio, inicia o cursor piscante
                {
                    if (!field.isBlinking)
                    {
                        field.isBlinking = true;
                        StartCoroutine(CursorBlink(field));
                    }
                }
                else
                {
                    field.isBlinking = false;
                    field.placeholder.text = ""; // Esconde o placeholder se houver texto digitado
                }
            }
            else // Se o usuário clicar fora do campo de entrada
            {
                field.isBlinking = false;

                // Se não houver texto, volta a exibir o placeholder original
                if (field.inputField.text == "")
                {
                    field.placeholder.text = field.defaultText;
                }
            }
        }
    }

    IEnumerator CursorBlink(InputFieldData field)
    {
        while (field.isBlinking)
        {
            field.placeholder.text = "";  // Exibe o cursor
            yield return new WaitForSeconds(0.5f); // Aguarda 0.5 segundos

            field.placeholder.text = "";  // Esconde o cursor
            yield return new WaitForSeconds(0.5f); // Aguarda mais 0.5 segundos
        }
    }
}
