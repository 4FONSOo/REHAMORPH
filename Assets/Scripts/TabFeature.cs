using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabNavigation : MonoBehaviour
{
    // Array de campos de entrada (InputFields ou botões) que serão navegados com a tecla Tab
    // Ordem esperada na Cena de Registro: nome, peso, idade, altura, email, password, role
    // Ordem esperada na Cena de Login: email, password
    public Selectable[] inputFields;

    void Start()
    {
        // Verificar se o array inputFields está preenchido
        if (inputFields == null || inputFields.Length == 0)
        {
            Debug.LogError("Array inputFields está vazio! Atribua os campos de entrada no Inspector.");
            return;
        }

        // Verificar se há um EventSystem na cena
        if (EventSystem.current == null)
        {
            Debug.LogError("EventSystem não encontrado na cena! Adicione um EventSystem à cena.");
            return;
        }

        // Logar os campos atribuídos
        for (int i = 0; i < inputFields.Length; i++)
        {
            if (inputFields[i] == null)
            {
                Debug.LogError($"Campo inputFields[{i}] está nulo! Verifique o Inspector.");
            }
            else
            {
                Debug.Log($"Campo inputFields[{i}]: {inputFields[i].name}");
            }
        }

        // Selecionar o primeiro campo automaticamente ao iniciar a cena
        if (inputFields.Length > 0 && inputFields[0] != null)
        {
            inputFields[0].Select();
            Debug.Log("Primeiro campo selecionado automaticamente: " + inputFields[0].name);
        }
    }

    void Update()
    {
        // Verifica se a tecla Tab foi pressionada
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Tecla Tab pressionada.");

            // Obtém o objeto atualmente selecionado pelo sistema de eventos da Unity
            Selectable current = EventSystem.current.currentSelectedGameObject?.GetComponent<Selectable>();

            // Se nenhum campo estiver selecionado, selecione o primeiro
            if (current == null)
            {
                Debug.LogWarning("Nenhum campo selecionado pelo EventSystem.");
                if (inputFields.Length > 0 && inputFields[0] != null)
                {
                    inputFields[0].Select();
                    Debug.Log("Selecionando o primeiro campo: " + inputFields[0].name);
                }
                return;
            }

            Debug.Log("Campo atualmente selecionado: " + current.name);

            // Obtém o índice do campo atual no array
            int index = System.Array.IndexOf(inputFields, current);

            if (index >= 0) // Se o campo atual estiver na lista
            {
                // Verificar se Shift+Tab foi pressionado para navegar para trás
                bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                int nextIndex;

                if (isShiftPressed)
                {
                    // Navegar para o campo anterior (se for o primeiro, vai para o último)
                    nextIndex = (index - 1 + inputFields.Length) % inputFields.Length;
                    Debug.Log("Navegando para o campo anterior: " + inputFields[nextIndex].name);
                }
                else
                {
                    // Navegar para o próximo campo (se for o último, volta ao primeiro)
                    nextIndex = (index + 1) % inputFields.Length;
                    Debug.Log("Navegando para o próximo campo: " + inputFields[nextIndex].name);
                }

                // Seleciona o próximo ou anterior campo na sequência
                inputFields[nextIndex].Select();
            }
            else
            {
                Debug.LogWarning("Campo atualmente selecionado não está no array inputFields: " + current.name);
            }
        }
    }
}