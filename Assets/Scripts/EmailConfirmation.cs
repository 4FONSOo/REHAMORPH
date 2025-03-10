using UnityEngine;
using TMPro;
using MailKit.Security;
using MimeKit;
using System;
using System.Net.Mail; // Para System.Net.Mail.SmtpClient
using MailKit.Net.Smtp;


public class EmailSender : MonoBehaviour
{
    public TMP_Text feedbackText;

    public void SendEmail(string userEmail, string token)
    {
        // Configurações do SMTP – ajuste conforme seu provedor
        string smtpServer = "smtp-mail.outlook.com";
        int smtpPort = 587;
        string fromEmail = "a22208265@alunos.ulht.pt";
        string fromPassword = "AgM2004uLP02";

        string subject = "Confirmação de Conta";
        string body = "Seu código de verificação é: " + token;

        try
        {
            // Monta a mensagem usando MimeMessage
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Seu Jogo", fromEmail));
            message.To.Add(new MailboxAddress("", userEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = body
            };

            // Envia o email utilizando MailKit
            using (var client = new SmtpClient())
            {
                client.Connect(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                client.Authenticate(fromEmail, fromPassword);
                client.Send(message);
                client.Disconnect(true);
            }

            feedbackText.text = "Email enviado!";
            feedbackText.color = Color.green;
        }
        catch (Exception ex)
        {
            feedbackText.text = "Erro ao enviar email: " + ex.Message;
            feedbackText.color = Color.red;
        }
    }
}
