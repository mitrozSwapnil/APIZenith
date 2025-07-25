using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class AccreditationsList
    {
        //[BsonRepresentation(BsonType.ObjectId)]
        //public string Id { get; set; }
        public string Name { get; set; }
    }
}
