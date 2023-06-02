using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Models
{
    //1 (1)
    public class MoodMark
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? AccountId { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime Date { get; set; }
        public string? Mood { get; set; }
        public List<MoodActivity>? Activities { get; set; }
        public string[]? Images { get; set; }
        public string? Note { get; set; }
    }
}
