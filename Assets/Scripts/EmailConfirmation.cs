using UnityEngine;
using TMPro;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Text.RegularExpressions;

public class EmailSender : MonoBehaviour
{
    public TMP_Text feedbackText;

    public void SendEmail(string userEmail, string token)
    {
        string smtpServer = "smtp.gmail.com";
        int smtpPort = 587;
        string fromEmail = "afonsoogncalvesmarques@gmail.com";
        // Use a senha de aplicativo gerada pelo Google (sem espaços)
        string fromPassword = "fqzbarsbgopnwphp";

        if (!IsValidEmail(userEmail))
        {
            feedbackText.text = "Email inválido!";
            feedbackText.color = Color.red;
            return;
        }

        string subject = "Confirmação de Conta";
        string body = $"Seu código de verificação é: {token}";

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("", fromEmail));
            message.To.Add(new MailboxAddress("", userEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using (var client = new SmtpClient())
            {
                client.LocalDomain = "localhost"; // ou substitua por "seu-dominio.com" se tiver um FQDN configurado
                client.Connect(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                client.Authenticate(fromEmail, fromPassword);
                client.Send(message);
                client.Disconnect(true);
            }

            feedbackText.text = "Email enviado com sucesso!";
            feedbackText.color = Color.green;
        }
        catch (System.Net.Sockets.SocketException)
        {
            feedbackText.text = "Erro de conexão com o servidor SMTP.";
            feedbackText.color = Color.red;
        }
        catch (MailKit.Security.AuthenticationException)
        {
            feedbackText.text = "Erro de autenticação. Verifique seu email e senha.";
            feedbackText.color = Color.red;
        }
        catch (Exception ex)
        {
            feedbackText.text = "Erro ao enviar email: " + ex.Message;
            feedbackText.color = Color.red;
        }
    }

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
}
