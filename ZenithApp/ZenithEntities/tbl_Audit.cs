using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_Audit
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string FileNumber { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_quotation { get; set; }

        public string ApplicationId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_sub_applicationId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_CertificateId { get; set; }

         [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_status { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Received_Date { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedBy { get; set; }
        public bool isDelete { get; set; } = false;
        public bool IsAuditNominationDone { get; set; } = false;

        // 🆕 new field for assigned users
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> AssignedUsers { get; set; } = new List<string>();
    }
}
