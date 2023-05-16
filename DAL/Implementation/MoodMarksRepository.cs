using DAL.Abstraction;
using Domain.Models;
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

        public async Task CreateAsync(MoodMark newUser) =>
            await _moodMarksCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(MoodMark updatedUser) =>
            await _moodMarksCollection.ReplaceOneAsync(x => x.Id == updatedUser.Id, updatedUser);

        public async Task RemoveAsync(string id) =>
            await _moodMarksCollection.DeleteOneAsync(x => x.Id == id);
    }
}
