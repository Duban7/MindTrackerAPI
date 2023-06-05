using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain.Models
{
    public class AccountWithData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public RefreshToken? RefreshToken { get; set; }
        public List<MoodGroupWithActivities>? Groups { get; set; }
        public List<MoodMarkWithActivities>? Records { get; set; }
    }
}
