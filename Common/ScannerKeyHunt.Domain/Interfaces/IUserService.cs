using ScannerKeyHunt.Domain.ModelDTOs;
using ScannerKeyHunt.Domain.Models;
using System.Security.Claims;

namespace ScannerKeyHunt.Domain.Interfaces
{
    public interface IUserService : IDisposable
    {
        Guid GetByUsername(string username);
        Guid GetByEmail(string email);
        List<Guid> GetAllUsers();
        List<UserModelDTO> GetAll();
        Guid? Add(UserModelDTO userDTO);
        bool Delete(UserModelDTO userDTO);
        Guid? RegisterUser(UserModelDTO userDTO, string urlMethod);
        UserModelDTO GetUserByGuid(Guid guid);
        UserModelDTO GetUserByEmail(string email);
        UserModelDTO GetUserByUsername(string username);
        bool ConfirmEmail(string token, string email);
        void SendEmail(EmailModelDTO emailDTO, PostsViewModelDTO postsViewModel);
        JwtToken Login(LoginModelDTO userDTO);
        JwtToken ConfirmCode(string username, string code);
        JwtToken GeraToken(string email, List<Claim> authClaims = null);
        JwtToken RefreshToken(string accessToken, string refreshToken);
        (string Code, Guid? UUID) ResetPasswordSolicitation(string email, string urlMethod);
        void ResetPassword((string code, string email, string password) resetPasswordData);
        void RevokeByUser(string username);
    }
}
