using ScannerKeyHunt.Data.DTOs;
using ScannerKeyHunt.Data.Entities;
using System.Security.Claims;

namespace ScannerKeyHunt.Repository.Interfaces.Cache
{
    public interface IUserRepositoryCache : IDisposable
    {
        User GetByEmail(string email, bool useCache = true);
        User GetByUsername(string username, bool useCache = true);
        List<Claim> GetRolesByUser(User user, bool useCache = true);
        List<Claim> GetRolesByGuid(Guid guid, bool useCache = true);
        List<Claim> GetRolesByEmail(string email, bool useCache = true);
        List<Claim> GetRolesByUsername(string username, bool useCache = true);
        bool CheckPasswordByUser(User user, string password, bool useCache = true);
        bool CheckPasswordByGuid(Guid guid, string password, bool useCache = true);
        bool CheckPasswordByEmail(string email, string password, bool useCache = true);
        bool CheckPasswordByUsername(string username, string password, bool useCache = true);
        User GetByGuid(Guid guid, bool useCache = true);
        bool ConfirmEmail(User user, string token, bool useCache = true);
        string GenerateEmailConfirmationToken(User user);
        string GenerateTwoFactorToken(User user);
        void SignOut();
        void PasswordSignIn(User user, string password);
        bool TwoFactorSignIn(string code);
        User CreateUser(UserDTO userDTO, bool is2FA = false);
        bool Update(User user);
        bool Update(Guid guid);
        List<User> GetAll();
        bool Delete(Guid guid);
        bool Delete(User user);
        string GeneratePasswordResetToken(User user);
        bool IsEmailConfirmed(User user, bool useCache = true);
        bool ResetPassword(User user, string code, string password, bool useCache = true);
    }
}
