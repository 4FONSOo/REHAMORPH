using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EmailSender : MonoBehaviour
{
    // URL da API do Node.js
    private string apiUrl = "http://localhost:3000/send-confirmation";

    public void SendConfirmationEmail(string recipientEmail, string token)
    {
        if (string.IsNullOrEmpty(recipientEmail))
        {
            Debug.LogError("Erro: O e-mail do destinatário está vazio ou nulo.");
            return;
        }

        StartCoroutine(SendEmailCoroutine(recipientEmail, token));
    }

    private IEnumerator SendEmailCoroutine(string recipientEmail, string token)
    {
        // Cria o JSON com os dados necessários
        string jsonData = "{\"email\":\"" + recipientEmail + "\", \"token\":\"" + token + "\"}";

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("E-mail enviado com sucesso!");
            }
            else
            {
                Debug.LogError("Erro ao enviar e-mail: " + request.error);
            }
        }
    }
}
