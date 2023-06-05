using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain.Models
{
    public class MoodActivity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? IconName { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? GroupId { get; set; }
    }
}
