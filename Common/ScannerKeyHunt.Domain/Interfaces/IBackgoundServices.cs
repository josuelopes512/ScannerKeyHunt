namespace ScannerKeyHunt.Domain.Interfaces
{
    public interface IBackgoundServices : IDisposable
    {
        void Start();
        void Stop();
        bool IsActive();
    }
}
