using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_Master_Remark
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? Remark_Name { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_Certificate { get; set; }
    }
}
