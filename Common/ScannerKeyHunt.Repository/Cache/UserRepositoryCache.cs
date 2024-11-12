using ScannerKeyHunt.Data.DTOs;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Data.Helpers;
using ScannerKeyHunt.Repository.Interfaces.Cache;
using ScannerKeyHunt.Repository.Interfaces.Repository;
using ScannerKeyHunt.Repository.Repository;
using ScannerKeyHunt.RepositoryCache.Cache.Interfaces;
using ScannerKeyHunt.Utils;
using System.Security.Claims;

namespace ScannerKeyHunt.Repository.Cache
{
    public class UserRepositoryCache : IUserRepositoryCache
    {
        private IUserRepository _userRepository;
        private string BaseKey = typeof(UserRepository).Name;
        private double CacheDurationMinutes = ConfigHelper.CacheDurationMinutes;
        private readonly ICacheService _cacheService;

        public UserRepositoryCache(
            IUserIdentityRepository userIdentityRepository,
            IUserStoreRepository userStoreRepository,
            ICacheService cacheService
        )
        {
            _cacheService = cacheService;
            _userRepository = new UserRepository(userIdentityRepository, userStoreRepository);
        }

        public User GetByEmail(string email, bool useCache = true)
        {
            Func<User> function = () => _userRepository.GetByEmail(email);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("GetByEmail", Util.CalculateMD5Hash(email));

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public User GetByUsername(string username, bool useCache = true)
        {
            Func<User> function = () => _userRepository.GetByUsername(username);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("GetByUsername", Util.CalculateMD5Hash(username));

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public List<Claim> GetRolesByUser(User user, bool useCache = true)
        {
            Func<List<Claim>> function = () => _userRepository.GetRolesByUser(user);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = string.Format("GetRolesByUser", Util.CalculateMD5Hash(user.Id));

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public List<Claim> GetRolesByGuid(Guid guid, bool useCache = true)
        {
            Func<List<Claim>> function = () => _userRepository.GetRolesByGuid(guid);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("GetRolesByGuid", Util.CalculateMD5Hash(guid.ToString()));

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public List<Claim> GetRolesByEmail(string email, bool useCache = true)
        {
            Func<List<Claim>> function = () => _userRepository.GetRolesByEmail(email);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("GetRolesByEmail", Util.CalculateMD5Hash(email));

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public List<Claim> GetRolesByUsername(string username, bool useCache = true)
        {
            Func<List<Claim>> function = () => _userRepository.GetRolesByUsername(username);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("GetRolesByUsername", Util.CalculateMD5Hash(username));

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public bool CheckPasswordByUser(User user, string password, bool useCache = true)
        {
            Func<bool> function = () => _userRepository.CheckPasswordByUser(user, password);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("CheckPasswordByUser", Util.CalculateMD5Hash($"{user.Id}:{password}"));

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public bool CheckPasswordByGuid(Guid guid, string password, bool useCache = true)
        {
            Func<bool> function = () => _userRepository.CheckPasswordByGuid(guid, password);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("CheckPasswordByGuid", Util.CalculateMD5Hash($"{guid}:{password}"));

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public bool CheckPasswordByEmail(string email, string password, bool useCache = true)
        {
            Func<bool> function = () => _userRepository.CheckPasswordByEmail(email, password);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("CheckPasswordByEmail", Util.CalculateMD5Hash($"{email}:{password}"));

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public bool CheckPasswordByUsername(string username, string password, bool useCache = true)
        {
            Func<bool> function = () => _userRepository.CheckPasswordByUsername(username, password);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("CheckPasswordByUsername", Util.CalculateMD5Hash($"{username}:{password}"));

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public User GetByGuid(Guid guid, bool useCache = true)
        {
            Func<User> function = () => _userRepository.GetByGuid(guid);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("GetByGuid", guid.ToString());

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public bool IsEmailConfirmed(User user, bool useCache = true)
        {
            Func<bool> function = () => _userRepository.IsEmailConfirmed(user);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("IsEmailConfirmed", user.Id);

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        public bool ResetPassword(User user, string code, string password, bool useCache = true)
        {
            bool result = _userRepository.ResetPassword(user, code, password);

            if (result && ConfigHelper.UseCache)
            {
                ResetCache<User>(user.Id);
            }

            return result;
        }

        public bool ConfirmEmail(User user, string token, bool useCache = true)
        {
            Func<bool> function = () => _userRepository.ConfirmEmail(user, token);

            if (useCache && ConfigHelper.UseCache)
            {
                string key = GenerateKey("ConfirmEmail", user.Id);

                return _cacheService.GetOrSet(key, function, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return function();
        }

        private void ResetCache<T>(string id)
        {
            _cacheService.Remove<T>(id);
        }

        public void SignOut()
        {
            _userRepository.SignOut();
        }

        public string GenerateEmailConfirmationToken(User user)
        {
            return _userRepository.GenerateEmailConfirmationToken(user);
        }

        public string GenerateTwoFactorToken(User user)
        {
            return _userRepository.GenerateTwoFactorToken(user);
        }

        public void PasswordSignIn(User user, string password)
        {
            _userRepository.PasswordSignIn(user, password);
        }

        public bool TwoFactorSignIn(string code)
        {
            return _userRepository.TwoFactorSignIn(code);
        }

        public User CreateUser(UserDTO userDTO, bool is2FA = false)
        {
            return _userRepository.CreateUser(userDTO, is2FA);
        }

        public string GeneratePasswordResetToken(User user)
        {
            return _userRepository.GeneratePasswordResetToken(user);
        }

        public List<User> GetAll()
        {
            return _userRepository.GetAll();
        }

        public bool Update(User user)
        {
            if (ConfigHelper.UseCache)
                ResetCache<User>(user.Id);

            return _userRepository.Update(user);
        }

        public bool Delete(User user)
        {
            if (ConfigHelper.UseCache)
                ResetCache<User>(user.Id);

            return _userRepository.Delete(user);
        }

        public bool Update(Guid guid)
        {
            if (ConfigHelper.UseCache)
                ResetCache<User>(guid.ToString());

            return _userRepository.Update(guid);
        }

        public bool Delete(Guid guid)
        {
            if (ConfigHelper.UseCache)
                ResetCache<User>(guid.ToString());

            return _userRepository.Delete(guid);
        }

        private string GenerateKey(string method, string key)
        {
            return string.Format("{0}_{1}_{2}", BaseKey, method, key);
        }

        public void Dispose()
        {
            _userRepository.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
