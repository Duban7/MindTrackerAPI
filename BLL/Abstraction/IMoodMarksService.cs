using Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Abstraction
{
    public interface IMoodMarksService
    {
        public Task<List<MoodMark>> GetAllMoodMarks(string accountId);
        public Task UpdateAll(List<MoodMark> moodMarks, string accoutnId);
        public Task DeleteAll(string accountId);
        public Task UpdateOne(MoodMark moodMark);
        public Task DeleteOne(DateTime date, string accountId);    
    }
}
