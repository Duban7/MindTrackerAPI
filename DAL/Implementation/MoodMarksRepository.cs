using DAL.Abstraction;
using Domain.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Implementation
{
    public class MoodMarksRepository : IMoodMarksRepository
    {
        public readonly IMongoCollection<MoodMark> _moodMarksCollection;
        public MoodMarksRepository(IMongoCollection<MoodMark> moodMarksCollection) 
        {
            _moodMarksCollection = moodMarksCollection;
        }

        public async Task<List<MoodMark>> GetAllAsync(string accountId) =>
             await _moodMarksCollection.Find(x => x.AccountId == accountId).ToListAsync();

        public async Task RemoveAllAsync(string accountId) =>
            await _moodMarksCollection.DeleteManyAsync(Builders<MoodMark>.Filter.Where(x=>x.AccountId == accountId));

        public async Task<MoodMark> GetOneAsync(DateTime date, string accountId) =>
            await _moodMarksCollection
                    .Find(x => x.AccountId == accountId && x.Date.Day == date.Day)
                    .FirstOrDefaultAsync();

        public async Task<long> RemoveAsync(DateTime date, string accountId)
        {
            var result =  await _moodMarksCollection.DeleteOneAsync(x => x.AccountId == accountId && x.Date.Day == date.Day);

            return result.DeletedCount;
        }

        public async Task<long> UpdateManyAsync(List<MoodMark> marksToUpdate)
        {
            var updates = new List<WriteModel<MoodMark>>();
            var filterBuilder = Builders<MoodMark>.Filter;

            foreach(MoodMark moodMark in marksToUpdate)
            {
                var filter = filterBuilder.Where(x => x.Id == moodMark.Id);
                updates.Add(new ReplaceOneModel<MoodMark>(filter, moodMark));
            }

            var result = await _moodMarksCollection.BulkWriteAsync(updates);
            
            //foreach(var m in moodMarks) await _moodMarksCollection.ReplaceOneAsync(x => x.AccountId == m.AccountId && x.Date.Day == m.Date.Day, m);

            return result.ModifiedCount; 
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
            moodMark.Id ??= ObjectId.GenerateNewId().ToString();

            var result =  await _moodMarksCollection.ReplaceOneAsync(x=>x.AccountId == moodMark.AccountId && x.Date.Day == moodMark.Date.Day, moodMark);
        
            return result.ModifiedCount;
        }
    }
}
