using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabNavigation : MonoBehaviour
{
    // Array de campos de entrada (InputFields ou bot�es) que ser�o navegados com a tecla Tab
    public Selectable[] inputFields;

    void Update()
    {
        // Verifica se a tecla Tab foi pressionada
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Obt�m o objeto atualmente selecionado pelo sistema de eventos da Unity
            Selectable current = EventSystem.current.currentSelectedGameObject?.GetComponent<Selectable>();

            if (current != null) // Se houver um campo atualmente selecionado
            {
                // Obt�m o �ndice do campo atual no array
                int index = System.Array.IndexOf(inputFields, current);

                if (index >= 0) // Se o campo atual estiver na lista
                {
                    // Calcula o �ndice do pr�ximo campo (se chegar ao �ltimo, volta ao primeiro)
                    int nextIndex = (index + 1) % inputFields.Length;

                    // Seleciona o pr�ximo campo na sequ�ncia
                    inputFields[nextIndex].Select();
                }
            }
        }
    }
}
