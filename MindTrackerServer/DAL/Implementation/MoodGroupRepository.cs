using DAL.Abstraction;
using Domain.Converters;
using Domain.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace DAL.Implementation
{
    public class MoodGroupRepository : IMoodGroupRepository
    {
        private readonly IMongoCollection<MoodGroup> _moodGroupcollection;
        private readonly ILogger<MoodActivityRepository> _logger;

        public MoodGroupRepository(IMongoCollection<MoodGroup> moodGroupCollection, ILogger<MoodActivityRepository> logger)
        {
            _moodGroupcollection = moodGroupCollection;
            _logger = logger;
        }

        public async Task<List<MoodGroup>> GetAllByAccountId(string accountId)=>
            await _moodGroupcollection.Find(x=>x.AccountId == accountId).ToListAsync();

        public async Task<List<MoodGroupWithActivities>> GetAllWithActivities(string accountId)
        {
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match",
                                     new BsonDocument("AccountId",new ObjectId("647b4b9b6a35ed7eea438f0b"))
                                ),
                new BsonDocument("$lookup",
                                    new BsonDocument
                                    {
                                        { "from", "MoodActivities" },
                                        { "localField", "Activities" },
                                        { "foreignField", "_id" },
                                        { "as", "Activities" }
                                    }
                                ),
                new BsonDocument("$unwind",
                new BsonDocument("path", "$Activities")),
                new BsonDocument("$group",
                new BsonDocument
                    {
                        { "_id", "$_id" },
                        { "Name",
                new BsonDocument("$first", "$Name") },
                        { "Activities",
                new BsonDocument("$push", "$Activities") },
                        { "AccountId",
                new BsonDocument("$first", "$AccountId") },
                        { "Visible",
                new BsonDocument("$first", "$Visible") },
                        { "Order",
                new BsonDocument("$first", "$Order") }
                    })
            };

            var groupsWithActivities = await _moodGroupcollection.Aggregate<MoodGroupWithActivities>(pipeline).ToListAsync();
            var groupsWithoutActivities = MoodGroupConverter.ConvertToMoodGroupWithActivitiesList(await _moodGroupcollection.Find(x => x.AccountId == accountId && x.Activities!.Count == 0).ToListAsync());

            groupsWithActivities.AddRange(groupsWithoutActivities);

            return groupsWithActivities;
        }

        public async Task<List<MoodGroup>> GetMoodGroupsByIds(List<string> ids) =>
            await _moodGroupcollection.Find(Builders<MoodGroup>.Filter.In(m => m.Id, ids)).ToListAsync();

        public async Task CreateAsync(MoodGroup newGroup) =>
         await _moodGroupcollection.InsertOneAsync(newGroup);

        public async Task UpdateAsync(MoodGroup updatedGroup)=>
            await _moodGroupcollection.ReplaceOneAsync(x => x.Id == updatedGroup.Id, updatedGroup);
        
        public async Task RemoveAsyncById(string id) =>
            await _moodGroupcollection.DeleteOneAsync(x => x.Id == id);

        public async Task InsertManyAsync(List<MoodGroup> groupsToInsert) =>
            await _moodGroupcollection.InsertManyAsync(groupsToInsert);

        public async Task<long> UpdateManyAsync(List<MoodGroup> groupsToUpdate)
        {
            var updates = new List<WriteModel<MoodGroup>>();
            var filterBuilder = Builders<MoodGroup>.Filter;

            foreach(MoodGroup moodGroup in groupsToUpdate)
            {
                var filter = filterBuilder.Where(x=>x.Id == moodGroup.Id);
                updates.Add(new ReplaceOneModel<MoodGroup>(filter, moodGroup));
            }

            var result = await _moodGroupcollection.BulkWriteAsync(updates);

            return result.MatchedCount;
        }
        
        public async Task<long> RemoveManyAsync(List<MoodGroup> groupsToDelete, string accountId)
        {
            IEnumerable<string?> ids = groupsToDelete.Select(x => x.Id);

            FilterDefinition<MoodGroup> idsFilter = Builders<MoodGroup>.Filter.In(m => m.Id, ids);
            FilterDefinition<MoodGroup> accountIdFilter = Builders<MoodGroup>.Filter.Eq(m => m.AccountId, accountId);

            var filter = Builders<MoodGroup>.Filter.And(idsFilter, accountIdFilter);

            var result = await _moodGroupcollection.DeleteManyAsync(filter);

            return result.DeletedCount;
        }

        public async Task RemoveAllAsyncByAccountId(string accountId)=>
            await _moodGroupcollection.DeleteManyAsync(x => x.AccountId == accountId);

        public string GenerateObjectId() =>
         ObjectId.GenerateNewId().ToString();
    }
}
