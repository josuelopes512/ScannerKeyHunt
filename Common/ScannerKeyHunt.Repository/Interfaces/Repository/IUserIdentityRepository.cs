using Microsoft.AspNetCore.Identity;
using ScannerKeyHunt.Data.Entities;
using System.Security.Claims;

namespace ScannerKeyHunt.Repository.Interfaces.Repository
{
    public interface IUserIdentityRepository : IDisposable
    {
        User FindByUsername(string username);
        User FindByGuid(string uuid);
        User FindByEmail(string email);
        IdentityResult Update(User user);
        IdentityResult Delete(User user);
        bool CheckPassword(User username, string password);
        IList<string> GetRoles(User username);
        IdentityResult CreateUser(User username, string password);
        IdentityResult AddRoleToUser(User username, string role);
        IList<User> GetAll();
        bool RoleAdminExists();
        bool RoleUserExists();
        bool RoleExists(string roleName);
        void CreateAdminRole();
        void CreateUserRole();
        void CreateRole(string roleName);
        List<Claim> GetRolesClaims(User user);
        bool ConfirmEmail(User user, string token);
        string GenerateEmailConfirmationToken(User user);
        string GenerateTwoFactorToken(User user);
        void SignOut();
        void PasswordSignIn(User user, string password);
        bool TwoFactorSignIn(string code);
        bool ResetPassword(User user, string code, string password);
        string GeneratePasswordResetToken(User user);
        bool IsEmailConfirmed(User user);
    }
}
