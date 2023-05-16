using Domain.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Abstraction
{
    public interface IMoodMarksRepository 
    {
        public Task<List<MoodMark>> GetAllAsync(string accountId);
        public Task UpdateAllAsync(List<MoodMark> moodMarks);
        public Task<MoodMark> GetOneAsync(DateTime date, string accountId);
        public Task UpdateAsync(MoodMark moodMark);
        public Task<DeleteResult> RemoveAsync(DateTime date, string accountId);
    }
}
