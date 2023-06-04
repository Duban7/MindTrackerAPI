using Domain.Models;

namespace BLL.Abstraction
{
    public interface IMoodMarksService
    {
        public Task<List<MoodMarkWithActivities>> GetAllMoodMarksWithActivities(string accountId);
        public Task<MoodMarkWithActivities> InsertOne(MoodMark moodMark, string accountId);
        public Task<MoodMarkWithActivities> UpdateOne(MoodMark moodMark, string accountId);
        public Task DeleteOne(string date, string accountId);
    }
}
