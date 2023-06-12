using Domain.Models;

namespace Domain.Converters
{
    public static class MoodMarkConverter
    {
        /// <summary>
        /// Converts list of MoodMark to list of MoodMarkWithActivities.
        /// Only available for MoodMark without activities
        /// </summary>
        /// <param name="moodMarks"></param>
        /// <returns></returns>
        public static List<MoodMarkWithActivities> ConvertToMoodMarksWithClearActivities(List<MoodMark> moodMarks)
        {
            var moodMarkWAList = new List<MoodMarkWithActivities>();
            foreach (MoodMark mark in moodMarks)
            {
                moodMarkWAList.Add(
                    new MoodMarkWithActivities()
                    {
                        Id = mark.Id,
                        AccountId = mark.AccountId,
                        Date = mark.Date,
                        Images = mark.Images,
                        Mood = mark.Mood,
                        Note = mark.Note,
                        Activities = new()
                    }
                  );
            }
            return moodMarkWAList;
        }
    }
}
