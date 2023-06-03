using Domain.Models;
using DAL.Abstraction;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace DAL.Implementation
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IMongoCollection<Account> _accountCollection;
        private readonly ILogger<AccountRepository> _logger;    

        public AccountRepository(IMongoCollection<Account> userCollection, ILogger<AccountRepository> logger)
        {
            _accountCollection = userCollection;
            _logger = logger;
        }
        public async Task<Account?> GetOneByEmailAsync(string email) =>
          await _accountCollection.Find(regUser => regUser.Email == email).FirstOrDefaultAsync();

        public async Task<Account?> GetOneByEmailAndPasswordAsync(string email, string password) =>
            await _accountCollection.Find(regUser => regUser.Email == email && regUser.Password == password).FirstOrDefaultAsync();

        public async Task<Account?> GetOneByIdAsync(string id) =>
            await _accountCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Account newUser) =>
            await _accountCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(Account updatedUser)=>
            await _accountCollection.ReplaceOneAsync(x => x.Id == updatedUser.Id, updatedUser);

        public async Task RemoveAsync(string id) =>
            await _accountCollection.DeleteOneAsync(x => x.Id == id);

        public string GenerateObjectID() =>
            ObjectId.GenerateNewId().ToString();
    }
}
