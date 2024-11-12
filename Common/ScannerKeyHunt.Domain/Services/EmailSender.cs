using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using ScannerKeyHunt.Domain.Interfaces;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ScannerKeyHunt.Domain.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration configuration)
        {
            _config = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (SmtpClient client = new SmtpClient())
            {
                string emailhost = _config.GetSection("UserSettings")["SMTPMail"];
                string password = _config.GetSection("UserSettings")["SMTPPassword"];

                string host = _config.GetSection("UserSettings")["SMTPHost"];
                int port = int.Parse(_config.GetSection("UserSettings")["SMTPPort"]);

                BodyBuilder builder = new BodyBuilder()
                {
                    HtmlBody = htmlMessage
                };

                MimeMessage emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress("Message Dispatch", emailhost));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = builder.ToMessageBody();

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                client.Connect(host, port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(emailhost, password);
                client.Send(emailMessage);
                client.Disconnect(true);
                client.Dispose();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
