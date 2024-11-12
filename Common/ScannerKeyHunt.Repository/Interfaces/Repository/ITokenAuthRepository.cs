using ScannerKeyHunt.Data.Entities;

namespace ScannerKeyHunt.Repository.Interfaces.Repository
{
    public interface ITokenAuthRepository : IBaseRepository<TokenAuth>
    {
        TokenAuth GetTokenAuthByUserId(Guid userId);
    }
}
