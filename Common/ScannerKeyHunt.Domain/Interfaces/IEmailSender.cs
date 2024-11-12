namespace ScannerKeyHunt.Domain.Interfaces
{
    public interface IEmailSender : IDisposable
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
