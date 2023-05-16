using Domain.Models;

namespace DAL.Abstraction
{
    public interface IAccountRepository : IRepository<Account>
    {
        public Task<Account?> GetOneByIdAsync(string id);
        public Task CreateAsync(Account newAccount);
        public Task UpdateAsync(Account updatedAccount);
        public Task RemoveAsync(string id);
        public Task<Account?> GetOneByEmailAsync(string email);
        public Task<Account?> GetOneByEmailAndPasswordAsync(string email, string password);
        public string GenerateObjectID();
    }
}
