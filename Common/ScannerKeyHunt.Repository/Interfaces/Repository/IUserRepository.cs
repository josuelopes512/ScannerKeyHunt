using ScannerKeyHunt.Data.DTOs;
using ScannerKeyHunt.Data.Entities;
using System.Security.Claims;

namespace ScannerKeyHunt.Repository.Interfaces.Repository
{
    public interface IUserRepository : IDisposable
    {
        User GetByEmail(string email);
        User GetByUsername(string username);
        List<Claim> GetRolesByUser(User user);
        List<Claim> GetRolesByGuid(Guid guid);
        List<Claim> GetRolesByEmail(string email);
        List<Claim> GetRolesByUsername(string username);
        bool CheckPasswordByUser(User user, string password);
        bool CheckPasswordByGuid(Guid guid, string password);
        bool CheckPasswordByEmail(string email, string password);
        bool CheckPasswordByUsername(string username, string password);
        bool ConfirmEmail(User user, string token);
        string GenerateEmailConfirmationToken(User user);
        string GenerateTwoFactorToken(User user);
        void SignOut();
        void PasswordSignIn(User user, string password);
        bool TwoFactorSignIn(string code);
        User CreateUser(UserDTO userDTO, bool is2FA = false);
        User GetByGuid(Guid guid);
        bool Update(User entity);
        bool Update(Guid guid);
        List<User> GetAll();
        bool Delete(Guid guid);
        bool Delete(User entity);
        bool ResetPassword(User user, string code, string password);
        string GeneratePasswordResetToken(User user);
        bool IsEmailConfirmed(User user);
    }
}
