using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ZenithApp.ZenithMessage
{
    public class tbl_audit_administration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string AuditId { get; set; }

        public string Type { get; set; }   // e.g., "administrate"

        public List<AdministrationCopy> AdministrationHardCopy { get; set; } = new();
        public List<AdministrationCopy> AdministrationSoftCopy { get; set; } = new();

        public string AdditionalComments { get; set; }
        public string ReviewedBy { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDelete { get; set; } = false;
    }

    public class AdministrationCopy
    {
        public string Copy { get; set; }
        public string Compliant { get; set; }
        public string ReviewerComments { get; set; }
        public string LeadAuditorResponse { get; set; }
        public string ReviewerAcceptance { get; set; }
    }
}
