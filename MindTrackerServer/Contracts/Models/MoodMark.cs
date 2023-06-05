using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Models
{
    public class MoodMark
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? AccountId { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime Date { get; set; }
        public int? Mood { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string>? Activities { get; set; } 
        public List<string>? Images { get; set; }
        public string? Note { get; set; }
    }
}
