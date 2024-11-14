using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Data.Helpers;
using ScannerKeyHunt.Domain.Interfaces;
using ScannerKeyHunt.Repository.Interfaces;
using ScannerKeyHunt.Utils;

namespace ScannerKeyHunt.Domain.Services
{
    public class SeedUserRoleInitial : ISeedUserRoleInitial
    {
        private readonly ILogger<SeedUserRoleInitial> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        private readonly IConfigurationSection _section;

        public SeedUserRoleInitial(
            ILogger<SeedUserRoleInitial> logger,
            IUnitOfWork unitOfWork,
            IConfiguration config,
            IEmailSender emailSender
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _config = config;
            _emailSender = emailSender;
            _section = _config.GetSection("CredencialsUser");
        }

        public void SeedRoles()
        {
            List<string> roles = new List<string> { "Admin", "Member", "User", "Service" };

            roles
                .Where(role => !_unitOfWork.UserIdentityRepository.RoleExists(role))
                .ToList()
                .ForEach(role =>
                {
                    try
                    {
                        _unitOfWork.UserIdentityRepository.CreateRole(role);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error to Seed Role: {role}");
                    }
                });

            _unitOfWork.Save();
        }

        private void CreateUser(string role)
        {
            try
            {
                ScannerKeyHunt.Data.Entities.User user = Activator.CreateInstance<ScannerKeyHunt.Data.Entities.User>();

                user.UserName = _section[$"User{role}"].Split('@').First().ToLower();
                user.Email = _section[$"User{role}"].ToLower();
                user.NormalizedUserName = _section[$"User{role}"].Split('@').First().ToUpper();
                user.NormalizedEmail = _section[$"User{role}"].ToUpper();
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                user.TwoFactorEnabled = role.Equals("Admin");

                string password = Util.GerarSenhaAleatoria();

                IdentityResult result = _unitOfWork.UserIdentityRepository.CreateUser(user, password);

                if (result.Succeeded)
                {
                    string htmlMessage = string.Format($"Email: {0} - Username: {1} - Password: {2}", user.Email, user.UserName, password);

                    _unitOfWork.UserIdentityRepository.AddRoleToUser(user, role);
                    _unitOfWork.Save();

                    _emailSender.SendEmailAsync(user.Email, "Criação de Conta", htmlMessage).Wait();
                }
                else
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to creating user");
                throw;
            }
        }

        public void SeedUsers()
        {
            Dictionary<string, string> credencials = new Dictionary<string, string>
            {
                { "Admin", ConfigHelper.CredencialsUserUserAdmin },
                { "Member", ConfigHelper.CredencialsUserUserMember },
                { "User", ConfigHelper.CredencialsUserUserUser },
                { "Service", ConfigHelper.CredencialsUserUserService },
            };

            credencials
                .Where(role => _unitOfWork.UserIdentityRepository.FindByEmail(role.Value) == null)
                .ToList()
                .ForEach(role =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(role.Value) && _unitOfWork.UserIdentityRepository.FindByEmail(role.Value) == null)
                            CreateUser(role);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error seeding user for role {role}");
                    }
                });

            _unitOfWork.Save();
        }

        private void CreateUser(KeyValuePair<string, string> role)
        {
            try
            {
                User user = Activator.CreateInstance<User>();

                user.UserName = role.Value.Split('@').First().ToLower();
                user.Email = role.Value.ToLower();
                user.NormalizedUserName = role.Value.Split('@').First().ToUpper();
                user.NormalizedEmail = role.Value.ToUpper();
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                user.TwoFactorEnabled = role.Key.Equals("Admin");

                string password = Util.GerarSenhaAleatoria();

                IdentityResult result = _unitOfWork.UserIdentityRepository.CreateUser(user, password);

                if (result.Succeeded)
                {
                    string htmlMessage = $"Email: {user.Email} - Username: {user.UserName} - Password: {password}";

                    _unitOfWork.UserIdentityRepository.AddRoleToUser(user, role.Key);
                    _unitOfWork.Save();

                    _emailSender.SendEmailAsync(user.Email, "Criação de Conta", htmlMessage).Wait();
                }
                else
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to creating user");
                throw;
            }
        }

        public void Dispose()
        {
            _emailSender.Dispose();
            _unitOfWork.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
