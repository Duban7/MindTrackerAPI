using BLL.Abstraction;
using BLL.Jwt;
using Domain.Models;
using DAL.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Domain.Exceptions;

namespace BLL.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
       

        private readonly JwtOptions _jwtOptions;

        public readonly ILogger<AccountService> _logger;

        public AccountService(IAccountRepository accountRepository,
                            IOptions<JwtOptions> jwtOptions,
                            ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
        }
        public async Task<Account?> CreateAccount(Account newAccount)
        {
            var user = await _accountRepository.GetOneByEmailAsync(newAccount.Email!);

            if (user == null) throw new AccountAlreadyExistsException("User is alread exists");

            string id = _accountRepository.GenerateObjectID();
            Account createdAccount = new Account()
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
            _ = await _accountRepository.GetOneByIdAsync(id) ?? throw new AccountNotFoundException("User doesn't exist");

            if (account.Id != id) throw new AccountIdMatchException("Account Id doesn't match request Id");

            await _accountRepository.UpdateAsync(account);
        }

        public async Task DeleteAccount(string id)
        {
            _ = await _accountRepository.GetOneByIdAsync(id) ?? throw new AccountNotFoundException("User doesn't exist");

            await _accountRepository.RemoveAsync(id);
        }

        public async Task<Account?> LogIn(Account logInAccount)
        {
            Account? foundAccount = await _accountRepository.GetOneByEmailAndPasswordAsync(logInAccount.Email!, logInAccount.Password!);

            return foundAccount ?? throw new AccountNotFoundException("User doesn't exist");
        }

        public async Task<Account?> GetAccount(string id) =>
             await _accountRepository.GetOneByIdAsync(id);

        public async Task<Account?> UpdateRefreshToken(RefreshToken oldRefreshToken, string id)
        {
            Account? foundAccount = await _accountRepository.GetOneByIdAsync(id) ?? throw new AccountNotFoundException("User doesn't exist");

            if (foundAccount.RefreshToken != oldRefreshToken) throw new AccountRefreshTokenMatchException("Account token doesn't match request token");

            RefreshToken newRefreshToken = GenerateRefreshToken();
            foundAccount.RefreshToken = newRefreshToken;
            await _accountRepository.UpdateAsync(foundAccount);

            return foundAccount;
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

        public RefreshToken GenerateRefreshToken()
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
