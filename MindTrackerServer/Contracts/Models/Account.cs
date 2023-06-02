using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Models
{
    //1 (1)
    public class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public RefreshToken? RefreshToken { get; set; }
    }
}
