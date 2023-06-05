using Domain.Models;

namespace BLL.Abstraction
{
    public interface IMoodMarksService
    {
        public Task<List<MoodMarkWithActivities>> GetAllMoodMarksWithActivities(string accountId);
        public Task<MoodMarkWithActivities> InsertOne(MoodMark moodMark, string accountId);
        public Task<MoodMarkWithActivities> InsertOneWithImages(MoodMarkRequest moodMarkRequest, string accountId);
        public Task<MoodMarkWithActivities> UpdateOne(MoodMark moodMark);
        public Task<MoodMarkWithActivities> UpdateOneWithImages(MoodMarkRequest moodMarkRequest);
        public Task DeleteOneWithImages(string id, string accountId);
        public Task DeleteOne(string date, string accountId);
    }
}
