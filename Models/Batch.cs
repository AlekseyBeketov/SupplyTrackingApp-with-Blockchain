using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Blockchain_Supply_Chain_Tracking_System.Models
{
    public class Batch
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BatchId { get; set; }

        public List<Product> Products { get; set; }
        public int UserId { get; set; }

    }

    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}