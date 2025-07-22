using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_Application_Remark
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_ApplicationId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_MasterRemark { get; set; }
        public string? IsApplicable { get; set; }
        public string? Comment { get; set; }
        public string? AdditionalRemark{ get; set; }
    }
}
