using UnityEngine;
using TMPro;
using System.Collections;

public class BlinkingCursorMultiple : MonoBehaviour
{
    [System.Serializable]
    public class InputFieldData
    {
        public TMP_InputField inputField; // Refer�ncia ao campo de entrada
        public TextMeshProUGUI placeholder; // Placeholder que ser� usado como cursor
        public string defaultText; // Texto padr�o do placeholder
        [HideInInspector] public bool isBlinking = false; // Controle do cursor piscante
    }

    public InputFieldData[] inputFields; // Lista de todos os campos

    void Start()
    {
        // Configura os placeholders iniciais e desativa os cursores padr�es
        foreach (var field in inputFields)
        {
            field.placeholder.text = field.defaultText;
            field.inputField.caretColor = new Color(0, 0, 0, 0); // Torna o cursor nativo invis�vel
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
            else // Se o usu�rio clicar fora do campo de entrada
            {
                field.isBlinking = false;

                // Se n�o houver texto, volta a exibir o placeholder original
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
