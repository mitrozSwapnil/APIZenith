using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_AuditProcess
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string AuditId { get; set; }

        public string ApplicationId { get; set; }
        public string CertificateId { get; set; }
        public string StepObjectId { get; set; }
        public string Url { get; set; }
        public string CreatedBy { get; set; }
        public string? Status { get; set; }
        public string UpdatedBy { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime UpdatedAt { get; set; }
    }
}
