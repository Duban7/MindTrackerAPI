using Domain.Models;

namespace Domain.Converters
{
    public static class MoodGroupConverter
    {
        /// <summary>
        /// Converts list of MoodGroupWithActivities to list of MoodGroup
        /// </summary>
        /// <param name="moodGroupsWithActivities"></param>
        /// <returns></returns>
        public static List<MoodGroup> ConverToMoodGroupList(List<MoodGroupWithActivities> moodGroupsWithActivities)
        {
            var moodGroupList = new List<MoodGroup>();
            foreach (MoodGroupWithActivities mgwa in moodGroupsWithActivities)
            {
                moodGroupList.Add(
                        new MoodGroup()
                        {
                            Id = mgwa.Id,
                            Name = mgwa.Name,
                            Activities = mgwa.Activities!.Select(x => x.Id).ToList()!,
                            AccountId = mgwa.AccountId,
                            Visible = mgwa.Visible,
                            Order = mgwa.Order
                        }
                    );
            }
            return moodGroupList;
        }

        /// <summary>
        /// Converts list of MoodGroup to list of MoodGroupWithActivities.
        /// Only available for moodGroups without activities
        /// </summary>
        /// <param name="moodGroups"></param>
        /// <returns></returns>
        public static List<MoodGroupWithActivities> ConvertToMoodGroupWithActivitiesList(List<MoodGroup> moodGroups)
        {
            var moodGroupList = new List<MoodGroupWithActivities>();
            foreach (MoodGroup moodGroup in moodGroups)
            {
                moodGroupList.Add(
                        new MoodGroupWithActivities()
                        {
                            Id = moodGroup.Id,
                            Name = moodGroup.Name,
                            Activities = new(),
                            AccountId = moodGroup.AccountId,
                            Visible = moodGroup.Visible,
                            Order = moodGroup.Order
                        }
                    );
            }
            return moodGroupList;
        }
    }
}
