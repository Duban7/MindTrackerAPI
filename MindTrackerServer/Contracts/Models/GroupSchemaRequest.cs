namespace Domain.Models
{
    public class GroupSchemaRequest
    {
        public List<MoodGroupWithActivities>? CreatedGroups { get; set; }
        public List<MoodGroupWithActivities>? UpdatedGroups { get; set; }
        public List<string>? DeletedGroups { get; set; }
    }
}
