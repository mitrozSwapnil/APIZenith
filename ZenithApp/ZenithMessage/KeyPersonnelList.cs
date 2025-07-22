using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class KeyPersonnelList
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? customerKeyPersonnelId { get; set; }
        public string? Name { get; set; }
        public string? Designation { get; set; }
        public string? EmailId { get; set; }
        public string? Contact_No { get; set; }
        public string? Type { get; set; } 
        // Top Management / Authorized Representative
    }
}
