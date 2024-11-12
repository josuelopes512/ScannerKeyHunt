using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Data.Helpers;
using ScannerKeyHunt.Domain.Interfaces;
using ScannerKeyHunt.Domain.Models;
using ScannerKeyHunt.Repository.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ScannerKeyHunt.Domain.Services
{
    public class TokenService : ITokenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            IUnitOfWork unitOfWork,
            ILogger<TokenService> logger
        )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public JwtToken RefreshToken(string accessToken, string refreshToken)
        {
            try
            {
                ClaimsPrincipal? principal = ValidateExpirationDateToken(accessToken);

                if (principal == null)
                    throw new Exception("Invalid access token or refresh token");

                User user = _unitOfWork.UserRepository.GetByEmail(principal.Identity.Name);

                TokenAuth token = _unitOfWork.TokenAuthRepository.GetTokenAuthByUserId(Guid.Parse(user.Id));

                if (token == null || token.UserId != Guid.Parse(user.Id) || token.RefreshToken != refreshToken || token.ExpirationDate <= DateTime.UtcNow)
                    throw new Exception("Invalid access token or refresh token");

                List<Claim> authClaims = GenerateClaims(user);

                return GenerateJwtToken(user, authClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to refreshing token");
                throw;
            }
        }

        public void RevokeByUser(string username)
        {
            try
            {
                User user = _unitOfWork.UserRepository.GetByUsername(username);

                if (user == null) throw new Exception("Invalid user name");

                DeleteTokenAuthByUserId(Guid.Parse(user.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to Revoking user");
                throw;
            }
        }

        public JwtToken GenerateJwtToken(User user, List<Claim> authClaims)
        {
            try
            {
                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigHelper.JWTKey));
                DateTime expiration = DateTime.UtcNow.AddHours(ConfigHelper.TokenConfigurationExpireHours);

                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: ConfigHelper.TokenConfigurationIssuer,
                    audience: ConfigHelper.TokenConfigurationAudience,
                    claims: authClaims,
                    expires: expiration,
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

                (string RefreshToken, DateTime ExpirationDate) refreshToken = GenerateJwtRefreshToken(Guid.Parse(user.Id));

                JwtToken jwtSecurityToken = new JwtToken
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = expiration,
                    RefreshToken = refreshToken.RefreshToken,
                    ExpirationRefreshToken = refreshToken.ExpirationDate
                };

                return jwtSecurityToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to Generating Jwt Token");
                throw;
            }
        }

        private ClaimsPrincipal? ValidateExpirationDateToken(string token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigHelper.JWTKey)),
                    ValidateLifetime = false
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to Validate Expiration Date Token");
                throw;
            }
        }

        private List<Claim> GenerateClaims(User user)
        {
            try
            {
                List<Claim> authClaims = new List<Claim>();

                authClaims.AddRange(_unitOfWork.UserRepository.GetRolesByUser(user));

                authClaims.AddRange(new[]
                {
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Email),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                });

                return authClaims;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to Generate Claims");
                throw;
            }
        }

        public void DeleteTokenAuthByUserId(Guid userId)
        {
            try
            {
                TokenAuth refreshToken = _unitOfWork.TokenAuthRepository.GetTokenAuthByUserId(userId);

                if (refreshToken == null)
                    return;

                _unitOfWork.TokenAuthRepository.Delete(refreshToken);

                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to Deleting Token Auth By User Id");
                throw;
            }
        }

        public TokenAuth CreateTokenAuth(Guid userId)
        {
            try
            {
                TokenAuth tokenAuth = new TokenAuth()
                {
                    UserId = userId,
                    RefreshToken = GenerateRefreshToken(),
                    ExpirationDate = DateTime.UtcNow.AddHours(ConfigHelper.TokenConfigurationExpireHours)
                };

                _unitOfWork.TokenAuthRepository.Add(tokenAuth);

                _unitOfWork.Save();

                return tokenAuth;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to Creating Token Auth");
                throw;
            }
        }

        private (string RefreshToken, DateTime ExpirationDate) GenerateJwtRefreshToken(Guid userId)
        {
            try
            {
                DeleteTokenAuthByUserId(userId);

                TokenAuth newRefreshToken = CreateTokenAuth(userId);

                return (RefreshToken: newRefreshToken.RefreshToken, ExpirationDate: newRefreshToken.ExpirationDate.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to Generating Jwt Refresh Token");
                throw;
            }
        }

        private string GenerateRefreshToken()
        {
            try
            {
                var randomNumber = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to Generating Refresh Token");
                throw;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
