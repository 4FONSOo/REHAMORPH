using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScritpt : MonoBehaviour
{
    private static Stack<int> sceneHistory = new Stack<int>(); // Pilha para armazenar cenas visitadas

    void OnEnable()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Evita adicionar a mesma cena consecutivamente no histórico
        if (sceneHistory.Count == 0 || sceneHistory.Peek() != currentSceneIndex)
        {
            sceneHistory.Push(currentSceneIndex);
        }
    }

    private void LoadSceneAndSave(int sceneIndex)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Salva a cena atual antes de mudar
        if (sceneHistory.Count == 0 || sceneHistory.Peek() != currentSceneIndex)
        {
            sceneHistory.Push(currentSceneIndex);
        }

        SceneManager.LoadScene(sceneIndex);
    }

    public void Nivel1()
    {
        LoadSceneAndSave(2);
    }

    public void Nivel0()
    {
        LoadSceneAndSave(1);
    }

    public void Menu()
    {
        LoadSceneAndSave(0);
    }

    public void GoBack()
    {
        if (sceneHistory.Count > 1) // Sempre mantém pelo menos uma cena na pilha
        {
            sceneHistory.Pop(); // Remove a cena atual
            int previousSceneIndex = sceneHistory.Peek(); // Obtém a anterior

            SceneManager.LoadScene(previousSceneIndex);
        }
        else
        {
            Debug.Log("Nenhuma cena anterior no histórico!");
        }
    }

    public void QuitGame()
    {
        Debug.Log("QUIT");
        Application.Quit();
    }

}

