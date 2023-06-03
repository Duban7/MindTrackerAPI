using Domain.Models;

namespace BLL.Abstraction
{
    public interface IMoodMarksService
    {
        public Task<List<MoodMark>> GetAllMoodMarks(string accountId);
        public Task<List<MoodMarkWithActivities>> GetAllMoodMarksWithActivities(string accountId);
        public Task UpdateAll(List<MoodMark> moodMarks, string accoutnId);
        public Task DeleteAll(string accountId);
        public Task InsertOne(MoodMark moodMark, string accountId);
        public Task UpdateOne(MoodMark moodMark, string accountId);
        public Task DeleteOne(DateTime date, string accountId);
    }
}
