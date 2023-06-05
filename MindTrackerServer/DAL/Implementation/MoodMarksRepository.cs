using DAL.Abstraction;
using Domain.Converters;
using Domain.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DAL.Implementation
{
    public class MoodMarksRepository : IMoodMarksRepository
    {
        private readonly IMongoCollection<MoodMark> _moodMarksCollection;
        private readonly ILogger<MoodMarksRepository> _logger;
        public MoodMarksRepository(IMongoCollection<MoodMark> moodMarksCollection, ILogger<MoodMarksRepository> logger) 
        {
            _moodMarksCollection = moodMarksCollection;
            _logger = logger;
        }

        public async Task<List<MoodMark>> GetAllAsync(string accountId) =>
             await _moodMarksCollection.Find(x => x.AccountId == accountId).ToListAsync();

        public async Task<List<MoodMarkWithActivities>> GetAllWithActivitiesAsync(string accountId)
        {
            var pipeline = new BsonDocument[]
           {
                new BsonDocument("$match",
                new BsonDocument("AccountId",
                new ObjectId(accountId))),
                new BsonDocument("$lookup",
                new BsonDocument
                    {
                        { "from", "MoodActivities" },
                        { "localField", "Activities" },
                        { "foreignField", "_id" },
                        { "as", "Activities" }
                    }),
                new BsonDocument("$unwind",
                new BsonDocument("path", "$Activities")),
                new BsonDocument("$group",
                new BsonDocument
                    {
                        { "_id", "$_id" },
                        { "AccountId",
                new BsonDocument("$first", "$AccountId") },
                        { "Date",
                new BsonDocument("$first", "$Date") },
                        { "Mood",
                new BsonDocument("$first", "$Mood") },
                        { "Activities",
                new BsonDocument("$push", "$Activities") },
                        { "Images",
                new BsonDocument("$first", "$Images") },
                        { "Note",
                new BsonDocument("$first", "$Note") }
                    })
            };
            var groupsWithActivities = await _moodMarksCollection.Aggregate<MoodMarkWithActivities>(pipeline).ToListAsync();
            var groupsWithoutActivities = MoodMarkconverter.ConvertToMoodMarksWithClearActivities(await _moodMarksCollection.Find(x => x.AccountId == accountId && x.Activities!.Count == 0).ToListAsync());

            groupsWithActivities.AddRange(groupsWithoutActivities);

            return groupsWithActivities;
        }

        public async Task<List<MoodMark>> GetAllByActivityIdAsync(string activityid) =>
            await _moodMarksCollection.Find(x => x.Activities!.Contains(activityid)).ToListAsync();

        public async Task RemoveAllAsync(string accountId) =>
            await _moodMarksCollection.DeleteManyAsync(Builders<MoodMark>.Filter.Where(x=>x.AccountId == accountId));

        public async Task<MoodMark> GetOneAsync(string id) =>
            await _moodMarksCollection.Find(x=>x.Id==id).FirstOrDefaultAsync();

        public async Task<MoodMark> GetOneByActivityIdAsync(string moodActivityId) =>
            await _moodMarksCollection.Find(x => x.Activities!.Contains(moodActivityId)).FirstOrDefaultAsync();

        public async Task<MoodMarkWithActivities> GetOneWithActivities(string moodMarkId)
        {
            var pipeline = new List<BsonDocument>
            {
                new BsonDocument("$match",
                new BsonDocument("_id",
                new ObjectId(moodMarkId))),
                new BsonDocument("$lookup",
                new BsonDocument
                    {
                        { "from", "MoodActivities" },
                        { "localField", "Activities" },
                        { "foreignField", "_id" },
                        { "as", "Activities" }
                    }),
                new BsonDocument("$unwind",
                new BsonDocument("path", "$Activities")),
                new BsonDocument("$group",
                new BsonDocument
                    {
                        { "_id", "$_id" },
                        { "AccountId",
                new BsonDocument("$first", "$AccountId") },
                        { "Date",
                new BsonDocument("$first", "$Date") },
                        { "Mood",
                new BsonDocument("$first", "$Mood") },
                        { "Activities",
                new BsonDocument("$push", "$Activities") },
                        { "Images",
                new BsonDocument("$first", "$Images") },
                        { "Note",
                new BsonDocument("$first", "$Note") }
                    })
            };
            return await _moodMarksCollection.Aggregate<MoodMarkWithActivities>(pipeline).FirstOrDefaultAsync();
        }

        public async Task<long> RemoveAsync(string id)
        {
            var result =  await _moodMarksCollection.DeleteOneAsync(x => x.Id == id);

            return result.DeletedCount;
        }

        public async Task<long> UpdateManyAsync(List<MoodMark> marksToUpdate)
        {
            var updates = new List<WriteModel<MoodMark>>();
            var filterBuilder = Builders<MoodMark>.Filter;

            foreach(MoodMark moodMark in marksToUpdate)
            {
                var filter = filterBuilder.Where(x=>x.Id == moodMark.Id);
                updates.Add(new ReplaceOneModel<MoodMark>(filter, moodMark));
            }

            var result = await _moodMarksCollection.BulkWriteAsync(updates);

           //foreach(var m in moodMarks) await _moodMarksCollection.ReplaceOneAsync(x => x.AccountId == m.AccountId && x.Date.Day == m.Date.Day, m);

            return result.MatchedCount; 
        }

        public async Task<long> RemoveManyAsync(List<MoodMark> marksToDelete)
        {
            IEnumerable<string?> ids = marksToDelete.Select(x => x.Id);

            FilterDefinition<MoodMark> idsFilter = Builders<MoodMark>.Filter.In(m => m.Id, ids);
            
            // foreach (var m in moodMarks) await _moodMarksCollection.DeleteOneAsync(x=>x.Id == m.Id);

            var result = await _moodMarksCollection.DeleteManyAsync(idsFilter);

            return result.DeletedCount;
        }

        public async Task InsertManyAsync(List<MoodMark> marksToInsert)=>
            await _moodMarksCollection.InsertManyAsync(marksToInsert);
        
        public async Task InsertAsync(MoodMark moodMark)=>
            await _moodMarksCollection.InsertOneAsync(moodMark);

        public async Task<long> UpdateAsync(MoodMark moodMark)
        {
            var result =  await _moodMarksCollection.ReplaceOneAsync(x=>x.Id == moodMark.Id, moodMark);
        
            return result.ModifiedCount;
        }
    }
}
