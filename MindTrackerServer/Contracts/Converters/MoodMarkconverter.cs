using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Converters
{
    public static class MoodMarkconverter
    {
        /// <summary>
        /// Converts list of MoodMarkWithActivities to list of MoodMark
        /// </summary>
        /// <param name="moodMarkWithActivities"></param>
        /// <returns></returns>
        public static List<MoodMark> ConvertToMoodMarks(List<MoodMarkWithActivities> moodMarkWithActivities)
        {
            var moodMarkList = new List<MoodMark>();
            foreach (MoodMarkWithActivities mark in moodMarkWithActivities)
            {
                moodMarkList.Add(
                    new MoodMark()
                    {
                        Id = mark.Id,
                        AccountId = mark.AccountId,
                        Date = mark.Date,
                        Images = mark.Images,
                        Mood = mark.Mood,
                        Note = mark.Note,
                        Activities = mark.Activities!.Select(x => x.Id).ToList()!
                    }
                  );
            }
            return moodMarkList;
        }

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
