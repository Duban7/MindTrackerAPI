using BLL.Abstraction;
using BLL.Jwt;
using Domain.Models;
using DAL.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Exceptions;
using MimeKit;
using MailKit.Net.Smtp;
using FluentValidation;
using FluentValidation.Results;
using MindTrackerServer.Validators;

namespace BLL.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger<AccountService> _logger;
        private readonly IValidator<Account> _accountValidator;

        public AccountService(IAccountRepository accountRepository,
                            IOptions<JwtOptions> jwtOptions,
                            ILogger<AccountService> logger,
                            IValidator<Account> validator)
        {
            _accountRepository = accountRepository;
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
            _accountValidator = validator;
        }
        public async Task<Account?> CreateAccount(Account newAccount)
        {
            ValidationResult result = await _accountValidator.ValidateAsync(newAccount);

            if (!result.IsValid)
            {
                StringBuilder errors = new();

                _logger.LogError("Invalid account");

                foreach (var error in result.Errors)
                {
                    _logger.LogError(error.ErrorMessage);
                    errors.Append(error.ErrorMessage);
                    errors.Append(";\n");
                }

                throw new InvalidAccountException(errors.ToString());
            }

            var user = await _accountRepository.GetOneByEmailAsync(newAccount.Email!);

            if (user != null) throw new AccountAlreadyExistsException("Account is alread exists");

            string id = _accountRepository.GenerateObjectID();
            Account createdAccount = new()
            {
                Id = id,
                Email = newAccount.Email,
                Password = newAccount.Password,
                RefreshToken = GenerateRefreshToken()
            };
            
            await _accountRepository.CreateAsync(createdAccount);

            return createdAccount;
        }

        public async Task UpdateAccount(Account account, string id)
        {
            _ = await _accountRepository.GetOneByIdAsync(id) ?? throw new AccountNotFoundException("Account doesn't exist");

            await _accountRepository.UpdateAsync(account);
        }

        public async Task DeleteAccount(string id)
        {
            _ = await _accountRepository.GetOneByIdAsync(id) ?? throw new AccountNotFoundException("Account doesn't exist");

            await _accountRepository.RemoveAsync(id);
        }

        public async Task<Account?> LogIn(Account logInAccount)
        {
            Account? foundAccount = await _accountRepository.GetOneByEmailAndPasswordAsync(logInAccount.Email!, logInAccount.Password!);

            return foundAccount ?? throw new AccountNotFoundException("Account doesn't exist");
        }

        public async Task<Account?> GetAccount(string id) =>
             await _accountRepository.GetOneByIdAsync(id);

        public async Task<Account?> UpdateRefreshToken(RefreshToken oldRefreshToken, string id)
        {
            Account? foundAccount = await _accountRepository.GetOneByIdAsync(id) ?? throw new AccountNotFoundException("Account doesn't exist");

            if (foundAccount.RefreshToken!.Token != oldRefreshToken.Token) throw new AccountRefreshTokenException("Account token doesn't match request token");

            if (foundAccount.RefreshToken.Expires < DateTime.UtcNow) throw new AccountRefreshTokenException("Refresh tolen is expired");

            RefreshToken newRefreshToken = GenerateRefreshToken();
            foundAccount.RefreshToken = newRefreshToken;
            await _accountRepository.UpdateAsync(foundAccount);

            return foundAccount;
        }

        public async Task ResetPassword(string email)
        {
            Account account = await _accountRepository.GetOneByEmailAsync(email) ?? throw new AccountNotFoundException("Account with this email doesn't exist");
            string newPassword = CreatePassword();
            account.Password = newPassword;

            await _accountRepository.UpdateAsync(account);

            using var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация сайта", "tosha1600@mail.ru"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "Сброс пароля для Трекера настроения";
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = $"Для вашего аккаунта сброшен пароль. Новый пароль: {newPassword}"
            };

            using SmtpClient client = new();

            await client.ConnectAsync("smtp.mail.ru", 465, true);
            await client.AuthenticateAsync("tosha1600@mail.ru", "bbstt8QcZCznaPZ49k8M");
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }

        public string GenerateJwtToken(Account user)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id!),
                new Claim(ClaimTypes.Email, user.Email!),
            };
            JwtSecurityToken jwt = new(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
                signingCredentials: new SigningCredentials(_jwtOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
           
            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        private static string CreatePassword(int length=10)
        {
            const string symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new();
            while (0 < length--)
            {
                res.Append(symbols[RandomNumberGenerator.GetInt32(symbols.Length)]);
            }
            if (!AccountValidator.IsPasswordValid(res.ToString()))
            {
                res.Append(symbols[RandomNumberGenerator.GetInt32(0, 25)]);
                res.Append(symbols[RandomNumberGenerator.GetInt32(26, 51)]);
                res.Append(symbols[RandomNumberGenerator.GetInt32(52, symbols.Length)]);
            }
            return res.ToString();
        }

        private static RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
            };
            return refreshToken;
        }
    }
}
