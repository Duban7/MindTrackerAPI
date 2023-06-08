using Domain.Models;

namespace DAL.Abstraction
{
    public interface IAccountRepository 
    {
        public Task<Account?> GetOneByIdAsync(string id);
        public Task CreateAsync(Account newAccount);
        public Task UpdateAsync(Account updatedAccount);
        public Task RemoveAsync(string id);
        public Task<Account?> GetOneByEmailAsync(string email);
        public string GenerateObjectID();
    }
}
