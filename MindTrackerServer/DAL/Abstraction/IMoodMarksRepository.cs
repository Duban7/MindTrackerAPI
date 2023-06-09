﻿using Domain.Models;

namespace DAL.Abstraction
{
    public interface IMoodMarksRepository 
    {
        public Task<List<MoodMark>> GetAllAsync(string accountId);
        public Task<List<MoodMarkWithActivities>> GetAllWithActivitiesAsync(string accountId);
        public Task<List<MoodMark>> GetAllByActivityIdAsync(string moodActivityId);
        public Task RemoveAllAsync(string accountId);
        public Task<long> UpdateManyAsync(List<MoodMark> moodMarks);
        public Task<long> RemoveManyAsync(List<MoodMark> moodMarks);
        public Task InsertManyAsync(List<MoodMark> moodMarks);
        public Task<MoodMark> GetOneAsync(string id);
        public Task<MoodMark> GetOneByActivityIdAsync(string moodActivityId);
        public Task<MoodMarkWithActivities> GetOneWithActivities(string moodMarkId);
        public Task InsertAsync(MoodMark moodMark);
        public Task<long> UpdateAsync(MoodMark moodMark);
        public Task<long> RemoveAsync(string id);
    }
}
