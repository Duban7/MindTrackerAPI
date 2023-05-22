﻿using Amazon.Runtime.Internal.Util;
using DAL.Abstraction;
using Domain.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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

        public async Task RemoveAllAsync(string accountId) =>
            await _moodMarksCollection.DeleteManyAsync(Builders<MoodMark>.Filter.Where(x=>x.AccountId == accountId));

        public async Task<MoodMark> GetOneAsync(DateTime date, string accountId) =>
            await _moodMarksCollection
                    .Find(GetDateAndAccountIdEqulsExpression(date, accountId))
                    .FirstOrDefaultAsync();

        public async Task<long> RemoveAsync(DateTime date, string accountId)
        {
            var result =  await _moodMarksCollection.DeleteOneAsync(GetDateAndAccountIdEqulsExpression(date, accountId));

            return result.DeletedCount;
        }

        public async Task<long> UpdateManyAsync(List<MoodMark> marksToUpdate, string accountId)
        {
            var updates = new List<WriteModel<MoodMark>>();
            var filterBuilder = Builders<MoodMark>.Filter;

            foreach(MoodMark moodMark in marksToUpdate)
            {
                var filter = filterBuilder.Where(GetDateAndAccountIdEqulsExpression(moodMark.Date, accountId));
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
            var result =  await _moodMarksCollection.ReplaceOneAsync(GetDateAndAccountIdEqulsExpression(moodMark.Date, moodMark.AccountId!), moodMark);
        
            return result.ModifiedCount;
        }

        private static Expression<Func<MoodMark, bool>> GetDateAndAccountIdEqulsExpression(DateTime date, string accountId) =>
            x => 
            x.Date.Day == date.Day 
            && x.Date.Month == date.Month 
            && x.Date.Year == date.Year 
            && x.AccountId == accountId;
    }
}
