using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabNavigation : MonoBehaviour
{
    // Array de campos de entrada (InputFields ou botões) que serão navegados com a tecla Tab
    public Selectable[] inputFields;

    void Update()
    {
        // Verifica se a tecla Tab foi pressionada
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Obtém o objeto atualmente selecionado pelo sistema de eventos da Unity
            Selectable current = EventSystem.current.currentSelectedGameObject?.GetComponent<Selectable>();

            if (current != null) // Se houver um campo atualmente selecionado
            {
                // Obtém o índice do campo atual no array
                int index = System.Array.IndexOf(inputFields, current);

                if (index >= 0) // Se o campo atual estiver na lista
                {
                    // Calcula o índice do próximo campo (se chegar ao último, volta ao primeiro)
                    int nextIndex = (index + 1) % inputFields.Length;

                    // Seleciona o próximo campo na sequência
                    inputFields[nextIndex].Select();
                }
            }
        }
    }
}
