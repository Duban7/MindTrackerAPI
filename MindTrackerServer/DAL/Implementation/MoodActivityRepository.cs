using DAL.Abstraction;
using Domain.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DAL.Implementation
{
    public class MoodActivityRepository : IMoodActivityRepository
    {
        private readonly IMongoCollection<MoodActivity> _moodActivitycollection;
        private readonly ILogger<MoodActivityRepository> _logger;

        public MoodActivityRepository(IMongoCollection<MoodActivity> moodActivityCollection, ILogger<MoodActivityRepository> logger)
        {
            _moodActivitycollection = moodActivityCollection;
            _logger = logger;
        }

        public async Task<List<MoodActivity>> GetAllByGroupid(string groupId) =>
            await _moodActivitycollection.Find(x => x.GroupId == groupId).ToListAsync();

        public async Task<List<MoodActivity>> GetActivitiesByIds(List<string> ids) =>
            await _moodActivitycollection.Find(Builders<MoodActivity>.Filter.In(a => a.Id, ids)).ToListAsync();

        public async Task CreateAsync(MoodActivity newActivity) =>
         await _moodActivitycollection.InsertOneAsync(newActivity);

        public async Task UpdateAsync(MoodActivity updatedActivity)=>
            await _moodActivitycollection.ReplaceOneAsync(x => x.Id == updatedActivity.Id, updatedActivity);
        
        public async Task RemoveAsync(string id) =>
            await _moodActivitycollection.DeleteOneAsync(x => x.Id == id);

        public async Task InsertManyAsync(List<MoodActivity> activitiesToInsert) =>
            await _moodActivitycollection.InsertManyAsync(activitiesToInsert);

        public async Task<long> UpdateManyAsync(List<MoodActivity> activitiesToupdate)
        {
            var updates = new List<WriteModel<MoodActivity>>();
            var filterBuilder = Builders<MoodActivity>.Filter;

            foreach (MoodActivity moodMark in activitiesToupdate)
            {
                var filter = filterBuilder.Where(x=>x.Id == moodMark.Id);
                updates.Add(new ReplaceOneModel<MoodActivity>(filter, moodMark));
            }

            var result = await _moodActivitycollection.BulkWriteAsync(updates);

            return result.MatchedCount;
        }

        public async Task<long> RemoveManyAsync(List<MoodActivity> activitiesToDelete)
        {
            IEnumerable<string?> ids = activitiesToDelete.Select(x => x.Id);

            FilterDefinition<MoodActivity> idsFilter = Builders<MoodActivity>.Filter.In(m => m.Id, ids);

            var result = await _moodActivitycollection.DeleteManyAsync(idsFilter);

            return result.DeletedCount;
        }

        public async Task<long> RemoveManyByIdsAsync(List<string> ids)
        {
            FilterDefinition<MoodActivity> idsFilter = Builders<MoodActivity>.Filter.In(m => m.Id, ids);

            var result = await _moodActivitycollection.DeleteManyAsync(idsFilter);

            return result.DeletedCount;
        }

        public string GenerateObjectId() =>
         ObjectId.GenerateNewId().ToString();
    }
}
