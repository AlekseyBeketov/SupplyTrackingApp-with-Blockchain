using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Blockchain_Supply_Chain_Tracking_System.Models
{
    public class UserGroup
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
        public List<int> UserIds { get; set; } = new List<int>();
    }
}
