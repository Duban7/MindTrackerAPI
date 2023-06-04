using Domain.Models;

namespace DAL.Abstraction
{
    public interface IMoodActivityRepository
    {
        public Task<List<MoodActivity>> GetAllByGroupid(string groupId);
        public Task<List<MoodActivity>> GetActivitiesByIds(List<string> ids);
        public Task CreateAsync(MoodActivity newAccount);
        public Task UpdateAsync(MoodActivity updatedAccount);
        public Task RemoveAsync(string id);
        public Task InsertManyAsync(List<MoodActivity> activitiesToInsert);
        public Task<long> UpdateManyAsync(List<MoodActivity> activitiesToupdate);
        public Task<long> RemoveManyAsync(List<MoodActivity> activitiesToDelete);
        public Task<long> RemoveManyByIdsAsync(List<string> ids);
        public string GenerateObjectId();
    }
}
