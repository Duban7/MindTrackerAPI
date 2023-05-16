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

        public async Task<MoodMark> GetOneAsync(DateTime date, string accountId) =>
            await _moodMarksCollection
                    .Find(x => x.AccountId == accountId && x.Date.Day == date.Day)
                    .FirstOrDefaultAsync();

        public async Task<DeleteResult> RemoveAsync(DateTime date, string accountId) =>
            await _moodMarksCollection.DeleteOneAsync(x => x.AccountId == accountId && x.Date.Day == date.Day);

        public async Task UpdateAllAsync(List<MoodMark> moodMarks)
        {
            string? accountId = moodMarks.FirstOrDefault()!.AccountId;

            List<MoodMark> currentMoodMarks = await _moodMarksCollection.Find(x => x.AccountId == accountId).ToListAsync();

            List<MoodMark> markToUpdate = currentMoodMarks.IntersectBy(moodMarks.Select(x=>x.Date.Day), x => x.Date.Day).ToList();
            List<MoodMark> markToDelete = currentMoodMarks.ExceptBy(moodMarks.Select(x => x.Date.Day), x => x.Date.Day).ToList();
            List<MoodMark> markToAdd = moodMarks.ExceptBy(currentMoodMarks.Select(x => x.Date.Day), x => x.Date.Day).ToList();

            var updates = new List<WriteModel<MoodMark>>();
            var filterBuilder = Builders<MoodMark>.Filter;

            foreach(MoodMark moodMark in moodMarks)
            {
                var filter = filterBuilder.Where(x => x.Id == moodMark.Id);
                updates.Add(new ReplaceOneModel<MoodMark>(filter, moodMark));
            }

            await _moodMarksCollection.BulkWriteAsync(updates);

            //foreach(var m in moodMarks) await _moodMarksCollection.ReplaceOneAsync(x => x.AccountId == m.AccountId && x.Date.Day == m.Date.Day, m);

            IEnumerable<string?> ids = markToDelete.Select(x => x.Id);

            FilterDefinition<MoodMark> idsFilter = Builders<MoodMark>.Filter.In(m=>m.Id, ids);

            await _moodMarksCollection.DeleteManyAsync(idsFilter);

            // foreach (var m in moodMarks) await _moodMarksCollection.DeleteOneAsync(x=>x.Id == m.Id);

            await _moodMarksCollection.InsertManyAsync(markToAdd);
        }

        public async Task UpdateAsync(MoodMark moodMark)
        {
            moodMark.Id ??= ObjectId.GenerateNewId().ToString();

            await _moodMarksCollection.ReplaceOneAsync(x=>x.AccountId == moodMark.AccountId && x.Date.Day == moodMark.Date.Day, moodMark);
        }
    }
}
