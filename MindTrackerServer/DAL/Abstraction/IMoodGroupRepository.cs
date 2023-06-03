using Domain.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DAL.Abstraction
{
    public interface IMoodGroupRepository
    {
        public Task<List<MoodGroup>> GetAllByAccountId(string accountId);
        public Task<List<MoodGroupWithActivities>> GetAllWithActivities(string accountId);
        public Task<List<MoodGroup>> GetMoodGroupsByIds(List<string> ids);
        public Task CreateAsync(MoodGroup newAccount);
        public Task UpdateAsync(MoodGroup updatedAccount);
        public Task RemoveAsyncById(string id);
        public Task InsertManyAsync(List<MoodGroup> groupsToInsert);
        public Task<long> UpdateManyAsync(List<MoodGroup> groupsToUpdate);
        public Task<long> RemoveManyAsync(List<MoodGroup> groupsToDelete, string accountId);
        public Task RemoveAllAsyncByAccountId(string accountId);
        public string GenerateObjectId();
    }
}
