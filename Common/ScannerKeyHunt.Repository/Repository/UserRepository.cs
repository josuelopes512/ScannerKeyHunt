using Microsoft.AspNetCore.Identity;
using ScannerKeyHunt.Data.DTOs;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Data.Enums;
using ScannerKeyHunt.Repository.Interfaces.Repository;
using System.Security.Claims;

namespace ScannerKeyHunt.Repository.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IUserIdentityRepository _userIdentityRepository;
        private readonly IUserStoreRepository _userStoreRepository;

        public UserRepository(IUserIdentityRepository userIdentityRepository, IUserStoreRepository userStoreRepository)
        {
            _userIdentityRepository = userIdentityRepository;
            _userStoreRepository = userStoreRepository;
        }

        public bool CheckPasswordByUser(User user, string password)
        {
            try
            {
                bool claims = false;

                try
                {
                    bool isEmail = CheckPasswordByEmail(user.Email, password);

                    return isEmail;
                }
                catch (Exception)
                {
                }

                try
                {
                    bool isUsername = CheckPasswordByUsername(user.UserName, password);

                    return isUsername;
                }
                catch (Exception)
                {
                }

                try
                {
                    bool isGuid = CheckPasswordByGuid(Guid.Parse(user.Id), password);

                    return isGuid;
                }
                catch (Exception)
                {
                }

                return claims;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool CheckPasswordByEmail(string email, string password)
        {
            User identityUser = _userIdentityRepository.FindByEmail(email);

            if (identityUser == null)
                throw new Exception($"Erro ao obter usuário com email = {email}.");

            if (identityUser.DeletionDate != null)
                throw new Exception($"Erro ao obter usuário com email = {email}.");

            bool check = _userIdentityRepository.CheckPassword(identityUser, password);

            return check;
        }

        public bool CheckPasswordByUsername(string username, string password)
        {
            User identityUser = _userIdentityRepository.FindByUsername(username);

            if (identityUser == null)
                throw new Exception($"Erro ao obter usuário com username = {username}.");

            if (identityUser.DeletionDate != null)
                throw new Exception($"Erro ao obter usuário com username = {username}.");

            bool check = _userIdentityRepository.CheckPassword(identityUser, password);

            return check;
        }

        public bool CheckPasswordByGuid(Guid guid, string password)
        {
            User identityUser = _userIdentityRepository.FindByGuid(guid.ToString());

            if (identityUser == null)
                throw new Exception($"Erro ao obter usuário com guid = {guid.ToString()}.");

            if (identityUser.DeletionDate != null)
                throw new Exception($"Erro ao obter usuário com guid = {guid.ToString()}.");

            bool check = _userIdentityRepository.CheckPassword(identityUser, password);

            return check;
        }

        public List<Claim> GetRolesByUser(User user)
        {
            try
            {
                List<Claim> claims = new List<Claim>();

                try
                {
                    List<Claim> email = GetRolesByEmail(user.Email);

                    if (email.Count > 0)
                        return email;
                }
                catch (Exception)
                {
                }

                try
                {
                    List<Claim> username = GetRolesByUsername(user.UserName);

                    if (username.Count > 0)
                        return username;
                }
                catch (Exception)
                {
                }

                try
                {
                    List<Claim> uuid = GetRolesByGuid(Guid.Parse(user.Id));

                    if (uuid.Count > 0)
                        return uuid;
                }
                catch (Exception)
                {
                }

                return claims;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Claim> GetRolesByGuid(Guid guid)
        {
            try
            {
                User identityUser = _userIdentityRepository.FindByGuid(guid.ToString());

                if (identityUser == null)
                    throw new Exception($"Erro ao obter usuário com guid = {guid.ToString()}.");

                if (identityUser.DeletionDate != null)
                    throw new Exception($"Erro ao obter usuário com guid = {guid.ToString()}.");

                List<Claim> roles = _userIdentityRepository.GetRolesClaims(identityUser);

                return roles;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Claim> GetRolesByUsername(string username)
        {
            try
            {
                User identityUser = _userIdentityRepository.FindByUsername(username);

                if (identityUser == null)
                    throw new Exception($"Erro ao obter usuário com username = {username}.");

                if (identityUser.DeletionDate != null)
                    throw new Exception($"Erro ao obter usuário com username = {username}.");

                List<Claim> roles = _userIdentityRepository.GetRolesClaims(identityUser);

                return roles;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Claim> GetRolesByEmail(string email)
        {
            try
            {
                User identityUser = _userIdentityRepository.FindByEmail(email);

                if (identityUser == null)
                    throw new Exception($"Erro ao obter usuário com email = {email}.");

                if (identityUser.DeletionDate != null)
                    throw new Exception($"Erro ao obter usuário com email = {email}.");

                List<Claim> roles = _userIdentityRepository.GetRolesClaims(identityUser);

                return roles;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public User GetByEmail(string email)
        {
            try
            {
                User identityUser = _userIdentityRepository.FindByEmail(email);

                if (identityUser == null)
                    throw new Exception($"Erro ao obter usuário com email = {email}.");

                if (identityUser.DeletionDate != null)
                    throw new Exception($"Erro ao obter usuário com email = {email}.");

                return identityUser;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public User GetByUsername(string username)
        {
            try
            {
                User identityUser = _userIdentityRepository.FindByUsername(username);

                if (identityUser == null)
                    throw new Exception($"Erro ao obter usuário com username = {username}.");

                if (identityUser.DeletionDate != null)
                    throw new Exception($"Erro ao obter usuário com username = {username}.");

                return identityUser;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public User CreateUser(UserDTO userDTO, bool is2FA = false)
        {
            try
            {
                User userExists = _userIdentityRepository.FindByEmail(userDTO.Email);

                if (userExists != null && userExists.DeletionDate == null)
                    throw new Exception("User already exists!");

                User user = Activator.CreateInstance<User>();

                _userStoreRepository.SetEmailAndUserName(user, userDTO.Email, userDTO.UserName);

                user.TwoFactorEnabled = is2FA;
                user.Role = userDTO.Role;

                IdentityResult identityResult = _userIdentityRepository.CreateUser(user, userDTO.Password);

                if (!identityResult.Succeeded)
                    throw new Exception(string.Join("; ", identityResult.Errors.Select(e => e.Description)));

                SaveUserRole(user, userDTO.Role);

                return user;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public User GetByGuid(Guid guid)
        {
            try
            {
                User identityUser = _userIdentityRepository.FindByGuid(guid.ToString());

                if (identityUser == null)
                    throw new Exception($"Erro ao obter usuário com uuid = {guid.ToString()}.");

                if (identityUser.DeletionDate != null)
                    throw new Exception($"Erro ao obter usuário com uuid = {guid.ToString()}.");

                return identityUser;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Update(User entity)
        {
            try
            {
                User identityUser = _userIdentityRepository.FindByGuid(entity.Id);

                if (identityUser == null)
                    throw new Exception($"Erro ao obter usuário com uuid = {entity.Id}.");

                if (identityUser.DeletionDate != null)
                    throw new Exception($"Erro ao obter usuário com uuid = {entity.Id}.");

                IdentityResult identityResult = _userIdentityRepository.Update(entity);

                if (identityResult == null)
                    throw new Exception($"Erro ao obter usuário com uuid = {entity.Id}.");

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Update(Guid guid)
        {
            try
            {
                User identityUser = _userIdentityRepository.FindByGuid(guid.ToString());

                if (identityUser == null)
                    throw new Exception($"Erro ao obter usuário com uuid = {guid.ToString()}.");

                if (identityUser.DeletionDate != null)
                    throw new Exception($"Erro ao obter usuário com uuid = {guid.ToString()}.");

                IdentityResult identityResult = _userIdentityRepository.Update(identityUser);

                if (identityResult == null)
                    throw new Exception($"Erro ao obter usuário com uuid = {guid.ToString()}.");

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Delete(Guid guid)
        {
            try
            {
                User user = _userIdentityRepository.FindByGuid(guid.ToString());

                user.UpdateDate = DateTime.UtcNow;
                user.DeletionDate = DateTime.UtcNow;

                IdentityResult identityResult = _userIdentityRepository.Update(user);

                if (identityResult == null)
                    throw new Exception($"Erro ao obter usuário com uuid = {guid.ToString()}.");

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Delete(User entity)
        {
            try
            {
                User user = _userIdentityRepository.FindByGuid(entity.Id);

                user.UpdateDate = DateTime.UtcNow;
                user.DeletionDate = DateTime.UtcNow;

                IdentityResult identityResult = _userIdentityRepository.Update(user);

                if (identityResult == null)
                    throw new Exception($"Erro ao obter usuário com uuid = {entity.Id.ToString()}.");

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<User> GetAll()
        {
            try
            {
                List<User> result = _userIdentityRepository.GetAll().Where(x => x.DeletionDate == null).Select(user => new User
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    CreationDate = user.CreationDate,
                    UpdateDate = user.UpdateDate
                }).ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool ConfirmEmail(User user, string token)
        {
            try
            {
                return _userIdentityRepository.ConfirmEmail(user, token);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GenerateEmailConfirmationToken(User user)
        {
            try
            {
                return _userIdentityRepository.GenerateEmailConfirmationToken(user);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GenerateTwoFactorToken(User user)
        {
            try
            {
                return _userIdentityRepository.GenerateTwoFactorToken(user);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SignOut()
        {
            try
            {
                _userIdentityRepository.SignOut();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void PasswordSignIn(User user, string password)
        {
            try
            {
                _userIdentityRepository.PasswordSignIn(user, password);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool TwoFactorSignIn(string code)
        {
            try
            {
                return _userIdentityRepository.TwoFactorSignIn(code);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SaveUserRole(User user, RoleEnum role)
        {
            switch (role)
            {
                case RoleEnum.Admin:
                case RoleEnum.User:
                case RoleEnum.Service:
                    if (!_userIdentityRepository.RoleExists(role.ToString()))
                        _userIdentityRepository.CreateRole(role.ToString());

                    if (_userIdentityRepository.RoleExists(role.ToString()))
                        _userIdentityRepository.AddRoleToUser(user, role.ToString());
                    break;
            }
        }

        public bool ResetPassword(User user, string code, string password)
        {
            try
            {
                return _userIdentityRepository.ResetPassword(user, code, password);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GeneratePasswordResetToken(User user)
        {
            try
            {
                return _userIdentityRepository.GeneratePasswordResetToken(user);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool IsEmailConfirmed(User user)
        {
            try
            {
                return _userIdentityRepository.IsEmailConfirmed(user);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Dispose()
        {
            _userIdentityRepository.Dispose();
            _userStoreRepository.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
