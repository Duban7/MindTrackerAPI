using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Models
{
    public class MoodGroup
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Name { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string>? Activities { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? AccountId { get; set; }
        public bool Visible { get; set; }
        public int Order { get; set; }
    }
}
