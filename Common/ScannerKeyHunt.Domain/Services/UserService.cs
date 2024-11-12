using AutoMapper;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MimeKit;
using ScannerKeyHunt.Data.DTOs;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Domain.Interfaces;
using ScannerKeyHunt.Domain.ModelDTOs;
using ScannerKeyHunt.Domain.Models;
using ScannerKeyHunt.Repository.Interfaces;
using ScannerKeyHunt.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace ScannerKeyHunt.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ITokenService tokenService,
            ILogger<UserService> logger
        )
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _tokenService = tokenService;
            _logger = logger;
            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserDTO, User>().ReverseMap();
                cfg.CreateMap<UserDTO, UserModelDTO>().ReverseMap();
            }));
        }

        public Guid GetByUsername(string username)
        {
            return Guid.Parse(_unitOfWork.UserRepository.GetByUsername(username)?.Id);
        }

        public Guid GetByEmail(string email)
        {
            return Guid.Parse(_unitOfWork.UserRepository.GetByEmail(email)?.Id);
        }

        public virtual bool Delete(UserModelDTO userDTO)
        {
            return _unitOfWork.UserRepository.Delete(_mapper.Map<User>(userDTO));
        }

        public List<Guid> GetAllUsers()
        {
            List<User> users = _unitOfWork.UserRepository.GetAll();
            return users.Select(x => Guid.Parse(x.Id)).ToList();
        }

        public virtual List<UserModelDTO> GetAll()
        {
            List<User> users = _unitOfWork.UserRepository.GetAll();
            return _mapper.Map<List<UserModelDTO>>(_mapper.Map<List<UserDTO>>(users));
        }

        public UserModelDTO GetUserByGuid(Guid guid)
        {
            return _mapper.Map<UserModelDTO>(_mapper.Map<UserDTO>(_unitOfWork.UserRepository.GetByGuid(guid)));
        }

        public UserModelDTO GetUserByEmail(string email)
        {
            return _mapper.Map<UserModelDTO>(_mapper.Map<UserDTO>(_unitOfWork.UserRepository.GetByEmail(email)));
        }

        public UserModelDTO GetUserByUsername(string username)
        {
            return _mapper.Map<UserModelDTO>(_mapper.Map<UserDTO>(_unitOfWork.UserRepository.GetByUsername(username)));
        }

        public JwtToken Login(LoginModelDTO loginDTO)
        {
            try
            {
                User user = _unitOfWork.UserRepository.GetByEmail(loginDTO.Email);

                if (!user.EmailConfirmed)
                    throw new Exception($"Email Is not Confirmed");

                bool check = _unitOfWork.UserRepository.CheckPasswordByUser(user, loginDTO.Password);

                if (!check)
                    throw new Exception("Email Or Password is Invallid");

                if (user.TwoFactorEnabled)
                {
                    _unitOfWork.UserRepository.SignOut();

                    _unitOfWork.UserRepository.PasswordSignIn(user, loginDTO.Password);

                    string pin = _unitOfWork.UserRepository.GenerateTwoFactorToken(user);

                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        SendPinOTP(user, pin);
                    });
                }
                else
                {
                    List<Claim> claims = _unitOfWork.UserRepository.GetRolesByUser(user);

                    JwtToken jwtToken = GeraToken(loginDTO.Email, claims);

                    return jwtToken;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user");
                throw;
            }
        }

        public virtual Guid? RegisterUser(UserModelDTO userDTO, string urlMethod)
        {
            try
            {
                UserDTO userData = _mapper.Map<UserDTO>(userDTO);

                User user = _unitOfWork.UserRepository.CreateUser(userData, true);

                if (user == null)
                    throw new Exception("User creation failed! Please check user details and try again.");

                string token = _unitOfWork.UserRepository.GenerateEmailConfirmationToken(user);

                string confirmationLink = Util.UrlData(urlMethod, new { token, email = user.Email });

                ThreadPool.QueueUserWorkItem(state =>
                {
                    SendEmailConfirmation(user, confirmationLink);
                });

                return Guid.Parse(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error register user");
                throw;
            }
        }

        public JwtToken ConfirmCode(string username, string code)
        {
            try
            {
                User user = _unitOfWork.UserRepository.GetByUsername(username);

                if (!user.EmailConfirmed)
                    throw new Exception($"Email Is not Confirmed");

                if (!user.TwoFactorEnabled)
                    throw new Exception("Unauthorized");

                bool check = _unitOfWork.UserRepository.TwoFactorSignIn(code);

                if (!check)
                    throw new Exception("Code Inválido");

                List<Claim> claims = _unitOfWork.UserRepository.GetRolesByUser(user);

                return GeraToken(user.Email, claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming code");
                throw;
            }
        }

        public void RevokeByUser(string username)
        {
            try
            {
                _tokenService.RevokeByUser(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                throw;
            }
        }

        public JwtToken RefreshToken(string accessToken, string refreshToken)
        {
            try
            {
                return _tokenService.RefreshToken(accessToken, refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                throw;
            }
        }

        public (string Code, Guid? UUID) ResetPasswordSolicitation(string email, string urlMethod)
        {
            try
            {
                User user = _unitOfWork.UserRepository.GetByEmail(email);

                if (!user.EmailConfirmed)
                    throw new Exception($"Email Is not Confirmed");

                (string Code, string ConfirmationLink) code = GenerateCode(user, urlMethod);

                ThreadPool.QueueUserWorkItem(state =>
                {
                    SendResetEmai(user, code.ConfirmationLink);
                });

                return (Code: code.Code, UUID: Guid.Parse(user.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                throw;
            }
        }

        public void ResetPassword((string code, string email, string password) resetPasswordData)
        {
            try
            {
                User user = _unitOfWork.UserRepository.GetByEmail(resetPasswordData.email);

                if (!user.EmailConfirmed)
                    throw new Exception($"Email Is not Confirmed");

                resetPasswordData.code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordData.code));

                bool isUpdated = _unitOfWork.UserRepository.ResetPassword(user, resetPasswordData.code, resetPasswordData.password);

                if (!isUpdated)
                    throw new Exception("Don't reveal that the user does not exist");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                throw;
            }
        }

        public virtual JwtToken GeraToken(string email, List<Claim> authClaims = null)
        {
            try
            {
                User user = _unitOfWork.UserRepository.GetByEmail(email);

                if (authClaims == null)
                {
                    authClaims = new List<Claim>();
                    authClaims.AddRange(_unitOfWork.UserRepository.GetRolesByUser(user));
                }

                authClaims.AddRange(new[]
                {
                    new Claim(JwtRegisteredClaimNames.UniqueName, email),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                });

                return _tokenService.GenerateJwtToken(user, authClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token");
                throw;
            }
        }

        public bool ConfirmEmail(string token, string email)
        {
            try
            {
                User user = _unitOfWork.UserRepository.GetByEmail(email);

                return _unitOfWork.UserRepository.ConfirmEmail(user, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email");
                throw;
            }
        }

        public virtual Guid? Add(UserModelDTO userDTO)
        {
            try
            {
                UserDTO userData = _mapper.Map<UserDTO>(userDTO);

                User user = _unitOfWork.UserRepository.CreateUser(userData);

                return Guid.Parse(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user");
                throw;
            }
        }

        public void SendEmail(EmailModelDTO emailDTO, PostsViewModelDTO postsViewModel)
        {
            try
            {
                string emailBody = _emailService.GetEmailBody(postsViewModel.Titulo, postsViewModel.Descricao, postsViewModel.ImagemUrl);

                List<MailboxAddress> mailboxAddresses = emailDTO.To.Select(to => new MailboxAddress(to.Name, to.Address)).ToList();

                MimeMessage mimeMessage = _emailService.GetEmailMessage(mailboxAddresses, postsViewModel.Titulo, emailBody);

                _emailService.SendEmail(mimeMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                throw;
            }
        }

        private void SendEmailTemplate(User user, PostsViewModelDTO postsViewModel)
        {
            try
            {
                EmailModelDTO emailDTO = new EmailModelDTO();

                emailDTO.To.Add(new EmailAddressModelDTO { Address = user.Email, Name = user.UserName });

                SendEmail(emailDTO, postsViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email template");
                throw;
            }
        }

        private void SendPinOTP(User user, string pin)
        {
            try
            {
                PostsViewModelDTO postsViewModel = new PostsViewModelDTO()
                {
                    Titulo = "Login de Conta",
                    Descricao = $"Pin para login da conta: {pin}"
                };

                SendEmailTemplate(user, postsViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending pin otp");
                throw;
            }
        }

        private void SendEmailConfirmation(User user, string confirmationLink)
        {
            try
            {
                PostsViewModelDTO postsViewModel = new PostsViewModelDTO()
                {
                    Titulo = "Confirmação de Criação de Conta",
                    Descricao = $"Clique no Link para confirmar a criação da conta <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>Clique aqui</a>."
                };

                SendEmailTemplate(user, postsViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email confirmation");
                throw;
            }
        }

        private void SendResetEmai(User user, string confirmationLink)
        {
            try
            {
                EmailModelDTO emailDTO = new EmailModelDTO();
                emailDTO.To.Add(new EmailAddressModelDTO { Address = user.Email, Name = user.UserName });

                PostsViewModelDTO postsViewModel = new PostsViewModelDTO
                {
                    Titulo = "Confirmação de Recuperação de Conta",
                    Descricao = $"Clique no Link para confirmar a recuperação da conta <a href='{confirmationLink}'>Clique aqui</a>."
                };

                SendEmail(emailDTO, postsViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reset email");
                throw;
            }
        }

        private (string Code, string ConfirmationLink) GenerateCode(User user, string urlMethod)
        {
            try
            {
                string code = _unitOfWork.UserRepository.GeneratePasswordResetToken(user);

                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                string confirmationLink = Util.UrlData(urlMethod, new { code });

                confirmationLink = HtmlEncoder.Default.Encode(confirmationLink);

                return (Code: code, ConfirmationLink: confirmationLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating code");
                throw;
            }
        }

        public void Dispose()
        {
            _tokenService.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
