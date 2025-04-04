using System.Diagnostics;
using UnityEngine;

public class LaunchGame : MonoBehaviour
{
    public string exePath = "C://Users//Utilizador//REHAMORPH-exe//Kinect.exe";
    public void StartGame()
    {
        Process process = Process.Start(exePath);
        process.WaitForExit(); // Espera até o jogo fechar
        Application.Quit();
    }

}
