using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ZenithApp.ZenithMessage;

namespace ZenithApp.ZenithEntities
{
    public class tbl_dynamic_audit_template
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_certificateId { get; set; }

        public List<StageTemplateModel> Stages { get; set; } = new List<StageTemplateModel>();

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedBy { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UpdatedBy { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDelete { get; set; } = false;
        public bool isApproval { get; set; } = false;
        public string AttachmentType { get; set; } 
        public string status { get; set; } 
    }

}
