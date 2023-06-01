using Domain.Models;

namespace DAL.Abstraction
{
    public interface IMoodMarksRepository 
    {
        public Task<List<MoodMark>> GetAllAsync(string accountId);
        public Task RemoveAllAsync(string accountId);
        public Task<long> UpdateManyAsync(List<MoodMark> moodMarks, string accountId);
        public Task<long> RemoveManyAsync(List<MoodMark> moodMarks);
        public Task InsertManyAsync(List<MoodMark> moodMarks);
        public Task<MoodMark> GetOneAsync(DateTime date, string accountId);
        public Task InsertAsync(MoodMark moodMark);
        public Task<long> UpdateAsync(MoodMark moodMark);
        public Task<long> RemoveAsync(DateTime date, string accountId);
    }
}
