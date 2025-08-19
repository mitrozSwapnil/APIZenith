using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ZenithApp.ZenithEntities
{
    public class tbl_master_technicalArea
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public bool isDelete { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedBy { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedOn { get; set; }
    }
}
