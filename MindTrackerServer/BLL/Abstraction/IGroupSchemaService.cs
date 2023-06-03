using Domain.Models;

namespace BLL.Abstraction
{
    public interface IGroupSchemaService
    {
        public Task CreateGroups(List<MoodGroupWithActivities> groupsToCreate, string accountId);
        public Task UpdateGroups(List<MoodGroupWithActivities> groupsToUpdate);
        public Task RemoveGroups(List<string> groupsToUpdate, string accountId);
        public Task<List<MoodGroupWithActivities>> GetAllGroupsWithActivities(string accountId);
    }
}
