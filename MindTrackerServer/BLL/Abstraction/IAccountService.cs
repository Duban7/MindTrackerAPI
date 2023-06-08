using Domain.Models;

namespace BLL.Abstraction
{
    public interface IAccountService
    {
        public Task<Account?> CreateAccount(Account newAccount);
        public Task UpdateAccount(Account account, string id);
        public Task DeleteAccount(string id);
        public Task<Account?> LogIn(Account logInAccount);
        public Task<Account?> GetAccount(string id);
        public Task<Account?> UpdateRefreshToken (RefreshToken oldRefreshToken, string id);
        public Task ResetPasswordQuery(string email);
        public Task ResetPassword(string idHash, string email);
        public string GenerateJwtToken(Account account);
    }
}
