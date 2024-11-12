using MimeKit;

namespace ScannerKeyHunt.Domain.Interfaces
{
    public interface IEmailService : IDisposable
    {
        void SendEmail(MimeMessage mailObj);
        MimeMessage GetEmailMessage(List<MailboxAddress> mailboxAddresses, string subject, string emailBody);
        string GetEmailBody(string titulo, string descricao, string imageUrl);
    }
}
