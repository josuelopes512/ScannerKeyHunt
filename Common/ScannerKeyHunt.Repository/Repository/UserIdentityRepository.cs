using Microsoft.AspNetCore.Identity;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Repository.Interfaces.Repository;
using System.Security.Claims;

namespace ScannerKeyHunt.Repository.Repository
{
    public class UserIdentityRepository : IUserIdentityRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public UserIdentityRepository(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<User> signInManager
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public void Dispose()
        {
            _userManager.Dispose();
            _roleManager.Dispose();
            GC.SuppressFinalize(this);
        }

        public User FindByUsername(string username)
        {
            return _userManager.FindByNameAsync(username).Result;
        }

        public User FindByGuid(string uuid)
        {
            return _userManager.FindByIdAsync(uuid).Result;
        }

        public User FindByEmail(string email)
        {
            return _userManager.FindByEmailAsync(email).Result;
        }

        public IdentityResult Update(User user)
        {
            return _userManager.UpdateAsync(user).Result;
        }

        public IdentityResult Delete(User user)
        {
            return _userManager.DeleteAsync(user).Result;
        }

        public bool CheckPassword(User username, string password)
        {
            return _userManager.CheckPasswordAsync(username, password).Result;
        }

        public IList<string> GetRoles(User username)
        {
            return _userManager.GetRolesAsync(username).Result;
        }

        public IdentityResult CreateUser(User username, string password)
        {
            return _userManager.CreateAsync(username, password).Result;
        }

        public IdentityResult AddRoleToUser(User username, string role)
        {
            return _userManager.AddToRoleAsync(username, role).Result;
        }

        public IList<User> GetAll()
        {
            List<User> users = new List<User>();

            users.AddRange(_userManager.GetUsersInRoleAsync("Admin").Result);
            users.AddRange(_userManager.GetUsersInRoleAsync("User").Result);
            users.AddRange(_userManager.GetUsersInRoleAsync("Member").Result);
            users.AddRange(_userManager.GetUsersInRoleAsync("Service").Result);

            return users;
        }

        public bool RoleAdminExists()
        {
            return _roleManager.RoleExistsAsync("Admin").Result;
        }

        public bool RoleUserExists()
        {
            return _roleManager.RoleExistsAsync("User").Result;
        }

        public bool RoleExists(string roleName)
        {
            return _roleManager.RoleExistsAsync(roleName).Result;
        }

        public void CreateAdminRole()
        {
            _ = _roleManager.CreateAsync(new IdentityRole("Admin")).Result;
        }

        public void CreateUserRole()
        {
            _ = _roleManager.CreateAsync(new IdentityRole("User")).Result;
        }

        public void CreateRole(string roleName)
        {
            _ = _roleManager.CreateAsync(new IdentityRole(roleName)).Result;
        }

        public List<Claim> GetRolesClaims(User user)
        {
            List<string> claims = _userManager.GetRolesAsync(user).Result.ToList();

            List<Claim> authClaims = claims.Select(userRole => new Claim(ClaimTypes.Role, userRole)).ToList();

            return authClaims;
        }

        public bool ConfirmEmail(User user, string token)
        {
            var userdata = _userManager.ConfirmEmailAsync(user, token).Result;
            return userdata.Succeeded;
        }

        public string GenerateEmailConfirmationToken(User user)
        {
            return _userManager.GenerateEmailConfirmationTokenAsync(user).Result;
        }

        public string GenerateTwoFactorToken(User user)
        {
            return _userManager.GenerateTwoFactorTokenAsync(user, "Email").Result;
        }

        public async void SignOut()
        {
            await _signInManager.SignOutAsync();
        }

        public void PasswordSignIn(User user, string password)
        {
            var result = _signInManager.PasswordSignInAsync(user, password, false, true).Result;
        }

        public bool TwoFactorSignIn(string code)
        {
            var result = _signInManager.TwoFactorSignInAsync("Email", code, false, false).Result;
            return result.Succeeded;
        }

        public bool IsEmailConfirmed(User user)
        {
            return _userManager.IsEmailConfirmedAsync(user).Result;
        }

        public string GeneratePasswordResetToken(User user)
        {
            return _userManager.GeneratePasswordResetTokenAsync(user).Result;
        }

        public bool ResetPassword(User user, string code, string password)
        {
            try
            {
                var result = _userManager.ResetPasswordAsync(FindByGuid(user.Id), code, password).Result;
                return result.Succeeded;
            }
            catch (Exception)
            {

            }

            return false;
        }
    }
}
