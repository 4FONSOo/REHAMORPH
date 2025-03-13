using UnityEngine;
using System.Net;
using System.Net.Mail;
using System.Collections;

public class EmailSender : MonoBehaviour
{
    private string smtpServer = "sandbox.smtp.mailtrap.io";
    private int smtpPort = 587;
    private string smtpUsername = "4b9ee014ca615f";
    private string smtpPassword = "a3ebdfd21db39c";

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
        yield return null;

        try
        {
            using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("no-reply@teujogo.com", "Meu Jogo");
                mail.To.Add(recipientEmail);
                mail.Subject = "Confirmação de Conta";

                string confirmationLink = "https://teuservidor.com/confirmar-conta?token=" + token;
                mail.Body = "Clica no link abaixo para confirmar a tua conta:\n\n" + confirmationLink;

                client.Send(mail);
                Debug.Log("✅ E-mail enviado com sucesso para " + recipientEmail);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ Erro ao enviar o e-mail: " + ex.Message);
        }
    }
}
