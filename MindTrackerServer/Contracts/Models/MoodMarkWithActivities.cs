using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Domain.Models
{
    public class MoodMarkWithActivities
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? AccountId { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime Date { get; set; }
        public int? Mood { get; set; }
        public List<MoodActivity>? Activities { get; set; }
        public List<string>? Images { get; set; }
        public string? Note { get; set; }
    }
}
