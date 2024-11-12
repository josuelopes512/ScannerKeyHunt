using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using ScannerKeyHunt.Domain.Interfaces;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ScannerKeyHunt.Domain.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void SendEmail(MimeMessage mailObj)
        {
            using (SmtpClient client = new SmtpClient())
            {
                string email = _config.GetSection("UserSettings")["SMTPMail"];
                string password = _config.GetSection("UserSettings")["SMTPPassword"];

                string host = _config.GetSection("UserSettings")["SMTPHost"];
                int port = int.Parse(_config.GetSection("UserSettings")["SMTPPort"]);

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                client.Connect(host, port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(email, password);
                client.Send(mailObj);
                client.Disconnect(true);
                client.Dispose();
            }
        }

        public MimeMessage GetEmailMessage(List<MailboxAddress> mailboxAddresses, string subject, string emailBody)
        {
            BodyBuilder builder = new BodyBuilder()
            {
                HtmlBody = emailBody
            };

            MimeMessage message = new MimeMessage()
            {
                Subject = subject,
                Body = builder.ToMessageBody()
            };

            string email = _config.GetSection("UserSettings")["SMTPMail"];

            message.From.Add(new MailboxAddress("Josué Lopes", email));

            message.To.AddRange(mailboxAddresses);

            //message = GetEmailFrom(message);
            //message = GetEmailsToBeSend(message, emailDTO);
            //message = FillEmailContent(message, subject, emailBody);

            return message;
        }

        public string GetEmailBody(string titulo, string descricao, string imageUrl)
        {
            string body = $"<h1>{titulo}</h1>";  //"<h1>" + postsViewModel.Titulo + "</h1>";
            body += $"{descricao}\r\n"; //$"Descrição: " + postsViewModel.Descricao + "\r\n";

            if (!string.IsNullOrEmpty(imageUrl))
                body += $"Imagem: {imageUrl}";  //$"Imagem: " + postsViewModel.ImagemUrl;

            return body;
        }

        private MimeMessage FillEmailContent(MimeMessage message, string subject, string body)
        {
            message.Subject = subject;

            BodyBuilder builder = new BodyBuilder();
            builder.HtmlBody = body;
            message.Body = builder.ToMessageBody();

            return message;
        }

        private MimeMessage GetEmailFrom(MimeMessage message)
        {
            message.From.Add(new MailboxAddress("Josué Lopes", "josue.s.lopes.512@gmail.com"));

            return message;
        }

        private MimeMessage GetEmailsToBeSend(MimeMessage message, List<MailboxAddress> mailboxAddresses)
        {
            message.To.AddRange(mailboxAddresses);

            return message;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
