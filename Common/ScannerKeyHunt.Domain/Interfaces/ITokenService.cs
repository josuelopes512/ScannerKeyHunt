using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Domain.Models;
using System.Security.Claims;

namespace ScannerKeyHunt.Domain.Interfaces
{
    public interface ITokenService : IDisposable
    {
        JwtToken GenerateJwtToken(User user, List<Claim> authClaims);

        JwtToken RefreshToken(string accessToken, string refreshToken);

        void RevokeByUser(string username);

        void DeleteTokenAuthByUserId(Guid userId);

        TokenAuth CreateTokenAuth(Guid userId);
    }
}
